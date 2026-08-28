using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public class NotificationForm : Form
    {
        // Розмір кутової плашки.
        private static readonly Size CornerSize = new Size(140, 40);
        private const int CornerMargin = 25;

        // Повноекранний режим: увесь екран рівномірно притемнюється, а по центру —
        // великий напис. Значення підібране так, щоб вміст під ним лишався видимим.
        private const double FullScreenOpacity = 0.45;

        // Висота напису — частка від висоти монітора.
        private const double FullScreenFontHeightRatio = 0.12;

        private readonly System.Windows.Forms.Timer fadeTimer;
        private readonly bool isFullScreen;
        private readonly Screen targetScreen;
        private double opacity;

        public NotificationForm(string text, Screen screen, bool fullScreen)
        {
            targetScreen = screen ?? Screen.PrimaryScreen;
            isFullScreen = fullScreen;
            opacity = fullScreen ? FullScreenOpacity : 1.0;

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.Manual;

            if (fullScreen)
            {
                this.BackColor = Color.FromArgb(24, 24, 24);
                this.Bounds = targetScreen.Bounds;
                this.Paint += (s, e) => PaintFullScreenBanner(e.Graphics, text);
            }
            else
            {
                this.BackColor = Color.FromArgb(32, 32, 32);
                this.Size = CornerSize;

                this.Controls.Add(new Label
                {
                    Text = text,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });

                this.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(Color.FromArgb(64, 64, 64), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                    }
                };
            }

            this.Opacity = opacity;

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
                if (!isFullScreen)
                {
                    Rectangle workingArea = targetScreen.WorkingArea;
                    this.Location = new Point(
                        workingArea.Right - this.Width - CornerMargin,
                        workingArea.Bottom - this.Height - CornerMargin);
                }

                waitTimer.Start();
            };
        }

        private void PaintFullScreenBanner(Graphics graphics, string text)
        {
            using (var font = new Font("Segoe UI", (float)(this.Height * FullScreenFontHeightRatio), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                graphics.DrawString(text, font, Brushes.White, this.ClientRectangle, format);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE (0x08000000) — не забирає фокус, тож набір не переривається
                // WS_EX_TOOLWINDOW (0x00000080) — не показується в Alt+Tab
                // WS_EX_TRANSPARENT (0x00000020) — кліки миші проходять наскрізь; без цього
                //   повноекранне вікно перехоплювало б їх і робило екран непридатним
                cp.ExStyle |= 0x08000000 | 0x00000080 | 0x00000020;
                return cp;
            }
        }

        public static void ShowNotification(string text, AppSettings settings)
        {
            if (MainForm.Instance != null && MainForm.Instance.InvokeRequired)
            {
                MainForm.Instance.BeginInvoke(new Action(() => ShowNotification(text, settings)));
                return;
            }

            try
            {
                bool fullScreen = settings != null && settings.FullScreenNotification;
                bool allScreens = settings != null && settings.NotifyOnAllScreens;

                foreach (Screen screen in allScreens ? Screen.AllScreens : new[] { GetActiveScreen() })
                {
                    new NotificationForm(text, screen, fullScreen).Show();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Trace($"Failed to show notification: {ex.Message}");
            }
        }

        private static Screen GetActiveScreen()
        {
            IntPtr foregroundWindow = Win32Interop.GetForegroundWindow();
            return foregroundWindow != IntPtr.Zero
                ? Screen.FromHandle(foregroundWindow)
                : Screen.PrimaryScreen;
        }
    }
}
