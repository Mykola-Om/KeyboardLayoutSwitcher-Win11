using System;
using System.Runtime.InteropServices;

namespace KeyboardLayoutSwitcher
{
    public static class LayoutSwitcher
    {
        private const uint KLF_ACTIVATE = 0x00000001;
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

        public static bool IsCurrentKeyboardLayoutEnglish()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            uint threadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
            IntPtr keyboardLayout = GetKeyboardLayout(threadId);

            uint keyboardLayoutId = (uint)keyboardLayout & 0xFFFF;
            return keyboardLayoutId == 0x0409; // 0x0409 - English (United States)
        }

        public static void SwitchKeyboardLayout(ref bool isEnglishLayout)
        {
            IntPtr foregroundWindow = GetForegroundWindow();

            if (isEnglishLayout)
            {
                IntPtr hkl = LoadKeyboardLayout("00000422", KLF_ACTIVATE); // Ukrainian
                PostMessage(foregroundWindow, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
            }
            else
            {
                IntPtr hkl = LoadKeyboardLayout("00000409", KLF_ACTIVATE); // English (US)
                PostMessage(foregroundWindow, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
            }

            isEnglishLayout = !isEnglishLayout;
        }

        // WinAPI functions.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
