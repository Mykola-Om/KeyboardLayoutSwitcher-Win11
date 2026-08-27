using System;
using KeyboardLayoutSwitcher;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Unit tests for KeyMapper heuristics.
    /// </summary>
    public class KeyMapperTests : TestBase
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
            TestClearCacheInvalidatesStaleIgnoredWord();
            TestMixedLayout();
            TestExcludeDotEnvInEnglishLayout();
            TestConvertDotEnvInUkrainianLayout();
            TestExcludeUnderscoreInEnglishLayout();
            TestConvertUnderscoreInUkrainianLayout();
            TestExcludeAllCapsInEnglishLayout();
            TestExcludeShortUkrainianAbbreviations();
            TestConvertGarbledAllCapsUkrainianLayout();
            TestExcludeAlphanumericMixed();
            TestExcludeCamelCaseSuffixInEnglish();
            TestConvertCamelCasePrefixInUkrainian();
            TestExcludePathSlashes();
            TestKeepUkrainianWordsWithBalancedVowels();
            TestStillConvertsMistypedUkrainianWords();
            TestCacheKeepsBoundaryContextSeparate();
            TestRestoresMissingApostrophe();
            TestRestoreApostrophePreservesCase();
            TestDoesNotTouchWordsThatNeedNoApostrophe();
            TestApostropheIsPartOfUkrainianWord();

            Console.WriteLine("\n✓ All tests completed!");
        }

        /// <summary>
        /// These Ukrainian words convert to Latin strings with the same vowel count
        /// ("дрон" -> "lhjy", "прогін" -> "ghjusy"), so the vowel tiebreaker alone used to
        /// flip them. The implausible-bigram penalty plus dictionary coverage must keep them.
        /// </summary>
        private void TestKeepUkrainianWordsWithBalancedVowels()
        {
            string[] words = { "дрон", "прогін", "прогону", "налаштування", "кеш", "кешування", "денний" };

            foreach (string word in words)
            {
                bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: false, settings);
                Assert(!result, $"Expected Ukrainian '{word}' to be left alone, but it was converted to '{KeyMapper.ConvertWord(word, false)}'");
            }

            Console.WriteLine("✓ Ukrainian words with balanced vowels are not converted");
        }

        /// <summary>
        /// The mirror case of <see cref="TestKeepUkrainianWordsWithBalancedVowels"/>: the same
        /// words actually mistyped on the English layout must still be corrected.
        /// </summary>
        private void TestStillConvertsMistypedUkrainianWords()
        {
            string[] words = { "lhjy", "ghjusy", "ghjujye", "yfkfinedfyyz", "rti", "rtiedfyyz", "ltyybq" };

            foreach (string word in words)
            {
                bool result = KeyMapper.IsWrongLayout(word, isEnglishLayout: true, settings);
                Assert(result, $"Expected mistyped '{word}' (should be '{KeyMapper.ConvertWord(word, true)}') to be detected as wrong layout");
            }

            Console.WriteLine("✓ Mistyped Ukrainian words are still corrected");
        }

        /// <summary>
        /// The verdict depends on the surrounding boundary characters, so the cache must key
        /// on them too. Otherwise whichever context was seen first wins for every later one:
        /// a plain word would poison ".word", and vice versa.
        /// </summary>
        private void TestCacheKeepsBoundaryContextSeparate()
        {
            KeyMapper.ClearCache();
            bool plainFirst = KeyMapper.IsWrongLayout("cnfnec", isEnglishLayout: true, settings);
            bool dottedAfter = KeyMapper.IsWrongLayout("cnfnec", true, settings, '\0', '.');

            Assert(plainFirst, "Expected plain 'cnfnec' to be detected as wrong layout");
            Assert(!dottedAfter, "Expected '.cnfnec' to stay untouched even after the plain word was cached");

            // Той самий сценарій у зворотному порядку.
            KeyMapper.ClearCache();
            bool dottedFirst = KeyMapper.IsWrongLayout("cnfnec", true, settings, '\0', '.');
            bool plainAfter = KeyMapper.IsWrongLayout("cnfnec", isEnglishLayout: true, settings);

            Assert(!dottedFirst, "Expected '.cnfnec' to be left alone");
            Assert(plainAfter, "Expected plain 'cnfnec' to still be corrected after the dotted form was cached");

            Console.WriteLine("✓ Cache keeps boundary contexts apart");
        }

        private void TestRestoresMissingApostrophe()
        {
            var expected = new System.Collections.Generic.Dictionary<string, string>
            {
                ["память"] = "пам'ять",
                ["компютер"] = "комп'ютер",
                ["обєкт"] = "об'єкт",
                ["сімя"] = "сім'я",
                ["здоровя"] = "здоров'я",
                ["зїзд"] = "з'їзд",
                ["мяч"] = "м'яч",
                ["пятниця"] = "п'ятниця",
            };

            foreach (var pair in expected)
            {
                bool restored = KeyMapper.TryRestoreApostrophe(pair.Key, out string corrected);
                Assert(restored, $"Expected '{pair.Key}' to get an apostrophe back");
                Assert(corrected == pair.Value, $"Expected '{pair.Key}' -> '{pair.Value}', got '{corrected}'");
            }

            Console.WriteLine("✓ Missing apostrophes are restored");
        }

        private void TestRestoreApostrophePreservesCase()
        {
            KeyMapper.TryRestoreApostrophe("Память", out string capitalized);
            Assert(capitalized == "Пам'ять", $"Expected 'Пам'ять', got '{capitalized}'");

            KeyMapper.TryRestoreApostrophe("ПАМЯТЬ", out string upper);
            Assert(upper == "ПАМ'ЯТЬ", $"Expected 'ПАМ'ЯТЬ', got '{upper}'");

            Console.WriteLine("✓ Apostrophe restoration preserves letter case");
        }

        /// <summary>
        /// These are correctly spelled without an apostrophe, so a naive
        /// "б/п/в/м/ф + я/ю/є/ї" rule would corrupt them.
        /// </summary>
        private void TestDoesNotTouchWordsThatNeedNoApostrophe()
        {
            string[] words = { "свято", "цвях", "морквяний", "духмяний", "різдвяний", "тьмяний", "пам'ять", "комп'ютер" };

            foreach (string word in words)
            {
                bool restored = KeyMapper.TryRestoreApostrophe(word, out string corrected);
                Assert(!restored, $"Expected '{word}' to be left alone, but it became '{corrected}'");
            }

            Console.WriteLine("✓ Words that need no apostrophe are left alone");
        }

        /// <summary>
        /// The apostrophe must be tracked as part of the word in the Ukrainian layout,
        /// otherwise "пам'ять" is seen as two separate fragments.
        /// </summary>
        private void TestApostropheIsPartOfUkrainianWord()
        {
            Assert(KeyMapper.IsLayoutWordCharacter('\'', isEnglishLayout: false),
                "Expected the apostrophe to count as a word character in the Ukrainian layout");

            Assert(!KeyMapper.IsWrongLayout("пам'ять", isEnglishLayout: false, settings),
                "Expected \"пам'ять\" to be recognized as a valid Ukrainian word");

            Console.WriteLine("✓ Apostrophe is part of the word in the Ukrainian layout");
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

        private void TestClearCacheInvalidatesStaleIgnoredWord()
        {
            // Reproduces the scenario ClearCache() exists for: a word gets auto-corrected
            // once (and cached), the user then adds it to the ignore list, and the app must
            // stop auto-correcting it without requiring a restart.
            const string englishWord = "react"; // hardcoded in commonEnglishWords, unused elsewhere
            string garbled = KeyMapper.ConvertWord(englishWord, isEnglishLayout: true);

            settings.IgnoredWordsText = string.Empty;
            bool beforeIgnoring = KeyMapper.IsWrongLayout(garbled, isEnglishLayout: false, settings);
            Assert(beforeIgnoring, $"Expected '{garbled}' to be flagged as wrong layout before being ignored");

            // Simulate the user adding the word to the ignore list after it was already cached.
            settings.IgnoredWordsText = garbled;
            bool staleResult = KeyMapper.IsWrongLayout(garbled, isEnglishLayout: false, settings);
            Assert(staleResult, $"Expected stale cache to still report '{garbled}' as wrong layout before ClearCache()");

            KeyMapper.ClearCache();
            bool freshResult = KeyMapper.IsWrongLayout(garbled, isEnglishLayout: false, settings);
            Assert(!freshResult, $"Expected '{garbled}' to be skipped after ClearCache() re-evaluates the ignore list");

            Console.WriteLine("✓ ClearCache invalidates stale ignore-list results");
        }

        private void TestMixedLayout()
        {
            // Mixed English/Ukrainian characters should be detected more carefully
            string mixedWord = "rkjnf";
            bool result = KeyMapper.IsWrongLayout(mixedWord, isEnglishLayout: true, settings);

            // This tests the unnaturalness scoring (consonant clusters, etc.)
            Console.WriteLine($"✓ Mixed/garbled layout detection (result: {result})");
        }

        private void TestExcludeDotEnvInEnglishLayout()
        {
            bool result = KeyMapper.IsWrongLayout("env", isEnglishLayout: true, settings: settings, boundaryChar: ' ', lastBoundaryChar: '.');
            Assert(!result, "Expected 'env' preceded by '.' in English layout to be skipped.");
            Console.WriteLine("✓ Exclude dot-prefixed env in English layout");
        }

        private void TestConvertDotEnvInUkrainianLayout()
        {
            bool result = KeyMapper.IsWrongLayout("уні", isEnglishLayout: false, settings: settings, boundaryChar: ' ', lastBoundaryChar: '.');
            Assert(result, "Expected 'уні' preceded by '.' in Ukrainian layout to be corrected.");
            Console.WriteLine("✓ Convert dot-prefixed env in Ukrainian layout");
        }

        private void TestExcludeUnderscoreInEnglishLayout()
        {
            bool result1 = KeyMapper.IsWrongLayout("DATABASE", isEnglishLayout: true, settings: settings, boundaryChar: '_', lastBoundaryChar: ' ');
            bool result2 = KeyMapper.IsWrongLayout("URL", isEnglishLayout: true, settings: settings, boundaryChar: ' ', lastBoundaryChar: '_');
            Assert(!result1 && !result2, "Expected underscore-adjacent words in English layout to be skipped.");
            Console.WriteLine("✓ Exclude underscore-adjacent words in English layout");
        }

        private void TestConvertUnderscoreInUkrainianLayout()
        {
            bool result1 = KeyMapper.IsWrongLayout("ВФИФІФІУ", isEnglishLayout: false, settings: settings, boundaryChar: '_', lastBoundaryChar: ' ');
            bool result2 = KeyMapper.IsWrongLayout("ГКД", isEnglishLayout: false, settings: settings, boundaryChar: ' ', lastBoundaryChar: '_');
            Assert(result1 && result2, "Expected underscore-adjacent words in Ukrainian layout to be corrected.");
            Console.WriteLine("✓ Convert underscore-adjacent words in Ukrainian layout");
        }

        private void TestExcludeAllCapsInEnglishLayout()
        {
            bool result = KeyMapper.IsWrongLayout("PORT", isEnglishLayout: true, settings: settings);
            Assert(!result, "Expected ALL_CAPS words in English layout to be skipped.");
            Console.WriteLine("✓ Exclude ALL_CAPS words in English layout");
        }

        private void TestExcludeShortUkrainianAbbreviations()
        {
            bool result1 = KeyMapper.IsWrongLayout("ФОП", isEnglishLayout: false, settings: settings);
            bool result2 = KeyMapper.IsWrongLayout("ТОВ", isEnglishLayout: false, settings: settings);
            Assert(!result1 && !result2, "Expected short Ukrainian abbreviations (ФОП, ТОВ) to be skipped.");
            Console.WriteLine("✓ Exclude short Ukrainian abbreviations");
        }

        private void TestConvertGarbledAllCapsUkrainianLayout()
        {
            bool result = KeyMapper.IsWrongLayout("РУДДЩ", isEnglishLayout: false, settings: settings);
            Assert(result, "Expected long ALL_CAPS Ukrainian mistake to be corrected.");
            Console.WriteLine("✓ Convert long ALL_CAPS Ukrainian mistake");
        }

        private void TestExcludeAlphanumericMixed()
        {
            bool result1 = KeyMapper.IsWrongLayout("oauth2", isEnglishLayout: true, settings: settings);
            bool result2 = KeyMapper.IsWrongLayout("v1", isEnglishLayout: true, settings: settings);
            bool result3 = KeyMapper.IsWrongLayout("щффер2", isEnglishLayout: false, settings: settings);
            Assert(!result1 && !result2 && !result3, "Expected alphanumeric mixed words to be skipped.");
            Console.WriteLine("✓ Exclude alphanumeric mixed words");
        }

        private void TestExcludeCamelCaseSuffixInEnglish()
        {
            bool result = KeyMapper.IsWrongLayout("Database", isEnglishLayout: true, settings: settings, boundaryChar: ' ', lastBoundaryChar: '\u0001');
            Assert(!result, "Expected camelCase suffix in English layout to be skipped.");
            Console.WriteLine("✓ Exclude camelCase suffix in English layout");
        }

        private void TestConvertCamelCasePrefixInUkrainian()
        {
            bool result = KeyMapper.IsWrongLayout("пуе", isEnglishLayout: false, settings: settings, boundaryChar: '\u0001', lastBoundaryChar: ' ');
            Assert(result, "Expected camelCase prefix in Ukrainian layout to be corrected.");
            Console.WriteLine("✓ Convert camelCase prefix in Ukrainian layout");
        }

        private void TestExcludePathSlashes()
        {
            bool result1 = KeyMapper.IsWrongLayout("src", isEnglishLayout: true, settings: settings, boundaryChar: '/', lastBoundaryChar: ' ');
            bool result2 = KeyMapper.IsWrongLayout("components", isEnglishLayout: true, settings: settings, boundaryChar: ' ', lastBoundaryChar: '/');
            bool result3 = KeyMapper.IsWrongLayout("https", isEnglishLayout: true, settings: settings, boundaryChar: '/', lastBoundaryChar: ' ');
            Assert(!result1 && !result2 && !result3, "Expected slash-adjacent words in English layout to be skipped.");
            Console.WriteLine("✓ Exclude slash-adjacent paths/URLs in English layout");
        }
    }
}
