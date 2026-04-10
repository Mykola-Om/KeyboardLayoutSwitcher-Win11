using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

class Program {
    static void Main() {
        Thread.Sleep(2000);
        SendKeys.SendWait("{BACKSPACE 2}ну ");
    }
}
