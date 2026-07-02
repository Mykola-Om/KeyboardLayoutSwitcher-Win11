using System;
using System.Runtime.InteropServices;

namespace KeyboardLayoutSwitcher
{
    public static class LayoutSwitcher
    {
        private const int PrimaryLanguageMask = 0x03FF;
        private const int EnglishPrimaryLanguageId = 0x0009;
        private const uint KLF_ACTIVATE = 0x00000001;
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int left; public int top; public int right; public int bottom; }

        [DllImport("user32.dll")]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        public static bool IsCurrentKeyboardLayoutEnglish()
        {
            GUITHREADINFO gui = new GUITHREADINFO();
            gui.cbSize = Marshal.SizeOf(gui);
            IntPtr keyboardLayout;

            if (GetGUIThreadInfo(0, ref gui) && gui.hwndFocus != IntPtr.Zero)
            {
                uint threadId = GetWindowThreadProcessId(gui.hwndFocus, IntPtr.Zero);
                keyboardLayout = GetKeyboardLayout(threadId);
            }
            else
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint threadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                keyboardLayout = GetKeyboardLayout(threadId);
            }

            uint keyboardLayoutId = (uint)keyboardLayout & 0xFFFF;
            bool result = (keyboardLayoutId & PrimaryLanguageMask) == EnglishPrimaryLanguageId;
            return result;
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



