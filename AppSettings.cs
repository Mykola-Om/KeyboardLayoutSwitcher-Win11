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

        public int MinimumMappedPercent { get; set; } = 80;

        [XmlIgnore]
        public HashSet<string> ProcessNames
        {
            get
            {
                if (cachedProcessNames == null)
                {
                    cachedProcessNames = new HashSet<string>(
                        ProcessFilterText
                            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(NormalizeProcessName)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase),
                        StringComparer.OrdinalIgnoreCase);
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
                    cachedIgnoredWords = new HashSet<string>(
                        IgnoredWordsText
                            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(word => word.Trim())
                            .Where(word => !string.IsNullOrWhiteSpace(word))
                            .Distinct(StringComparer.OrdinalIgnoreCase),
                        StringComparer.OrdinalIgnoreCase);
                }
                return cachedIgnoredWords;
            }
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