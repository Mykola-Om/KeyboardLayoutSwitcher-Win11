using System;
using System.Runtime.InteropServices;

namespace KeyboardLayoutSwitcher
{
    public static class LayoutSwitcher
    {
        private const int PrimaryLanguageMask = 0x03FF;
        private const int EnglishPrimaryLanguageId = 0x0009;
        private const int UkrainianPrimaryLanguageId = 0x0022;
        private const uint KLF_ACTIVATE = 0x00000001;
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

        // KLID (Keyboard Layout ID) рядки для LoadKeyboardLayout.
        private const string UkrainianKeyboardLayoutId = "00000422";
        private const string EnglishUsKeyboardLayoutId = "00000409";

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
            SetKeyboardLayout(GetForegroundWindow(), !isEnglishLayout);
            isEnglishLayout = !isEnglishLayout;
        }

        /// <summary>
        /// Виставляє конкретну розкладку у вказаному вікні (на відміну від
        /// <see cref="SwitchKeyboardLayout"/>, який просто перемикає на протилежну).
        /// Повідомлення надсилається асинхронно, тож одразу читати результат назад не можна.
        /// </summary>
        public static void SetKeyboardLayout(IntPtr window, bool english)
        {
            if (window == IntPtr.Zero)
            {
                return;
            }

            PostMessage(window, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, ResolveKeyboardLayout(english));
        }

        /// <summary>
        /// Обирає серед уже завантажених розкладок першу з потрібною мовою.
        ///
        /// Жорсткий ідентифікатор ("00000422") тут не годиться: користувач цілком може мати
        /// власну розкладку тієї ж мови (напр. "d0010422") і саме нею користуватись — тоді
        /// LoadKeyboardLayout підсовував би стандартну, якою він не друкує. Порядок списку
        /// відповідає порядку розкладок користувача, тому перша відповідна — та, що треба.
        /// </summary>
        public static IntPtr ResolveKeyboardLayout(bool english)
        {
            int targetPrimaryLanguage = english ? EnglishPrimaryLanguageId : UkrainianPrimaryLanguageId;

            foreach (IntPtr layout in GetInstalledLayouts())
            {
                uint languageId = (uint)(layout.ToInt64() & 0xFFFF);
                if ((languageId & PrimaryLanguageMask) == targetPrimaryLanguage)
                {
                    return layout;
                }
            }

            // Потрібної мови серед завантажених немає — лишається завантажити стандартну.
            return LoadKeyboardLayout(english ? EnglishUsKeyboardLayoutId : UkrainianKeyboardLayoutId, KLF_ACTIVATE);
        }

        /// <summary>
        /// Розкладки, встановлені в системі, у порядку налаштувань користувача.
        /// </summary>
        public static IntPtr[] GetInstalledLayouts()
        {
            int layoutCount = GetKeyboardLayoutList(0, null);
            if (layoutCount <= 0)
            {
                return new IntPtr[0];
            }

            IntPtr[] layouts = new IntPtr[layoutCount];
            layoutCount = GetKeyboardLayoutList(layoutCount, layouts);

            if (layoutCount < layouts.Length)
            {
                Array.Resize(ref layouts, Math.Max(0, layoutCount));
            }

            return layouts;
        }

        /// <summary>
        /// Розкладка конкретного вікна. Windows може тримати розкладку окремо для кожного
        /// вікна, тому перевіряти треба саме те вікно, яке нас цікавить.
        /// </summary>
        public static bool IsLayoutEnglishForWindow(IntPtr window)
        {
            if (window == IntPtr.Zero)
            {
                return IsCurrentKeyboardLayoutEnglish();
            }

            uint threadId = GetWindowThreadProcessId(window, IntPtr.Zero);
            uint keyboardLayoutId = (uint)GetKeyboardLayout(threadId) & 0xFFFF;
            return (keyboardLayoutId & PrimaryLanguageMask) == EnglishPrimaryLanguageId;
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

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}



