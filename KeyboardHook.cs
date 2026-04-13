using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    public class KeyboardHook : IDisposable
    {
        private static readonly Dictionary<int, char> englishLowerMap = CreateEnglishLowerMap();
        private static readonly Dictionary<int, char> englishUpperMap = CreateEnglishUpperMap();
        private static readonly Dictionary<int, char> ukrainianLowerMap = CreateUkrainianLowerMap();
        private static readonly Dictionary<int, char> ukrainianUpperMap = CreateUkrainianUpperMap();
        private static readonly Dictionary<char, Tuple<ushort, bool>> charToVirtualKeyMap = BuildCharToVirtualKeyMap();

        private readonly AppSettings settings;
        private readonly SynchronizationContext synchronizationContext;
        private readonly string logPath;
        private IntPtr hookId = IntPtr.Zero;
        private LowLevelKeyboardProc proc;
        private bool isEnglishLayout;
        private StringBuilder currentWord = new StringBuilder();
        private bool isReplacing;
        private IntPtr lastForegroundWindow = IntPtr.Zero;
        private bool? lastWordIsEnglish = null;

        public KeyboardHook(AppSettings settings)
        {
            this.settings = settings ?? new AppSettings();
            synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
            logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyboardLayoutSwitcher", "trace.log");
            proc = HookCallback;
            isEnglishLayout = LayoutSwitcher.IsCurrentKeyboardLayoutEnglish();
            Trace("KeyboardHook initialized");
        }

        public void Start()
        {
            if (hookId != IntPtr.Zero)
            {
                Trace("Start skipped: hook already active");
                return;
            }

            hookId = SetHook(proc);
            Trace("Start hook result: " + hookId + " | lastError=" + Marshal.GetLastWin32Error());
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
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, IntPtr.Zero, 0);
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

                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != lastForegroundWindow)
                {
                    lastForegroundWindow = foregroundWindow;
                    currentWord.Clear();
                    lastWordIsEnglish = null;
                }

                if (!settings.IsProcessAllowed(GetProcessName(foregroundWindow)))
                {
                    currentWord.Clear();
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                isEnglishLayout = LayoutSwitcher.IsCurrentKeyboardLayoutEnglish();

                int vkCode = (int)hookData.vkCode;
                if (vkCode == VK_RETURN)
                {
                    if (TryReplaceCurrentWordAtBoundary('\n', ref isEnglishLayout))
                    {
                        return (IntPtr)1;
                    }

                    currentWord.Clear();
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                if (vkCode == VK_TAB)
                {
                    if (TryReplaceCurrentWordAtBoundary('\t', ref isEnglishLayout))
                    {
                        return (IntPtr)1;
                    }

                    currentWord.Clear();
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                if (HandleEditingKey(vkCode))
                {
                    return CallNextHookEx(hookId, nCode, wParam, lParam);
                }

                char ch = GetCharFromKey(vkCode, isEnglishLayout);

                if (KeyMapper.IsLayoutWordCharacter(ch, isEnglishLayout))
                {
                    currentWord.Append(ch);
                    Trace("Append letter: " + ch + " | word=" + currentWord);
                }
                else if (char.IsWhiteSpace(ch) || (char.IsPunctuation(ch) && !KeyMapper.IsLayoutWordCharacter(ch, isEnglishLayout)))
                {
                    Trace("Boundary: " + ((int)ch) + " | word=" + currentWord);
                    if (TryReplaceCurrentWordAtBoundary(ch, ref isEnglishLayout))
                    {
                        Trace("Boundary swallowed for replacement");
                        return (IntPtr)1;
                    }

                    currentWord.Clear();
                }
                else if (!IsModifierKey(vkCode) && (ch == '\0' || !char.IsLetterOrDigit(ch)))
                {
                    // Do not clear currentWord for undefined keys
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private bool HandleEditingKey(int vkCode)
        {
            if (vkCode == VK_BACK)
            {
                if (currentWord.Length > 0)
                {
                    currentWord.Length -= 1;
                }
                else
                {
                    currentWord.Clear();
                }

                return true;
            }

            if (vkCode == VK_DELETE ||
                vkCode == VK_ESCAPE ||
                vkCode == VK_LEFT ||
                vkCode == VK_RIGHT ||
                vkCode == VK_UP ||
                vkCode == VK_DOWN ||
                vkCode == VK_HOME ||
                vkCode == VK_END ||
                vkCode == VK_PRIOR ||
                vkCode == VK_NEXT)
            {
                currentWord.Clear();
                return true;
            }

            return false;
        }

        private bool TryReplaceCurrentWordAtBoundary(char boundaryChar, ref bool currentLayoutIsEnglish)
        {
            if (currentWord.Length == 0)
            {
                return false;
            }

            string word = currentWord.ToString();
            Trace("Try replace | word=" + word + " | english=" + currentLayoutIsEnglish + " | context=" + lastWordIsEnglish);
            
            if (!KeyMapper.IsWrongLayout(word, currentLayoutIsEnglish, settings, lastWordIsEnglish))
            {
                Trace("Rejected by heuristic | word=" + word);
                // Update context even if not switching, as we've confirmed the language
                if (word.Length >= 2) lastWordIsEnglish = currentLayoutIsEnglish;
                return false;
            }

            string correctedWord = KeyMapper.ConvertWord(word, currentLayoutIsEnglish);
            Trace("Accepted | word=" + word + " | corrected=" + correctedWord);

            bool oldLayout = currentLayoutIsEnglish;
            currentLayoutIsEnglish = !currentLayoutIsEnglish;
            lastWordIsEnglish = currentLayoutIsEnglish;

            QueueReplacement(word.Length, correctedWord, boundaryChar, oldLayout);
            currentWord.Clear();
            return true;
        }

        private void QueueReplacement(int originalLength, string correctedWord, char boundaryChar, bool oldLayout)
        {
            isReplacing = true;
            Trace("Queue replacement | len=" + originalLength + " | corrected=" + correctedWord + " | boundary=" + (int)boundaryChar);
            synchronizationContext.Post(_ =>
            {
                try
                {
                    Trace("Execute replacement start");
                    
                    bool dummyLayout = oldLayout;
                    LayoutSwitcher.SwitchKeyboardLayout(ref dummyLayout);
                    Thread.Sleep(30);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < originalLength; i++)
                    {
                        sb.Append("{BACKSPACE}");
                    }
                    sb.Append(EscapeForSendKeys(correctedWord));
                    
                    if (boundaryChar == '\r' || boundaryChar == '\n')
                        sb.Append("{ENTER}");
                    else if (boundaryChar == '\t')
                        sb.Append("{TAB}");
                    else
                        sb.Append(EscapeForSendKeys(boundaryChar.ToString()));

                    SendKeys.SendWait(sb.ToString());
                    
                    Trace("Execute replacement done");
                }
                catch (Exception e)
                {
                    Trace("Error in replacement: " + e.Message);
                }
                finally
                {
                    isReplacing = false;
                }
            }, null);
        }

        private static string EscapeForSendKeys(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            StringBuilder sb = new StringBuilder(text.Length * 2);
            foreach (char c in text)
            {
                if (c == '+' || c == '^' || c == '%' || c == '~' || c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']')
                {
                    sb.Append('{');
                    sb.Append(c);
                    sb.Append('}');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private void Trace(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                File.AppendAllText(logPath, DateTime.Now.ToString("HH:mm:ss.fff") + " | " + message + Environment.NewLine);
            }
            catch
            {
            }
        }

        private static void SendBoundary(char boundaryChar)
        {
            if (boundaryChar == '\r' || boundaryChar == '\n')
            {
                SendVirtualKey(VK_RETURN);
                return;
            }

            if (boundaryChar == '\t')
            {
                SendVirtualKey(VK_TAB);
                return;
            }

            if (boundaryChar == ' ')
            {
                // Force key release in case they are still holding it physically
                INPUT[] spaceUp = new INPUT[] { CreateVirtualKeyInput(VK_SPACE, KEYEVENTF_KEYUP) };
                SendInput((uint)spaceUp.Length, spaceUp, Marshal.SizeOf(typeof(INPUT)));
                
                SendVirtualKey(VK_SPACE);
                return;
            }

            SendVirtualKeyText(boundaryChar.ToString());
        }

        private static void SendBackspaces(int count)
        {
            for (int index = 0; index < count; index++)
            {
                SendVirtualKey(VK_BACK);
            }
        }

        private static void SendUnicodeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            INPUT[] inputs = new INPUT[text.Length * 2];
            int inputIndex = 0;

            foreach (char character in text)
            {
                inputs[inputIndex++] = CreateUnicodeInput(character, KEYEVENTF_UNICODE);
                inputs[inputIndex++] = CreateUnicodeInput(character, KEYEVENTF_KEYUP | KEYEVENTF_UNICODE);
            }

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendVirtualKey(ushort virtualKey)
        {
            INPUT[] inputs = new INPUT[]
            {
                CreateVirtualKeyInput(virtualKey, 0),
                CreateVirtualKeyInput(virtualKey, KEYEVENTF_KEYUP)
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT CreateVirtualKeyInput(ushort virtualKey, uint flags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = flags,
                        dwExtraInfo = IntPtr.Zero,
                        time = 0
                    }
                }
            };
        }

        private static INPUT CreateUnicodeInput(char character, uint flags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = character,
                        dwFlags = flags,
                        dwExtraInfo = IntPtr.Zero,
                        time = 0
                    }
                }
            };
        }

        private static bool IsModifierKey(int vkCode)
        {
            return vkCode == VK_SHIFT ||
                   vkCode == VK_LSHIFT ||
                   vkCode == VK_RSHIFT ||
                   vkCode == VK_CONTROL ||
                   vkCode == VK_LCONTROL ||
                   vkCode == VK_RCONTROL ||
                   vkCode == VK_MENU ||
                   vkCode == VK_LMENU ||
                   vkCode == VK_RMENU ||
                   vkCode == VK_CAPITAL;
        }

        private static string GetProcessName(IntPtr foregroundWindow)
        {
            if (foregroundWindow == IntPtr.Zero)
            {
                return string.Empty;
            }

            GetWindowThreadProcessId(foregroundWindow, out uint processId);
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

        private char GetCharFromKey(int vkCode, bool isEnglishKeyboardLayout)
        {
            bool isShiftPressed = IsKeyPressed(VK_SHIFT) || IsKeyPressed(VK_LSHIFT) || IsKeyPressed(VK_RSHIFT);
            bool isCapsLockEnabled = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;

            if (vkCode >= (int)Keys.A && vkCode <= (int)Keys.Z)
            {
                bool useUpperCase = isShiftPressed ^ isCapsLockEnabled;
                return GetMappedChar(vkCode, isEnglishKeyboardLayout, useUpperCase);
            }

            return GetMappedChar(vkCode, isEnglishKeyboardLayout, isShiftPressed);
        }

        private static char GetMappedChar(int vkCode, bool isEnglishKeyboardLayout, bool useUpperCase)
        {
            Dictionary<int, char> map = isEnglishKeyboardLayout
                ? (useUpperCase ? englishUpperMap : englishLowerMap)
                : (useUpperCase ? ukrainianUpperMap : ukrainianLowerMap);

            if (map.TryGetValue(vkCode, out char character))
            {
                return character;
            }

            return '\0';
        }

        public void Dispose()
        {
            Stop();
        }

        // WinAPI functions and constants.
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int INPUT_KEYBOARD = 1;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const uint LLKHF_INJECTED = 0x00000010;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_SCANCODE = 0x0008;
        private const int VK_BACK = 0x08;
        private const int VK_TAB = 0x09;
        private const int VK_RETURN = 0x0D;
        private const int VK_SPACE = 0x20;
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_CAPITAL = 0x14;
        private const int VK_ESCAPE = 0x1B;
        private const int VK_PRIOR = 0x21;
        private const int VK_NEXT = 0x22;
        private const int VK_END = 0x23;
        private const int VK_HOME = 0x24;
        private const int VK_LEFT = 0x25;
        private const int VK_UP = 0x26;
        private const int VK_RIGHT = 0x27;
        private const int VK_DOWN = 0x28;
        private const int VK_DELETE = 0x2E;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        private const int VK_OEM_1 = 0xBA;
        private const int VK_OEM_PLUS = 0xBB;
        private const int VK_OEM_COMMA = 0xBC;
        private const int VK_OEM_MINUS = 0xBD;
        private const int VK_OEM_PERIOD = 0xBE;
        private const int VK_OEM_2 = 0xBF;
        private const int VK_OEM_3 = 0xC0;
        private const int VK_OEM_4 = 0xDB;
        private const int VK_OEM_5 = 0xDC;
        private const int VK_OEM_6 = 0xDD;
        private const int VK_OEM_7 = 0xDE;

        [StructLayout(LayoutKind.Sequential)]
        private struct KbdLlHookStruct
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private static bool IsKeyPressed(int virtualKey)
        {
            return (GetKeyState(virtualKey) & 0x8000) != 0;
        }

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
                [VK_OEM_4] = '[', [VK_OEM_6] = ']', [VK_OEM_1] = ';', [VK_OEM_7] = '\'',
                [VK_OEM_COMMA] = ',', [VK_OEM_PERIOD] = '.', [VK_SPACE] = ' '
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
                [VK_OEM_4] = '{', [VK_OEM_6] = '}', [VK_OEM_1] = ':', [VK_OEM_7] = '"',
                [VK_OEM_COMMA] = '<', [VK_OEM_PERIOD] = '>', [VK_SPACE] = ' '
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
                [VK_OEM_4] = 'х', [VK_OEM_6] = 'ї', [VK_OEM_1] = 'ж', [VK_OEM_7] = 'є',
                [VK_OEM_COMMA] = 'б', [VK_OEM_PERIOD] = 'ю', [VK_SPACE] = ' '
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
                [VK_OEM_4] = 'Х', [VK_OEM_6] = 'Ї', [VK_OEM_1] = 'Ж', [VK_OEM_7] = 'Є',
                [VK_OEM_COMMA] = 'Б', [VK_OEM_PERIOD] = 'Ю', [VK_SPACE] = ' '
            };
        }

        private static Dictionary<char, Tuple<ushort, bool>> BuildCharToVirtualKeyMap()
        {
            var map = new Dictionary<char, Tuple<ushort, bool>>();
            foreach (var kvp in englishLowerMap) map[kvp.Value] = Tuple.Create((ushort)kvp.Key, false);
            foreach (var kvp in englishUpperMap) map[kvp.Value] = Tuple.Create((ushort)kvp.Key, true);
            foreach (var kvp in ukrainianLowerMap) map[kvp.Value] = Tuple.Create((ushort)kvp.Key, false);
            foreach (var kvp in ukrainianUpperMap) map[kvp.Value] = Tuple.Create((ushort)kvp.Key, true);
            return map;
        }

        private static void SendVirtualKeyText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            foreach (char ch in text)
            {
                if (charToVirtualKeyMap.TryGetValue(ch, out var vkInfo))
                {
                    ushort vk = vkInfo.Item1;
                    bool shift = vkInfo.Item2;

                    if (shift)
                    {
                        INPUT[] shiftDown = new INPUT[] { CreateVirtualKeyInput(VK_SHIFT, 0) };
                        SendInput((uint)shiftDown.Length, shiftDown, Marshal.SizeOf(typeof(INPUT)));
                    }

                    SendVirtualKey(vk);

                    if (shift)
                    {
                        INPUT[] shiftUp = new INPUT[] { CreateVirtualKeyInput(VK_SHIFT, KEYEVENTF_KEYUP) };
                        SendInput((uint)shiftUp.Length, shiftUp, Marshal.SizeOf(typeof(INPUT)));
                    }
                }
                else
                {
                    SendUnicodeText(ch.ToString());
                }
            }
        }
    }
}

