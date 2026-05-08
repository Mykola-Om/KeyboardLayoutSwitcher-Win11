using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;

namespace KeyboardLayoutSwitcher
{
    public partial class MainForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private readonly AppSettings settings;
        private KeyboardHook keyboardHook;
        private bool isInitializing;
        private bool isExiting;
        private int countdownValue;
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

            // Configure tray icon. Extract from the compiled exe so we don't need dedicated external files.
            Icon exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Icon = exeIcon;
            notifyIcon.Icon = exeIcon;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            cmbProcessMode.Items.AddRange(new object[] { "Вимкнено", "Працювати тільки у вибраних", "Працювати всюди, крім вибраних" });

            // Wire events.
            chkEnableSwitching.CheckedChanged += ChkEnableSwitching_CheckedChanged;
            chkStartWithWindows.CheckedChanged += SettingsControlChanged;
            cmbProcessMode.SelectedIndexChanged += SettingsControlChanged;
            
            
            numMinimumMappedPercent.ValueChanged += SettingsControlChanged;
            
            // Завдяки гібридному словнику та новій системі штрафів налаштування чутливості 
            // більше не потрібні користувачу, вони надійно працюють під капотом зі стандартними значеннями (80%).
            grpHeuristic.Visible = false;
            
            btnExit.Click += BtnExit_Click;
            btnAddProcess.Click += BtnAddProcess_Click;
            btnRemoveProcess.Click += BtnRemoveProcess_Click;
            btnAddIgnoredWord.Click += BtnAddIgnoredWord_Click;
            btnRemoveIgnoredWord.Click += BtnRemoveIgnoredWord_Click;
            txtNewProcess.KeyDown += TxtNewProcess_KeyDown;
            txtNewIgnoredWord.KeyDown += TxtNewIgnoredWord_KeyDown;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            this.FormClosing += MainForm_FormClosing;

            ApplyNativeDarkMode();
            SetupTooltips();

