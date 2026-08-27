using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeyboardLayoutSwitcher
{
    public static class KeyMapper
    {
        private class LruCache
        {
            private readonly int capacity;
            private readonly Dictionary<string, LinkedListNode<CacheItem>> cacheMap = new Dictionary<string, LinkedListNode<CacheItem>>(StringComparer.OrdinalIgnoreCase);
            private readonly LinkedList<CacheItem> lruList = new LinkedList<CacheItem>();
            private readonly object lockObj = new object();

            private class CacheItem { public string Key; public bool Value; }

            public LruCache(int capacity) { this.capacity = capacity; }

            public bool TryGetValue(string key, out bool value)
            {
                lock (lockObj)
                {
                    if (cacheMap.TryGetValue(key, out var node))
                    {
                        lruList.Remove(node);
                        lruList.AddFirst(node);
                        value = node.Value.Value;
                        return true;
                    }
                    value = false;
                    return false;
                }
            }

            public void Set(string key, bool value)
            {
                lock (lockObj)
                {
                    if (cacheMap.TryGetValue(key, out var node))
                    {
                        lruList.Remove(node);
                        node.Value.Value = value;
                        lruList.AddFirst(node);
                    }
                    else
                    {
                        if (cacheMap.Count >= capacity)
                        {
                            cacheMap.Remove(lruList.Last.Value.Key);
                            lruList.RemoveLast();
                        }
                        var newNode = new LinkedListNode<CacheItem>(new CacheItem { Key = key, Value = value });
                        lruList.AddFirst(newNode);
                        cacheMap[key] = newNode;
                    }
                }
            }

            public void Clear()
            {
                lock (lockObj)
                {
                    cacheMap.Clear();
                    lruList.Clear();
                }
            }
        }

        // Розмір LRU-кешу результатів IsWrongLayout (окремо для кожної розкладки).
        private const int WordCacheCapacity = 500;

        // Скільки символів слова дозволено лишити "непереконвертованими" (напр. цифри чи апострофи),
        // щоб все одно вважати слово повністю переведеним у протилежну розкладку.
        private const int AlmostFullyMappedTolerance = 1;

        // Поріг довжини безперервного ланцюжка приголосних, з якого починається штраф за "неприродність".
        private const int ConsecutiveConsonantsThreshold = 4;
        private const int ConsecutiveConsonantsPenalty = 40;   // штраф при досягненні порогу
        private const int ExtraConsecutiveConsonantPenalty = 20; // додатковий штраф за кожен наступний приголосний понад поріг

        private const int NoVowelsMinimumWordLength = 3;
        private const int NoVowelsPenalty = 30;

        // Штраф за поєднання символів, які практично не зустрічаються у відповідній мові
        // (подвоєні рідкісні літери, кириличні літери з інших мов, неможливі англійські біграми).
        private const int ForbiddenCombinationPenalty = 50;

        // Штраф за слово, що починається на "x" з наступним приголосним (нетипово для англійської).
        private const int UnlikelyLeadingXPenalty = 30;

        // Штраф за пару сусідніх приголосних, яка жодного разу не трапляється у словнику
        // відповідної мови. Текст, набраний не в тій розкладці, майже завжди містить такі
        // пари ("lh", "hj", "kf" для англійської), тоді як у справжніх словах їх немає.
        private const int ImplausibleBigramPenalty = 15;

        // Мінімальний розмір набору біграм, за якого йому можна довіряти. Якщо словник
        // не завантажився, набір буде крихітним і покарав би геть усе — тому в такому
        // разі перевірка біграм просто вимикається.
        private const int MinimumBigramSetSize = 100;

        private static readonly LruCache enCache = new LruCache(WordCacheCapacity);
        private static readonly LruCache ukCache = new LruCache(WordCacheCapacity);

        private static readonly HashSet<char> englishVowels = new HashSet<char>
        {
            'a', 'e', 'i', 'o', 'u', 'y', 'A', 'E', 'I', 'O', 'U', 'Y'
        };

        private static readonly HashSet<string> commonEnglishWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "i", "me", "my", "we", "us", "our", "you", "your", "he", "him", "his", "she", "her",
            "it", "its", "they", "them", "their", "this", "that", "these", "those", "here", "there",
            "yes", "no", "and", "or", "but", "not", "to", "of", "in", "on", "is", "are", "was",
            "were", "be", "am", "do", "did", "done", "what", "how", "when", "where", "who", "why",
            "github", "python", "javascript", "typescript", "dotnet", "react", "angular", "docker",
            "hi", "as", "at", "if", "an", "up", "so", "by", "ok", "go"
        };

        private static readonly HashSet<char> ukrainianVowels = new HashSet<char>
        {
            '\u0430', '\u0435', '\u0438', '\u0456', '\u043E', '\u0443', '\u044F', '\u044E', '\u0454', '\u0457',
            '\u0410', '\u0415', '\u0418', '\u0406', '\u041E', '\u0423', '\u042F', '\u042E', '\u0404', '\u0407'
        };

        private static readonly HashSet<string> commonUkrainianWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "я", "ти", "ми", "ви", "він", "вона", "воно", "вони", "мені", "тобі", "нам", "вам", "їм",
            "нас", "вас", "їх", "його", "її", "мій", "моя", "моє", "мої", "твій", "твоя", "твоє", "твої", 
            "свій", "своя", "своє", "свої", "наш", "наша", "наше", "наші", "ваш", "ваша", "ваше", "ваші", 
            "цей", "ця", "це", "ці", "той", "та", "те", "ті", "такий", "така", "таке", "такі"
        };

        private static readonly Dictionary<char, char> engToUkrMap = BuildMap();

        private static readonly Dictionary<char, char> ukrToEngMap = BuildReverseMap();

        // Пари сусідніх приголосних, що реально зустрічаються у словах кожної мови.
        private static readonly HashSet<string> englishConsonantBigrams = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> ukrainianConsonantBigrams = new HashSet<string>(StringComparer.Ordinal);

        // Апостроф, який вводиться клавішею VK_OEM_3 в українській розкладці.
        private const char Apostrophe = '\'';

        // Типографський апостроф: деякі редактори підставляють його замість звичайного,
        // тому перед пошуком у словнику слово нормалізуємо.
        private const char TypographicApostrophe = '’';

        // Слово без апострофа -> правильна форма з апострофом ("память" -> "пам'ять").
        // Ключі виводяться зі списку правильних форм, тому розсинхронізуватись не можуть.
        private static readonly Dictionary<string, string> apostropheCorrections =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static KeyMapper()
        {
            LoadDictionaryFile("Dictionaries\\en.txt", commonEnglishWords);
            LoadDictionaryFile("Dictionaries\\uk.txt", commonUkrainianWords);

            // Завантаження технічних слів, які зазвичай вводяться на англійській розкладці
            LoadDictionaryFile("Dictionaries\\tech.txt", commonEnglishWords);

            // Слова з апострофом — це теж повноцінні українські слова, тому вони мають
            // потрапити і в загальний словник, інакше евристика вважатиме їх помилковими.
            LoadApostropheDictionary("Dictionaries\\uk-apostrophe.txt");

            CollectConsonantBigrams(commonEnglishWords, englishVowels, englishConsonantBigrams);
            CollectConsonantBigrams(commonUkrainianWords, ukrainianVowels, ukrainianConsonantBigrams);
        }

        private static void LoadApostropheDictionary(string relativePath)
        {
            var wordsWithApostrophe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LoadDictionaryFile(relativePath, wordsWithApostrophe);

            foreach (string word in wordsWithApostrophe)
            {
                commonUkrainianWords.Add(word);

                string withoutApostrophe = word.Replace(Apostrophe.ToString(), string.Empty);
                if (withoutApostrophe.Length == word.Length)
                {
                    continue;
                }

                // Якщо форма без апострофа сама є словом, відновлювати апостроф небезпечно.
                if (commonUkrainianWords.Contains(withoutApostrophe))
                {
                    continue;
                }

                apostropheCorrections[withoutApostrophe] = word;
            }
        }

        /// <summary>
        /// Відновлює пропущений апостроф ("память" -> "пам'ять"). Працює строго за словником:
        /// правило "після б/п/в/м/ф перед я/ю/є/ї" зламало б слова, де апострофа немає
        /// ("свято", "цвях", "морквяний"), тому воно тут свідомо не використовується.
        /// </summary>
        public static bool TryRestoreApostrophe(string word, out string correctedWord)
        {
            correctedWord = null;

            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            if (word.IndexOf(Apostrophe) >= 0 || word.IndexOf(TypographicApostrophe) >= 0)
            {
                return false; // Апостроф уже на місці
            }

            // Слово, яке і без апострофа є коректним, не чіпаємо.
            if (commonUkrainianWords.Contains(word))
            {
                return false;
            }

            if (!apostropheCorrections.TryGetValue(word, out string correction))
            {
                return false;
            }

            correctedWord = ApplyCasePattern(word, correction);
            return true;
        }

        private static string ApplyCasePattern(string sourceWord, string correction)
        {
            if (IsAllCaps(sourceWord, false))
            {
                return correction.ToUpperInvariant();
            }

            string lowerCorrection = correction.ToLowerInvariant();
            if (char.IsUpper(sourceWord[0]))
            {
                return char.ToUpperInvariant(lowerCorrection[0]) + lowerCorrection.Substring(1);
            }

            return lowerCorrection;
        }

        private static void CollectConsonantBigrams(IEnumerable<string> words, HashSet<char> vowels, HashSet<string> target)
        {
            foreach (string word in words)
            {
                string lowerWord = word.ToLowerInvariant();
                for (int index = 0; index < lowerWord.Length - 1; index++)
                {
                    if (IsConsonant(lowerWord[index], vowels) && IsConsonant(lowerWord[index + 1], vowels))
                    {
                        target.Add(lowerWord.Substring(index, 2));
                    }
                }
            }
        }

        private static bool IsConsonant(char character, HashSet<char> vowels)
        {
            return char.IsLetter(character) && !vowels.Contains(character);
        }

        private static void LoadDictionaryFile(string relativePath, HashSet<string> targetSet)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                if (File.Exists(fullPath))
                {
                    string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
                    foreach (string line in lines)
                    {
                        string word = line.Trim();
                        if (!string.IsNullOrEmpty(word))
                        {
                            targetSet.Add(word.ToLowerInvariant());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Словник не завантажився - використовуємо базові слова, визначені в коді.
                Console.WriteLine($"Failed to load dictionary {relativePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears cached IsWrongLayout results. Must be called whenever settings that
        /// affect the heuristic (IgnoredWords, MinimumMappedPercent) change, since the
        /// cache is keyed only by word+layout and is otherwise unaware of settings changes.
        /// </summary>
        public static void ClearCache()
        {
            enCache.Clear();
            ukCache.Clear();
        }

        public static string ConvertWord(string word, bool isEnglishLayout)
        {
            StringBuilder correctedWord = new StringBuilder();

            foreach (char c in word)
            {
                if (isEnglishLayout && engToUkrMap.ContainsKey(c))
                    correctedWord.Append(engToUkrMap[c]);
                else if (!isEnglishLayout && ukrToEngMap.ContainsKey(c))
                    correctedWord.Append(ukrToEngMap[c]);
                else
                    correctedWord.Append(c);
            }

            return correctedWord.ToString();
        }

        public static bool IsWrongLayout(string word, bool isEnglishLayout, AppSettings settings, char boundaryChar = '\0', char lastBoundaryChar = '\0')
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            var cache = isEnglishLayout ? enCache : ukCache;
            string cacheKey = BuildCacheKey(word, boundaryChar, lastBoundaryChar);
            if (cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            bool result = CalculateIsWrongLayout(word, isEnglishLayout, settings, boundaryChar, lastBoundaryChar);

            cache.Set(cacheKey, result);

            return result;
        }

        // Символи-межі, від яких залежить вердикт: на них тримаються правила для крапкових
        // файлів, ALL_CAPS-змінних, шляхів і camelCase. Решта меж (пробіл, кома…) на
        // результат не впливає, тому в ключ кешу не потрапляє — інакше він розросся б
        // на кожен розділовий знак.
        private static readonly char[] contextSensitiveBoundaries = { '.', '_', '/', '\\', '\u0001' };

        // Роздільник у ключі кешу; у словах не трапляється.
        private const char CacheKeySeparator = '\u0002';

        /// <summary>
        /// Ключ кешу мусить враховувати межові символи: без них вердикт для "cnfnec"
        /// після крапки перекривав би вердикт для звичайного "cnfnec" і навпаки.
        /// </summary>
        private static string BuildCacheKey(string word, char boundaryChar, char lastBoundaryChar)
        {
            char boundary = NormalizeBoundaryForCache(boundaryChar);
            char lastBoundary = NormalizeBoundaryForCache(lastBoundaryChar);

            if (boundary == '\0' && lastBoundary == '\0')
            {
                return word;
            }

            return word + CacheKeySeparator + boundary + lastBoundary;
        }

        private static char NormalizeBoundaryForCache(char boundaryChar)
        {
            return Array.IndexOf(contextSensitiveBoundaries, boundaryChar) >= 0 ? boundaryChar : '\0';
        }

        private static bool CalculateIsWrongLayout(string word, bool isEnglishLayout, AppSettings settings, char boundaryChar, char lastBoundaryChar)
        {
            if (isEnglishLayout)
            {
                // Dot-prefixed files/extensions (e.g. .env, .gitignore)
                if (lastBoundaryChar == '.')
                {
                    return false;
                }

                // Underscore-based constants/variables (e.g. DATABASE_URL, MY_VAR)
                if (boundaryChar == '_' || lastBoundaryChar == '_')
                {
                    return false;
                }

                // Path and URL slashes (e.g. src/components, http://)
                if (boundaryChar == '/' || boundaryChar == '\\' || lastBoundaryChar == '/' || lastBoundaryChar == '\\')
                {
                    return false;
                }

                // camelCase suffix protection (preceded by case transition)
                if (lastBoundaryChar == '\u0001')
                {
                    return false;
                }

                // ALL_CAPS constants/variables in English (only uppercase English letters and digits)
                if (IsAllCaps(word, true))
                {
                    return false;
                }
            }
            else
            {
                // Short Ukrainian abbreviations in uppercase (length <= 3, e.g. ФОП, ТОВ, ЗСУ)
                // but we allow them to be corrected if they are adjacent to '_' or '.' (part of a variable/file name)
                if (word.Length <= 3 && IsAllCaps(word, false))
                {
                    if (boundaryChar != '_' && lastBoundaryChar != '_' && lastBoundaryChar != '.')
                    {
                        return false;
                    }
                }
            }

            // Alphanumeric mixed strings (both letters and digits) are technical terms
            if (ContainsLettersAndDigits(word))
            {
                return false;
            }

            string convertedWord = ConvertWord(word, isEnglishLayout);
            int sourceMappedChars = CountMappedChars(word, isEnglishLayout ? engToUkrMap : ukrToEngMap);

            if (MatchesFrequentWord(word, isEnglishLayout))
            {
                return false; // Already a valid word in current layout, don't convert it!
            }

            // Українське слово з пропущеним апострофом ("сімї", "вюн") — це саме українське
            // слово, а не помилкова розкладка. Інакше евристика перетворила б його на
            // латинське сміття ще до того, як апостроф встигне відновитись.
            if (!isEnglishLayout && apostropheCorrections.ContainsKey(NormalizeWord(word)))
            {
                return false;
            }

            if (settings != null && settings.IgnoredWords != null && settings.IgnoredWords.Contains(word))
            {
                return false; // User explicitly ignored this word
            }

            if (MatchesFrequentWord(convertedWord, !isEnglishLayout) &&
                sourceMappedChars >= Math.Max(1, word.Length - AlmostFullyMappedTolerance))
            {
                return true;
            }

            if (word.Length < 2)
            {
                return false;
            }

            int sourceVowelCount = CountVowels(word, isEnglishLayout ? englishVowels : ukrainianVowels);
            int convertedVowelCount = CountVowels(convertedWord, isEnglishLayout ? ukrainianVowels : englishVowels);

            int minimumMappedPercent = Math.Max(1, Math.Min(100, settings?.MinimumMappedPercent ?? AppSettings.DefaultMinimumMappedPercent));
            int mappedThreshold = (int)Math.Ceiling(word.Length * minimumMappedPercent / 100.0);

            if (sourceMappedChars < mappedThreshold)
            {
                return false;
            }

            int sourcePenalty = CalculateUnnaturalnessScore(word, isEnglishLayout);
            int convertedPenalty = CalculateUnnaturalnessScore(convertedWord, !isEnglishLayout);

            // Якщо штрафи відрізняються, обираємо варіант з меншим штрафом (менш "неприродний")
            if (sourcePenalty > convertedPenalty)
            {
                return true;
            }
            else if (sourcePenalty < convertedPenalty)
            {
                return false;
            }

            // Якщо штрафи однакові, повертаємось до класичного підрахунку голосних як "тайбрейкера":
            // у правильному слові кількість голосних не повинна впасти після конвертації.
            return convertedVowelCount - sourceVowelCount >= 0;
        }

        private static int CalculateUnnaturalnessScore(string text, bool isEnglishLayout)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            
            string lowerText = text.ToLowerInvariant();
            int score = 0;
            int consecutiveConsonants = 0;
            int vowelCount = 0;
            
            HashSet<char> vowels = isEnglishLayout ? englishVowels : ukrainianVowels;
            HashSet<string> knownBigrams = isEnglishLayout ? englishConsonantBigrams : ukrainianConsonantBigrams;
            bool checkBigrams = knownBigrams.Count >= MinimumBigramSetSize;

            for (int i = 0; i < lowerText.Length; i++)
            {
                char c = lowerText[i];
                if (char.IsLetter(c))
                {
                    if (vowels.Contains(c))
                    {
                        vowelCount++;
                        consecutiveConsonants = 0;
                    }
                    else
                    {
                        consecutiveConsonants++;
                        if (consecutiveConsonants == ConsecutiveConsonantsThreshold) score += ConsecutiveConsonantsPenalty;
                        else if (consecutiveConsonants > ConsecutiveConsonantsThreshold) score += ExtraConsecutiveConsonantPenalty;

                        // Пара приголосних, якої немає в жодному слові мови — сильна ознака
                        // тексту, набраного не в тій розкладці.
                        if (checkBigrams && consecutiveConsonants >= 2 && char.IsLetter(lowerText[i - 1]) &&
                            !knownBigrams.Contains(lowerText.Substring(i - 1, 2)))
                        {
                            score += ImplausibleBigramPenalty;
                        }
                    }
                }
            }

            // Штраф за відсутність голосних у довгому слові
            if (lowerText.Length >= NoVowelsMinimumWordLength && vowelCount == 0)
            {
                score += NoVowelsPenalty;
            }

            // Штрафи за неприродні або заборонені комбінації
            if (!isEnglishLayout) // Українська
            {
                if (lowerText.StartsWith("ь") || lowerText.StartsWith("и")) score += ForbiddenCombinationPenalty;
                if (lowerText.Contains("ьь") || lowerText.Contains("йй") || lowerText.Contains("щщ")) score += ForbiddenCombinationPenalty;
                if (lowerText.Contains("ьы") || lowerText.Contains("ы") || lowerText.Contains("э") || lowerText.Contains("ё") || lowerText.Contains("ъ")) score += ForbiddenCombinationPenalty; // російські літери
            }
            else // Англійська
            {
                if (lowerText.Contains("zx") || lowerText.Contains("jq") || lowerText.Contains("pq") || lowerText.Contains("qx") || lowerText.Contains("xz") || lowerText.Contains("qv")) score += ForbiddenCombinationPenalty;
                if (lowerText.StartsWith("x") && lowerText.Length > 2 && !vowels.Contains(lowerText[1])) score += UnlikelyLeadingXPenalty;
            }

            return score;
        }

        public static bool IsLayoutWordCharacter(char character, bool isEnglishLayout)
        {
            if (char.IsLetter(character))
            {
                return true;
            }

            // В українській розкладці апостроф — частина слова ("пам'ять"), а не межа.
            // (В англійській він і так проходить нижче, бо мапиться на літеру "є".)
            if (!isEnglishLayout && character == Apostrophe)
            {
                return true;
            }

            if (isEnglishLayout && engToUkrMap.TryGetValue(character, out char mappedUkrainianCharacter))
            {
                return char.IsLetter(mappedUkrainianCharacter);
            }

            if (!isEnglishLayout && ukrToEngMap.TryGetValue(character, out char mappedEnglishCharacter))
            {
                return char.IsLetter(mappedEnglishCharacter);
            }

            return false;
        }

        private static bool MatchesFrequentWord(string word, bool isEnglishLayout)
        {
            string normalizedWord = NormalizeWord(word);
            if (string.IsNullOrEmpty(normalizedWord))
            {
                return false;
            }

            return isEnglishLayout
                ? commonEnglishWords.Contains(normalizedWord)
                : commonUkrainianWords.Contains(normalizedWord);
        }

        private static string NormalizeWord(string word)
        {
            return (word ?? string.Empty)
                .Trim()
                .Trim('\'', '"', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}')
                .ToLowerInvariant();
        }

        private static Dictionary<char, char> BuildMap()
        {
            // \u0421\u043F\u0435\u0440\u0448\u0443 \u043F\u0438\u0442\u0430\u0454\u043C\u043E \u0440\u0435\u0430\u043B\u044C\u043D\u0456 \u0440\u043E\u0437\u043A\u043B\u0430\u0434\u043A\u0438: \u043A\u043E\u0440\u0438\u0441\u0442\u0443\u0432\u0430\u0446\u044C\u043A\u0430 \u043C\u043E\u0436\u0435 \u0432\u0456\u0434\u0440\u0456\u0437\u043D\u044F\u0442\u0438\u0441\u044C \u0432\u0456\u0434
            // \u0441\u0442\u0430\u043D\u0434\u0430\u0440\u0442\u043D\u043E\u0457, \u0456 \u0442\u043E\u0434\u0456 \u0437\u0430\u0448\u0438\u0442\u0430 \u0432\u0456\u0434\u043F\u043E\u0432\u0456\u0434\u043D\u0456\u0441\u0442\u044C \u0441\u0438\u043C\u0432\u043E\u043B\u0456\u0432 \u0431\u0443\u043B\u0430 \u0431 \u043D\u0435\u043F\u0440\u0430\u0432\u0438\u043B\u044C\u043D\u043E\u044E.
            Dictionary<char, char> queriedMap = KeyboardLayoutMap.BuildEnglishToUkrainianMap();
            if (queriedMap.Count > 0)
            {
                return queriedMap;
            }

            return BuildFallbackMap();
        }

        private static Dictionary<char, char> BuildFallbackMap()
        {
            Dictionary<char, char> map = new Dictionary<char, char>();
            AddMappings(map, "qwertyuiop[]", "\u0439\u0446\u0443\u043A\u0435\u043D\u0433\u0448\u0449\u0437\u0445\u0457");
            AddMappings(map, "asdfghjkl;'", "\u0444\u0456\u0432\u0430\u043F\u0440\u043E\u043B\u0434\u0436\u0454");
            AddMappings(map, "zxcvbnm,.", "\u044F\u0447\u0441\u043C\u0438\u0442\u044C\u0431\u044E");
            AddMappings(map, "QWERTYUIOP{}", "\u0419\u0426\u0423\u041A\u0415\u041D\u0413\u0428\u0429\u0417\u0425\u0407");
            AddMappings(map, "ASDFGHJKL:\"", "\u0424\u0406\u0412\u0410\u041F\u0420\u041E\u041B\u0414\u0416\u0404");
            AddMappings(map, "ZXCVBNM<>", "\u042F\u0427\u0421\u041C\u0418\u0422\u042C\u0411\u042E");
            return map;
        }

        private static Dictionary<char, char> BuildReverseMap()
        {
            Dictionary<char, char> reverseMap = new Dictionary<char, char>();
            foreach (KeyValuePair<char, char> pair in engToUkrMap)
            {
                reverseMap[pair.Value] = pair.Key;
            }

            return reverseMap;
        }

        private static void AddMappings(IDictionary<char, char> map, string source, string target)
        {
            for (int index = 0; index < source.Length; index++)
            {
                map[source[index]] = target[index];
            }
        }

        private static int CountVowels(string word, HashSet<char> vowels)
        {
            int count = 0;
            foreach (char c in word)
            {
                if (vowels.Contains(c))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMappedChars(string word, Dictionary<char, char> map)
        {
            int count = 0;
            foreach (char c in word)
            {
                if (map.ContainsKey(c))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsAllCaps(string word, bool englishOnly)
        {
            if (string.IsNullOrEmpty(word)) return false;

            bool hasLetter = false;
            foreach (char c in word)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                    if (englishOnly)
                    {
                        if (c < 'A' || c > 'Z') return false;
                    }
                    else
                    {
                        if (char.IsLower(c)) return false;
                    }
                }
                else if (char.IsDigit(c))
                {
                    // allowed
                }
                else
                {
                    return false;
                }
            }
            return hasLetter;
        }

        private static bool ContainsLettersAndDigits(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            bool hasLetter = false;
            bool hasDigit = false;
            foreach (char c in word)
            {
                if (char.IsLetter(c)) hasLetter = true;
                else if (char.IsDigit(c)) hasDigit = true;

                if (hasLetter && hasDigit) return true;
            }
            return false;
        }
    }
}
