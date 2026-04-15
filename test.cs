using System;
using KeyboardLayoutSwitcher;

class Program
{
    static void Main()
    {
        var settings = new AppSettings { MinimumWordLength = 3, MinimumMappedPercent = 80, MinimumVowelDelta = 0 };
        bool b = KeyMapper.IsWrongLayout("syis", true, settings);
        Console.WriteLine("syis wrong: " + b);
        b = KeyMapper.IsWrongLayout("syisq", true, settings);
        Console.WriteLine("syisq wrong: " + b);
        
        b = KeyMapper.IsWrongLayout("іні", false, settings);
        Console.WriteLine("іні wrong: " + b);
    }
}
