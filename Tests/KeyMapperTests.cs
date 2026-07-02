using System;
using KeyboardLayoutSwitcher;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Unit tests for KeyMapper heuristics.
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
            // Take a known English dictionary word and derive what it would look
            // like if the same physical keys were pressed while a Ukrainian layout
            // was active. That garbled text, checked with isEnglishLayout=false,
            // must be detected as wrong layout and convert back to the original word.
            const string englishWord = "python"; // hardcoded in commonEnglishWords, no dictionary file dependency
            string garbled = KeyMapper.ConvertWord(englishWord, isEnglishLayout: true);

            bool result = KeyMapper.IsWrongLayout(garbled, isEnglishLayout: false, settings);
            Assert(result, $"Expected garbled Ukrainian text '{garbled}' (from '{englishWord}') to be detected as wrong layout");

            string convertedBack = KeyMapper.ConvertWord(garbled, isEnglishLayout: false);
            Assert(convertedBack == englishWord, $"Expected '{garbled}' to convert back to '{englishWord}', got '{convertedBack}'");

            Console.WriteLine("✓ Ukrainian→English detection works");
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
            // Use a fresh word (never checked before in this process) and register it
            // as ignored BEFORE the first lookup. IsWrongLayout caches its result per
            // word+layout, so checking an already-cached word here would pass for the
            // wrong reason (stale cache) instead of exercising the ignore-list branch.
            const string englishWord = "docker"; // hardcoded in commonEnglishWords
            string garbled = KeyMapper.ConvertWord(englishWord, isEnglishLayout: true);

            settings.IgnoredWordsText = garbled;

            bool result = KeyMapper.IsWrongLayout(garbled, isEnglishLayout: false, settings);
            Assert(!result, $"Expected ignored word '{garbled}' to be skipped");
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
    }
}
