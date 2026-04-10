using System;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаємо головну форму
            Application.Run(new MainForm());
        }
    }
}
