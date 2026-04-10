using System;
using System.Collections.Generic;
using System.Text;

namespace KeyboardLayoutSwitcher
{
    public static class KeyMapper
    {
        private static readonly HashSet<char> englishVowels = new HashSet<char>
        {
            'a', 'e', 'i', 'o', 'u', 'y', 'A', 'E', 'I', 'O', 'U', 'Y'
        };

        private static readonly HashSet<char> ukrainianVowels = new HashSet<char>
        {
            '�', '�', '�', '�', '�', '�', '�', '�', '�', '�',
            '�', '�', '�', '�', '�', '�', '�', '�', '�', '�'
        };

        private static readonly Dictionary<char, char> engToUkrMap = new Dictionary<char, char>()
        {
            {'q', '�'}, {'w', '�'}, {'e', '�'}, {'r', '�'}, {'t', '�'},
            {'y', '�'}, {'u', '�'}, {'i', '�'}, {'o', '�'}, {'p', '�'},
            {'[', '�'}, {']', '�'}, {'a', '�'}, {'s', '�'}, {'d', '�'},
            {'f', '�'}, {'g', '�'}, {'h', '�'}, {'j', '�'}, {'k', '�'},
            {'l', '�'}, {';', '�'}, {'\'', '�'}, {'z', '�'}, {'x', '�'},
            {'c', '�'}, {'v', '�'}, {'b', '�'}, {'n', '�'}, {'m', '�'},
            {',', '�'}, {'.', '�'},
            // ����� �����
            {'Q', '�'}, {'W', '�'}, {'E', '�'}, {'R', '�'}, {'T', '�'},
            {'Y', '�'}, {'U', '�'}, {'I', '�'}, {'O', '�'}, {'P', '�'},
            {'{', '�'}, {'}', '�'}, {'A', '�'}, {'S', '�'}, {'D', '�'},
            {'F', '�'}, {'G', '�'}, {'H', '�'}, {'J', '�'}, {'K', '�'},
            {'L', '�'}, {':', '�'}, {'"', '�'}, {'Z', '�'}, {'X', '�'},
            {'C', '�'}, {'V', '�'}, {'B', '�'}, {'N', '�'}, {'M', '�'},
            {'<', '�'}, {'>', '�'}
        };

        private static readonly Dictionary<char, char> ukrToEngMap = new Dictionary<char, char>()
        {
            {'�', 'q'}, {'�', 'w'}, {'�', 'e'}, {'�', 'r'}, {'�', 't'},
            {'�', 'y'}, {'�', 'u'}, {'�', 'i'}, {'�', 'o'}, {'�', 'p'},
            {'�', '['}, {'�', ']'}, {'�', 'a'}, {'�', 's'}, {'�', 'd'},
            {'�', 'f'}, {'�', 'g'}, {'�', 'h'}, {'�', 'j'}, {'�', 'k'},
            {'�', 'l'}, {'�', ';'}, {'�', '\''}, {'�', 'z'}, {'�', 'x'},
            {'�', 'c'}, {'�', 'v'}, {'�', 'b'}, {'�', 'n'}, {'�', 'm'},
            {'�', ','}, {'�', '.'},
            // ����� �����
            {'�', 'Q'}, {'�', 'W'}, {'�', 'E'}, {'�', 'R'}, {'�', 'T'},
            {'�', 'Y'}, {'�', 'U'}, {'�', 'I'}, {'�', 'O'}, {'�', 'P'},
            {'�', '{'}, {'�', '}'}, {'�', 'A'}, {'�', 'S'}, {'�', 'D'},
            {'�', 'F'}, {'�', 'G'}, {'�', 'H'}, {'�', 'J'}, {'�', 'K'},
            {'�', 'L'}, {'�', ':'}, {'�', '"'}, {'�', 'Z'}, {'�', 'X'},
            {'�', 'C'}, {'�', 'V'}, {'�', 'B'}, {'�', 'N'}, {'�', 'M'},
            {'�', '<'}, {'�', '>'}
        };

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

        public static bool IsWrongLayout(string word, bool isEnglishLayout)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
            {
                return false;
            }

            string convertedWord = ConvertWord(word, isEnglishLayout);

            int sourceVowelCount = CountVowels(word, isEnglishLayout ? englishVowels : ukrainianVowels);
            int convertedVowelCount = CountVowels(convertedWord, isEnglishLayout ? ukrainianVowels : englishVowels);

            int sourceMappedChars = CountMappedChars(word, isEnglishLayout ? engToUkrMap : ukrToEngMap);
            int mappedThreshold = Math.Max(2, word.Length - 1);

            // Consider layout as wrong only when:
            // 1) almost all characters can be keyboard-mapped to the opposite layout,
            // 2) converted word looks more pronounceable than the source word.
            return sourceMappedChars >= mappedThreshold && convertedVowelCount > sourceVowelCount;
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
