using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public class KeyboardHook : IDisposable
    {
        private IntPtr hookId = IntPtr.Zero;
        private LowLevelKeyboardProc proc;
        private bool isEnglishLayout;
        private StringBuilder currentWord = new StringBuilder();
        private bool isReplacing;

        public KeyboardHook()
        {
            proc = HookCallback;
            isEnglishLayout = LayoutSwitcher.IsCurrentKeyboardLayoutEnglish();
        }

        public void Start()
        {
            if (hookId != IntPtr.Zero)
            {
                return;
            }

            hookId = SetHook(proc);
        }

        public void Stop()
        {
            if (hookId == IntPtr.Zero)
            {
                return;
            }

            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && ((wParam == (IntPtr)WM_KEYDOWN) || (wParam == (IntPtr)WM_SYSKEYDOWN)))
            {
                KbdLlHookStruct hookData = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
                if (isReplacing || (hookData.flags & LLKHF_INJECTED) == LLKHF_INJECTED)
                {
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                int vkCode = (int)hookData.vkCode;
                char ch = GetCharFromKey(vkCode);

                if (char.IsLetter(ch))
                {
                    currentWord.Append(ch);

                    if (currentWord.Length >= 3)
                    {
                        if (KeyMapper.IsWrongLayout(currentWord.ToString(), isEnglishLayout))
                        {
                            ReplaceLastWord();
                            LayoutSwitcher.SwitchKeyboardLayout(ref isEnglishLayout);
                            currentWord.Clear();
                        }
                    }
                }
                else if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch))
                {
                    currentWord.Clear();
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private char GetCharFromKey(int vkCode)
        {
            byte[] keyboardState = new byte[256];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
                return '\0';

            uint scanCode = MapVirtualKey((uint)vkCode, 0);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)vkCode, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            if (result > 0)
            {
                return stringBuilder[0];
            }
            else
            {
                return '\0';
            }
        }

        private void ReplaceLastWord()
        {
            isReplacing = true;
            try
            {
                for (int i = 0; i < currentWord.Length; i++)
                {
                    SendKeys.SendWait("{BACKSPACE}");
                }

                string correctedWord = KeyMapper.ConvertWord(currentWord.ToString(), isEnglishLayout);

                SendKeys.SendWait(correctedWord);
            }
            finally
            {
                isReplacing = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        // WinAPI functions and constants.
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const uint LLKHF_INJECTED = 0x00000010;

        [StructLayout(LayoutKind.Sequential)]
        private struct KbdLlHookStruct
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode,
            byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
            int cchBuff, uint wFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
