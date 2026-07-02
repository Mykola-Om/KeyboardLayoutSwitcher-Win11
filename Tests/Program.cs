using System;

namespace KeyboardLayoutSwitcher.Tests
{
    internal static class Program
    {
        internal static int Main()
        {
            int failures = 0;
            failures += RunSuite("KeyMapperTests", () => new KeyMapperTests().RunAllTests());
            failures += RunSuite("WordTrackerTests", () => new WordTrackerTests().RunAllTests());

            Console.WriteLine();
            if (failures > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{failures} test suite(s) failed.");
                Console.ResetColor();
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All test suites passed.");
            Console.ResetColor();
            return 0;
        }

        private static int RunSuite(string name, Action suite)
        {
            try
            {
                suite();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ {name} FAILED: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
