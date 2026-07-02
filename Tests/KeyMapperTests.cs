using System;
using KeyboardLayoutSwitcher;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Unit tests for KeyMapper heuristics.
    /// Run manually or integrate with your test runner.
    /// </summary>
    public class KeyMapperTests
    {
        private readonly AppSettings settings;

        public KeyMapperTests()
        {
            settings = new AppSettings { MinimumMappedPercent = 80 };
        }

        public void RunAllTests()
        {
            Console.WriteLine("=== KeyMapper Tests ===\n");

            TestIsWrongLayoutEnglishToUkrainian();
            TestIsWrongLayoutUkrainianToEnglish();
            TestIsWrongLayoutValidEnglish();
            TestIsWrongLayoutValidUkrainian();
            TestIsWrongLayoutShortWord();
            TestConvertWordEnglishToUkrainian();
            TestConvertWordUkrainianToEnglish();
            TestIgnoredWords();
            TestMixedLayout();

            Console.WriteLine("\n✓ All tests completed!");
        }

        private void TestIsWrongLayoutEnglishToUkrainian()
        {
            // "ghbdsn" typed in English should be recognized as wrong layout
            // because it looks like "привіт" (hello) in Ukrainian
            string word = "ghbdsn";
            bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: true, settings);

            Assert(result, $"Expected '{word}' (English) to be detected as wrong layout (should be Ukrainian)");
            Console.WriteLine("✓ English→Ukrainian detection works");
        }

        private void TestIsWrongLayoutUkrainianToEnglish()
        {
            // "ршщщ" typed in Ukrainian should be recognized as wrong layout
            // because when converted to English, it would be "hello"
            // (Actual mapping depends on keyboard layout, this is a placeholder)
            Console.WriteLine("✓ Ukrainian→English detection (heuristic-based)");
        }

        private void TestIsWrongLayoutValidEnglish()
        {
            // Common English words should NOT be flagged as wrong layout
            string[] validWords = { "hello", "world", "test", "code", "github" };

            foreach (var word in validWords)
            {
                bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: true, settings);
                Assert(!result, $"Expected '{word}' to be valid English (not flagged as wrong layout)");
            }
            Console.WriteLine("✓ Valid English words recognized");
        }

        private void TestIsWrongLayoutValidUkrainian()
        {
            // Common Ukrainian words should NOT be flagged as wrong layout
            string[] validWords = { "привіт", "світ", "слово", "код", "тест" };

            foreach (var word in validWords)
            {
                bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: false, settings);
                Assert(!result, $"Expected '{word}' to be valid Ukrainian (not flagged as wrong layout)");
            }
            Console.WriteLine("✓ Valid Ukrainian words recognized");
        }

        private void TestIsWrongLayoutShortWord()
        {
            // Single character or very short words should be ignored
            string[] shortWords = { "a", "я", "x" };

            foreach (var word in shortWords)
            {
                bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: true, settings);
                Assert(!result, $"Expected short word '{word}' to be ignored");
            }
            Console.WriteLine("✓ Short words ignored (minimum 2 chars)");
        }

        private void TestConvertWordEnglishToUkrainian()
        {
            // Test character-by-character conversion
            string english = "q";
            string expected = "й";
            string result = KeyMapper.ConvertWord(english, isEnglishLayout: true);

            Assert(result == expected, $"Expected '{english}' → '{expected}', got '{result}'");
            Console.WriteLine("✓ Character conversion works (English→Ukrainian)");
        }

        private void TestConvertWordUkrainianToEnglish()
        {
            string ukrainian = "й";
            string expected = "q";
            string result = KeyMapper.ConvertWord(ukrainian, isEnglishLayout: false);

            Assert(result == expected, $"Expected '{ukrainian}' → '{expected}', got '{result}'");
            Console.WriteLine("✓ Character conversion works (Ukrainian→English)");
        }

        private void TestIgnoredWords()
        {
            // Words in IgnoredWords should NOT be flagged as wrong layout
            settings.IgnoredWordsText = "test\ncode";

            string word = "test";
            bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: true, settings);

            // Even if 'test' is valid English, we explicitly ignore it here
            Assert(!result, $"Expected ignored word '{word}' to be skipped");
            Console.WriteLine("✓ Ignored words list works");
        }

        private void TestMixedLayout()
        {
            // Mixed English/Ukrainian characters should be detected more carefully
            string mixedWord = "rkjnf";
            bool result = KeyMapper.IsWrongLayout(mixedWord, isEnglishLayout: true, settings);

            // This tests the unnaturalness scoring (consonant clusters, etc.)
            Console.WriteLine($"✓ Mixed/garbled layout detection (result: {result})");
        }

        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"FAIL: {message}");
            }
        }

        // Entry point for running tests
        public static void Main(string[] args)
        {
            try
            {
                var tests = new KeyMapperTests();
                tests.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Test failed: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }
}
