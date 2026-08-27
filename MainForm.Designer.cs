using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private CheckBox chkEnableSwitching;
        private CheckBox chkStartWithWindows;
        private CheckBox chkRestoreApostrophes;
        private Button btnExit;
        private Label lblStatus;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem menuItemOpen;
        private ToolStripMenuItem menuItemPause;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemExit;
        private Label lblMinimumMappedPercent;
        private NumericUpDown numMinimumMappedPercent;
        
        private GroupBox grpProcesses;
        private Label lblProcessMode;
        private ComboBox cmbProcessMode;
        private ListBox lstProcesses;
        private TextBox txtNewProcess;
        private Button btnAddProcess;
        private Button btnRemoveProcess;
        private Button btnPickActive;
        private Timer pickTimer;
        
        private GroupBox grpLayoutRules;
        private CheckBox chkEnableLayoutRules;
        private ListBox lstLayoutRules;
        private TextBox txtNewLayoutRuleProcess;
        private ComboBox cmbNewLayoutRuleLayout;
        private Button btnAddLayoutRule;
        private Button btnRemoveLayoutRule;
        private Button btnPickActiveLayoutRule;

        private GroupBox grpEnterCorrection;
        private CheckBox chkSkipEnterCorrection;
        private TextBox txtSkipEnterProcesses;

        private GroupBox grpIgnoredWords;
        private ListBox lstIgnoredWords;
        private TextBox txtNewIgnoredWord;
        private Button btnAddIgnoredWord;
        private Button btnRemoveIgnoredWord;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                keyboardHook?.Dispose();
                layoutRuleEnforcer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.chkEnableSwitching = new System.Windows.Forms.CheckBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.chkRestoreApostrophes = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemPause = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            
            this.lblMinimumMappedPercent = new System.Windows.Forms.Label();
            this.numMinimumMappedPercent = new System.Windows.Forms.NumericUpDown();
            
            this.grpProcesses = new System.Windows.Forms.GroupBox();
            this.lblProcessMode = new System.Windows.Forms.Label();
            this.cmbProcessMode = new System.Windows.Forms.ComboBox();
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.txtNewProcess = new System.Windows.Forms.TextBox();
            this.btnAddProcess = new System.Windows.Forms.Button();
            this.btnRemoveProcess = new System.Windows.Forms.Button();
            this.btnPickActive = new System.Windows.Forms.Button();
            this.pickTimer = new System.Windows.Forms.Timer(this.components);
            
            this.grpLayoutRules = new System.Windows.Forms.GroupBox();
            this.chkEnableLayoutRules = new System.Windows.Forms.CheckBox();
            this.lstLayoutRules = new System.Windows.Forms.ListBox();
            this.txtNewLayoutRuleProcess = new System.Windows.Forms.TextBox();
            this.cmbNewLayoutRuleLayout = new System.Windows.Forms.ComboBox();
            this.btnAddLayoutRule = new System.Windows.Forms.Button();
            this.btnRemoveLayoutRule = new System.Windows.Forms.Button();
            this.btnPickActiveLayoutRule = new System.Windows.Forms.Button();

            this.grpEnterCorrection = new System.Windows.Forms.GroupBox();
            this.chkSkipEnterCorrection = new System.Windows.Forms.CheckBox();
            this.txtSkipEnterProcesses = new System.Windows.Forms.TextBox();

            this.grpIgnoredWords = new System.Windows.Forms.GroupBox();
            this.lstIgnoredWords = new System.Windows.Forms.ListBox();
            this.txtNewIgnoredWord = new System.Windows.Forms.TextBox();
            this.btnAddIgnoredWord = new System.Windows.Forms.Button();
            this.btnRemoveIgnoredWord = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.numMinimumMappedPercent)).BeginInit();
            this.grpProcesses.SuspendLayout();
            this.grpIgnoredWords.SuspendLayout();
            this.grpLayoutRules.SuspendLayout();
            this.grpEnterCorrection.SuspendLayout();
            this.SuspendLayout();

            // chkEnableSwitching
            this.chkEnableSwitching.AutoSize = true;
            this.chkEnableSwitching.Checked = true;
            this.chkEnableSwitching.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableSwitching.Location = new System.Drawing.Point(12, 12);
            this.chkEnableSwitching.Name = "chkEnableSwitching";
            this.chkEnableSwitching.Size = new System.Drawing.Size(199, 19);
            this.chkEnableSwitching.TabIndex = 0;
            this.chkEnableSwitching.Text = "Автоматична заміна розкладки";

            // chkStartWithWindows
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(12, 37);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(176, 19);
            this.chkStartWithWindows.TabIndex = 1;
            this.chkStartWithWindows.Text = "Запускати разом з Windows";

            // chkRestoreApostrophes
            this.chkRestoreApostrophes.AutoSize = true;
            this.chkRestoreApostrophes.Checked = true;
            this.chkRestoreApostrophes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRestoreApostrophes.Location = new System.Drawing.Point(260, 12);
            this.chkRestoreApostrophes.Name = "chkRestoreApostrophes";
            this.chkRestoreApostrophes.Size = new System.Drawing.Size(160, 19);
            this.chkRestoreApostrophes.TabIndex = 2;
            this.chkRestoreApostrophes.Text = "Відновлювати апостроф";

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 67);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(130, 15);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Автозаміна: увімкнено";

            // btnExit
            this.btnExit.Location = new System.Drawing.Point(400, 601);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(120, 30);
            this.btnExit.TabIndex = 6;
            this.btnExit.Text = "Вихід";

            // contextMenuStrip
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemOpen,
            this.menuItemPause,
            this.toolStripSeparator1,
            this.menuItemExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(161, 76);

            // menuItemOpen
            this.menuItemOpen.Name = "menuItemOpen";
            this.menuItemOpen.Size = new System.Drawing.Size(160, 22);
            this.menuItemOpen.Text = "Налаштування";
            this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);

            // menuItemPause
            this.menuItemPause.Name = "menuItemPause";
            this.menuItemPause.Size = new System.Drawing.Size(160, 22);
            this.menuItemPause.Text = "Пауза";
            this.menuItemPause.Click += new System.EventHandler(this.menuItemPause_Click);

            // toolStripSeparator1
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);

            // menuItemExit
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(160, 22);
            this.menuItemExit.Text = "Вийти";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);

            // notifyIcon
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = "Перемикач розкладки";

            // Відсоток розпізнавання — єдине живе налаштування колишньої групи
            // "Чутливість алгоритму"; сама група була прихована, тож регулятор
            // лишався недосяжним, хоч і мав підказку. Решта її полів ні на що не впливали.
            this.lblMinimumMappedPercent.AutoSize = true;
            this.lblMinimumMappedPercent.Location = new System.Drawing.Point(285, 40);
            this.lblMinimumMappedPercent.Name = "lblMinimumMappedPercent";
            this.lblMinimumMappedPercent.Size = new System.Drawing.Size(135, 15);
            this.lblMinimumMappedPercent.TabIndex = 3;
            this.lblMinimumMappedPercent.Text = "Відсоток розпізнавання";

            // numMinimumMappedPercent
            this.numMinimumMappedPercent.Location = new System.Drawing.Point(450, 37);
            this.numMinimumMappedPercent.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numMinimumMappedPercent.Name = "numMinimumMappedPercent";
            this.numMinimumMappedPercent.Size = new System.Drawing.Size(60, 23);
            this.numMinimumMappedPercent.TabIndex = 4;
            this.numMinimumMappedPercent.Value = new decimal(new int[] { 80, 0, 0, 0 });

            // grpProcesses
            this.grpProcesses.Controls.Add(this.lblProcessMode);
            this.grpProcesses.Controls.Add(this.cmbProcessMode);
            this.grpProcesses.Controls.Add(this.lstProcesses);
            this.grpProcesses.Controls.Add(this.txtNewProcess);
            this.grpProcesses.Controls.Add(this.btnAddProcess);
            this.grpProcesses.Controls.Add(this.btnRemoveProcess);
            this.grpProcesses.Controls.Add(this.btnPickActive);
            this.grpProcesses.Location = new System.Drawing.Point(12, 169);
            this.grpProcesses.Name = "grpProcesses";
            this.grpProcesses.Size = new System.Drawing.Size(508, 140);
            this.grpProcesses.TabIndex = 4;
            this.grpProcesses.TabStop = false;
            this.grpProcesses.Text = "Фільтр програм";

            // lblProcessMode
            this.lblProcessMode.AutoSize = true;
            this.lblProcessMode.Location = new System.Drawing.Point(14, 25);
            this.lblProcessMode.Name = "lblProcessMode";
            this.lblProcessMode.Size = new System.Drawing.Size(91, 15);
            this.lblProcessMode.TabIndex = 0;
            this.lblProcessMode.Text = "Режим фільтра";

            // cmbProcessMode
            this.cmbProcessMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProcessMode.FormattingEnabled = true;
            this.cmbProcessMode.Location = new System.Drawing.Point(120, 22);
            this.cmbProcessMode.Name = "cmbProcessMode";
            this.cmbProcessMode.Size = new System.Drawing.Size(250, 23);
            this.cmbProcessMode.TabIndex = 1;

            // txtNewProcess
            this.txtNewProcess.Location = new System.Drawing.Point(17, 51);
            this.txtNewProcess.Name = "txtNewProcess";
            this.txtNewProcess.Size = new System.Drawing.Size(217, 23);
            this.txtNewProcess.TabIndex = 2;

            // btnAddProcess
            this.btnAddProcess.Location = new System.Drawing.Point(240, 50);
            this.btnAddProcess.Name = "btnAddProcess";
            this.btnAddProcess.Size = new System.Drawing.Size(80, 25);
            this.btnAddProcess.TabIndex = 3;
            this.btnAddProcess.Text = "Додати";

            // btnRemoveProcess
            this.btnRemoveProcess.Location = new System.Drawing.Point(326, 50);
            this.btnRemoveProcess.Name = "btnRemoveProcess";
            this.btnRemoveProcess.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveProcess.TabIndex = 4;
            this.btnRemoveProcess.Text = "Видалити";

            // btnPickActive
            this.btnPickActive.Location = new System.Drawing.Point(412, 50);
            this.btnPickActive.Name = "btnPickActive";
            this.btnPickActive.Size = new System.Drawing.Size(80, 25);
            this.btnPickActive.TabIndex = 6;
            this.btnPickActive.Text = "Активна";
            this.btnPickActive.Click += new System.EventHandler(this.btnPickActive_Click);

            // pickTimer
            this.pickTimer.Interval = 1000;
            this.pickTimer.Tick += new System.EventHandler(this.pickTimer_Tick);

            // lstProcesses
            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.ItemHeight = 15;
            this.lstProcesses.Location = new System.Drawing.Point(17, 80);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(474, 49);
            this.lstProcesses.TabIndex = 5;

            // grpLayoutRules
            this.grpLayoutRules.Controls.Add(this.chkEnableLayoutRules);
            this.grpLayoutRules.Controls.Add(this.txtNewLayoutRuleProcess);
            this.grpLayoutRules.Controls.Add(this.cmbNewLayoutRuleLayout);
            this.grpLayoutRules.Controls.Add(this.btnAddLayoutRule);
            this.grpLayoutRules.Controls.Add(this.btnRemoveLayoutRule);
            this.grpLayoutRules.Controls.Add(this.btnPickActiveLayoutRule);
            this.grpLayoutRules.Controls.Add(this.lstLayoutRules);
            this.grpLayoutRules.Location = new System.Drawing.Point(12, 315);
            this.grpLayoutRules.Name = "grpLayoutRules";
            this.grpLayoutRules.Size = new System.Drawing.Size(508, 140);
            this.grpLayoutRules.TabIndex = 7;
            this.grpLayoutRules.TabStop = false;
            this.grpLayoutRules.Text = "Розкладка для програм";

            // chkEnableLayoutRules
            this.chkEnableLayoutRules.AutoSize = true;
            this.chkEnableLayoutRules.Checked = true;
            this.chkEnableLayoutRules.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableLayoutRules.Location = new System.Drawing.Point(17, 22);
            this.chkEnableLayoutRules.Name = "chkEnableLayoutRules";
            this.chkEnableLayoutRules.Size = new System.Drawing.Size(300, 19);
            this.chkEnableLayoutRules.TabIndex = 0;
            this.chkEnableLayoutRules.Text = "Вмикати задану розкладку при переході у вікно";

            // txtNewLayoutRuleProcess
            this.txtNewLayoutRuleProcess.Location = new System.Drawing.Point(17, 51);
            this.txtNewLayoutRuleProcess.Name = "txtNewLayoutRuleProcess";
            this.txtNewLayoutRuleProcess.Size = new System.Drawing.Size(140, 23);
            this.txtNewLayoutRuleProcess.TabIndex = 1;

            // cmbNewLayoutRuleLayout
            this.cmbNewLayoutRuleLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNewLayoutRuleLayout.FormattingEnabled = true;
            this.cmbNewLayoutRuleLayout.Location = new System.Drawing.Point(163, 51);
            this.cmbNewLayoutRuleLayout.Name = "cmbNewLayoutRuleLayout";
            this.cmbNewLayoutRuleLayout.Size = new System.Drawing.Size(71, 23);
            this.cmbNewLayoutRuleLayout.TabIndex = 2;

            // btnAddLayoutRule
            this.btnAddLayoutRule.Location = new System.Drawing.Point(240, 50);
            this.btnAddLayoutRule.Name = "btnAddLayoutRule";
            this.btnAddLayoutRule.Size = new System.Drawing.Size(80, 25);
            this.btnAddLayoutRule.TabIndex = 3;
            this.btnAddLayoutRule.Text = "Додати";

            // btnRemoveLayoutRule
            this.btnRemoveLayoutRule.Location = new System.Drawing.Point(326, 50);
            this.btnRemoveLayoutRule.Name = "btnRemoveLayoutRule";
            this.btnRemoveLayoutRule.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveLayoutRule.TabIndex = 4;
            this.btnRemoveLayoutRule.Text = "Видалити";

            // btnPickActiveLayoutRule
            this.btnPickActiveLayoutRule.Location = new System.Drawing.Point(412, 50);
            this.btnPickActiveLayoutRule.Name = "btnPickActiveLayoutRule";
            this.btnPickActiveLayoutRule.Size = new System.Drawing.Size(80, 25);
            this.btnPickActiveLayoutRule.TabIndex = 6;
            this.btnPickActiveLayoutRule.Text = "Активна";

            // lstLayoutRules
            this.lstLayoutRules.FormattingEnabled = true;
            this.lstLayoutRules.ItemHeight = 15;
            this.lstLayoutRules.Location = new System.Drawing.Point(17, 82);
            this.lstLayoutRules.Name = "lstLayoutRules";
            this.lstLayoutRules.Size = new System.Drawing.Size(475, 49);
            this.lstLayoutRules.TabIndex = 5;

            // grpEnterCorrection
            this.grpEnterCorrection.Controls.Add(this.chkSkipEnterCorrection);
            this.grpEnterCorrection.Controls.Add(this.txtSkipEnterProcesses);
            this.grpEnterCorrection.Location = new System.Drawing.Point(12, 95);
            this.grpEnterCorrection.Name = "grpEnterCorrection";
            this.grpEnterCorrection.Size = new System.Drawing.Size(508, 68);
            this.grpEnterCorrection.TabIndex = 8;
            this.grpEnterCorrection.TabStop = false;
            this.grpEnterCorrection.Text = "Виправлення при Enter";

            // chkSkipEnterCorrection
            this.chkSkipEnterCorrection.AutoSize = true;
            this.chkSkipEnterCorrection.Checked = true;
            this.chkSkipEnterCorrection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSkipEnterCorrection.Location = new System.Drawing.Point(17, 20);
            this.chkSkipEnterCorrection.Name = "chkSkipEnterCorrection";
            this.chkSkipEnterCorrection.Size = new System.Drawing.Size(300, 19);
            this.chkSkipEnterCorrection.TabIndex = 0;
            this.chkSkipEnterCorrection.Text = "Не виправляти при Enter у цих програмах:";

            // txtSkipEnterProcesses
            this.txtSkipEnterProcesses.Location = new System.Drawing.Point(17, 41);
            this.txtSkipEnterProcesses.Name = "txtSkipEnterProcesses";
            this.txtSkipEnterProcesses.Size = new System.Drawing.Size(475, 23);
            this.txtSkipEnterProcesses.TabIndex = 1;

            // grpIgnoredWords
            this.grpIgnoredWords.Controls.Add(this.lstIgnoredWords);
            this.grpIgnoredWords.Controls.Add(this.txtNewIgnoredWord);
            this.grpIgnoredWords.Controls.Add(this.btnAddIgnoredWord);
            this.grpIgnoredWords.Controls.Add(this.btnRemoveIgnoredWord);
            this.grpIgnoredWords.Location = new System.Drawing.Point(12, 461);
            this.grpIgnoredWords.Name = "grpIgnoredWords";
            this.grpIgnoredWords.Size = new System.Drawing.Size(508, 130);
            this.grpIgnoredWords.TabIndex = 5;
            this.grpIgnoredWords.TabStop = false;
            this.grpIgnoredWords.Text = "Слова-винятки (не перекладати)";

            // txtNewIgnoredWord
            this.txtNewIgnoredWord.Location = new System.Drawing.Point(17, 25);
            this.txtNewIgnoredWord.Name = "txtNewIgnoredWord";
            this.txtNewIgnoredWord.Size = new System.Drawing.Size(217, 23);
            this.txtNewIgnoredWord.TabIndex = 0;

            // btnAddIgnoredWord
            this.btnAddIgnoredWord.Location = new System.Drawing.Point(240, 24);
            this.btnAddIgnoredWord.Name = "btnAddIgnoredWord";
            this.btnAddIgnoredWord.Size = new System.Drawing.Size(80, 25);
            this.btnAddIgnoredWord.TabIndex = 1;
            this.btnAddIgnoredWord.Text = "Додати";

            // btnRemoveIgnoredWord
            this.btnRemoveIgnoredWord.Location = new System.Drawing.Point(326, 24);
            this.btnRemoveIgnoredWord.Name = "btnRemoveIgnoredWord";
            this.btnRemoveIgnoredWord.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveIgnoredWord.TabIndex = 2;
            this.btnRemoveIgnoredWord.Text = "Видалити";

            // lstIgnoredWords
            this.lstIgnoredWords.FormattingEnabled = true;
            this.lstIgnoredWords.ItemHeight = 15;
            this.lstIgnoredWords.Location = new System.Drawing.Point(17, 55);
            this.lstIgnoredWords.Name = "lstIgnoredWords";
            this.lstIgnoredWords.Size = new System.Drawing.Size(474, 64);
            this.lstIgnoredWords.TabIndex = 3;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 646);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.grpIgnoredWords);
            this.Controls.Add(this.grpLayoutRules);
            this.Controls.Add(this.grpEnterCorrection);
            this.Controls.Add(this.grpProcesses);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chkRestoreApostrophes);
            this.Controls.Add(this.lblMinimumMappedPercent);
            this.Controls.Add(this.numMinimumMappedPercent);
            this.Controls.Add(this.chkStartWithWindows);
            this.Controls.Add(this.chkEnableSwitching);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Перемикач розкладки";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumMappedPercent)).EndInit();
            this.grpProcesses.ResumeLayout(false);
            this.grpProcesses.PerformLayout();
            this.grpIgnoredWords.ResumeLayout(false);
            this.grpLayoutRules.ResumeLayout(false);
            this.grpEnterCorrection.ResumeLayout(false);
            this.grpEnterCorrection.PerformLayout();
            this.grpLayoutRules.PerformLayout();
            this.grpIgnoredWords.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
