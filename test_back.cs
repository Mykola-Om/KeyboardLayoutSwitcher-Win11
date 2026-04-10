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

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    static void Main() {
        Thread.Sleep(2000);
        ushort vk = 0x08; // VK_BACK
        ushort scan = (ushort)MapVirtualKey(vk, 0);

        INPUT[] inputs = new INPUT[20];
        for (int i=0; i<10; i++) {
            inputs[i*2] = new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = 0x0008, time = 0, dwExtraInfo = IntPtr.Zero } } };
            inputs[i*2+1] = new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = 0x0008 | 0x0002, time = 0, dwExtraInfo = IntPtr.Zero } } };
        }
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        Console.WriteLine("Done");
    }
}
