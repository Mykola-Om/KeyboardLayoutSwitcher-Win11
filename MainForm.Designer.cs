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
        private TextBox txtProcesses;
        private Label lblProcessHint;

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
            components = new System.ComponentModel.Container();
            chkEnableSwitching = new CheckBox();
            chkStartWithWindows = new CheckBox();
            btnExit = new Button();
            lblStatus = new Label();
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip();
            menuItemOpen = new ToolStripMenuItem();
            menuItemExit = new ToolStripMenuItem();
            grpHeuristic = new GroupBox();
            lblMinimumWordLength = new Label();
            numMinimumWordLength = new NumericUpDown();
            lblMinimumMappedPercent = new Label();
            numMinimumMappedPercent = new NumericUpDown();
            lblMinimumVowelDelta = new Label();
            numMinimumVowelDelta = new NumericUpDown();
            grpProcesses = new GroupBox();
            lblProcessMode = new Label();
            cmbProcessMode = new ComboBox();
            txtProcesses = new TextBox();
            lblProcessHint = new Label();
            grpHeuristic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMinimumWordLength).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumMappedPercent).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumVowelDelta).BeginInit();
            grpProcesses.SuspendLayout();
            SuspendLayout();

            // chkEnableSwitching
            chkEnableSwitching.AutoSize = true;
            chkEnableSwitching.Location = new System.Drawing.Point(12, 12);
            chkEnableSwitching.Text = "Enable automatic replacement";
            chkEnableSwitching.Checked = true;

            // chkStartWithWindows
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new System.Drawing.Point(12, 37);
            chkStartWithWindows.Text = "Start with Windows";

            // btnExit
            btnExit.Location = new System.Drawing.Point(400, 374);
            btnExit.Size = new System.Drawing.Size(120, 30);
            btnExit.Text = "Exit";

            // lblStatus
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(12, 67);
            lblStatus.Text = "Auto replace is enabled";

            // notifyIcon
            notifyIcon.Text = "Keyboard Layout Switcher";

            // contextMenuStrip
            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                menuItemOpen,
                menuItemExit
            });

            // menuItemOpen
            menuItemOpen.Text = "Open";
            menuItemOpen.Click += menuItemOpen_Click;

            // menuItemExit
            menuItemExit.Text = "Exit";
            menuItemExit.Click += menuItemExit_Click;

            // grpHeuristic
            grpHeuristic.Controls.Add(lblMinimumWordLength);
            grpHeuristic.Controls.Add(numMinimumWordLength);
            grpHeuristic.Controls.Add(lblMinimumMappedPercent);
            grpHeuristic.Controls.Add(numMinimumMappedPercent);
            grpHeuristic.Controls.Add(lblMinimumVowelDelta);
            grpHeuristic.Controls.Add(numMinimumVowelDelta);
            grpHeuristic.Location = new System.Drawing.Point(12, 95);
            grpHeuristic.Size = new System.Drawing.Size(508, 108);
            grpHeuristic.Text = "Heuristic sensitivity";

            // lblMinimumWordLength
            lblMinimumWordLength.AutoSize = true;
            lblMinimumWordLength.Location = new System.Drawing.Point(14, 29);
            lblMinimumWordLength.Text = "Minimum word length";

            // numMinimumWordLength
            numMinimumWordLength.Location = new System.Drawing.Point(200, 27);
            numMinimumWordLength.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            numMinimumWordLength.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMinimumWordLength.Size = new System.Drawing.Size(72, 23);
            numMinimumWordLength.Value = new decimal(new int[] { 3, 0, 0, 0 });

            // lblMinimumMappedPercent
            lblMinimumMappedPercent.AutoSize = true;
            lblMinimumMappedPercent.Location = new System.Drawing.Point(14, 56);
            lblMinimumMappedPercent.Text = "Minimum mapped percent";

            // numMinimumMappedPercent
            numMinimumMappedPercent.Location = new System.Drawing.Point(200, 54);
            numMinimumMappedPercent.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numMinimumMappedPercent.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numMinimumMappedPercent.Size = new System.Drawing.Size(72, 23);
            numMinimumMappedPercent.Value = new decimal(new int[] { 80, 0, 0, 0 });

            // lblMinimumVowelDelta
            lblMinimumVowelDelta.AutoSize = true;
            lblMinimumVowelDelta.Location = new System.Drawing.Point(14, 83);
            lblMinimumVowelDelta.Text = "Minimum vowel gain";

            // numMinimumVowelDelta
            numMinimumVowelDelta.Location = new System.Drawing.Point(200, 81);
            numMinimumVowelDelta.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numMinimumVowelDelta.Size = new System.Drawing.Size(72, 23);
            numMinimumVowelDelta.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // grpProcesses
            grpProcesses.Controls.Add(lblProcessMode);
            grpProcesses.Controls.Add(cmbProcessMode);
            grpProcesses.Controls.Add(txtProcesses);
            grpProcesses.Controls.Add(lblProcessHint);
            grpProcesses.Location = new System.Drawing.Point(12, 214);
            grpProcesses.Size = new System.Drawing.Size(508, 154);
            grpProcesses.Text = "Process filter";

            // lblProcessMode
            lblProcessMode.AutoSize = true;
            lblProcessMode.Location = new System.Drawing.Point(14, 29);
            lblProcessMode.Text = "Filter mode";

            // cmbProcessMode
            cmbProcessMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProcessMode.Location = new System.Drawing.Point(101, 26);
            cmbProcessMode.Size = new System.Drawing.Size(171, 23);

            // txtProcesses
            txtProcesses.Location = new System.Drawing.Point(17, 60);
            txtProcesses.Multiline = true;
            txtProcesses.ScrollBars = ScrollBars.Vertical;
            txtProcesses.Size = new System.Drawing.Size(474, 64);

            // lblProcessHint
            lblProcessHint.AutoSize = true;
            lblProcessHint.Location = new System.Drawing.Point(14, 129);
            lblProcessHint.Text = "Enter one process name per line, for example: code or notepad";

            // MainForm
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(534, 416);
            this.Controls.Add(chkEnableSwitching);
            this.Controls.Add(chkStartWithWindows);
            this.Controls.Add(lblStatus);
            this.Controls.Add(grpHeuristic);
            this.Controls.Add(grpProcesses);
            this.Controls.Add(btnExit);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Keyboard Layout Switcher";
            this.FormClosing += MainForm_FormClosing;
            grpHeuristic.ResumeLayout(false);
            grpHeuristic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMinimumWordLength).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumMappedPercent).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumVowelDelta).EndInit();
            grpProcesses.ResumeLayout(false);
            grpProcesses.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
