// Файл: TeamBattle.WinForms/MainForm.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading; // Для ThreadPriority
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamBattle.Core; // Подключаем нашу библиотеку логики

namespace TeamBattle.WinForms
{
    public partial class MainForm : Form
    {
        private readonly SimulationManager _simulationManager;

        public MainForm()
        {
            InitializeComponent();

            _simulationManager = new SimulationManager();

            // Подписываемся на события менеджера симуляции
            _simulationManager.LogMessageGenerated += SimulationManager_LogMessageGenerated;
            _simulationManager.SimulationStateChanged += SimulationManager_SimulationStateChanged;
            _simulationManager.TeamStateChangedRelay += SimulationManager_TeamStateChangedRelay;

            // Инициализируем ComboBox приоритетов
            InitializePriorityComboBox();

            // Устанавливаем начальное состояние контролов
            UpdateUIState();
        }

        // --- Инициализация и обновление UI ---

        private void InitializePriorityComboBox()
        {
            // Заполняем ComboBox значениями из перечисления ThreadPriority
            comboBoxPriority.DataSource = Enum.GetValues(typeof(ThreadPriority));
            comboBoxPriority.SelectedItem = ThreadPriority.Normal; // Значение по умолчанию
        }

        /// <summary>
        /// Обновляет состояние элементов управления в зависимости от состояния симуляции.
        /// </summary>
        private void UpdateUIState()
        {
            bool isRunning = _simulationManager.IsRunning;
            bool isConfigured = _simulationManager.Teams.Any(); // Считаем настроенной, если есть команды

            // Группа Настройки
            numericUpDownTeamCount.Enabled = !isRunning;
            numericUpDownPoolSize.Enabled = !isRunning;
            numericUpDownInitialTeamSize.Enabled = !isRunning;
            buttonSetup.Enabled = !isRunning;

            // Группа Управления
            buttonStart.Enabled = isConfigured && !isRunning; // Можно стартовать, если настроено и не запущено
            buttonStop.Enabled = isRunning; // Можно остановить, если запущено

            // Группа Состояния
            buttonSetPriority.Enabled = isRunning && listViewTeams.SelectedItems.Count > 0; // Можно менять приоритет у запущенной и выбранной команды
            comboBoxPriority.Enabled = buttonSetPriority.Enabled;

            // Обновляем пул
            UpdatePoolLabel();
        }

        /// <summary>
        /// Обновляет метку с состоянием пула (потокобезопасно).
        /// </summary>
        private void UpdatePoolLabel()
        {
            // Используем ?. для безопасного доступа, если _pool еще null
            string poolText = _simulationManager.Pool?.AvailableFighters.ToString() ?? "N/A";
            // Безопасное обновление UI из любого потока
            SafeUpdateControlText(labelPoolStateValue, poolText);
        }

        /// <summary>
        /// Обновляет список команд в ListView (потокобезопасно).
        /// </summary>
        private void UpdateTeamListView()
        {
            // Используем BeginInvoke для асинхронного обновления, чтобы не блокировать вызывающий поток
            if (listViewTeams.InvokeRequired)
            {
                listViewTeams.BeginInvoke(new Action(UpdateTeamListViewInternal));
            }
            else
            {
                UpdateTeamListViewInternal();
            }
        }

        private void UpdateTeamListViewInternal()
        {
            listViewTeams.BeginUpdate(); // Начинаем пакетное обновление для производительности
            listViewTeams.Items.Clear(); // Очищаем старые записи

            foreach (var team in _simulationManager.Teams) // Получаем актуальный список
            {
                var listViewItem = new ListViewItem(team.Id.ToString()) // Скрытый столбец с ID
                {
                    Tag = team.Id // Сохраняем ID в Tag для легкого доступа
                };
                listViewItem.SubItems.Add(team.Name);
                listViewItem.SubItems.Add(team.FighterCount.ToString());
                // Получаем приоритет потока, если он жив
                string priorityText = "N/A";
                if (team.AssociatedThread != null && team.AssociatedThread.IsAlive)
                {
                    try { priorityText = team.AssociatedThread.Priority.ToString(); }
                    catch { /* Поток мог завершиться между IsAlive и доступом к Priority */ }
                }
                listViewItem.SubItems.Add(priorityText);

                // Добавляем цвет, если бойцов мало или нет
                if (team.FighterCount <= 0)
                {
                    listViewItem.ForeColor = Color.Gray;
                }
                else if (team.FighterCount < 5) // Пример порога
                {
                    listViewItem.ForeColor = Color.OrangeRed;
                }

                listViewTeams.Items.Add(listViewItem);
            }
            listViewTeams.EndUpdate(); // Завершаем пакетное обновление
        }


        /// <summary>
        /// Обновляет данные конкретной команды в ListView (потокобезопасно).
        /// </summary>
        private void UpdateSingleTeamInView(Team team)
        {
            if (listViewTeams.InvokeRequired)
            {
                listViewTeams.BeginInvoke(new Action<Team>(UpdateSingleTeamInViewInternal), team);
            }
            else
            {
                UpdateSingleTeamInViewInternal(team);
            }
        }

