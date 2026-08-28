using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;

namespace KeyboardLayoutSwitcher
{
    public partial class MainForm : Form
    {
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        // "Обрати активне вікно" чекає перед фіксацією процесу, щоб користувач встиг
        // перемкнутись на потрібне вікно. pickTimer.Interval у Designer.cs виставлений
        // на 1000 мс, тож це значення саме в секундах — за узгодженням з тим таймером.
        private const int PickActiveWindowCountdownSeconds = 3;

        private readonly AppSettings settings;
        private KeyboardHook keyboardHook;
        private LayoutRuleEnforcer layoutRuleEnforcer;
        private bool isInitializing;
        private bool isExiting;
        private int countdownValue;

        // Яка кнопка зараз веде зворотний відлік і що зробити з обраним вікном.
        private Button pickButton;
        private Action<IntPtr, string> pickCompleted;

        public static MainForm Instance { get; private set; }

        public MainForm()
        {
            Instance = this;
            settings = AppSettingsStore.Load();
            InitializeComponent();

            // Initialize the global keyboard hook.
            keyboardHook = new KeyboardHook(settings);
            layoutRuleEnforcer = new LayoutRuleEnforcer(settings);

            // Configure tray icon. Extract from the compiled exe so we don't need dedicated external files.
            Icon exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Icon = exeIcon;
            notifyIcon.Icon = exeIcon;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            cmbProcessMode.Items.AddRange(new object[] { "Вимкнено", "Працювати тільки у вибраних", "Працювати всюди, крім вибраних" });
            cmbNewLayoutRuleLayout.Items.AddRange(new object[] { "Укр", "Англ" });
            cmbNewLayoutRuleLayout.SelectedIndex = 0;

            // Wire events.
            chkEnableSwitching.CheckedChanged += ChkEnableSwitching_CheckedChanged;
            chkStartWithWindows.CheckedChanged += SettingsControlChanged;
            chkRestoreApostrophes.CheckedChanged += SettingsControlChanged;
            cmbProcessMode.SelectedIndexChanged += SettingsControlChanged;
            chkEnableLayoutRules.CheckedChanged += SettingsControlChanged;
            chkSkipEnterCorrection.CheckedChanged += SettingsControlChanged;
            chkFullScreenNotification.CheckedChanged += SettingsControlChanged;
            chkNotifyOnAllScreens.CheckedChanged += SettingsControlChanged;
            txtSkipEnterProcesses.Leave += SettingsControlChanged;
            
            btnExit.Click += BtnExit_Click;
            btnAddProcess.Click += BtnAddProcess_Click;
            btnRemoveProcess.Click += BtnRemoveProcess_Click;
            btnAddIgnoredWord.Click += BtnAddIgnoredWord_Click;
            btnRemoveIgnoredWord.Click += BtnRemoveIgnoredWord_Click;
            btnAddLayoutRule.Click += BtnAddLayoutRule_Click;
            btnRemoveLayoutRule.Click += BtnRemoveLayoutRule_Click;
            txtNewLayoutRuleProcess.KeyDown += TxtNewLayoutRuleProcess_KeyDown;
            btnPickActiveLayoutRule.Click += BtnPickActiveLayoutRule_Click;
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

            AddInfoIcon(lblMinimumMappedPercent, numMinimumMappedPercent, tip, "Який відсоток літер у слові має 'співпадати' з іншою розкладкою,\r\nщоб програма вирішила змінити мову. (напр. 80% - це майже все слово)");

            tip.SetToolTip(chkRestoreApostrophes, "Додає пропущений апостроф в українських словах:\r\nпамять → пам'ять, компютер → комп'ютер, сімя → сім'я.\r\nПрацює за словником, тож слова без апострофа (свято, цвях) не чіпає.");
            tip.SetToolTip(chkSkipEnterCorrection, "У адресному рядку браузера Enter підтверджує підказку автодоповнення.\r\nЯкщо втрутитись саме там, браузер піде шукати замість переходу на сайт.\r\nПеречисли процеси через кому. В інших програмах Enter працює як раніше.");
            tip.SetToolTip(chkFullScreenNotification, "Замість плашки в кутку показує великий напис по центру екрана.\r\nНапис напівпрозорий і не перехоплює ні клавіатуру, ні мишу.");
            tip.SetToolTip(chkNotifyOnAllScreens, "Показувати сповіщення одразу на всіх моніторах,\r\nа не лише на тому, де зараз активне вікно.");
            tip.SetToolTip(chkEnableLayoutRules, "Коли ти переходиш у вікно програми зі списку,\r\nрозкладка автоматично стає заданою.\r\nЯкщо перемкнеш її вручну — це вікно більше не чіпається.");
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
                Win32Interop.DwmSetWindowAttribute(this.Handle, Win32Interop.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
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
            else if (control is GroupBox)
            {
                // Тему тут не застосовуємо: з DarkMode_Explorer заголовок рамки малює
                // система власним темним кольором, ігноруючи ForeColor, і він зливається
                // з фоном. Без теми GroupBox малює підпис сам — уже нашим кольором.
                control.ForeColor = Color.White;
            }
            else if (control is CheckBox)
            {
                SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                control.ForeColor = Color.White;
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

        private void TxtNewProcess_KeyDown(object sender, KeyEventArgs e) { ForwardEnterToClick(e, BtnAddProcess_Click); }
        private void TxtNewIgnoredWord_KeyDown(object sender, KeyEventArgs e) { ForwardEnterToClick(e, BtnAddIgnoredWord_Click); }

        private static void ForwardEnterToClick(KeyEventArgs e, EventHandler clickHandler)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                clickHandler(null, EventArgs.Empty);
            }
        }

        private void BtnAddProcess_Click(object sender, EventArgs e)
        {
            AddTagIfNew(lstProcesses, txtNewProcess);
        }

        private void BtnRemoveProcess_Click(object sender, EventArgs e)
        {
            RemoveSelectedTag(lstProcesses);
        }

        // Спільна логіка для обох "тег-листів" форми (процеси та слова-винятки):
        // додати непорожній, ще не присутній текст і повідомити про зміну налаштувань.
        private void AddTagIfNew(ListBox list, TextBox input)
        {
            string trimmed = input.Text.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || list.Items.Contains(trimmed))
            {
                return;
            }

            list.Items.Add(trimmed);
            input.Clear();
            SettingsControlChanged(null, EventArgs.Empty);
        }

