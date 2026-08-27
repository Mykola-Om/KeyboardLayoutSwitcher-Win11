using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyboardLayoutSwitcher
{
    public class InputReplacer
    {
        // Час, який даємо Windows застосувати WM_INPUTLANGCHANGEREQUEST перед відправкою
        // замінюючих натискань. Без затримки SendInput інколи випереджає фактичну зміну
        // розкладки, і символи йдуть у старій розкладці.
        private const int LayoutSwitchSettleDelayMs = 10;

        private readonly SynchronizationContext synchronizationContext;
        private bool isReplacing;

        public InputReplacer()
        {
            synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public bool IsReplacing => isReplacing;

        public void QueueReplacement(int originalLength, string correctedWord, char boundaryChar, bool oldLayout)
        {
            QueueReplacement(originalLength, correctedWord, boundaryChar, oldLayout, switchLayout: true);
        }

        /// <summary>
        /// Замінює слово, не чіпаючи розкладку. Використовується для виправлень у межах
        /// однієї мови (напр. відновлення апострофа), де перемикати розкладку не треба.
        /// </summary>
        public void QueueSameLayoutReplacement(int originalLength, string correctedWord, char boundaryChar)
        {
            QueueReplacement(originalLength, correctedWord, boundaryChar, oldLayout: false, switchLayout: false);
        }

        private void QueueReplacement(int originalLength, string correctedWord, char boundaryChar, bool oldLayout, bool switchLayout)
        {
            isReplacing = true;
            synchronizationContext.Post(_ =>
            {
                try
                {
                    if (switchLayout)
                    {
                        bool dummyLayout = oldLayout;
                        LayoutSwitcher.SwitchKeyboardLayout(ref dummyLayout);

                        Thread.Sleep(LayoutSwitchSettleDelayMs);
                    }

                    List<Win32Interop.INPUT> inputs = new List<Win32Interop.INPUT>();

                    for (int i = 0; i < originalLength; i++)
                    {
                        inputs.Add(CreateKeyInput(Win32Interop.VK_BACK, false));
                        inputs.Add(CreateKeyInput(Win32Interop.VK_BACK, true));
                    }

                    foreach (char c in correctedWord)
                    {
                        inputs.AddRange(CreateUnicodeInput(c));
                    }

                    if (boundaryChar == '\r' || boundaryChar == '\n')
                    {
                        inputs.Add(CreateKeyInput(Win32Interop.VK_RETURN, false));
                        inputs.Add(CreateKeyInput(Win32Interop.VK_RETURN, true));
                    }
                    else if (boundaryChar == '\t')
                    {
                        inputs.Add(CreateKeyInput(Win32Interop.VK_TAB, false));
                        inputs.Add(CreateKeyInput(Win32Interop.VK_TAB, true));
                    }
                    else if (boundaryChar != '\0' && boundaryChar != '\u0001')
                    {
                        inputs.AddRange(CreateUnicodeInput(boundaryChar));
                    }

                    Win32Interop.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(Win32Interop.INPUT)));
                }
                catch (Exception e)
                {
                    TraceLogger.Trace($"Error in replacement: {e.Message}");
                }
                finally
                {
                    isReplacing = false;
                }
            }, null);
        }

        public void QueueUndo(int backspaceCount, string originalWord, bool targetLayoutIsEnglish)
        {
            QueueUndo(backspaceCount, originalWord, targetLayoutIsEnglish, switchLayout: true);
        }

        /// <summary>
        /// Скасування виправлення, зробленого без перемикання розкладки.
        /// </summary>
        public void QueueSameLayoutUndo(int backspaceCount, string originalWord)
        {
            QueueUndo(backspaceCount, originalWord, targetLayoutIsEnglish: false, switchLayout: false);
        }

        private void QueueUndo(int backspaceCount, string originalWord, bool targetLayoutIsEnglish, bool switchLayout)
        {
            isReplacing = true;
            synchronizationContext.Post(_ =>
            {
                try
                {
                    if (switchLayout)
                    {
                        bool switchLayoutState = !targetLayoutIsEnglish;
                        LayoutSwitcher.SwitchKeyboardLayout(ref switchLayoutState);

                        Thread.Sleep(LayoutSwitchSettleDelayMs);
                    }

                    List<Win32Interop.INPUT> inputs = new List<Win32Interop.INPUT>();

                    for (int i = 0; i < backspaceCount; i++)
                    {
                        inputs.Add(CreateKeyInput(Win32Interop.VK_BACK, false));
                        inputs.Add(CreateKeyInput(Win32Interop.VK_BACK, true));
                    }

                    foreach (char c in originalWord)
                    {
                        inputs.AddRange(CreateUnicodeInput(c));
                    }

                    Win32Interop.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(Win32Interop.INPUT)));
                }
                catch (Exception e)
                {
                    TraceLogger.Trace($"Error in undo: {e.Message}");
                }
                finally
                {
                    isReplacing = false;
                }
            }, null);
        }

        private static Win32Interop.INPUT CreateKeyInput(int wVk, bool isKeyUp)
        {
            return new Win32Interop.INPUT
            {
                type = Win32Interop.INPUT_KEYBOARD,
                u = new Win32Interop.InputUnion
                {
                    ki = new Win32Interop.KEYBDINPUT
                    {
                        wVk = (ushort)wVk,
                        dwFlags = isKeyUp ? Win32Interop.KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        private static Win32Interop.INPUT[] CreateUnicodeInput(char c)
        {
            Win32Interop.INPUT down = new Win32Interop.INPUT
            {
                type = Win32Interop.INPUT_KEYBOARD,
                u = new Win32Interop.InputUnion
                {
                    ki = new Win32Interop.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = Win32Interop.KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            Win32Interop.INPUT up = new Win32Interop.INPUT
            {
                type = Win32Interop.INPUT_KEYBOARD,
                u = new Win32Interop.InputUnion
                {
                    ki = new Win32Interop.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = Win32Interop.KEYEVENTF_UNICODE | Win32Interop.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            return new[] { down, up };
        }
    }
}