        private void UpdateSingleTeamInViewInternal(Team team)
        {
            // Ищем элемент по ID (сохраненному в Tag)
            foreach (ListViewItem item in listViewTeams.Items)
            {
                if (item.Tag is Guid id && id == team.Id)
                {
                    // Обновляем значения в SubItems
                    item.SubItems[2].Text = team.FighterCount.ToString(); // Индекс столбца "Бойцы"

                    string priorityText = "N/A";
                    if (team.AssociatedThread != null && team.AssociatedThread.IsAlive)
                    {
                        try { priorityText = team.AssociatedThread.Priority.ToString(); }
                        catch { }
                    }
                    item.SubItems[3].Text = priorityText; // Индекс столбца "Приоритет"

                    // Обновляем цвет
                    if (team.FighterCount <= 0) item.ForeColor = Color.Gray;
                    else if (team.FighterCount < 5) item.ForeColor = Color.OrangeRed;
                    else item.ForeColor = SystemColors.WindowText; // Возвращаем стандартный цвет

                    break; // Нашли и обновили, выходим из цикла
                }
            }
        }


        /// <summary>
        /// Добавляет сообщение в лог (потокобезопасно).
        /// </summary>
        private void AddLogMessage(string message)
        {
            if (textBoxLog.InvokeRequired)
            {
                // Используем BeginInvoke для асинхронности
                textBoxLog.BeginInvoke(new Action<string>(AddLogMessageInternal), message);
            }
            else
            {
                AddLogMessageInternal(message);
            }
        }

        private void AddLogMessageInternal(string message)
        {
            // Ограничиваем размер лога для производительности
            if (textBoxLog.Lines.Length > 500)
            {
                textBoxLog.Text = string.Join(Environment.NewLine, textBoxLog.Lines.Skip(100)); // Оставляем последние 400 строк
                textBoxLog.AppendText("...(лог урезан)..." + Environment.NewLine);
            }
            textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss.fff}: {message}{Environment.NewLine}");
            textBoxLog.ScrollToCaret(); // Автопрокрутка вниз
        }

        /// <summary>
        /// Потокобезопасно устанавливает текст для контрола.
        /// </summary>
        private void SafeUpdateControlText(Control control, string text)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(new Action<Control, string>(SafeUpdateControlTextInternal), control, text);
            }
            else
            {
                SafeUpdateControlTextInternal(control, text);
            }
        }
        private void SafeUpdateControlTextInternal(Control control, string text)
        {
            control.Text = text;
        }


        // --- Обработчики событий от SimulationManager ---

        private void SimulationManager_LogMessageGenerated(string message)
        {
            AddLogMessage(message); // Вызываем потокобезопасный метод добавления в лог
        }

        private void SimulationManager_SimulationStateChanged()
        {
            // Обновляем состояние UI и список команд (в потоке GUI)
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    UpdateUIState();
                    UpdateTeamListView(); // Обновляем весь список при старте/стопе
                    UpdatePoolLabel();
                }));
            }
            else
            {
                UpdateUIState();
                UpdateTeamListView();
                UpdatePoolLabel();
            }
        }

        private void SimulationManager_TeamStateChangedRelay(Team team)
        {
            // Обновляем только одну команду в списке и пул
            UpdateSingleTeamInView(team);
            UpdatePoolLabel(); // Пул мог измениться при найме
        }


        // --- Обработчики событий контролов ---

        private void buttonSetup_Click(object sender, EventArgs e)
        {
            int teamCount = (int)numericUpDownTeamCount.Value;
            int poolSize = (int)numericUpDownPoolSize.Value;
            int initialTeamSize = (int)numericUpDownInitialTeamSize.Value;

            _simulationManager.SetupSimulation(teamCount, poolSize, initialTeamSize);
            // Состояние UI и список обновятся через событие SimulationStateChanged
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            _simulationManager.StartSimulation();
            // Состояние UI обновится через событие SimulationStateChanged
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _simulationManager.StopSimulation();
            // Состояние UI обновится через событие SimulationStateChanged
        }

        private void listViewTeams_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Включаем кнопку установки приоритета только если выбрана команда и симуляция запущена
            UpdateUIState();
        }

        private void buttonSetPriority_Click(object sender, EventArgs e)
        {
            if (listViewTeams.SelectedItems.Count == 1 && comboBoxPriority.SelectedItem is ThreadPriority priority)
            {
                ListViewItem selectedItem = listViewTeams.SelectedItems[0];
                if (selectedItem.Tag is Guid teamId) // Получаем ID из Tag
                {
                    _simulationManager.SetTeamPriority(teamId, priority);
                    // Обновление приоритета в списке произойдет через событие TeamStateChangedRelay
                }
            }
            else
            {
                MessageBox.Show("Выберите команду из списка и приоритет.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // --- Обработка закрытия формы ---
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Корректно останавливаем симуляцию при закрытии формы
            AddLogMessage("Закрытие формы, остановка симуляции...");
            // Используем Dispose менеджера, который вызовет StopSimulation
            _simulationManager.Dispose();
            AddLogMessage("Симуляция остановлена.");
        }

        private void numericUpDownPoolSize_ValueChanged(object sender, EventArgs e)
        {

        }


    }
}