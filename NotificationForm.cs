using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public class NotificationForm : Form
    {
        private readonly System.Windows.Forms.Timer fadeTimer;
        private double opacity = 1.0;

        public NotificationForm(string text)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(140, 40);
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.Manual;

            Label label = new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(label);

            this.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(64, 64, 64), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            fadeTimer = new System.Windows.Forms.Timer { Interval = 40 };
            fadeTimer.Tick += (s, e) =>
            {
                opacity -= 0.08;
                if (opacity <= 0)
                {
                    fadeTimer.Stop();
                    this.Close();
                }
                else
                {
                    this.Opacity = opacity;
                }
            };
 
            var waitTimer = new System.Windows.Forms.Timer { Interval = 800 };
            waitTimer.Tick += (s, e) =>
            {
                waitTimer.Stop();
                fadeTimer.Start();
            };
 
            this.Load += (s, e) =>
            {
                // Position at bottom-right above taskbar of the active monitor
                IntPtr foregroundWindow = Win32Interop.GetForegroundWindow();
                Screen activeScreen = foregroundWindow != IntPtr.Zero 
                    ? Screen.FromHandle(foregroundWindow) 
                    : Screen.PrimaryScreen;

                Rectangle workingArea = activeScreen.WorkingArea;
                this.Location = new Point(
                    workingArea.Right - this.Width - 25,
                    workingArea.Bottom - this.Height - 25
                );

                waitTimer.Start();
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE (0x08000000) prevents focus
                // WS_EX_TOOLWINDOW (0x00000080) hides from Alt+Tab
                cp.ExStyle |= 0x08000000 | 0x00000080;
                return cp;
            }
        }

        public static void ShowNotification(string text)
        {
            if (MainForm.Instance != null && MainForm.Instance.InvokeRequired)
            {
                MainForm.Instance.BeginInvoke(new Action(() => ShowNotification(text)));
                return;
            }

            try
            {
                NotificationForm form = new NotificationForm(text);
                form.Show();
            }
            catch (Exception ex)
            {
                TraceLogger.Trace($"Failed to show notification: {ex.Message}");
            }
        }
    }
}
