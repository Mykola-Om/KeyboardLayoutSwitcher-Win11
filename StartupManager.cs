using Microsoft.Win32;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "KeyboardLayoutSwitcher";

        public static void SetStartWithWindows(bool isEnabled)
        {
            try
            {
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (runKey == null)
                    {
                        return;
                    }

                    if (isEnabled)
                    {
                        runKey.SetValue(ValueName, '"' + Application.ExecutablePath + '"');
                    }
                    else
                    {
                        runKey.DeleteValue(ValueName, false);
                    }
                }
            }
            catch
            {
            }
        }

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    return runKey?.GetValue(ValueName) != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}