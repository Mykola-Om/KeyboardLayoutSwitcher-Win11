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

        // Роздільник між іменем процесу та бажаною розкладкою в LayoutRulesText ("telegram=uk").
        public const char LayoutRuleSeparator = '=';

        // Позначки розкладок у правилах. Модель бінарна (англійська / українська),
        // як і решта застосунку.
        public const string EnglishLayoutTag = "en";
        public const string UkrainianLayoutTag = "uk";

        private string processFilterText = string.Empty;
        private string ignoredWordsText = string.Empty;
        private string layoutRulesText = string.Empty;
        private string skipEnterCorrectionProcessesText = DefaultSkipEnterCorrectionProcesses;
        private HashSet<string> cachedProcessNames;
        private HashSet<string> cachedIgnoredWords;
        private Dictionary<string, bool> cachedLayoutRules;
        private HashSet<string> cachedSkipEnterCorrectionProcesses;

        // Браузери, де Enter в адресному рядку підтверджує автодоповнення. Виправлення саме
        // на Enter там шкідливе: ми ковтаємо Enter, стираємо набране й вводимо свій варіант,
        // а браузер до того моменту вже втратив підказку і виконує пошук замість переходу.
        public const string DefaultSkipEnterCorrectionProcesses = "chrome, msedge, firefox, brave, opera, vivaldi";

        public bool IsSwitchingEnabled { get; set; } = true;

        public bool StartWithWindows { get; set; }

        public bool EnableTrace { get; set; } = false;

        // Відновлення пропущеного апострофа в українських словах ("память" -> "пам'ять").
        public bool RestoreApostrophes { get; set; } = true;

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

        // Правила "процес=розкладка", по одному в рядку ("telegram=uk", "code=en").
        public string LayoutRulesText
        {
            get { return layoutRulesText; }
            set
            {
                layoutRulesText = value ?? string.Empty;
                cachedLayoutRules = null;
            }
        }

        // Вмикає застосування LayoutRules при перемиканні активного вікна.
        public bool EnableLayoutRules { get; set; } = true;

        // Вимикає виправлення слова, коли межею є Enter, у перелічених програмах.
        public bool SkipEnterCorrection { get; set; } = true;

        public string SkipEnterCorrectionProcessesText
        {
            get { return skipEnterCorrectionProcessesText; }
            set
            {
                skipEnterCorrectionProcessesText = value ?? string.Empty;
                cachedSkipEnterCorrectionProcesses = null;
            }
        }

        [XmlIgnore]
        public HashSet<string> SkipEnterCorrectionProcesses
        {
            get
            {
                if (cachedSkipEnterCorrectionProcesses == null)
                {
                    cachedSkipEnterCorrectionProcesses = ParseList(SkipEnterCorrectionProcessesText, NormalizeProcessName);
                }
                return cachedSkipEnterCorrectionProcesses;
            }
        }

        /// <summary>
        /// Чи слід пропустити виправлення слова, коли межею стало натискання Enter.
        /// </summary>
        public bool IsEnterCorrectionSkipped(string processName)
        {
            return SkipEnterCorrection && SkipEnterCorrectionProcesses.Contains(NormalizeProcessName(processName));
        }

        /// <summary>
        /// Розібрані правила: нормалізоване ім'я процесу -> чи має бути англійська розкладка.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, bool> LayoutRules
        {
            get
            {
                if (cachedLayoutRules == null)
                {
                    cachedLayoutRules = ParseLayoutRules(LayoutRulesText);
                }
                return cachedLayoutRules;
            }
        }

        private static Dictionary<string, bool> ParseLayoutRules(string text)
        {
            var rules = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (string entry in text.Split(ListDelimiters, StringSplitOptions.RemoveEmptyEntries))
            {
                int separatorIndex = entry.IndexOf(LayoutRuleSeparator);
                if (separatorIndex <= 0)
                {
                    continue; // Рядок без розкладки — правило неповне, ігноруємо
                }

                string processName = NormalizeProcessName(entry.Substring(0, separatorIndex));
                string layoutTag = entry.Substring(separatorIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(processName))
                {
                    continue;
                }

                if (layoutTag.Equals(EnglishLayoutTag, StringComparison.OrdinalIgnoreCase))
                {
                    rules[processName] = true;
                }
                else if (layoutTag.Equals(UkrainianLayoutTag, StringComparison.OrdinalIgnoreCase))
                {
                    rules[processName] = false;
                }
            }

            return rules;
        }

        /// <summary>
        /// Бажана розкладка для процесу, або null, якщо правила для нього немає.
        /// </summary>
        public bool? GetDesiredLayoutIsEnglish(string processName)
        {
            if (!EnableLayoutRules)
            {
                return null;
            }

            if (LayoutRules.TryGetValue(NormalizeProcessName(processName), out bool isEnglish))
            {
                return isEnglish;
            }

            return null;
        }

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