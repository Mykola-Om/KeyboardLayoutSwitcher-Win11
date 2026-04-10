using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public partial class MainForm : Form
    {
        private readonly AppSettings settings;
        private KeyboardHook keyboardHook;
        private bool isInitializing;
        private bool isExiting;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0050) // WM_INPUTLANGCHANGE
            {
                System.IO.File.AppendAllText("C:\\Users\\ommiv\\AppData\\Local\\KeyboardLayoutSwitcher\\trace.log", "WM_INPUTLANGCHANGE: " + m.LParam.ToString("X") + "\r\n");
            }
            base.WndProc(ref m);
        }

        public MainForm()
        {
            settings = AppSettingsStore.Load();
            InitializeComponent();

            // Initialize the global keyboard hook.
            keyboardHook = new KeyboardHook(settings);

            // Configure tray icon. Use a fallback icon when project resources are missing.
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            cmbProcessMode.DataSource = Enum.GetValues(typeof(ProcessFilterMode));

            // Wire events.
            chkEnableSwitching.CheckedChanged += ChkEnableSwitching_CheckedChanged;
            chkStartWithWindows.CheckedChanged += SettingsControlChanged;
            cmbProcessMode.SelectedIndexChanged += SettingsControlChanged;
            txtProcesses.TextChanged += SettingsControlChanged;
            numMinimumWordLength.ValueChanged += SettingsControlChanged;
            numMinimumMappedPercent.ValueChanged += SettingsControlChanged;
            numMinimumVowelDelta.ValueChanged += SettingsControlChanged;
            btnExit.Click += BtnExit_Click;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            this.FormClosing += MainForm_FormClosing;

            LoadSettingsIntoControls();
            ApplyRuntimeSettings();
        }

        private void ChkEnableSwitching_CheckedChanged(object sender, EventArgs e)
        {
            if (!isInitializing)
            {
                ApplyControlsToSettings();
                SaveSettings();
            }

            ApplyRuntimeSettings();
        }

        private void SettingsControlChanged(object sender, EventArgs e)
        {
            if (isInitializing)
            {
                return;
            }

            ApplyControlsToSettings();
            SaveSettings();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isExiting)
            {
                return;
            }

            e.Cancel = true;
            Hide();
        }

        // NotifyIcon context menu actions.
        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void LoadSettingsIntoControls()
        {
            isInitializing = true;

            chkEnableSwitching.Checked = settings.IsSwitchingEnabled;
            chkStartWithWindows.Checked = settings.StartWithWindows || StartupManager.IsEnabled();
            cmbProcessMode.SelectedItem = settings.ProcessFilterMode;
            txtProcesses.Text = settings.ProcessFilterText;
            numMinimumWordLength.Value = Clamp(numMinimumWordLength, settings.MinimumWordLength);
            numMinimumMappedPercent.Value = Clamp(numMinimumMappedPercent, settings.MinimumMappedPercent);
            numMinimumVowelDelta.Value = Clamp(numMinimumVowelDelta, settings.MinimumVowelDelta);

            settings.StartWithWindows = chkStartWithWindows.Checked;
            isInitializing = false;
        }

        private void ApplyControlsToSettings()
        {
            settings.IsSwitchingEnabled = chkEnableSwitching.Checked;
            settings.StartWithWindows = chkStartWithWindows.Checked;
            settings.ProcessFilterMode = (ProcessFilterMode)cmbProcessMode.SelectedItem;
            settings.ProcessFilterText = txtProcesses.Text;
            settings.MinimumWordLength = (int)numMinimumWordLength.Value;
            settings.MinimumMappedPercent = (int)numMinimumMappedPercent.Value;
            settings.MinimumVowelDelta = (int)numMinimumVowelDelta.Value;

            ApplyRuntimeSettings();
        }

        private void ApplyRuntimeSettings()
        {
            if (settings.IsSwitchingEnabled)
            {
                keyboardHook.Start();
                lblStatus.Text = "Auto replace is enabled";
            }
            else
            {
                keyboardHook.Stop();
                lblStatus.Text = "Auto replace is disabled";
            }

            StartupManager.SetStartWithWindows(settings.StartWithWindows);
        }

        private void SaveSettings()
        {
            AppSettingsStore.Save(settings);
        }

        private void ExitApplication()
        {
            isExiting = true;
            notifyIcon.Visible = false;
            keyboardHook.Stop();
            SaveSettings();
            Close();
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private static decimal Clamp(NumericUpDown control, int value)
        {
            return Math.Min(control.Maximum, Math.Max(control.Minimum, value));
        }
    }
}