        private void RemoveSelectedTag(ListBox list)
        {
            if (list.SelectedIndex < 0)
            {
                return;
            }

            list.Items.RemoveAt(list.SelectedIndex);
            SettingsControlChanged(null, EventArgs.Empty);
        }

        private void BtnAddLayoutRule_Click(object sender, EventArgs e)
        {
            string processName = AppSettings.NormalizeProcessName(txtNewLayoutRuleProcess.Text);
            if (string.IsNullOrWhiteSpace(processName) || cmbNewLayoutRuleLayout.SelectedIndex < 0)
            {
                return;
            }

            string layoutTag = cmbNewLayoutRuleLayout.SelectedIndex == 0
                ? AppSettings.UkrainianLayoutTag
                : AppSettings.EnglishLayoutTag;

            // Одна програма — одне правило: нове значення замінює попереднє.
            RemoveLayoutRuleFor(processName);

            lstLayoutRules.Items.Add(processName + AppSettings.LayoutRuleSeparator + layoutTag);
            txtNewLayoutRuleProcess.Clear();
            SettingsControlChanged(null, EventArgs.Empty);
        }

        private void BtnRemoveLayoutRule_Click(object sender, EventArgs e)
        {
            RemoveSelectedTag(lstLayoutRules);
        }

        private void RemoveLayoutRuleFor(string processName)
        {
            string prefix = processName + AppSettings.LayoutRuleSeparator;

            for (int index = lstLayoutRules.Items.Count - 1; index >= 0; index--)
            {
                if (lstLayoutRules.Items[index].ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    lstLayoutRules.Items.RemoveAt(index);
                }
            }
        }

