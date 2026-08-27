using System;
using System.Collections.Generic;

namespace KeyboardLayoutSwitcher
{
    /// <summary>
    /// Виставляє задану розкладку, коли активується вікно програми, для якої є правило.
    ///
    /// Ручний вибір користувача має пріоритет: якщо після нашого перемикання розкладка у
    /// вікні виявляється іншою, ніж ми ставили, вважаємо, що її змінили свідомо, і більше
    /// це вікно не чіпаємо — доки воно не закриється.
    /// </summary>
    public class LayoutRuleEnforcer : IDisposable
    {
        // Скільки вікон тримаємо в пам'яті, перш ніж прибрати вже закриті.
        private const int WindowStateCleanupThreshold = 64;

        private readonly AppSettings settings;
        private readonly Win32Interop.WinEventProc callback;
        private IntPtr winEventHook = IntPtr.Zero;

        // Розкладка, яку ми самі виставили вікну. Розбіжність із фактичною означає,
        // що користувач перемкнув її вручну.
        private readonly Dictionary<IntPtr, bool> layoutAppliedByUs = new Dictionary<IntPtr, bool>();

        // Вікна, де користувач перебив наше правило.
        private readonly HashSet<IntPtr> manuallyOverridden = new HashSet<IntPtr>();

        public LayoutRuleEnforcer(AppSettings settings)
        {
            this.settings = settings ?? new AppSettings();
            callback = OnForegroundWindowChanged;
        }

        public void Start()
        {
            if (winEventHook != IntPtr.Zero)
            {
                return;
            }

            winEventHook = Win32Interop.SetWinEventHook(
                Win32Interop.EVENT_SYSTEM_FOREGROUND,
                Win32Interop.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                callback,
                0,
                0,
                Win32Interop.WINEVENT_OUTOFCONTEXT | Win32Interop.WINEVENT_SKIPOWNPROCESS);

            TraceLogger.Trace("LayoutRuleEnforcer hook: " + winEventHook);
        }

        public void Stop()
        {
            if (winEventHook != IntPtr.Zero)
            {
                Win32Interop.UnhookWinEvent(winEventHook);
                winEventHook = IntPtr.Zero;
            }

            layoutAppliedByUs.Clear();
            manuallyOverridden.Clear();
        }

        /// <summary>
        /// Скидає запам'ятані ручні перемикання. Викликається після зміни правил, щоб нові
        /// налаштування застосувались одразу, а не після закриття вікон.
        /// </summary>
        public void ResetWindowState()
        {
            layoutAppliedByUs.Clear();
            manuallyOverridden.Clear();
        }

        private void OnForegroundWindowChanged(IntPtr hookHandle, uint eventType, IntPtr window, int objectId, int childId, uint eventThread, uint eventTime)
        {
            if (eventType != Win32Interop.EVENT_SYSTEM_FOREGROUND || window == IntPtr.Zero)
            {
                return;
            }

            try
            {
                ApplyRuleTo(window);
            }
            catch (Exception e)
            {
                // Хук викликається операційною системою — виняток звідси вбив би застосунок.
                TraceLogger.Trace($"LayoutRuleEnforcer error: {e.Message}");
            }
        }

        /// <summary>
        /// Що зробити з вікном, яке щойно стало активним.
        /// </summary>
        public enum RuleAction
        {
            /// <summary>Правила для цієї програми немає.</summary>
            NoRule,

            /// <summary>Користувач уже перебив правило в цьому вікні — не чіпаємо.</summary>
            SkipOverridden,

            /// <summary>Розкладка змінилась не нами: запам'ятати вибір і більше не втручатись.</summary>
            MarkManualOverride,

            /// <summary>Виставити задану розкладку.</summary>
            ApplyLayout,

            /// <summary>Розкладка вже така, як треба — лише запам'ятати стан.</summary>
            AlreadyCorrect
        }

        /// <summary>
        /// Чисте рішення без звернень до системи — щоб поведінку "ручний вибір має пріоритет"
        /// можна було перевірити тестами, а не лише руками.
        /// </summary>
        public static RuleAction Decide(
            bool? desiredLayoutIsEnglish,
            bool currentLayoutIsEnglish,
            bool wasAppliedByUs,
            bool previouslyAppliedLayoutIsEnglish,
            bool alreadyOverridden)
        {
            if (desiredLayoutIsEnglish == null)
            {
                return RuleAction.NoRule;
            }

            if (alreadyOverridden)
            {
                return RuleAction.SkipOverridden;
            }

            // Ми вже виставляли розкладку цьому вікну — якщо вона більше не наша,
            // значить її змінили свідомо.
            if (wasAppliedByUs && currentLayoutIsEnglish != previouslyAppliedLayoutIsEnglish)
            {
                return RuleAction.MarkManualOverride;
            }

            return currentLayoutIsEnglish == desiredLayoutIsEnglish.Value
                ? RuleAction.AlreadyCorrect
                : RuleAction.ApplyLayout;
        }

        private void ApplyRuleTo(IntPtr window)
        {
            string processName = ProcessNameResolver.GetProcessName(window);
            bool? desiredLayoutIsEnglish = settings.GetDesiredLayoutIsEnglish(processName);

            if (desiredLayoutIsEnglish == null)
            {
                return;
            }

            bool currentLayoutIsEnglish = LayoutSwitcher.IsLayoutEnglishForWindow(window);
            bool wasAppliedByUs = layoutAppliedByUs.TryGetValue(window, out bool previouslyApplied);

            RuleAction action = Decide(
                desiredLayoutIsEnglish,
                currentLayoutIsEnglish,
                wasAppliedByUs,
                previouslyApplied,
                manuallyOverridden.Contains(window));

            switch (action)
            {
                case RuleAction.NoRule:
                case RuleAction.SkipOverridden:
                    return;

                case RuleAction.MarkManualOverride:
                    manuallyOverridden.Add(window);
                    TraceLogger.Trace($"Layout rule yields to manual choice: {processName}");
                    return;

                case RuleAction.ApplyLayout:
                    LayoutSwitcher.SetKeyboardLayout(window, desiredLayoutIsEnglish.Value);
                    TraceLogger.Trace($"Layout rule applied: {processName} -> {(desiredLayoutIsEnglish.Value ? "en" : "uk")}");
                    break;
            }

            layoutAppliedByUs[window] = desiredLayoutIsEnglish.Value;

            if (layoutAppliedByUs.Count > WindowStateCleanupThreshold)
            {
                ForgetClosedWindows();
            }
        }

        private void ForgetClosedWindows()
        {
            var closedWindows = new List<IntPtr>();

            foreach (IntPtr window in layoutAppliedByUs.Keys)
            {
                if (!Win32Interop.IsWindow(window))
                {
                    closedWindows.Add(window);
                }
            }

            foreach (IntPtr window in closedWindows)
            {
                layoutAppliedByUs.Remove(window);
                manuallyOverridden.Remove(window);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
