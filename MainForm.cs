using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public partial class MainForm : Form
    {
        private KeyboardHook keyboardHook;
        private bool isSwitchingEnabled = false;

        public MainForm()
        {
            InitializeComponent();

            // Initialize the global keyboard hook.
            keyboardHook = new KeyboardHook();

            // Configure tray icon. Use a fallback icon when project resources are missing.
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // Wire events.
            chkEnableSwitching.CheckedChanged += ChkEnableSwitching_CheckedChanged;
            btnExit.Click += BtnExit_Click;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            this.FormClosing += MainForm_FormClosing;

            // Apply initial state from the checkbox default value.
            ChkEnableSwitching_CheckedChanged(chkEnableSwitching, EventArgs.Empty);
        }

        private void ChkEnableSwitching_CheckedChanged(object sender, EventArgs e)
        {
            isSwitchingEnabled = chkEnableSwitching.Checked;

            if (isSwitchingEnabled)
            {
                keyboardHook.Start();
                lblStatus.Text = "Автоматичне перемикання ввімкнено";
            }
            else
            {
                keyboardHook.Stop();
                lblStatus.Text = "Автоматичне перемикання вимкнено";
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            keyboardHook.Stop();
            Application.Exit();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        // NotifyIcon context menu actions.
        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            keyboardHook.Stop();
            Application.Exit();
        }
    }
}