            LoadSettingsIntoControls();
            ApplyRuntimeSettings();
        }

        private void SetupTooltips()
        {
            var tip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 500, ReshowDelay = 100, ShowAlways = true };

            AddInfoIcon(lblMinimumWordLength, numMinimumWordLength, tip, "Мінімальна кількість літер у слові, з якої починається перевірка.\r\nКоротші слова або окремі літери алгоритм проігнорує.");
            AddInfoIcon(lblMinimumMappedPercent, numMinimumMappedPercent, tip, "Який відсоток літер у слові має 'співпадати' з іншою розкладкою,\r\nщоб програма вирішила змінити мову. (напр. 80% - це майже все слово)");
            AddInfoIcon(lblMinimumVowelDelta, numMinimumVowelDelta, tip, "Різниця голосних. Алгоритм очікує, що у 'правильному' слові\r\nбуде нормальна кількість голосних, а у 'неправильному' (абракадабрі) - замало або забагато.");
        }

        private void AddInfoIcon(Control label, Control input, ToolTip tip, string text)
        {
            var icon = new Label
            {
                Text = "ⓘ", 
                Font = new Font("Segoe UI Symbol", 11F, FontStyle.Regular),
                AutoSize = true,
                Cursor = Cursors.Help,
                ForeColor = Color.FromArgb(120, 180, 255),
                BackColor = Color.Transparent
            };
            
            icon.Location = new Point(input.Left - 24, label.Top - 3);
            label.Parent.Controls.Add(icon);
            icon.BringToFront();

            tip.SetToolTip(icon, text);
            tip.SetToolTip(label, text);
            
            icon.MouseEnter += (s, e) => icon.ForeColor = Color.White;
            icon.MouseLeave += (s, e) => icon.ForeColor = Color.FromArgb(120, 180, 255);
        }

        private void ApplyNativeDarkMode()
        {
            try
            {
                int useImmersiveDarkMode = 1;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            }
            catch { }

            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5F);

            // Important: Handle controls as soon as handle is created so theme applies correctly
            this.Load += (s, e) =>
            {
                foreach (Control ctrl in this.Controls)
                {
                    StyleControlNativeDark(ctrl);
                }
            };
            
            if (contextMenuStrip != null)
            {
                contextMenuStrip.BackColor = Color.FromArgb(43, 43, 43);
                contextMenuStrip.ForeColor = Color.White;
                contextMenuStrip.ShowImageMargin = false;
                contextMenuStrip.RenderMode = ToolStripRenderMode.System;
            }
        }

        private void StyleControlNativeDark(Control control)
        {
            // Windows 11 Dark Mode styling via uxtheme
            if (control is ComboBox || control is TextBox || control is NumericUpDown || control.GetType().Name == "TextBox")
            {
                SetWindowTheme(control.Handle, "DarkMode_CFD", null);
                control.BackColor = Color.FromArgb(43, 43, 43);
                control.ForeColor = Color.White;
            }
            else if (control is CheckBox || control is GroupBox)
            {
                SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            }
            else if (control is Button btn)
            {
                // Native native buttons in WinForms don't take pure DarkMode well,
                // so FlatStyle with subtle Win11 button colors works best
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = Color.FromArgb(60, 60, 60);
                btn.ForeColor = Color.White;
            }

            foreach (Control child in control.Controls)
            {
                StyleControlNativeDark(child);
            }
        }

        private void ChkEnableSwitching_CheckedChanged(object sender, EventArgs e)
        {
            if (menuItemPause != null)
            {
                menuItemPause.Checked = !chkEnableSwitching.Checked;
                menuItemPause.Text = chkEnableSwitching.Checked ? "Пауза" : "Продовжити";
            }

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

                private void TxtNewProcess_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnAddProcess_Click(sender, e); } }
        private void TxtNewIgnoredWord_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnAddIgnoredWord_Click(sender, e); } }

        private void BtnAddProcess_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewProcess.Text) && !lstProcesses.Items.Contains(txtNewProcess.Text.Trim())) {
                lstProcesses.Items.Add(txtNewProcess.Text.Trim());
                txtNewProcess.Clear();
                SettingsControlChanged(null, EventArgs.Empty);
            }
        }

        private void BtnRemoveProcess_Click(object sender, EventArgs e)
        {
            if (lstProcesses.SelectedIndex >= 0) {
                lstProcesses.Items.RemoveAt(lstProcesses.SelectedIndex);
                SettingsControlChanged(null, EventArgs.Empty);
            }
        }

        private void btnPickActive_Click(object sender, EventArgs e)
        {
            countdownValue = 3;
            btnPickActive.Enabled = false;
            btnPickActive.Text = $"{countdownValue}с...";
            pickTimer.Start();
        }

        private void pickTimer_Tick(object sender, EventArgs e)
        {
            countdownValue--;
            if (countdownValue > 0)
            {
                btnPickActive.Text = $"{countdownValue}с...";
            }
            else
            {
                pickTimer.Stop();
                btnPickActive.Enabled = true;
                btnPickActive.Text = "Активна";

                IntPtr foregroundWindow = GetForegroundWindow();
                // Ensure we don't pick our own window
                if (foregroundWindow != IntPtr.Zero && foregroundWindow != this.Handle)
                {
                    string processName = GetProcessName(foregroundWindow);
                    string normalized = AppSettings.NormalizeProcessName(processName);
                    
                    if (!string.IsNullOrEmpty(normalized) && !lstProcesses.Items.Contains(normalized))
                    {
                        lstProcesses.Items.Add(normalized);
                        SettingsControlChanged(null, EventArgs.Empty);
                    }
                }
                
                // Bring back focus after countdown
                this.Activate();
            }
        }

        private string GetProcessName(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return string.Empty;
            
            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);
            if (processId == 0) return string.Empty;

            try
            {
                using (Process proc = Process.GetProcessById((int)processId))
                {
                    return proc.ProcessName;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private void BtnAddIgnoredWord_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewIgnoredWord.Text) && !lstIgnoredWords.Items.Contains(txtNewIgnoredWord.Text.Trim())) {
                lstIgnoredWords.Items.Add(txtNewIgnoredWord.Text.Trim());
                txtNewIgnoredWord.Clear();
                SettingsControlChanged(null, EventArgs.Empty);
            }
        }

        private void BtnRemoveIgnoredWord_Click(object sender, EventArgs e)
        {
            if (lstIgnoredWords.SelectedIndex >= 0) {
                lstIgnoredWords.Items.RemoveAt(lstIgnoredWords.SelectedIndex);
                SettingsControlChanged(null, EventArgs.Empty);
            }
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

        private void menuItemPause_Click(object sender, EventArgs e)
        {
            chkEnableSwitching.Checked = !chkEnableSwitching.Checked;
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
            cmbProcessMode.SelectedIndex = (int)settings.ProcessFilterMode;
                        lstProcesses.Items.Clear();
            if (!string.IsNullOrWhiteSpace(settings.ProcessFilterText)) {
                foreach (var p in settings.ProcessFilterText.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)) lstProcesses.Items.Add(p.Trim());
            }
            lstIgnoredWords.Items.Clear();
            if (!string.IsNullOrWhiteSpace(settings.IgnoredWordsText)) {
                foreach (var w in settings.IgnoredWordsText.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)) lstIgnoredWords.Items.Add(w.Trim());
            }
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
            if (cmbProcessMode.SelectedIndex >= 0) settings.ProcessFilterMode = (ProcessFilterMode)cmbProcessMode.SelectedIndex;
                        settings.ProcessFilterText = string.Join(Environment.NewLine, lstProcesses.Items.Cast<string>());
            settings.IgnoredWordsText = string.Join(Environment.NewLine, lstIgnoredWords.Items.Cast<string>());
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
                lblStatus.Text = "Автозаміна: УВІМКНЕНО";
            }
            else
            {
                keyboardHook.Stop();
                lblStatus.Text = "Автозаміна: ВИМКНЕНО";
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
