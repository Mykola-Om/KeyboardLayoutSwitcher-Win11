using System;

namespace KeyboardLayoutSwitcher.Tests
{
    public abstract class TestBase
    {
        protected static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"FAIL: {message}");
            }
        }
    }
}
