using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeyboardLayoutSwitcher
{
    public static class KeyMapper
    {
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

        static KeyMapper()
        {
            LoadDictionaryFile("Dictionaries\\en.txt", commonEnglishWords);
            LoadDictionaryFile("Dictionaries\\uk.txt", commonUkrainianWords);
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

        public static bool IsWrongLayout(string word, bool isEnglishLayout, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string convertedWord = ConvertWord(word, isEnglishLayout);
            int sourceMappedChars = CountMappedChars(word, isEnglishLayout ? engToUkrMap : ukrToEngMap);

            if (MatchesFrequentWord(word, isEnglishLayout))
            {
                return false; // Already a valid word in current layout, don't convert it!
            }

            if (settings != null && settings.IgnoredWords != null && settings.IgnoredWords.Contains(word))
            {
                return false; // User explicitly ignored this word
            }

            if (MatchesFrequentWord(convertedWord, !isEnglishLayout) &&
                sourceMappedChars >= Math.Max(1, word.Length - 1))
            {
                return true;
            }

            int minimumWordLength = 2; // override user settings to ensure short valid words work
            if (word.Length < minimumWordLength)
            {
                return false;
            }

            int sourceVowelCount = CountVowels(word, isEnglishLayout ? englishVowels : ukrainianVowels);
            int convertedVowelCount = CountVowels(convertedWord, isEnglishLayout ? ukrainianVowels : englishVowels);

            int minimumMappedPercent = Math.Max(1, Math.Min(100, settings?.MinimumMappedPercent ?? 80));
            int minimumVowelDelta = settings?.MinimumVowelDelta ?? 0;
            int mappedThreshold = (int)Math.Ceiling(word.Length * minimumMappedPercent / 100.0);

            // Consider layout as wrong only when:
            // 1) almost all characters can be keyboard-mapped to the opposite layout,
            // 2) converted word looks more pronounceable than the source word.
            return sourceMappedChars >= mappedThreshold &&
                   convertedVowelCount - sourceVowelCount >= minimumVowelDelta;
        }

        public static bool IsLayoutWordCharacter(char character, bool isEnglishLayout)
        {
            if (char.IsLetter(character))
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
    }
}
