using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    /// <summary>
    /// Питає у самих розкладок, який символ дає кожна клавіша, замість того щоб покладатись
    /// на зашиту таблицю.
    ///
    /// Зашита таблиця описувала стандартну українську розкладку, але користувач може мати
    /// власну: наприклад, на клавіші "~" стандартна "00000422" дає "ё", а користувацька —
    /// апостроф. Через це відновлення апострофа не спрацювало б у того, хто друкує в
    /// стандартній розкладці.
    ///
    /// Якщо потрібної розкладки в системі немає (або опитування не вдалося), лишається
    /// зашита таблиця — тоді поведінка така сама, як була раніше.
    /// </summary>
    public static class KeyboardLayoutMap
    {
        // Клавіші, які взагалі можуть давати символ слова в обох розкладках.
        private static readonly int[] queriedVirtualKeys = BuildQueriedVirtualKeys();

        private static readonly object buildLock = new object();

        private static IntPtr builtEnglishLayout = IntPtr.Zero;
        private static IntPtr builtUkrainianLayout = IntPtr.Zero;

        private static Dictionary<int, char> englishLower = new Dictionary<int, char>();
        private static Dictionary<int, char> englishUpper = new Dictionary<int, char>();
        private static Dictionary<int, char> ukrainianLower = new Dictionary<int, char>();
        private static Dictionary<int, char> ukrainianUpper = new Dictionary<int, char>();

        /// <summary>
        /// Чи вдалося побудувати таблиці з системних розкладок. Якщо ні — викликач
        /// має скористатись власним запасним варіантом.
        /// </summary>
        public static bool IsAvailable { get; private set; }

        // EnsureBuilt викликається на кожне натискання, а розкладки в системі змінюються
        // хіба що вручну — тому перевіряємо їх не частіше, ніж раз на цей інтервал.
        private const int LayoutRecheckIntervalMs = 2000;

        private static int lastLayoutCheckTicks = Environment.TickCount - LayoutRecheckIntervalMs;

        /// <summary>
        /// Перебудовує таблиці, якщо розкладки в системі змінились.
        /// </summary>
        public static void EnsureBuilt()
        {
            if (IsAvailable && unchecked(Environment.TickCount - lastLayoutCheckTicks) < LayoutRecheckIntervalMs)
            {
                return;
            }

            lastLayoutCheckTicks = Environment.TickCount;

            IntPtr englishLayout = LayoutSwitcher.ResolveKeyboardLayout(english: true);
            IntPtr ukrainianLayout = LayoutSwitcher.ResolveKeyboardLayout(english: false);

            if (englishLayout == builtEnglishLayout && ukrainianLayout == builtUkrainianLayout && IsAvailable)
            {
                return;
            }

            lock (buildLock)
            {
                var newEnglishLower = new Dictionary<int, char>();
                var newEnglishUpper = new Dictionary<int, char>();
                var newUkrainianLower = new Dictionary<int, char>();
                var newUkrainianUpper = new Dictionary<int, char>();

                foreach (int virtualKey in queriedVirtualKeys)
                {
                    AddIfResolved(newEnglishLower, virtualKey, englishLayout, shift: false);
                    AddIfResolved(newEnglishUpper, virtualKey, englishLayout, shift: true);
                    AddIfResolved(newUkrainianLower, virtualKey, ukrainianLayout, shift: false);
                    AddIfResolved(newUkrainianUpper, virtualKey, ukrainianLayout, shift: true);
                }

                // Українська сторона мусить дати кирилицю — інакше розкладки в системі
                // немає і опитування повернуло ту саму латиницю.
                IsAvailable = ContainsCyrillic(newUkrainianLower) && newEnglishLower.Count > 0;

                if (IsAvailable)
                {
                    englishLower = newEnglishLower;
                    englishUpper = newEnglishUpper;
                    ukrainianLower = newUkrainianLower;
                    ukrainianUpper = newUkrainianUpper;
                    builtEnglishLayout = englishLayout;
                    builtUkrainianLayout = ukrainianLayout;
                }

                TraceLogger.Trace($"KeyboardLayoutMap built: available={IsAvailable}, keys={newUkrainianLower.Count}");
            }
        }

        /// <summary>
        /// Символ, який дає клавіша у вказаній розкладці, або '\0'.
        /// </summary>
        public static char GetChar(int virtualKey, bool isEnglishLayout, bool useUpperCase)
        {
            EnsureBuilt();

            if (!IsAvailable)
            {
                return '\0';
            }

            Dictionary<int, char> map = isEnglishLayout
                ? (useUpperCase ? englishUpper : englishLower)
                : (useUpperCase ? ukrainianUpper : ukrainianLower);

            return map.TryGetValue(virtualKey, out char character) ? character : '\0';
        }

        /// <summary>
        /// Відповідність символів між розкладками, побудована з тих самих клавіш:
        /// англійський символ -> український на тій самій клавіші.
        /// </summary>
        public static Dictionary<char, char> BuildEnglishToUkrainianMap()
        {
            EnsureBuilt();

            var map = new Dictionary<char, char>();
            if (!IsAvailable)
            {
                return map;
            }

            AddPairs(map, englishLower, ukrainianLower);
            AddPairs(map, englishUpper, ukrainianUpper);
            return map;
        }

        private static void AddPairs(Dictionary<char, char> map, Dictionary<int, char> from, Dictionary<int, char> to)
        {
            foreach (KeyValuePair<int, char> pair in from)
            {
                if (!to.TryGetValue(pair.Key, out char target) || pair.Value == target)
                {
                    continue;
                }

                // Пари, де жоден бік не є літерою (напр. "`" та апостроф), у конвертацію не
                // йдуть: інакше апостроф усередині слова перетворювався б на "`".
                if (!char.IsLetter(pair.Value) && !char.IsLetter(target))
                {
                    continue;
                }

                map[pair.Value] = target;
            }
        }

        private static void AddIfResolved(Dictionary<int, char> map, int virtualKey, IntPtr layout, bool shift)
        {
            char character = QueryLayout(virtualKey, layout, shift);
            if (character != '\0')
            {
                map[virtualKey] = character;
            }
        }

        private static char QueryLayout(int virtualKey, IntPtr layout, bool shift)
        {
            if (layout == IntPtr.Zero)
            {
                return '\0';
            }

            byte[] keyState = new byte[256];
            if (shift)
            {
                keyState[Win32Interop.VK_SHIFT] = 0x80;
            }

            uint scanCode = Win32Interop.MapVirtualKeyEx((uint)virtualKey, Win32Interop.MAPVK_VK_TO_VSC, layout);
            StringBuilder buffer = new StringBuilder(8);

            int written = Win32Interop.ToUnicodeEx(
                (uint)virtualKey,
                scanCode,
                keyState,
                buffer,
                buffer.Capacity,
                Win32Interop.TOUNICODE_NO_KEYBOARD_STATE_CHANGE,
                layout);

            // Один символ — те, що нас цікавить. Нуль означає "клавіша нічого не дає",
            // від'ємне — dead key, який окремим символом не є.
            return written == 1 ? buffer[0] : '\0';
        }

        private static bool ContainsCyrillic(Dictionary<int, char> map)
        {
            foreach (char character in map.Values)
            {
                if (character >= 'Ѐ' && character <= 'ӿ')
                {
                    return true;
                }
            }

            return false;
        }

        private static int[] BuildQueriedVirtualKeys()
        {
            var keys = new List<int>();

            for (int virtualKey = (int)Keys.A; virtualKey <= (int)Keys.Z; virtualKey++)
            {
                keys.Add(virtualKey);
            }

            keys.AddRange(new[]
            {
                Win32Interop.VK_OEM_1, Win32Interop.VK_OEM_2, Win32Interop.VK_OEM_3,
                Win32Interop.VK_OEM_4, Win32Interop.VK_OEM_5, Win32Interop.VK_OEM_6,
                Win32Interop.VK_OEM_7, Win32Interop.VK_OEM_COMMA, Win32Interop.VK_OEM_PERIOD,
                Win32Interop.VK_OEM_MINUS, Win32Interop.VK_OEM_PLUS
            });

            for (int digitKey = (int)Keys.D0; digitKey <= (int)Keys.D9; digitKey++)
            {
                keys.Add(digitKey);
            }

            return keys.ToArray();
        }
    }
}
