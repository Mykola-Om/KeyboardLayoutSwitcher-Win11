using System;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Unit tests for AppSettings list parsing (ProcessNames/IgnoredWords).
    /// </summary>
    public class AppSettingsTests : TestBase
    {
        public void RunAllTests()
        {
            Console.WriteLine("=== AppSettings Tests ===\n");

            TestProcessNamesSplitsOnAllDelimiters();
            TestProcessNamesNormalizesExeSuffixAndCase();
            TestIgnoredWordsSplitsOnAllDelimiters();
            TestProcessNamesCacheInvalidatesOnTextChange();
            TestLayoutRulesParsing();
            TestLayoutRulesIgnoreMalformedEntries();
            TestLayoutRulesRespectEnableFlag();
            TestEnterCorrectionSkipList();

            Console.WriteLine("\n✓ All AppSettings tests passed!");
        }

        private void TestEnterCorrectionSkipList()
        {
            var settings = new AppSettings();

            // Браузери мають потрапляти в список без будь-якого налаштування:
            // саме там Enter підтверджує автодоповнення адресного рядка.
            Assert(settings.IsEnterCorrectionSkipped("chrome"), "Expected chrome to skip Enter correction by default");
            Assert(settings.IsEnterCorrectionSkipped("Chrome.exe"), "Expected .exe suffix and casing to be normalized");
            Assert(settings.IsEnterCorrectionSkipped("firefox"), "Expected firefox to be in the default list");
            Assert(!settings.IsEnterCorrectionSkipped("notepad"), "Expected Enter correction to stay active in other apps");

            settings.SkipEnterCorrection = false;
            Assert(!settings.IsEnterCorrectionSkipped("chrome"), "Expected the flag to disable the whole list");

            settings.SkipEnterCorrection = true;
            settings.SkipEnterCorrectionProcessesText = "telegram";
            Assert(!settings.IsEnterCorrectionSkipped("chrome"), "Expected the list to be replaced, not merged");
            Assert(settings.IsEnterCorrectionSkipped("telegram"), "Expected the custom entry to apply");

            Console.WriteLine("✓ Enter-correction skip list works");
        }

        private void TestLayoutRulesParsing()
        {
            var settings = new AppSettings { LayoutRulesText = "telegram=uk\nCode.exe=en;Notepad=UK" };

            Assert(settings.GetDesiredLayoutIsEnglish("telegram") == false, "Expected telegram -> Ukrainian");
            Assert(settings.GetDesiredLayoutIsEnglish("code") == true, "Expected code -> English (.exe suffix stripped)");
            Assert(settings.GetDesiredLayoutIsEnglish("NOTEPAD") == false, "Expected case-insensitive match for notepad");
            Assert(settings.GetDesiredLayoutIsEnglish("chrome") == null, "Expected no rule for chrome");

            Console.WriteLine("✓ LayoutRules parse process names and layout tags");
        }

        private void TestLayoutRulesIgnoreMalformedEntries()
        {
            // A name with no layout, an unknown tag, and a missing name must all be skipped
            // rather than producing a rule that silently forces the wrong layout.
            var settings = new AppSettings { LayoutRulesText = "telegram\nchrome=de\n=uk\nslack=en" };

            Assert(settings.GetDesiredLayoutIsEnglish("telegram") == null, "Expected entry without a layout to be ignored");
            Assert(settings.GetDesiredLayoutIsEnglish("chrome") == null, "Expected unknown layout tag to be ignored");
            Assert(settings.LayoutRules.Count == 1, $"Expected only the valid rule to survive, got {settings.LayoutRules.Count}");
            Assert(settings.GetDesiredLayoutIsEnglish("slack") == true, "Expected the valid rule to still parse");

            Console.WriteLine("✓ LayoutRules skip malformed entries");
        }

        private void TestLayoutRulesRespectEnableFlag()
        {
            var settings = new AppSettings { LayoutRulesText = "telegram=uk", EnableLayoutRules = false };

            Assert(settings.GetDesiredLayoutIsEnglish("telegram") == null, "Expected rules to be inert while disabled");

            settings.EnableLayoutRules = true;
            Assert(settings.GetDesiredLayoutIsEnglish("telegram") == false, "Expected rules to apply once enabled");

            Console.WriteLine("✓ LayoutRules honour the enable flag");
        }

        private void TestProcessNamesSplitsOnAllDelimiters()
        {
            // Regression test: ProcessNames/IgnoredWords must split on the same delimiter
            // set (AppSettings.ListDelimiters) that the UI uses to populate its list boxes,
            // otherwise a ';'-separated entry saved in settings.xml would parse here but
            // silently fail to show up in the UI (or vice versa).
            var settings = new AppSettings { ProcessFilterText = "chrome\nfirefox,notepad;cmd" };

            Assert(settings.ProcessNames.Contains("chrome"), "Expected 'chrome' to be parsed");
            Assert(settings.ProcessNames.Contains("firefox"), "Expected 'firefox' to be parsed");
            Assert(settings.ProcessNames.Contains("notepad"), "Expected 'notepad' to be parsed");
            Assert(settings.ProcessNames.Contains("cmd"), "Expected ';'-separated 'cmd' to be parsed");
            Assert(settings.ProcessNames.Count == 4, $"Expected 4 process names, got {settings.ProcessNames.Count}");

            Console.WriteLine("✓ ProcessNames splits on \\r, \\n, ',', ';'");
        }

        private void TestProcessNamesNormalizesExeSuffixAndCase()
        {
            var settings = new AppSettings { ProcessFilterText = "Chrome.EXE" };

            Assert(settings.ProcessNames.Contains("chrome"), "Expected 'Chrome.EXE' to normalize to 'chrome'");

            Console.WriteLine("✓ ProcessNames normalizes .exe suffix and casing");
        }

        private void TestIgnoredWordsSplitsOnAllDelimiters()
        {
            var settings = new AppSettings { IgnoredWordsText = "скрін\nфільтр,інфо;тест" };

            Assert(settings.IgnoredWords.Contains("скрін"), "Expected 'скрін' to be parsed");
            Assert(settings.IgnoredWords.Contains("фільтр"), "Expected 'фільтр' to be parsed");
            Assert(settings.IgnoredWords.Contains("інфо"), "Expected 'інфо' to be parsed");
            Assert(settings.IgnoredWords.Contains("тест"), "Expected ';'-separated 'тест' to be parsed");

            Console.WriteLine("✓ IgnoredWords splits on \\r, \\n, ',', ';'");
        }

        private void TestProcessNamesCacheInvalidatesOnTextChange()
        {
            var settings = new AppSettings { ProcessFilterText = "chrome" };
            Assert(settings.ProcessNames.Contains("chrome"), "Expected initial 'chrome' to be parsed");

            settings.ProcessFilterText = "firefox";
            Assert(!settings.ProcessNames.Contains("chrome"), "Expected stale 'chrome' to be gone after text change");
            Assert(settings.ProcessNames.Contains("firefox"), "Expected new 'firefox' to be parsed");

            Console.WriteLine("✓ ProcessNames cache invalidates when ProcessFilterText changes");
        }
    }
}
