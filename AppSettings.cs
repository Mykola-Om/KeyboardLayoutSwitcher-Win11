using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace KeyboardLayoutSwitcher
{
    public enum ProcessFilterMode
    {
        Disabled,
        Whitelist,
        Blacklist
    }

    public sealed class AppSettings
    {
        // Дефолт застосовується і як стартове значення властивості нижче, і як фолбек
        // у KeyMapper, коли settings відсутні (напр. у тестах) — тримаємо в одному місці,
        // щоб дві копії не розійшлись.
        public const int DefaultMinimumMappedPercent = 80;

        // Спільний набір розділювачів для ProcessFilterText/IgnoredWordsText — і для парсингу
        // тут, і для наповнення відповідних ListBox у MainForm, щоб обидва місця розуміли
        // текст однаково (раніше MainForm використовував інший набір без ';').
        public static readonly char[] ListDelimiters = { '\r', '\n', ',', ';' };

        private string processFilterText = string.Empty;
        private string ignoredWordsText = string.Empty;
        private HashSet<string> cachedProcessNames;
        private HashSet<string> cachedIgnoredWords;

        public bool IsSwitchingEnabled { get; set; } = true;

        public bool StartWithWindows { get; set; }

        public bool EnableTrace { get; set; } = false;

        public ProcessFilterMode ProcessFilterMode { get; set; } = ProcessFilterMode.Disabled;

        public string ProcessFilterText
        {
            get { return processFilterText; }
            set
            {
                processFilterText = value ?? string.Empty;
                cachedProcessNames = null;
            }
        }

        public string IgnoredWordsText
        {
            get { return ignoredWordsText; }
            set
            {
                ignoredWordsText = value ?? string.Empty;
                cachedIgnoredWords = null;
            }
        }

        public int MinimumMappedPercent { get; set; } = DefaultMinimumMappedPercent;

        [XmlIgnore]
        public HashSet<string> ProcessNames
        {
            get
            {
                if (cachedProcessNames == null)
                {
                    cachedProcessNames = ParseList(ProcessFilterText, NormalizeProcessName);
                }
                return cachedProcessNames;
            }
        }

        [XmlIgnore]
        public HashSet<string> IgnoredWords
        {
            get
            {
                if (cachedIgnoredWords == null)
                {
                    cachedIgnoredWords = ParseList(IgnoredWordsText, word => word.Trim());
                }
                return cachedIgnoredWords;
            }
        }

        private static HashSet<string> ParseList(string text, Func<string, string> normalize)
        {
            return new HashSet<string>(
                text.Split(ListDelimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Select(normalize)
                    .Where(item => !string.IsNullOrWhiteSpace(item)),
                StringComparer.OrdinalIgnoreCase);
        }

        public bool IsProcessAllowed(string processName)
        {
            if (ProcessFilterMode == ProcessFilterMode.Disabled)
            {
                return true;
            }

            string normalizedProcessName = NormalizeProcessName(processName);
            bool isListed = ProcessNames.Contains(normalizedProcessName);

            return ProcessFilterMode == ProcessFilterMode.Whitelist ? isListed : !isListed;
        }

        public static string NormalizeProcessName(string processName)
        {
            string normalizedValue = (processName ?? string.Empty).Trim();
            if (normalizedValue.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                normalizedValue = normalizedValue.Substring(0, normalizedValue.Length - 4);
            }

            return normalizedValue.ToLowerInvariant();
        }
    }
}