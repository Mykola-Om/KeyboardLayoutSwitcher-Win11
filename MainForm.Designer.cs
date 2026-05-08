using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private CheckBox chkEnableSwitching;
        private CheckBox chkStartWithWindows;
        private Button btnExit;
        private Label lblStatus;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem menuItemOpen;
        private ToolStripMenuItem menuItemPause;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemExit;
        private GroupBox grpHeuristic;
        private Label lblMinimumWordLength;
        private NumericUpDown numMinimumWordLength;
        private Label lblMinimumMappedPercent;
        private NumericUpDown numMinimumMappedPercent;
        private Label lblMinimumVowelDelta;
        private NumericUpDown numMinimumVowelDelta;
        
        private GroupBox grpProcesses;
        private Label lblProcessMode;
        private ComboBox cmbProcessMode;
        private ListBox lstProcesses;
        private TextBox txtNewProcess;
        private Button btnAddProcess;
        private Button btnRemoveProcess;
        private Button btnPickActive;
        private Timer pickTimer;
        
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
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.chkEnableSwitching = new System.Windows.Forms.CheckBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemPause = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            
            this.grpHeuristic = new System.Windows.Forms.GroupBox();
            this.lblMinimumWordLength = new System.Windows.Forms.Label();
            this.lblMinimumMappedPercent = new System.Windows.Forms.Label();
            this.lblMinimumVowelDelta = new System.Windows.Forms.Label();
            this.numMinimumWordLength = new System.Windows.Forms.NumericUpDown();
            this.numMinimumMappedPercent = new System.Windows.Forms.NumericUpDown();
            this.numMinimumVowelDelta = new System.Windows.Forms.NumericUpDown();
            
            this.grpProcesses = new System.Windows.Forms.GroupBox();
            this.lblProcessMode = new System.Windows.Forms.Label();
            this.cmbProcessMode = new System.Windows.Forms.ComboBox();
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.txtNewProcess = new System.Windows.Forms.TextBox();
            this.btnAddProcess = new System.Windows.Forms.Button();
            this.btnRemoveProcess = new System.Windows.Forms.Button();
            this.btnPickActive = new System.Windows.Forms.Button();
            this.pickTimer = new System.Windows.Forms.Timer(this.components);
            
            this.grpIgnoredWords = new System.Windows.Forms.GroupBox();
            this.lstIgnoredWords = new System.Windows.Forms.ListBox();
            this.txtNewIgnoredWord = new System.Windows.Forms.TextBox();
            this.btnAddIgnoredWord = new System.Windows.Forms.Button();
            this.btnRemoveIgnoredWord = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.numMinimumWordLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumMappedPercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumVowelDelta)).BeginInit();
            this.grpHeuristic.SuspendLayout();
            this.grpProcesses.SuspendLayout();
            this.grpIgnoredWords.SuspendLayout();
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

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 67);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(130, 15);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Автозаміна: увімкнено";

            // btnExit
            this.btnExit.Location = new System.Drawing.Point(400, 381);
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

            // grpHeuristic
            this.grpHeuristic.Controls.Add(this.lblMinimumWordLength);
            this.grpHeuristic.Controls.Add(this.numMinimumWordLength);
            this.grpHeuristic.Controls.Add(this.lblMinimumMappedPercent);
            this.grpHeuristic.Controls.Add(this.numMinimumMappedPercent);
            this.grpHeuristic.Controls.Add(this.lblMinimumVowelDelta);
            this.grpHeuristic.Controls.Add(this.numMinimumVowelDelta);
            this.grpHeuristic.Location = new System.Drawing.Point(12, 95);
            this.grpHeuristic.Name = "grpHeuristic";
            this.grpHeuristic.Visible = false;
            this.grpHeuristic.Size = new System.Drawing.Size(508, 108);
            this.grpHeuristic.TabIndex = 3;
            this.grpHeuristic.TabStop = false;
            this.grpHeuristic.Text = "Чутливість алгоритму";

            // lblMinimumWordLength
            this.lblMinimumWordLength.AutoSize = true;
            this.lblMinimumWordLength.Location = new System.Drawing.Point(14, 29);
            this.lblMinimumWordLength.Name = "lblMinimumWordLength";
            this.lblMinimumWordLength.Size = new System.Drawing.Size(120, 15);
            this.lblMinimumWordLength.TabIndex = 0;
            this.lblMinimumWordLength.Text = "Мін. довжина слова";

            // numMinimumWordLength
            this.numMinimumWordLength.Location = new System.Drawing.Point(200, 27);
            this.numMinimumWordLength.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            this.numMinimumWordLength.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMinimumWordLength.Name = "numMinimumWordLength";
            this.numMinimumWordLength.Size = new System.Drawing.Size(72, 23);
            this.numMinimumWordLength.TabIndex = 1;
            this.numMinimumWordLength.Value = new decimal(new int[] { 3, 0, 0, 0 });

            // lblMinimumMappedPercent
            this.lblMinimumMappedPercent.AutoSize = true;
            this.lblMinimumMappedPercent.Location = new System.Drawing.Point(14, 56);
            this.lblMinimumMappedPercent.Name = "lblMinimumMappedPercent";
            this.lblMinimumMappedPercent.Size = new System.Drawing.Size(135, 15);
            this.lblMinimumMappedPercent.TabIndex = 2;
            this.lblMinimumMappedPercent.Text = "Відсоток розпізнавання";

            // numMinimumMappedPercent
            this.numMinimumMappedPercent.Location = new System.Drawing.Point(200, 54);
            this.numMinimumMappedPercent.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numMinimumMappedPercent.Name = "numMinimumMappedPercent";
            this.numMinimumMappedPercent.Size = new System.Drawing.Size(72, 23);
            this.numMinimumMappedPercent.TabIndex = 3;
            this.numMinimumMappedPercent.Value = new decimal(new int[] { 80, 0, 0, 0 });

            // lblMinimumVowelDelta
            this.lblMinimumVowelDelta.AutoSize = true;
            this.lblMinimumVowelDelta.Location = new System.Drawing.Point(14, 83);
            this.lblMinimumVowelDelta.Name = "lblMinimumVowelDelta";
            this.lblMinimumVowelDelta.Size = new System.Drawing.Size(131, 15);
            this.lblMinimumVowelDelta.TabIndex = 4;
            this.lblMinimumVowelDelta.Text = "Точність за голосними";

            // numMinimumVowelDelta
            this.numMinimumVowelDelta.Location = new System.Drawing.Point(200, 81);
            this.numMinimumVowelDelta.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numMinimumVowelDelta.Name = "numMinimumVowelDelta";
            this.numMinimumVowelDelta.Size = new System.Drawing.Size(72, 23);
            this.numMinimumVowelDelta.TabIndex = 5;
            this.numMinimumVowelDelta.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // grpProcesses
            this.grpProcesses.Controls.Add(this.lblProcessMode);
            this.grpProcesses.Controls.Add(this.cmbProcessMode);
            this.grpProcesses.Controls.Add(this.lstProcesses);
            this.grpProcesses.Controls.Add(this.txtNewProcess);
            this.grpProcesses.Controls.Add(this.btnAddProcess);
            this.grpProcesses.Controls.Add(this.btnRemoveProcess);
            this.grpProcesses.Controls.Add(this.btnPickActive);
            this.grpProcesses.Location = new System.Drawing.Point(12, 95);
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

            // lstProcesses
            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.ItemHeight = 15;
            this.lstProcesses.Location = new System.Drawing.Point(17, 80);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(474, 49);
            this.lstProcesses.TabIndex = 5;

            // grpIgnoredWords
            this.grpIgnoredWords.Controls.Add(this.lstIgnoredWords);
            this.grpIgnoredWords.Controls.Add(this.txtNewIgnoredWord);
            this.grpIgnoredWords.Controls.Add(this.btnAddIgnoredWord);
            this.grpIgnoredWords.Controls.Add(this.btnRemoveIgnoredWord);
            this.grpIgnoredWords.Location = new System.Drawing.Point(12, 241);
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
            this.ClientSize = new System.Drawing.Size(534, 426);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.grpIgnoredWords);
            this.Controls.Add(this.grpProcesses);
            this.Controls.Add(this.grpHeuristic);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chkStartWithWindows);
            this.Controls.Add(this.chkEnableSwitching);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Перемикач розкладки";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumWordLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumMappedPercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumVowelDelta)).EndInit();
            this.grpHeuristic.ResumeLayout(false);
            this.grpHeuristic.PerformLayout();
            this.grpProcesses.ResumeLayout(false);
            this.grpProcesses.PerformLayout();
            this.grpIgnoredWords.ResumeLayout(false);
            this.grpIgnoredWords.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}


