using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public class KeyboardHook : IDisposable
    {
        private static readonly System.Collections.Generic.Dictionary<int, char> englishLowerMap = CreateEnglishLowerMap();
        private static readonly System.Collections.Generic.Dictionary<int, char> englishUpperMap = CreateEnglishUpperMap();
        private static readonly System.Collections.Generic.Dictionary<int, char> ukrainianLowerMap = CreateUkrainianLowerMap();
        private static readonly System.Collections.Generic.Dictionary<int, char> ukrainianUpperMap = CreateUkrainianUpperMap();

        private readonly AppSettings settings;
        private IntPtr hookId = IntPtr.Zero;
        private IntPtr mouseHookId = IntPtr.Zero;
        private LowLevelKeyboardProc proc;
        private LowLevelMouseProc mouseProc;
        private bool isEnglishLayout;
        private readonly WordTracker wordTracker = new WordTracker();
        private readonly InputReplacer inputReplacer = new InputReplacer();
        private IntPtr lastForegroundWindow = IntPtr.Zero;
        private string cachedProcessName = string.Empty;
        private readonly object processNameCacheLock = new object();

        public KeyboardHook(AppSettings settings)
        {
            this.settings = settings ?? new AppSettings();
            proc = HookCallback;
            mouseProc = MouseHookCallback;
            isEnglishLayout = LayoutSwitcher.IsCurrentKeyboardLayoutEnglish();
            TraceLogger.Trace("KeyboardHook initialized");
        }

        public void Start()
        {
            if (hookId != IntPtr.Zero)
            {
                TraceLogger.Trace("Start skipped: hook already active");
                return;
            }

            hookId = SetHook(proc);
            mouseHookId = SetMouseHook(mouseProc);
            TraceLogger.Trace("Start hook result: " + hookId + " | lastError=" + Marshal.GetLastWin32Error());
        }

        public void Stop()
        {
            if (hookId != IntPtr.Zero)
            {
                Win32Interop.UnhookWindowsHookEx(hookId);
                hookId = IntPtr.Zero;
            }
            if (mouseHookId != IntPtr.Zero)
            {
                Win32Interop.UnhookWindowsHookEx(mouseHookId);
                mouseHookId = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            return Win32Interop.SetWindowsHookEx(Win32Interop.WH_KEYBOARD_LL, proc, IntPtr.Zero, 0);
        }

        private IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            return Win32Interop.SetWindowsHookEx(Win32Interop.WH_MOUSE_LL, proc, IntPtr.Zero, 0);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = (int)wParam;
                if (msg == Win32Interop.WM_LBUTTONDOWN || msg == Win32Interop.WM_RBUTTONDOWN || msg == Win32Interop.WM_MBUTTONDOWN)
                {
                    wordTracker.Clear();
                }
            }
            return Win32Interop.CallNextHookEx(mouseHookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && ((wParam == (IntPtr)Win32Interop.WM_KEYDOWN) || (wParam == (IntPtr)Win32Interop.WM_SYSKEYDOWN)))
            {
                Win32Interop.KbdLlHookStruct hookData = Marshal.PtrToStructure<Win32Interop.KbdLlHookStruct>(lParam);
                if (inputReplacer.IsReplacing || (hookData.flags & Win32Interop.LLKHF_INJECTED) == Win32Interop.LLKHF_INJECTED)
                {
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                IntPtr foregroundWindow = Win32Interop.GetForegroundWindow();
                if (foregroundWindow != lastForegroundWindow)
                {
                    lastForegroundWindow = foregroundWindow;
                    wordTracker.Clear();
                    lock (processNameCacheLock)
                    {
                        cachedProcessName = ProcessNameResolver.GetProcessName(foregroundWindow);
                    }
                }

                if (!settings.IsProcessAllowed(cachedProcessName))
                {
                    wordTracker.Clear();
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                isEnglishLayout = LayoutSwitcher.IsCurrentKeyboardLayoutEnglish();

                int vkCode = (int)hookData.vkCode;
                if (vkCode == Win32Interop.VK_RETURN)
                {
                    if (TryReplaceCurrentWordAtBoundary('\n', ref isEnglishLayout))
                    {
                        return (IntPtr)1;
                    }

                    wordTracker.Clear();
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                if (vkCode == Win32Interop.VK_TAB)
                {
                    if (TryReplaceCurrentWordAtBoundary('\t', ref isEnglishLayout))
                    {
                        return (IntPtr)1;
                    }

                    wordTracker.Clear();
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                if (HandleEditingKey(vkCode))
                {
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                bool isCtrlPressed = IsKeyPressed(Win32Interop.VK_CONTROL) || IsKeyPressed(Win32Interop.VK_LCONTROL) || IsKeyPressed(Win32Interop.VK_RCONTROL);
                bool isAltPressed = IsKeyPressed(Win32Interop.VK_MENU) || IsKeyPressed(Win32Interop.VK_LMENU) || IsKeyPressed(Win32Interop.VK_RMENU);
                bool isWinPressed = IsKeyPressed(Win32Interop.VK_LWIN) || IsKeyPressed(Win32Interop.VK_RWIN);

                if (isCtrlPressed || isAltPressed || isWinPressed)
                {
                    wordTracker.Clear();
                    return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                char ch = GetCharFromKey(vkCode, isEnglishLayout);

                if (KeyMapper.IsLayoutWordCharacter(ch, isEnglishLayout))
                {
                    wordTracker.AppendChar(ch);
                }
                else if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch))
                {
                    if (TryReplaceCurrentWordAtBoundary(ch, ref isEnglishLayout))
                    {
                        TraceLogger.Trace("Boundary swallowed for replacement");
                        return (IntPtr)1;
                    }

                    wordTracker.Clear();
                }
                else if (!IsModifierKey(vkCode))
                {
                    wordTracker.Clear();
                }
            }
            return Win32Interop.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private bool HandleEditingKey(int vkCode)
        {
            if (vkCode == Win32Interop.VK_BACK)
            {
                wordTracker.RemoveLastChar();
                return true;
            }

            if (vkCode == Win32Interop.VK_DELETE ||
                vkCode == Win32Interop.VK_ESCAPE ||
                vkCode == Win32Interop.VK_LEFT ||
                vkCode == Win32Interop.VK_RIGHT ||
                vkCode == Win32Interop.VK_UP ||
                vkCode == Win32Interop.VK_DOWN ||
                vkCode == Win32Interop.VK_HOME ||
                vkCode == Win32Interop.VK_END ||
                vkCode == Win32Interop.VK_PRIOR ||
                vkCode == Win32Interop.VK_NEXT)
            {
                wordTracker.Clear();
                return true;
            }

            return false;
        }

        private bool TryReplaceCurrentWordAtBoundary(char boundaryChar, ref bool currentLayoutIsEnglish)
        {
            if (!wordTracker.TryGetWordAtBoundary(out string word))
            {
                return false;
            }

            if (!KeyMapper.IsWrongLayout(word, currentLayoutIsEnglish, settings))
            {
                return false;
            }

            string correctedWord = KeyMapper.ConvertWord(word, currentLayoutIsEnglish);
            TraceLogger.Trace($"Replacement: len={word.Length}");

            bool oldLayout = currentLayoutIsEnglish;
            currentLayoutIsEnglish = !currentLayoutIsEnglish;

            inputReplacer.QueueReplacement(word.Length, correctedWord, boundaryChar, oldLayout);
            wordTracker.Clear();
            return true;
        }

        private static bool IsModifierKey(int vkCode)
        {
            return vkCode == Win32Interop.VK_SHIFT ||
                   vkCode == Win32Interop.VK_LSHIFT ||
                   vkCode == Win32Interop.VK_RSHIFT ||
                   vkCode == Win32Interop.VK_CONTROL ||
                   vkCode == Win32Interop.VK_LCONTROL ||
                   vkCode == Win32Interop.VK_RCONTROL ||
                   vkCode == Win32Interop.VK_MENU ||
                   vkCode == Win32Interop.VK_LMENU ||
                   vkCode == Win32Interop.VK_RMENU ||
                   vkCode == Win32Interop.VK_LWIN ||
                   vkCode == Win32Interop.VK_RWIN ||
                   vkCode == Win32Interop.VK_CAPITAL;
        }

        private char GetCharFromKey(int vkCode, bool isEnglishKeyboardLayout)
        {
            bool isShiftPressed = IsKeyPressed(Win32Interop.VK_SHIFT) || IsKeyPressed(Win32Interop.VK_LSHIFT) || IsKeyPressed(Win32Interop.VK_RSHIFT);
            bool isCapsLockEnabled = (Win32Interop.GetKeyState(Win32Interop.VK_CAPITAL) & Win32Interop.KEYSTATE_TOGGLED) != 0;

            if (vkCode >= (int)Keys.A && vkCode <= (int)Keys.Z)
            {
                bool useUpperCase = isShiftPressed ^ isCapsLockEnabled;
                return GetMappedChar(vkCode, isEnglishKeyboardLayout, useUpperCase);
            }

            return GetMappedChar(vkCode, isEnglishKeyboardLayout, isShiftPressed);
        }

        private static char GetMappedChar(int vkCode, bool isEnglishKeyboardLayout, bool useUpperCase)
        {
            System.Collections.Generic.Dictionary<int, char> map = isEnglishKeyboardLayout
                ? (useUpperCase ? englishUpperMap : englishLowerMap)
                : (useUpperCase ? ukrainianUpperMap : ukrainianLowerMap);

            if (map.TryGetValue(vkCode, out char character))
            {
                return character;
            }

            return '\0';
        }

        private static bool IsKeyPressed(int virtualKey)
        {
            return (Win32Interop.GetKeyState(virtualKey) & Win32Interop.KEYSTATE_PRESSED) != 0;
        }

        public void Dispose()
        {
            Stop();
        }

        // Hook delegates
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static Dictionary<int, char> CreateEnglishLowerMap()
        {
            return new Dictionary<int, char>
            {
                [(int)Keys.A] = 'a', [(int)Keys.B] = 'b', [(int)Keys.C] = 'c', [(int)Keys.D] = 'd',
                [(int)Keys.E] = 'e', [(int)Keys.F] = 'f', [(int)Keys.G] = 'g', [(int)Keys.H] = 'h',
                [(int)Keys.I] = 'i', [(int)Keys.J] = 'j', [(int)Keys.K] = 'k', [(int)Keys.L] = 'l',
                [(int)Keys.M] = 'm', [(int)Keys.N] = 'n', [(int)Keys.O] = 'o', [(int)Keys.P] = 'p',
                [(int)Keys.Q] = 'q', [(int)Keys.R] = 'r', [(int)Keys.S] = 's', [(int)Keys.T] = 't',
                [(int)Keys.U] = 'u', [(int)Keys.V] = 'v', [(int)Keys.W] = 'w', [(int)Keys.X] = 'x',
                [(int)Keys.Y] = 'y', [(int)Keys.Z] = 'z',
                [Win32Interop.VK_OEM_4] = '[', [Win32Interop.VK_OEM_6] = ']', [Win32Interop.VK_OEM_1] = ';', [Win32Interop.VK_OEM_7] = '\'',
                [Win32Interop.VK_OEM_COMMA] = ',', [Win32Interop.VK_OEM_PERIOD] = '.', [Win32Interop.VK_SPACE] = ' ',
                [(int)Keys.D1] = '1', [(int)Keys.D2] = '2', [(int)Keys.D3] = '3', [(int)Keys.D4] = '4', [(int)Keys.D5] = '5',
                [(int)Keys.D6] = '6', [(int)Keys.D7] = '7', [(int)Keys.D8] = '8', [(int)Keys.D9] = '9', [(int)Keys.D0] = '0',
                [Win32Interop.VK_OEM_MINUS] = '-', [Win32Interop.VK_OEM_PLUS] = '=', [Win32Interop.VK_OEM_2] = '/', [Win32Interop.VK_OEM_3] = '`', [Win32Interop.VK_OEM_5] = '\\'
            };
        }

        private static Dictionary<int, char> CreateEnglishUpperMap()
        {
            return new Dictionary<int, char>
            {
                [(int)Keys.A] = 'A', [(int)Keys.B] = 'B', [(int)Keys.C] = 'C', [(int)Keys.D] = 'D',
                [(int)Keys.E] = 'E', [(int)Keys.F] = 'F', [(int)Keys.G] = 'G', [(int)Keys.H] = 'H',
                [(int)Keys.I] = 'I', [(int)Keys.J] = 'J', [(int)Keys.K] = 'K', [(int)Keys.L] = 'L',
                [(int)Keys.M] = 'M', [(int)Keys.N] = 'N', [(int)Keys.O] = 'O', [(int)Keys.P] = 'P',
                [(int)Keys.Q] = 'Q', [(int)Keys.R] = 'R', [(int)Keys.S] = 'S', [(int)Keys.T] = 'T',
                [(int)Keys.U] = 'U', [(int)Keys.V] = 'V', [(int)Keys.W] = 'W', [(int)Keys.X] = 'X',
                [(int)Keys.Y] = 'Y', [(int)Keys.Z] = 'Z',
                [Win32Interop.VK_OEM_4] = '{', [Win32Interop.VK_OEM_6] = '}', [Win32Interop.VK_OEM_1] = ':', [Win32Interop.VK_OEM_7] = '"',
                [Win32Interop.VK_OEM_COMMA] = '<', [Win32Interop.VK_OEM_PERIOD] = '>', [Win32Interop.VK_SPACE] = ' ',
                [(int)Keys.D1] = '!', [(int)Keys.D2] = '@', [(int)Keys.D3] = '#', [(int)Keys.D4] = '$', [(int)Keys.D5] = '%',
                [(int)Keys.D6] = '^', [(int)Keys.D7] = '&', [(int)Keys.D8] = '*', [(int)Keys.D9] = '(', [(int)Keys.D0] = ')',
                [Win32Interop.VK_OEM_MINUS] = '_', [Win32Interop.VK_OEM_PLUS] = '+', [Win32Interop.VK_OEM_2] = '?', [Win32Interop.VK_OEM_3] = '~', [Win32Interop.VK_OEM_5] = '|'
            };
        }

        private static Dictionary<int, char> CreateUkrainianLowerMap()
        {
            return new Dictionary<int, char>
            {
                [(int)Keys.Q] = 'й', [(int)Keys.W] = 'ц', [(int)Keys.E] = 'у', [(int)Keys.R] = 'к',
                [(int)Keys.T] = 'е', [(int)Keys.Y] = 'н', [(int)Keys.U] = 'г', [(int)Keys.I] = 'ш',
                [(int)Keys.O] = 'щ', [(int)Keys.P] = 'з',
                [(int)Keys.A] = 'ф', [(int)Keys.S] = 'і', [(int)Keys.D] = 'в', [(int)Keys.F] = 'а',
                [(int)Keys.G] = 'п', [(int)Keys.H] = 'р', [(int)Keys.J] = 'о', [(int)Keys.K] = 'л',
                [(int)Keys.L] = 'д',
                [(int)Keys.Z] = 'я', [(int)Keys.X] = 'ч', [(int)Keys.C] = 'с', [(int)Keys.V] = 'м',
                [(int)Keys.B] = 'и', [(int)Keys.N] = 'т', [(int)Keys.M] = 'ь',
                [Win32Interop.VK_OEM_4] = 'х', [Win32Interop.VK_OEM_6] = 'ї', [Win32Interop.VK_OEM_1] = 'ж', [Win32Interop.VK_OEM_7] = 'є',
                [Win32Interop.VK_OEM_COMMA] = 'б', [Win32Interop.VK_OEM_PERIOD] = 'ю', [Win32Interop.VK_SPACE] = ' ',
                [(int)Keys.D1] = '1', [(int)Keys.D2] = '2', [(int)Keys.D3] = '3', [(int)Keys.D4] = '4', [(int)Keys.D5] = '5',
                [(int)Keys.D6] = '6', [(int)Keys.D7] = '7', [(int)Keys.D8] = '8', [(int)Keys.D9] = '9', [(int)Keys.D0] = '0',
                [Win32Interop.VK_OEM_MINUS] = '-', [Win32Interop.VK_OEM_PLUS] = '=', [Win32Interop.VK_OEM_2] = '.', [Win32Interop.VK_OEM_3] = '\'', [Win32Interop.VK_OEM_5] = '\\'
            };
        }

        private static Dictionary<int, char> CreateUkrainianUpperMap()
        {
            return new Dictionary<int, char>
            {
                [(int)Keys.Q] = 'Й', [(int)Keys.W] = 'Ц', [(int)Keys.E] = 'У', [(int)Keys.R] = 'К',
                [(int)Keys.T] = 'Е', [(int)Keys.Y] = 'Н', [(int)Keys.U] = 'Г', [(int)Keys.I] = 'Ш',
                [(int)Keys.O] = 'Щ', [(int)Keys.P] = 'З',
                [(int)Keys.A] = 'Ф', [(int)Keys.S] = 'І', [(int)Keys.D] = 'В', [(int)Keys.F] = 'А',
                [(int)Keys.G] = 'П', [(int)Keys.H] = 'Р', [(int)Keys.J] = 'О', [(int)Keys.K] = 'Л',
                [(int)Keys.L] = 'Д',
                [(int)Keys.Z] = 'Я', [(int)Keys.X] = 'Ч', [(int)Keys.C] = 'С', [(int)Keys.V] = 'М',
                [(int)Keys.B] = 'И', [(int)Keys.N] = 'Т', [(int)Keys.M] = 'Ь',
                [Win32Interop.VK_OEM_4] = 'Х', [Win32Interop.VK_OEM_6] = 'Ї', [Win32Interop.VK_OEM_1] = 'Ж', [Win32Interop.VK_OEM_7] = 'Є',
                [Win32Interop.VK_OEM_COMMA] = 'Б', [Win32Interop.VK_OEM_PERIOD] = 'Ю', [Win32Interop.VK_SPACE] = ' ',
                [(int)Keys.D1] = '!', [(int)Keys.D2] = '"', [(int)Keys.D3] = '№', [(int)Keys.D4] = ';', [(int)Keys.D5] = '%',
                [(int)Keys.D6] = ':', [(int)Keys.D7] = '?', [(int)Keys.D8] = '*', [(int)Keys.D9] = '(', [(int)Keys.D0] = ')',
                [Win32Interop.VK_OEM_MINUS] = '_', [Win32Interop.VK_OEM_PLUS] = '+', [Win32Interop.VK_OEM_2] = ',', [Win32Interop.VK_OEM_3] = '₴', [Win32Interop.VK_OEM_5] = '/'
            };
        }
    }
}



