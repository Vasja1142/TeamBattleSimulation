namespace TeamBattle.WinForms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            llabelTeamCount = new Label();
            numericUpDownTeamCount = new NumericUpDown();
            labelPoolSize = new Label();
            numericUpDownPoolSize = new NumericUpDown();
            labelInitialTeamSize = new Label();
            numericUpDownInitialTeamSize = new NumericUpDown();
            buttonSetup = new Button();
            groupBoxSetup = new GroupBox();
            groupBoxControl = new GroupBox();
            buttonStop = new Button();
            buttonStart = new Button();
            groupBoxState = new GroupBox();
            buttonSetPriority = new Button();
            comboBoxPriority = new ComboBox();
            labelPriority = new Label();
            listViewTeams = new ListView();
            columnHeaderId = new ColumnHeader();
            columnHeaderName = new ColumnHeader();
            columnHeaderFighters = new ColumnHeader();
            columnHeaderPriority = new ColumnHeader();
            labelPoolStateValue = new Label();
            labelPoolStateLabel = new Label();
            groupBoxLog = new GroupBox();
            textBoxLog = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTeamCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPoolSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownInitialTeamSize).BeginInit();
            groupBoxSetup.SuspendLayout();
            groupBoxControl.SuspendLayout();
            groupBoxState.SuspendLayout();
            groupBoxLog.SuspendLayout();
            SuspendLayout();
            // 
            // llabelTeamCount
            // 
            llabelTeamCount.AutoSize = true;
            llabelTeamCount.Location = new Point(23, 37);
            llabelTeamCount.Name = "llabelTeamCount";
            llabelTeamCount.Size = new Size(149, 20);
            llabelTeamCount.TabIndex = 0;
            llabelTeamCount.Text = "Количество команд:";
            // 
            // numericUpDownTeamCount
            // 
            numericUpDownTeamCount.Location = new Point(243, 35);
            numericUpDownTeamCount.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            numericUpDownTeamCount.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numericUpDownTeamCount.Name = "numericUpDownTeamCount";
            numericUpDownTeamCount.Size = new Size(150, 27);
            numericUpDownTeamCount.TabIndex = 1;
            numericUpDownTeamCount.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // labelPoolSize
            // 
            labelPoolSize.AutoSize = true;
            labelPoolSize.Location = new Point(23, 82);
            labelPoolSize.Name = "labelPoolSize";
            labelPoolSize.Size = new Size(177, 20);
            labelPoolSize.TabIndex = 2;
            labelPoolSize.Text = "Начальный пул бойцов:";
            // 
            // numericUpDownPoolSize
            // 
            numericUpDownPoolSize.Location = new Point(243, 80);
            numericUpDownPoolSize.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDownPoolSize.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownPoolSize.Name = "numericUpDownPoolSize";
            numericUpDownPoolSize.Size = new Size(150, 27);
            numericUpDownPoolSize.TabIndex = 3;
            numericUpDownPoolSize.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDownPoolSize.ValueChanged += numericUpDownPoolSize_ValueChanged;
            // 
            // labelInitialTeamSize
            // 
            labelInitialTeamSize.AutoSize = true;
            labelInitialTeamSize.Location = new Point(23, 128);
            labelInitialTeamSize.Name = "labelInitialTeamSize";
            labelInitialTeamSize.Size = new Size(214, 20);
            labelInitialTeamSize.TabIndex = 4;
            labelInitialTeamSize.Text = "Начально бойцов в команде:";
            // 
            // numericUpDownInitialTeamSize
            // 
            numericUpDownInitialTeamSize.Location = new Point(243, 126);
            numericUpDownInitialTeamSize.Name = "numericUpDownInitialTeamSize";
            numericUpDownInitialTeamSize.Size = new Size(150, 27);
            numericUpDownInitialTeamSize.TabIndex = 5;
            numericUpDownInitialTeamSize.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // buttonSetup
            // 
            buttonSetup.Location = new Point(186, 175);
            buttonSetup.Name = "buttonSetup";
            buttonSetup.Size = new Size(94, 29);
            buttonSetup.TabIndex = 6;
            buttonSetup.Text = "Настроить";
            buttonSetup.UseVisualStyleBackColor = true;
            buttonSetup.Click += buttonSetup_Click;
            // 
            // groupBoxSetup
            // 
            groupBoxSetup.Controls.Add(llabelTeamCount);
            groupBoxSetup.Controls.Add(buttonSetup);
            groupBoxSetup.Controls.Add(numericUpDownTeamCount);
            groupBoxSetup.Controls.Add(numericUpDownInitialTeamSize);
            groupBoxSetup.Controls.Add(labelPoolSize);
            groupBoxSetup.Controls.Add(labelInitialTeamSize);
            groupBoxSetup.Controls.Add(numericUpDownPoolSize);
            groupBoxSetup.Location = new Point(12, 12);
            groupBoxSetup.Name = "groupBoxSetup";
            groupBoxSetup.Size = new Size(429, 219);
            groupBoxSetup.TabIndex = 7;
            groupBoxSetup.TabStop = false;
            groupBoxSetup.Text = "Настройка Симуляции";
            // 
            // groupBoxControl
            // 
            groupBoxControl.Controls.Add(buttonStop);
            groupBoxControl.Controls.Add(buttonStart);
            groupBoxControl.Location = new Point(104, 594);
            groupBoxControl.Name = "groupBoxControl";
            groupBoxControl.Size = new Size(250, 84);
            groupBoxControl.TabIndex = 8;
            groupBoxControl.TabStop = false;
            groupBoxControl.Text = "Управление Симуляцией";
            // 
            // buttonStop
            // 
            buttonStop.Enabled = false;
            buttonStop.Location = new Point(140, 41);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(94, 29);
            buttonStop.TabIndex = 1;
            buttonStop.Text = "Стоп";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            // 
            // buttonStart
            // 
            buttonStart.Enabled = false;
            buttonStart.Location = new Point(23, 41);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(94, 29);
            buttonStart.TabIndex = 0;
            buttonStart.Text = "Старт";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // groupBoxState
            // 
            groupBoxState.BackgroundImageLayout = ImageLayout.None;
            groupBoxState.Controls.Add(buttonSetPriority);
            groupBoxState.Controls.Add(comboBoxPriority);
            groupBoxState.Controls.Add(labelPriority);
            groupBoxState.Controls.Add(listViewTeams);
            groupBoxState.Controls.Add(labelPoolStateValue);
            groupBoxState.Controls.Add(labelPoolStateLabel);
            groupBoxState.Location = new Point(12, 253);
            groupBoxState.Name = "groupBoxState";
            groupBoxState.Size = new Size(429, 320);
            groupBoxState.TabIndex = 9;
            groupBoxState.TabStop = false;
            groupBoxState.Text = "Состояние Симуляции";
            // 
            // buttonSetPriority
            // 
            buttonSetPriority.Enabled = false;
            buttonSetPriority.Location = new Point(252, 271);
            buttonSetPriority.Name = "buttonSetPriority";
            buttonSetPriority.Size = new Size(101, 29);
            buttonSetPriority.TabIndex = 5;
            buttonSetPriority.Text = "Установить";
            buttonSetPriority.UseVisualStyleBackColor = true;
            buttonSetPriority.Click += buttonSetPriority_Click;
            // 
            // comboBoxPriority
            // 
            comboBoxPriority.FormattingEnabled = true;
            comboBoxPriority.Location = new Point(69, 272);
            comboBoxPriority.Name = "comboBoxPriority";
            comboBoxPriority.Size = new Size(151, 28);
            comboBoxPriority.TabIndex = 4;
            // 
            // labelPriority
            // 
            labelPriority.AutoSize = true;
            labelPriority.Location = new Point(15, 248);
            labelPriority.Name = "labelPriority";
            labelPriority.Size = new Size(349, 20);
            labelPriority.TabIndex = 3;
            labelPriority.Text = "Установить приоритет для выбранной команды:";
            // 
            // listViewTeams
            // 
            listViewTeams.Columns.AddRange(new ColumnHeader[] { columnHeaderId, columnHeaderName, columnHeaderFighters, columnHeaderPriority });
            listViewTeams.FullRowSelect = true;
            listViewTeams.GridLines = true;
            listViewTeams.Location = new Point(49, 71);
            listViewTeams.MultiSelect = false;
            listViewTeams.Name = "listViewTeams";
            listViewTeams.Size = new Size(304, 174);
            listViewTeams.TabIndex = 2;
            listViewTeams.UseCompatibleStateImageBehavior = false;
            listViewTeams.View = View.Details;
            listViewTeams.SelectedIndexChanged += listViewTeams_SelectedIndexChanged;
            // 
            // columnHeaderId
            // 
            columnHeaderId.Text = "ID";
            columnHeaderId.Width = 0;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Команда";
            columnHeaderName.Width = 120;
            // 
            // columnHeaderFighters
            // 
            columnHeaderFighters.Text = "Бойцы";
            columnHeaderFighters.TextAlign = HorizontalAlignment.Right;
            columnHeaderFighters.Width = 80;
            // 
            // columnHeaderPriority
            // 
            columnHeaderPriority.Text = "Приоритет";
            columnHeaderPriority.Width = 100;
            // 
            // labelPoolStateValue
            // 
            labelPoolStateValue.AutoSize = true;
            labelPoolStateValue.Location = new Point(243, 38);
            labelPoolStateValue.Name = "labelPoolStateValue";
            labelPoolStateValue.Size = new Size(36, 20);
            labelPoolStateValue.TabIndex = 1;
            labelPoolStateValue.Text = "N/A";
            // 
            // labelPoolStateLabel
            // 
            labelPoolStateLabel.AutoSize = true;
            labelPoolStateLabel.Location = new Point(142, 38);
            labelPoolStateLabel.Name = "labelPoolStateLabel";
            labelPoolStateLabel.Size = new Size(95, 20);
            labelPoolStateLabel.TabIndex = 0;
            labelPoolStateLabel.Text = "Пул бойцов:";
            // 
            // groupBoxLog
            // 
            groupBoxLog.Controls.Add(textBoxLog);
            groupBoxLog.Location = new Point(483, 12);
            groupBoxLog.Name = "groupBoxLog";
            groupBoxLog.Size = new Size(716, 474);
            groupBoxLog.TabIndex = 10;
            groupBoxLog.TabStop = false;
            groupBoxLog.Text = "Лог Симуляции";
            // 
            // textBoxLog
            // 
            textBoxLog.Dock = DockStyle.Fill;
            textBoxLog.Location = new Point(3, 23);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(710, 448);
            textBoxLog.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1237, 713);
            Controls.Add(groupBoxLog);
            Controls.Add(groupBoxState);
            Controls.Add(groupBoxControl);
            Controls.Add(groupBoxSetup);
            Name = "MainForm";
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)numericUpDownTeamCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPoolSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownInitialTeamSize).EndInit();
            groupBoxSetup.ResumeLayout(false);
            groupBoxSetup.PerformLayout();
            groupBoxControl.ResumeLayout(false);
            groupBoxState.ResumeLayout(false);
            groupBoxState.PerformLayout();
            groupBoxLog.ResumeLayout(false);
            groupBoxLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label llabelTeamCount;
        private NumericUpDown numericUpDownTeamCount;
        private Label labelPoolSize;
        private NumericUpDown numericUpDownPoolSize;
        private Label labelInitialTeamSize;
        private NumericUpDown numericUpDownInitialTeamSize;
        private Button buttonSetup;
        private GroupBox groupBoxSetup;
        private GroupBox groupBoxControl;
        private Button buttonStop;
        private Button buttonStart;
        private GroupBox groupBoxState;
        private ListView listViewTeams;
        private Label labelPoolStateValue;
        private Label labelPoolStateLabel;
        private ColumnHeader columnHeaderId;
        private ColumnHeader columnHeaderName;
        private ColumnHeader columnHeaderFighters;
        private ColumnHeader columnHeaderPriority;
        private Button buttonSetPriority;
        private ComboBox comboBoxPriority;
        private Label labelPriority;
        private GroupBox groupBoxLog;
        private TextBox textBoxLog;
    }
}