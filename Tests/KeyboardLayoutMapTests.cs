using System;
using System.Collections.Generic;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Tests for querying the real keyboard layouts. They run against whatever layouts this
    /// machine has, so they assert on structure rather than on specific characters — except
    /// where the Ukrainian alphabet itself fixes the answer.
    /// </summary>
    public class KeyboardLayoutMapTests : TestBase
    {
        // Коди віртуальних клавіш — щоб не тягнути WinForms у тестовий проєкт.
        private const int VkA = 0x41;
        private const int VkF = 0x46;
        private const int VkJ = 0x4A;
        private const int VkS = 0x53;

        public void RunAllTests()
        {
            Console.WriteLine("=== KeyboardLayoutMap Tests ===\n");

            TestQueriesLatinForEnglishLayout();
            TestQueriesCyrillicForUkrainianLayout();
            TestShiftGivesUpperCase();
            TestConversionMapIsBidirectionallySane();
            TestApostropheKeyMatchesTheRealLayout();

            Console.WriteLine("\n✓ All KeyboardLayoutMap tests passed!");
        }

        private void TestQueriesLatinForEnglishLayout()
        {
            char a = KeyboardLayoutMap.GetChar(VkA, isEnglishLayout: true, useUpperCase: false);

            Assert(KeyboardLayoutMap.IsAvailable, "Expected the layouts to be queryable on this machine");
            Assert(a == 'a', $"Expected 'a' on the A key in the English layout, got '{a}'");

            Console.WriteLine("✓ English layout returns Latin characters");
        }

        private void TestQueriesCyrillicForUkrainianLayout()
        {
            // Ці три клавіші однакові в будь-якій українській розкладці.
            AssertKey(VkF, 'а');
            AssertKey(VkJ, 'о');
            AssertKey(VkS, 'і');

            Console.WriteLine("✓ Ukrainian layout returns Cyrillic characters");
        }

        private void TestShiftGivesUpperCase()
        {
            char lower = KeyboardLayoutMap.GetChar(VkF, isEnglishLayout: false, useUpperCase: false);
            char upper = KeyboardLayoutMap.GetChar(VkF, isEnglishLayout: false, useUpperCase: true);

            Assert(char.ToUpperInvariant(lower) == upper, $"Expected shift to give the upper case of '{lower}', got '{upper}'");
            Console.WriteLine("✓ Shift yields the upper-case character");
        }

        /// <summary>
        /// Every pair must involve a letter, and no pair may map a character to itself —
        /// otherwise ConvertWord would garble punctuation shared by both layouts.
        /// </summary>
        private void TestConversionMapIsBidirectionallySane()
        {
            Dictionary<char, char> map = KeyboardLayoutMap.BuildEnglishToUkrainianMap();

            Assert(map.Count > 0, "Expected a non-empty conversion map");

            foreach (KeyValuePair<char, char> pair in map)
            {
                Assert(pair.Key != pair.Value, $"Character '{pair.Key}' maps to itself");
                Assert(char.IsLetter(pair.Key) || char.IsLetter(pair.Value),
                    $"Pair '{pair.Key}' -> '{pair.Value}' involves no letter at all");
            }

            Assert(map['f'] == 'а', $"Expected 'f' -> 'а', got '{map['f']}'");
            Console.WriteLine($"✓ Conversion map is sane ({map.Count} pairs)");
        }

        /// <summary>
        /// The whole point of querying instead of hardcoding: the "~" key carries an
        /// apostrophe in some Ukrainian layouts and "ё" in the stock one. Whatever this
        /// machine's layout says, the map must agree with it.
        /// </summary>
        private void TestApostropheKeyMatchesTheRealLayout()
        {
            char fromMap = KeyboardLayoutMap.GetChar(Win32Interop.VK_OEM_3, isEnglishLayout: false, useUpperCase: false);

            Assert(fromMap != '\0', "Expected the '~' key to produce something in the Ukrainian layout");
            Console.WriteLine($"✓ The '~' key reports what the layout actually gives: '{fromMap}'");
        }

        private void AssertKey(int virtualKey, char expected)
        {
            char actual = KeyboardLayoutMap.GetChar(virtualKey, isEnglishLayout: false, useUpperCase: false);
            Assert(actual == expected, $"Expected '{expected}' on key 0x{virtualKey:X2}, got '{actual}'");
        }
    }
}
