using System;
using System.Diagnostics;

namespace KeyboardLayoutSwitcher
{
    public static class ProcessNameResolver
    {
        public static string GetProcessName(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return string.Empty;
            }

            Win32Interop.GetWindowThreadProcessId(hwnd, out uint processId);
            if (processId == 0)
            {
                return string.Empty;
            }

            try
            {
                using (Process process = Process.GetProcessById((int)processId))
                {
                    return process.ProcessName;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
