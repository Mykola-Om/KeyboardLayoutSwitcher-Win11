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

            Console.WriteLine("\n✓ All AppSettings tests passed!");
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
