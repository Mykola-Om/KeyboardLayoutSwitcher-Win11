using System;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Tests for keyboard layout resolution. These run against the layouts actually
    /// installed on this machine, so they assert on the language of the result rather
    /// than on a specific layout identifier.
    /// </summary>
    public class LayoutSwitcherTests : TestBase
    {
        private const int PrimaryLanguageMask = 0x03FF;
        private const int EnglishPrimaryLanguageId = 0x0009;
        private const int UkrainianPrimaryLanguageId = 0x0022;

        public void RunAllTests()
        {
            Console.WriteLine("=== LayoutSwitcher Tests ===\n");

            TestResolvesRequestedLanguage();
            TestPrefersAnInstalledLayout();

            Console.WriteLine("\n✓ All LayoutSwitcher tests passed!");
        }

        private void TestResolvesRequestedLanguage()
        {
            AssertLanguage(LayoutSwitcher.ResolveKeyboardLayout(english: true), EnglishPrimaryLanguageId, "English");
            AssertLanguage(LayoutSwitcher.ResolveKeyboardLayout(english: false), UkrainianPrimaryLanguageId, "Ukrainian");

            Console.WriteLine("✓ Resolves a layout of the requested language");
        }

        /// <summary>
        /// The point of the fix: when the user has a custom layout of that language
        /// (e.g. "d0010422" alongside the stock "00000422"), we must return one that is
        /// actually loaded rather than force-loading the stock identifier.
        /// </summary>
        private void TestPrefersAnInstalledLayout()
        {
            IntPtr resolved = LayoutSwitcher.ResolveKeyboardLayout(english: false);
            bool isInstalled = false;

            foreach (IntPtr layout in LayoutSwitcher.GetInstalledLayouts())
            {
                if (layout == resolved)
                {
                    isInstalled = true;
                    break;
                }
            }

            Assert(isInstalled, $"Expected the resolved layout 0x{resolved.ToInt64():X} to be one of the installed ones");
            Console.WriteLine("✓ Prefers a layout that is already installed");
        }

        private void AssertLanguage(IntPtr layout, int expectedPrimaryLanguage, string languageName)
        {
            Assert(layout != IntPtr.Zero, $"Expected a non-null layout for {languageName}");

            uint languageId = (uint)(layout.ToInt64() & 0xFFFF);
            Assert((languageId & PrimaryLanguageMask) == expectedPrimaryLanguage,
                $"Expected a {languageName} layout, got language id 0x{languageId:X4}");
        }
    }
}
