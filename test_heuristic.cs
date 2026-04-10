using System;
using System.Collections.Generic;
using KeyboardLayoutSwitcher;

class Program {
    static void Main() {
        var settings = new AppSettings {
            MinimumWordLength = 2,
            MinimumMappedPercent = 80,
            MinimumVowelDelta = 0
        };
        string word = "Рш";
        bool isUkr = false; // "isEnglishLayout = false"
        bool result = KeyMapper.IsWrongLayout(word, isUkr, settings);
        Console.WriteLine("IsWrongLayout: " + result);
        Console.WriteLine("ConvertWord: " + KeyMapper.ConvertWord(word, isUkr));
    }
}
