// Файл: TeamBattle.Core/SimulationManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TeamBattle.Core
{
    /// <summary>
    /// Управляет симуляцией противостояния команд.
    /// Отвечает за создание команд, пула, запуск/остановку потоков
    /// и предоставление событий для внешнего интерфейса (GUI).
    /// </summary>
    public class SimulationManager : IDisposable // Реализуем IDisposable для корректной остановки
    {
        private List<Team> _teams = new List<Team>();
        private SharedPool? _pool;
        private bool _isRunning = false;
        private readonly List<Thread> _workerThreads = new List<Thread>(); // Храним ссылки на потоки

        // --- Свойства ---

        /// <summary>
        /// Получает список команд, участвующих в симуляции.
        /// Возвращает копию списка для предотвращения внешних модификаций.
        /// </summary>
        public IReadOnlyList<Team> Teams => _teams.ToList().AsReadOnly();

        /// <summary>
        /// Получает общий пул бойцов.
        /// </summary>
        public SharedPool? Pool => _pool;

        /// <summary>
        /// Получает статус симуляции (запущена или нет).
        /// </summary>
        public bool IsRunning => _isRunning;

        // --- События ---

        /// <summary>
        /// Генерируется при появлении нового лог-сообщения от симуляции.
        /// </summary>
        public event Action<string>? LogMessageGenerated;

        /// <summary>
        /// Генерируется при изменении общего состояния симуляции (например, старт/стоп, изменение пула).
        /// </summary>
        public event Action? SimulationStateChanged;

        /// <summary>
        /// Генерируется при изменении состояния конкретной команды (перенаправляется от Team.StateChanged).
        /// </summary>
        public event Action<Team>? TeamStateChangedRelay;


        // --- Методы управления симуляцией ---

        /// <summary>
        /// Настраивает параметры симуляции: создает команды и пул.
        /// Должна вызываться перед StartSimulation().
        /// </summary>
        /// <param name="teamCount">Количество команд.</param>
        /// <param name="initialPoolSize">Начальный размер пула бойцов.</param>
        /// <param name="initialTeamSize">Начальный размер каждой команды (по умолчанию 0).</param>
        public void SetupSimulation(int teamCount, int initialPoolSize, int initialTeamSize = 5)
        {
            if (_isRunning)
            {
                OnLogMessageGenerated("Ошибка: Нельзя изменить настройки во время запущенной симуляции.");
                return;
            }
            if (teamCount <= 1)
            {
                OnLogMessageGenerated("Ошибка: Для симуляции требуется как минимум 2 команды.");
                return;
            }

            StopSimulationInternal(waitForThreads: false); // Останавливаем предыдущую симуляцию, если была

            _teams.Clear();
            _pool = new SharedPool(initialPoolSize);

            for (int i = 0; i < teamCount; i++)
            {
                Team newTeam = new Team($"Команда {i + 1}", initialTeamSize);
                // Подписываемся на событие изменения состояния команды, чтобы ретранслировать его
                newTeam.StateChanged += HandleTeamStateChanged;
                _teams.Add(newTeam);
            }

            OnLogMessageGenerated($"Симуляция настроена: {teamCount} команд, Пул: {initialPoolSize} бойцов.");
            OnSimulationStateChanged(); // Уведомляем об изменении состояния (новые команды, пул)
        }

        /// <summary>
        /// Запускает симуляцию, создавая и запуская потоки для каждой команды.
        /// </summary>
        public void StartSimulation()
        {
            if (_isRunning)
            {
                OnLogMessageGenerated("Симуляция уже запущена.");
                return;
            }
            if (_teams == null || _teams.Count < 2 || _pool == null)
            {
                OnLogMessageGenerated("Ошибка: Симуляция не настроена. Вызовите SetupSimulation() сначала.");
                return;
            }

            _isRunning = true;
            _workerThreads.Clear(); // Очищаем список старых потоков

            OnLogMessageGenerated("Запуск симуляции...");

            foreach (var team in _teams)
            {
                team.IsActive = true; // Активируем команду
                // Создаем поток, передавая метод TeamWorker и сам объект команды как параметр
                Thread teamThread = new Thread(TeamWorker);
                teamThread.IsBackground = true; // Делаем потоки фоновыми, чтобы они не мешали завершению приложения
                teamThread.Name = $"Thread_{team.Name}"; // Даем потоку имя для отладки
                team.AssociatedThread = teamThread; // Сохраняем ссылку на поток в команде

                _workerThreads.Add(teamThread); // Добавляем в список для управления
                teamThread.Start(team); // Запускаем поток, передавая команду
                OnLogMessageGenerated($"Поток для {team.Name} запущен.");
            }

            OnSimulationStateChanged(); // Уведомляем об изменении состояния (запущена)
        }

        /// <summary>
        /// Останавливает симуляцию, сигнализируя потокам о завершении и ожидая их.
        /// </summary>
        public void StopSimulation()
        {
            StopSimulationInternal(waitForThreads: true);
        }

        /// <summary>
        /// Внутренний метод остановки с опцией ожидания потоков.
        /// </summary>
        private void StopSimulationInternal(bool waitForThreads)
        {
            if (!_isRunning && !waitForThreads) // Если не запущена и ждать не надо - выходим
                return;

            if (_isRunning) // Только если была запущена
            {
                OnLogMessageGenerated("Остановка симуляции...");
                _isRunning = false; // Сначала меняем флаг

                // Сигнализируем всем потокам остановиться
                foreach (var team in _teams)
                {
                    team.IsActive = false;
                    // Можно попытаться прервать сон потока, если он долго спит
                    team.AssociatedThread?.Interrupt();
                }
            }


            if (waitForThreads && _workerThreads.Any())
            {
                OnLogMessageGenerated("Ожидание завершения потоков...");
                // Ожидаем завершения всех рабочих потоков
                foreach (var thread in _workerThreads)
                {
                    try
                    {
                        // Даем потоку время на завершение (например, 1.5 секунды)
                        // Join() без таймаута может заблокировать навсегда, если поток завис
                        if (!thread.Join(TimeSpan.FromMilliseconds(1500)))
                        {
                            OnLogMessageGenerated($"Предупреждение: Поток {thread.Name} не завершился вовремя.");
                            // В крайнем случае можно использовать Abort(), но это очень не рекомендуется
                            // thread.Abort(); // Не использовать без крайней необходимости!
                        }
                    }
                    catch (ThreadStateException ex) { OnLogMessageGenerated($"Ошибка состояния при ожидании потока {thread.Name}: {ex.Message}"); }
                    catch (ThreadInterruptedException ex) { OnLogMessageGenerated($"Ожидание потока {thread.Name} прервано: {ex.Message}"); }
                }
                OnLogMessageGenerated("Все потоки завершены (или таймаут ожидания истек).");
                _workerThreads.Clear(); // Очищаем список потоков
            }
            else
            {
                _workerThreads.Clear(); // Очищаем список, даже если не ждали
            }


            // Сбрасываем флаги активности команд на всякий случай
            foreach (var team in _teams)
            {
                team.IsActive = false;
                team.AssociatedThread = null; // Убираем ссылку на завершенный поток
            }

            OnSimulationStateChanged(); // Уведомляем об изменении состояния (остановлена)
        }


        /// <summary>
        /// Устанавливает приоритет для потока указанной команды.
        /// </summary>
        /// <param name="teamId">ID команды.</param>
        /// <param name="priority">Новый приоритет потока.</param>
        public void SetTeamPriority(Guid teamId, ThreadPriority priority)
        {
            Team? team = _teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                OnLogMessageGenerated($"Ошибка: Команда с ID {teamId} не найдена.");
                return;
            }

            Thread? thread = team.AssociatedThread;
            if (thread == null || !thread.IsAlive) // Проверяем, что поток существует и активен
            {
                OnLogMessageGenerated($"Предупреждение: Поток для команды {team.Name} не активен. Приоритет не изменен.");
                return;
            }

            try
            {
                thread.Priority = priority;
                OnLogMessageGenerated($"Приоритет для {team.Name} установлен на {priority}.");
                OnTeamStateChanged(team); // Уведомляем об изменении (чтобы GUI обновил приоритет)
            }
            catch (ThreadStateException ex)
            {
                OnLogMessageGenerated($"Ошибка установки приоритета для {team.Name}: Поток не в том состоянии. ({ex.Message})");
            }
            catch (ArgumentException ex) // Если передан неверный ThreadPriority
            {
                OnLogMessageGenerated($"Ошибка установки приоритета для {team.Name}: Неверное значение приоритета. ({ex.Message})");
            }
        }

        // --- Рабочий метод потока ---

        /// <summary>
        /// Метод, выполняемый в отдельном потоке для каждой команды.
        /// </summary>
        /// <param name="teamObj">Объект команды (переданный через Thread.Start).</param>
        private void TeamWorker(object? teamObj)
        {
            if (teamObj is not Team team)
            {
                OnLogMessageGenerated("Ошибка потока: неверный тип параметра.");
                return;
            }

            OnLogMessageGenerated($"{team.Name}: Рабочий поток начал выполнение.");


            //// ---> ДОБАВИТЬ СЛУЧАЙНУЮ ЗАДЕРЖКУ ЗДЕСЬ <--- Меняет исход битвы
            //try
            //{
            //    // Даем каждому потоку немного разное время на "раскачку"
            //    int initialDelay = team._random.Next(50, 300); // от 50 до 300 мс
            //    Thread.Sleep(initialDelay);
            //}
            //catch (ThreadInterruptedException) { /* Обработка прерывания, если нужно */ }


            try
            {
                // Основной цикл работы потока
                while (team.IsActive) // Проверяем флаг на каждой итерации
                {
                    // Выполняем один ход симуляции для команды
                    List<string> turnLogs = team.SimulateTurn(_pool!, _teams); // _pool и _teams не должны быть null здесь

                    // Генерируем события для каждого лог-сообщения
                    foreach (var log in turnLogs)
                    {
                        OnLogMessageGenerated(log);
                    }

                    // Проверка на истощение пула (можно добавить другие условия завершения)
                    if (_pool!.AvailableFighters <= 0 && _teams.All(t => t.FighterCount <= 0))
                    {
                        OnLogMessageGenerated($"Симуляция завершена: пул пуст и бойцов не осталось.");
                        // Сигнализируем главному потоку остановить все
                        // Не очень хорошо вызывать StopSimulation прямо отсюда,
                        // лучше просто выставить флаг или сгенерировать событие.
                        // Но для простоты пока оставим так, или просто выйдем из цикла.
                        team.IsActive = false; // Поток сам завершится
                                               // Можно инициировать остановку всех:
                                               // Task.Run(() => StopSimulation()); // Запустить остановку в другом потоке
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Это исключение возникает при вызове Thread.Abort() (не рекомендуется)
                OnLogMessageGenerated($"{team.Name}: Поток принудительно прерван (Abort).");
                // Важно сбросить прерывание, если нужно выполнить код очистки
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                // Ловим другие непредвиденные исключения в потоке
                OnLogMessageGenerated($"Критическая ошибка в потоке {team.Name}: {ex.Message}\n{ex.StackTrace}");
                // Возможно, стоит остановить всю симуляцию при критической ошибке
                // Task.Run(() => StopSimulation());
            }
            finally
            {
                OnLogMessageGenerated($"{team.Name}: Рабочий поток завершил выполнение.");
                // Можно добавить логику очистки ресурсов, если она нужна здесь
            }
        }


        // --- Вспомогательные методы для вызова событий ---

        private void OnLogMessageGenerated(string message)
        {
            LogMessageGenerated?.Invoke(message);
        }

        private void OnSimulationStateChanged()
        {
            SimulationStateChanged?.Invoke();
        }

        // Обработчик события от Team, который ретранслирует его наружу
        private void HandleTeamStateChanged(Team team)
        {
            OnTeamStateChanged(team);
        }

        private void OnTeamStateChanged(Team team)
        {
            TeamStateChangedRelay?.Invoke(team);
        }


        // --- Реализация IDisposable ---

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Подавляем финализацию, т.к. ресурсы освобождены
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Освобождаем управляемые ресурсы (останавливаем потоки)
                StopSimulationInternal(waitForThreads: true); // Гарантированно останавливаем

                // Отписываемся от событий команд (если нужно, хотя при очистке _teams это не так критично)
                foreach (var team in _teams)
                {
                    team.StateChanged -= HandleTeamStateChanged;
                }
            }

            // Освобождение неуправляемых ресурсов (если бы они были)

            _disposed = true;
        }

        // Финализатор (на случай, если Dispose не был вызван)
        ~SimulationManager()
        {
            Dispose(false);
        }
    }
}