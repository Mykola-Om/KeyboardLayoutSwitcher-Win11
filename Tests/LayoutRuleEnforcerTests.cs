using System;
using Action = KeyboardLayoutSwitcher.LayoutRuleEnforcer.RuleAction;

namespace KeyboardLayoutSwitcher.Tests
{
    /// <summary>
    /// Tests for the decision behind per-application layout rules — in particular that a
    /// layout the user picked by hand is never overridden again.
    /// </summary>
    public class LayoutRuleEnforcerTests : TestBase
    {
        private const bool English = true;
        private const bool Ukrainian = false;

        public void RunAllTests()
        {
            Console.WriteLine("=== LayoutRuleEnforcer Tests ===\n");

            TestNoRuleMeansNoAction();
            TestAppliesLayoutOnFirstActivation();
            TestDoesNothingWhenLayoutAlreadyMatches();
            TestDetectsManualChange();
            TestKeepsIgnoringAfterManualChange();
            TestOurOwnLayoutIsNotMistakenForManualChange();

            Console.WriteLine("\n✓ All LayoutRuleEnforcer tests passed!");
        }

        private void TestNoRuleMeansNoAction()
        {
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: null,
                currentLayoutIsEnglish: English,
                wasAppliedByUs: false,
                previouslyAppliedLayoutIsEnglish: false,
                alreadyOverridden: false);

            Assert(result == Action.NoRule, $"Expected NoRule, got {result}");
            Console.WriteLine("✓ A process without a rule is left alone");
        }

        private void TestAppliesLayoutOnFirstActivation()
        {
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: Ukrainian,
                currentLayoutIsEnglish: English,
                wasAppliedByUs: false,
                previouslyAppliedLayoutIsEnglish: false,
                alreadyOverridden: false);

            Assert(result == Action.ApplyLayout, $"Expected ApplyLayout, got {result}");
            Console.WriteLine("✓ The rule is applied on first activation");
        }

        private void TestDoesNothingWhenLayoutAlreadyMatches()
        {
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: Ukrainian,
                currentLayoutIsEnglish: Ukrainian,
                wasAppliedByUs: false,
                previouslyAppliedLayoutIsEnglish: false,
                alreadyOverridden: false);

            Assert(result == Action.AlreadyCorrect, $"Expected AlreadyCorrect, got {result}");
            Console.WriteLine("✓ Nothing is switched when the layout already matches");
        }

        /// <summary>
        /// We set Ukrainian, the user switched to English: that is a deliberate choice.
        /// </summary>
        private void TestDetectsManualChange()
        {
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: Ukrainian,
                currentLayoutIsEnglish: English,
                wasAppliedByUs: true,
                previouslyAppliedLayoutIsEnglish: Ukrainian,
                alreadyOverridden: false);

            Assert(result == Action.MarkManualOverride, $"Expected MarkManualOverride, got {result}");
            Console.WriteLine("✓ A manual switch is detected");
        }

        private void TestKeepsIgnoringAfterManualChange()
        {
            // Навіть коли розкладка знову збіглася з правилом, вікно лишається недоторканним.
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: Ukrainian,
                currentLayoutIsEnglish: Ukrainian,
                wasAppliedByUs: true,
                previouslyAppliedLayoutIsEnglish: Ukrainian,
                alreadyOverridden: true);

            Assert(result == Action.SkipOverridden, $"Expected SkipOverridden, got {result}");
            Console.WriteLine("✓ Once overridden, the window stays untouched");
        }

        /// <summary>
        /// Returning to a window we set ourselves must not look like a manual change,
        /// otherwise the rule would disable itself on the second activation.
        /// </summary>
        private void TestOurOwnLayoutIsNotMistakenForManualChange()
        {
            Action result = LayoutRuleEnforcer.Decide(
                desiredLayoutIsEnglish: Ukrainian,
                currentLayoutIsEnglish: Ukrainian,
                wasAppliedByUs: true,
                previouslyAppliedLayoutIsEnglish: Ukrainian,
                alreadyOverridden: false);

            Assert(result == Action.AlreadyCorrect, $"Expected AlreadyCorrect, got {result}");
            Console.WriteLine("✓ Our own layout is not mistaken for a manual change");
        }
    }
}
