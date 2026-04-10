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
        private string processFilterText = string.Empty;
        private string ignoredWordsText = string.Empty;

        public bool IsSwitchingEnabled { get; set; } = true;

        public bool StartWithWindows { get; set; }

        public ProcessFilterMode ProcessFilterMode { get; set; } = ProcessFilterMode.Disabled;

        public string ProcessFilterText
        {
            get { return processFilterText; }
            set { processFilterText = value ?? string.Empty; }
        }

        public string IgnoredWordsText
        {
            get { return ignoredWordsText; }
            set { ignoredWordsText = value ?? string.Empty; }
        }

        public int MinimumWordLength { get; set; } = 2;

        public int MinimumMappedPercent { get; set; } = 80;

        public int MinimumVowelDelta { get; set; } = 0;

        [XmlIgnore]
        public IReadOnlyCollection<string> ProcessNames
        {
            get
            {
                return ProcessFilterText
                    .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(NormalizeProcessName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        [XmlIgnore]
        public IReadOnlyCollection<string> IgnoredWords
        {
            get
            {
                return IgnoredWordsText
                    .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word.Trim())
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        public bool IsProcessAllowed(string processName)
        {
            if (ProcessFilterMode == ProcessFilterMode.Disabled)
            {
                return true;
            }

            string normalizedProcessName = NormalizeProcessName(processName);
            bool isListed = new HashSet<string>(ProcessNames, StringComparer.OrdinalIgnoreCase)
                .Contains(normalizedProcessName);

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