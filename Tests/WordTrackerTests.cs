using System;
using KeyboardLayoutSwitcher;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Unit tests for WordTracker state machine.
    /// </summary>
    public class WordTrackerTests : TestBase
    {
        public void RunAllTests()
        {
            Console.WriteLine("=== WordTracker Tests ===\n");

            TestInitiallyEmpty();
            TestAppendChar();
            TestGetWordAtBoundary();
            TestClear();
            TestRemoveLastChar();
            TestRemoveLastCharOnEmpty();
            TestMultipleChars();

            Console.WriteLine("\n✓ All WordTracker tests passed!");
        }

        private void TestInitiallyEmpty()
        {
            var tracker = new WordTracker();
            Assert(tracker.IsEmpty, "WordTracker should be initially empty");
            Assert(tracker.Length == 0, "Length should be 0");
            Console.WriteLine("✓ Initial state: empty");
        }

        private void TestAppendChar()
        {
            var tracker = new WordTracker();
            tracker.AppendChar('h');

            Assert(tracker.Length == 1, "Length should be 1");
            Assert(tracker.Current == "h", "Current should be 'h'");
            Assert(!tracker.IsEmpty, "Should not be empty");
            Console.WriteLine("✓ AppendChar works");
        }

        private void TestGetWordAtBoundary()
        {
            var tracker = new WordTracker();
            tracker.AppendChar('t');
            tracker.AppendChar('e');
            tracker.AppendChar('s');
            tracker.AppendChar('t');

            bool hasWord = tracker.TryGetWordAtBoundary(out string word);

            Assert(hasWord, "Should have a word");
            Assert(word == "test", $"Word should be 'test', got '{word}'");
            Console.WriteLine("✓ TryGetWordAtBoundary works");
        }

        private void TestClear()
        {
            var tracker = new WordTracker();
            tracker.AppendChar('a');
            tracker.AppendChar('b');
            tracker.AppendChar('c');

            tracker.Clear();

            Assert(tracker.IsEmpty, "Should be empty after clear");
            Assert(tracker.Length == 0, "Length should be 0");
            Console.WriteLine("✓ Clear works");
        }

        private void TestRemoveLastChar()
        {
            var tracker = new WordTracker();
            tracker.AppendChar('h');
            tracker.AppendChar('e');
            tracker.AppendChar('l');
            tracker.AppendChar('l');
            tracker.AppendChar('o');

            tracker.RemoveLastChar();

            Assert(tracker.Current == "hell", $"After RemoveLastChar, should be 'hell', got '{tracker.Current}'");
            Assert(tracker.Length == 4, "Length should be 4");
            Console.WriteLine("✓ RemoveLastChar works");
        }

        private void TestRemoveLastCharOnEmpty()
        {
            var tracker = new WordTracker();
            tracker.RemoveLastChar(); // Should not throw

            Assert(tracker.IsEmpty, "Should remain empty");
            Assert(tracker.Length == 0, "Length should be 0");
            Console.WriteLine("✓ RemoveLastChar on empty (no-op)");
        }

        private void TestMultipleChars()
        {
            var tracker = new WordTracker();
            string text = "keyboard";

            foreach (char c in text)
            {
                tracker.AppendChar(c);
            }

            Assert(tracker.Current == "keyboard", $"Should accumulate to '{text}'");
            Assert(tracker.Length == text.Length, $"Length should be {text.Length}");
            Console.WriteLine("✓ Multiple appends work correctly");
        }
    }
}
