// Файл: TeamBattle.Core/Team.cs
using System;
using System.Collections.Generic;
using System.Threading; // Для Thread, ThreadPriority

namespace TeamBattle.Core
{
    /// <summary>
    /// Представляет одну команду в симуляции противостояния.
    /// </summary>
    public class Team
    {
        private int _fighterCount;
        private readonly object _fighterCountLock = new object(); // Блокировка для счетчика бойцов
        public readonly Random _random; // Локальный рандом для действий команды

        public Guid Id { get; } = Guid.NewGuid(); // Уникальный ID команды
        public string Name { get; }
        public volatile bool IsActive; // Флаг активности потока (volatile для потокобезопасности чтения/записи)
        public Thread? AssociatedThread { get; set; } // Ссылка на поток

        // Событие для уведомления об изменении состояния (например, для GUI)
        public event Action<Team>? StateChanged;

        /// <summary>
        /// Получает текущее количество бойцов в команде.
        /// Доступ потокобезопасен.
        /// </summary>
        public int FighterCount
        {
            get
            {
                lock (_fighterCountLock)
                {
                    return _fighterCount;
                }
            }
            private set // Приватный сеттер, изменение через Recruit и TakeDamage
            {
                lock (_fighterCountLock)
                {
                    if (_fighterCount != value) // Изменяем только если значение действительно поменялось
                    {
                        _fighterCount = value;
                        // Уведомляем подписчиков об изменении состояния
                        OnStateChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Инициализирует новую команду.
        /// </summary>
        /// <param name="name">Имя команды.</param>
        /// <param name="initialFighters">Начальное количество бойцов.</param>
        public Team(string name, int initialFighters = 0)
        {
            Name = name;
            _fighterCount = Math.Max(0, initialFighters); // Убедимся, что не отрицательное
            IsActive = false; // Изначально не активна
            // Создаем свой экземпляр Random для каждого потока, чтобы избежать проблем с потокобезопасностью System.Random
            // Используем хитрый способ получения уникального seed
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Логика найма бойцов из общего пула.
        /// </summary>
        /// <param name="pool">Общий пул бойцов.</param>
        /// <returns>Сообщение о результате найма.</returns>
        public string Recruit(SharedPool pool)
        {
            if (pool == null) return $"{Name}: Ошибка - пул не найден.";

            int requested = _random.Next(1, 6); // Запрашиваем от 1 до 5 бойцов
            string message;

            if (pool.TryTakeFighters(requested, out int taken))
            {
                if (taken > 0)
                {
                    // Потокобезопасно увеличиваем счетчик своих бойцов
                    lock (_fighterCountLock)
                    {
                        _fighterCount += taken;
                        OnStateChanged(); // Уведомляем об изменении
                    }
                    message = $"{Name}: Нанято {taken} (из {requested}) бойцов. Всего: {FighterCount}. Пул: {pool.AvailableFighters}.";
                }
                else // TryTakeFighters вернул true, но taken = 0 (маловероятно при нашей логике, но возможно)
                {
                    message = $"{Name}: Не удалось нанять бойцов (запрошено {requested}). Пул пуст? ({pool.AvailableFighters})";
                }
            }
            else // Пул был пуст или requested <= 0
            {
                message = $"{Name}: Не удалось нанять бойцов (запрошено {requested}). Пул пуст ({pool.AvailableFighters}).";
            }
            return message;
        }

        /// <summary>
        /// Логика атаки на другую команду.
        /// </summary>
        /// <param name="target">Команда-цель.</param>
        /// <returns>Сообщение о результате атаки.</returns>
        public string Attack(Team target)
        {
            if (target == null || target == this || target.FighterCount <= 0 || this.FighterCount <= 0)
            {
                return $"{Name}: Не может атаковать (нет цели/себя/бойцов)."; // Не атакуем себя, пустые или мертвые команды
            }

            // Сила атаки зависит от количества своих бойцов (упрощенно)
            int maxDamage = Math.Max(1, this.FighterCount / 5); // Максимальный урон - 20% от своих бойцов (минимум 1)
            int damage = _random.Next(1, maxDamage + 1); // Наносим урон от 1 до maxDamage

            // Вызываем метод цели для получения урона (он потокобезопасный)
            int actualLosses = target.TakeDamage(damage);

            string message = $"{Name} ({FighterCount}) атакует {target.Name} ({target.FighterCount + actualLosses}) на {damage} урона. Потери цели: {actualLosses}. У цели осталось: {target.FighterCount}.";
            return message;
        }

        /// <summary>
        /// Метод для получения урона командой. Потокобезопасен.
        /// </summary>
        /// <param name="damage">Полученный урон.</param>
        /// <returns>Реальное количество потерянных бойцов.</returns>
        public int TakeDamage(int damage)
        {
            if (damage <= 0) return 0;

            int actualLosses = 0;
            lock (_fighterCountLock) // Блокируем счетчик бойцов цели
            {
                if (_fighterCount <= 0) return 0; // Уже нет бойцов

                actualLosses = Math.Min(damage, _fighterCount); // Нельзя потерять больше, чем есть
                _fighterCount -= actualLosses;
                OnStateChanged(); // Уведомляем об изменении
            }
            return actualLosses;
        }

        /// <summary>
        /// Выполняет один "ход" команды: найм и атака.
        /// </summary>
        /// <param name="pool">Общий пул.</param>
        /// <param name="allTeams">Список всех команд для выбора цели.</param>
        /// <returns>Список лог-сообщений за этот ход.</returns>
        public List<string> SimulateTurn(SharedPool pool, List<Team> allTeams)
        {
            List<string> turnLogs = new List<string>();

            // 1. Попытка найма
            if (_random.NextDouble() < 0.6) // Нанимаем с вероятностью 60%
            {
                turnLogs.Add(Recruit(pool));
            }

            // 2. Попытка атаки
            if (this.FighterCount > 0 && _random.NextDouble() < 0.8) // Атакуем с вероятностью 80%, если есть кем
            {
                // Выбираем случайную другую живую команду для атаки
                List<Team> potentialTargets = allTeams
                                              .Where(t => t != this && t.FighterCount > 0)
                                              .ToList();
                if (potentialTargets.Any())
                {
                    Team target = potentialTargets[_random.Next(potentialTargets.Count)];
                    turnLogs.Add(Attack(target));
                }
                else
                {
                    turnLogs.Add($"{Name}: Некого атаковать.");
                }
            }

            // 3. Небольшая случайная задержка перед следующим ходом
            try
            {
                Thread.Sleep(_random.Next(200, 1000)); // Задержка от 0.2 до 1 секунды
            }
            catch (ThreadInterruptedException)
            {
                // Поток прерывается (например, при остановке симуляции)
                IsActive = false; // Устанавливаем флаг для выхода из цикла
                turnLogs.Add($"{Name}: Поток прерван.");
                Thread.CurrentThread.Interrupt(); // Повторно выставляем флаг прерывания
            }


            return turnLogs;
        }


        /// <summary>
        /// Безопасно вызывает событие StateChanged.
        /// </summary>
        protected virtual void OnStateChanged()
        {
            // Вызываем событие в текущем потоке.
            // GUI должен будет использовать Invoke/BeginInvoke для обновления контролов.
            StateChanged?.Invoke(this);
        }

        /// <summary>
        /// Возвращает строковое представление команды.
        /// </summary>
        public override string ToString()
        {
            return $"{Name}: {FighterCount} бойцов (ID: {Id})";
        }
    }
}