        private void TxtNewLayoutRuleProcess_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnAddLayoutRule_Click(null, EventArgs.Empty);
            }
        }

        private void btnPickActive_Click(object sender, EventArgs e)
        {
            StartPickingActiveWindow(btnPickActive, (window, processName) =>
            {
                if (!lstProcesses.Items.Contains(processName))
                {
                    lstProcesses.Items.Add(processName);
                    SettingsControlChanged(null, EventArgs.Empty);
                }
            });
        }

        private void BtnPickActiveLayoutRule_Click(object sender, EventArgs e)
        {
            StartPickingActiveWindow(btnPickActiveLayoutRule, (window, processName) =>
            {
                // Підставляємо назву й поточну розкладку того вікна, але правило не додаємо —
                // рішення лишається за користувачем, він ще може змінити мову перед "Додати".
                txtNewLayoutRuleProcess.Text = processName;
                cmbNewLayoutRuleLayout.SelectedIndex = LayoutSwitcher.IsLayoutEnglishForWindow(window) ? 1 : 0;
            });
        }

        // Обидві групи ("Фільтр програм" і "Розкладка для програм") користуються одним
        // таймером зворотного відліку — під час нього користувач встигає перемкнутись на
        // потрібне вікно, тож активним є вже не наше.
        private void StartPickingActiveWindow(Button button, Action<IntPtr, string> onPicked)
        {
            pickButton = button;
            pickCompleted = onPicked;
            countdownValue = PickActiveWindowCountdownSeconds;
            button.Enabled = false;
            button.Text = $"{countdownValue}с...";
            pickTimer.Start();
        }

        private void pickTimer_Tick(object sender, EventArgs e)
        {
            countdownValue--;
            if (countdownValue > 0)
            {
                pickButton.Text = $"{countdownValue}с...";
            }
            else
            {
                pickTimer.Stop();
                pickButton.Enabled = true;
                pickButton.Text = "Активна";

                IntPtr foregroundWindow = Win32Interop.GetForegroundWindow();
                // Ensure we don't pick our own window
                if (foregroundWindow != IntPtr.Zero && foregroundWindow != this.Handle)
                {
                    string processName = ProcessNameResolver.GetProcessName(foregroundWindow);
                    string normalized = AppSettings.NormalizeProcessName(processName);

                    if (!string.IsNullOrEmpty(normalized))
                    {
                        pickCompleted?.Invoke(foregroundWindow, normalized);
                    }
                }

                pickCompleted = null;

                // Bring back focus after countdown
                this.Activate();
            }
        }

        private void BtnAddIgnoredWord_Click(object sender, EventArgs e)
        {
            AddTagIfNew(lstIgnoredWords, txtNewIgnoredWord);
        }

        private void BtnRemoveIgnoredWord_Click(object sender, EventArgs e)
        {
            RemoveSelectedTag(lstIgnoredWords);
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
            chkRestoreApostrophes.Checked = settings.RestoreApostrophes;
            chkEnableLayoutRules.Checked = settings.EnableLayoutRules;
            PopulateListFromText(lstLayoutRules, settings.LayoutRulesText);
            chkSkipEnterCorrection.Checked = settings.SkipEnterCorrection;
            chkFullScreenNotification.Checked = settings.FullScreenNotification;
            chkNotifyOnAllScreens.Checked = settings.NotifyOnAllScreens;
            txtSkipEnterProcesses.Text = settings.SkipEnterCorrectionProcessesText;
            cmbProcessMode.SelectedIndex = (int)settings.ProcessFilterMode;
            PopulateListFromText(lstProcesses, settings.ProcessFilterText);
            PopulateListFromText(lstIgnoredWords, settings.IgnoredWordsText);
            numMinimumMappedPercent.Value = Clamp(numMinimumMappedPercent, settings.MinimumMappedPercent);

            settings.StartWithWindows = chkStartWithWindows.Checked;
            isInitializing = false;
        }

        // Наповнює ListBox рядками з тексту, розбитого за AppSettings.ListDelimiters —
        // тим самим набором, яким AppSettings.ProcessNames/IgnoredWords парсять цей текст,
        // щоб UI і збережені налаштування завжди розуміли роздільники однаково.
        private static void PopulateListFromText(ListBox list, string text)
        {
            list.Items.Clear();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            foreach (string item in text.Split(AppSettings.ListDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Items.Add(item.Trim());
            }
        }

        private static string JoinListItems(ListBox list)
        {
            return string.Join(Environment.NewLine, list.Items.Cast<string>());
        }

        private void ApplyControlsToSettings()
        {
            settings.IsSwitchingEnabled = chkEnableSwitching.Checked;
            settings.StartWithWindows = chkStartWithWindows.Checked;
            settings.RestoreApostrophes = chkRestoreApostrophes.Checked;
            settings.EnableLayoutRules = chkEnableLayoutRules.Checked;
            settings.LayoutRulesText = JoinListItems(lstLayoutRules);
            settings.SkipEnterCorrection = chkSkipEnterCorrection.Checked;
            settings.FullScreenNotification = chkFullScreenNotification.Checked;
            settings.NotifyOnAllScreens = chkNotifyOnAllScreens.Checked;
            settings.SkipEnterCorrectionProcessesText = txtSkipEnterProcesses.Text;
            if (cmbProcessMode.SelectedIndex >= 0) settings.ProcessFilterMode = (ProcessFilterMode)cmbProcessMode.SelectedIndex;
            settings.ProcessFilterText = JoinListItems(lstProcesses);
            settings.IgnoredWordsText = JoinListItems(lstIgnoredWords);
            settings.MinimumMappedPercent = (int)numMinimumMappedPercent.Value;

            // IgnoredWords/MinimumMappedPercent affect KeyMapper's heuristic, but its cache
            // is keyed only by word+layout, so stale entries from before this change must
            // be dropped or they'd keep returning the old verdict until evicted.
            KeyMapper.ClearCache();

            // Так само для правил розкладки: без скидання вікно, де користувач раніше
            // перебив правило вручну, лишалось би "недоторканним" і за новими правилами.
            layoutRuleEnforcer?.ResetWindowState();

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

            if (settings.EnableLayoutRules && settings.LayoutRules.Count > 0)
            {
                layoutRuleEnforcer.Start();
            }
            else
            {
                layoutRuleEnforcer.Stop();
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
