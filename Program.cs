using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    static class Program
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static Mutex appMutex;

        [STAThread]
        static void Main()
        {
            // Унікальний GUID для Mutex, який ідентифікує конкретно цю програму
            const string mutexName = "Global\\KeyboardLayoutSwitcher-Win11-instance";
            
            bool createdNew;
            appMutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Якщо Mutex вже існує (createdNew == false), значить програма вже запущена.
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ShowDarkMessage("Програма Keyboard Layout Switcher вже запущена і працює в треї.", "Увага");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSettings settings = AppSettingsStore.Load();
            TraceLogger.Initialize(settings.EnableTrace);

            Application.Run(new MainForm());

            // Важливо звільнити Mutex після завершення роботи
            GC.KeepAlive(appMutex);
        }

        private static void ShowDarkMessage(string text, string caption)
        {
            using (var f = new Form())
            {
                f.Text = caption;
                f.Size = new Size(380, 150);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterScreen;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowIcon = false;
                f.BackColor = Color.FromArgb(32, 32, 32);
                f.ForeColor = Color.White;
                f.Font = new Font("Segoe UI", 9.5f);

                try
                {
                    int useImmersiveDarkMode = 1;
                    DwmSetWindowAttribute(f.Handle, 20, ref useImmersiveDarkMode, sizeof(int));
                }
                catch { }

                var lbl = new Label() 
                { 
                    Text = text, 
                    Location = new Point(20, 25), 
                    AutoSize = true, 
                    MaximumSize = new Size(320, 0) 
                };

                var btn = new Button() 
                { 
                    Text = "OK", 
                    Location = new Point(135, 70), 
                    Size = new Size(90, 28), 
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                btn.FlatAppearance.BorderSize = 1;

                f.Controls.Add(lbl);
                f.Controls.Add(btn);
                f.AcceptButton = btn;

                f.ShowDialog();
            }
        }
    }
}
