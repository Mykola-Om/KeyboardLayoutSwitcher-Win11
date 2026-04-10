using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program {
    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT { public uint uMsg; public ushort wParamL; public ushort wParamH; }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT { public int type; public InputUnion U; }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; [FieldOffset(0)] public HARDWAREINPUT hi; }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    static void Main() {
        string text = "Тест!";
        INPUT[] inputs = new INPUT[text.Length * 2];
        for (int i = 0; i < text.Length; i++) {
            inputs[i * 2] = new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = text[i], dwFlags = 0x0004, time = 0, dwExtraInfo = IntPtr.Zero } } };
            inputs[i * 2 + 1] = new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = text[i], dwFlags = 0x0004 | 0x0002, time = 0, dwExtraInfo = IntPtr.Zero } } };
        }
        Thread.Sleep(500);
        uint res = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}
