# ⚔️ TeamBattle: Многопоточная Симуляция (Лабораторная работа №15) ⚔️

Настольное C# WinForms приложение, моделирующее эпические сражения между несколькими командами. Каждая команда, работая в собственном потоке, стремится нанять бойцов из общего пула и сокрушить своих оппонентов. Проект наглядно демонстрирует принципы многопоточного программирования, синхронизации ресурсов и взаимодействия потоков в .NET.

Этот проект выполнен в рамках Лабораторной работы №15 "Работа с потоками", вариант №7:
> *Противостояние нескольких команд. Каждая команда увеличивается на случайное количество бойцов и убивает случайное количество бойцов участника. Борьба каждой команды реализуется в отдельном потоке. Бойцы для каждой команды выбираются из общего пула бойцов. Общий ресурс – пул бойцов.*

## Демонстрация Работы

![Пример работы приложения](image/screenshot.png)

## ✨ Основные Возможности

*   **Динамическая настройка симуляции:**
    *   Задание количества команд.
    *   Определение начального размера общего пула бойцов.
    *   Установка начального количества бойцов в каждой команде.
*   **Многопоточное выполнение:**
    *   Каждая команда действует в отдельном, управляемом потоке (`System.Threading.Thread`).
    *   Возможность запуска и корректной остановки симуляции.
*   **Управление приоритетами потоков:**
    *   Пользователь может изменять приоритет потока для выбранной команды (`ThreadPriority`) во время симуляции.
*   **Синхронизация доступа к общим ресурсам:**
    *   Безопасный доступ к общему пулу бойцов (`SharedPool`) с использованием `lock`.
    *   Потокобезопасное изменение количества бойцов в команде (`Team.FighterCount`).
*   **Интерактивный GUI на WinForms:**
    *   Отображение списка команд с их текущим количеством бойцов и приоритетом потока.
    *   Визуализация состояния общего пула бойцов.
    *   Подробный лог событий симуляции.
*   **Событийная модель:**
    *   Обновление UI происходит на основе событий от `SimulationManager` и `Team`.
    *   Корректное обновление UI из рабочих потоков с помощью `Invoke`/`BeginInvoke`.
*   **Чистое завершение работы:**
    *   Реализация `IDisposable` в `SimulationManager` для освобождения ресурсов и ожидания завершения потоков.

## ⚙️ Технологии и Библиотеки

*   **C#**: Основной язык разработки.
*   **.NET Framework / .NET Core**: Платформа выполнения (судя по путям `net8.0`, это .NET 8).
*   **WinForms**: Для создания графического пользовательского интерфейса.
*   **System.Threading**: Для работы с потоками (`Thread`, `ThreadPriority`, `lock`, `Monitor`).
*   **System.Threading.Tasks**: (Хотя `Task` здесь не основная единица, TPL упоминается в теоретической части).

## 🚀 Структура Проекта

Проект разделен на два основных компонента:

1.  **`TeamBattle.Core`**: Библиотека классов, содержащая всю логику симуляции.
    *   `SharedPool.cs`: Управляет общим пулом бойцов, доступным для найма. Обеспечивает потокобезопасный доступ.
    *   `Team.cs`: Представляет одну команду. Включает логику найма, атаки, получения урона и основной цикл действий команды. Каждая команда ассоциирована с потоком.
    *   `SimulationManager.cs`: Оркестратор симуляции. Отвечает за создание команд, пула, запуск/остановку потоков, управление приоритетами и предоставление событий для GUI.

2.  **`TeamBattle.WinForms`**: Проект Windows Forms приложения, предоставляющий пользовательский интерфейс.
    *   `MainForm.cs`: Главная форма приложения. Обрабатывает взаимодействие с пользователем, отображает состояние симуляции и управляет вызовами `SimulationManager`.
    *   `Program.cs`: Точка входа в приложение.

## 💡 Ключевые Концепции и Реализация

*   **Создание и управление потоками:**
    *   Каждая команда (`Team`) получает собственный экземпляр `Thread`.
    *   Потоки запускаются методом `StartSimulation()` в `SimulationManager`.
    *   Остановка потоков инициируется изменением флага `IsActive` у команды и вызовом `Interrupt()` для прерывания ожидания (`Thread.Sleep`). Метод `Join()` используется для ожидания завершения потоков.
*   **Синхронизация:**
    *   `lock(_poolLock)` в `SharedPool`: Гарантирует, что только один поток одновременно может изменять количество доступных бойцов или брать их из пула.
    *   `lock(_fighterCountLock)` в `Team`: Защищает счетчик бойцов команды при найме или получении урона.
    *   `volatile bool IsActive`: Используется для флага активности команды, чтобы его изменения были видны всем потокам без задержек кэширования.
*   **Общие ресурсы:**
    *   `SharedPool`: Является основным общим ресурсом, из которого команды нанимают бойцов.
    *   Список всех команд (`_teams` в `SimulationManager`): Используется командами для выбора цели атаки. Доступ к списку на чтение во время симуляции считается безопасным, т.к. состав команд не меняется после старта.
*   **Обновление GUI из потоков:**
    *   События, генерируемые `SimulationManager` (например, `LogMessageGenerated`, `SimulationStateChanged`, `TeamStateChangedRelay`), обрабатываются в `MainForm`.
    *   Для обновления элементов управления из рабочих потоков используются `Control.InvokeRequired` и `Control.BeginInvoke` для маршалинга вызовов в основной UI-поток.
*   **Обработка исключений в потоках:**
    *   Основная логика каждого потока команды (`TeamWorker`) обернута в `try-catch` для перехвата `ThreadAbortException` (хотя `Abort()` не рекомендуется и не используется напрямую для штатной остановки) и других общих исключений.
*   **Приоритеты потоков:**
    *   `SimulationManager.SetTeamPriority()` позволяет изменять приоритет потока для выбранной команды. Это демонстрирует, как ОС может распределять процессорное время между потоками.

## 🛠️ Сборка и Запуск

1.  Клонируйте репозиторий:
    ```bash
    git clone https://github.com/your-username/TeamBattle.git
    ```
2.  Откройте файл решения `TeamBattle.sln` в Visual Studio (рекомендуется VS 2022 или новее с поддержкой .NET 8).
3.  Соберите решение (Build -> Build Solution).
4.  Запустите проект `TeamBattle.WinForms` (установите его как Startup Project, если необходимо).

## 📄 Задание (Вариант 7)

> **Противостояние нескольких команд.** Каждая команда увеличивается на случайное количество бойцов и убивает случайное количество бойцов участника. Борьба каждой команды реализуется в отдельном потоке. Бойцы для каждой команды выбираются из общего пула бойцов. Общий ресурс – пул бойцов.

Проект полностью реализует данное задание, включая управление приоритетами потоков и графический интерфейс.

---
