using System;
using System.Text;

namespace KeyboardLayoutSwitcher
{
    public class WordTracker
    {
        private StringBuilder currentWord = new StringBuilder();

        public int Length => currentWord.Length;

        public string Current => currentWord.ToString();

        public void AppendChar(char ch)
        {
            currentWord.Append(ch);
        }

        public bool TryGetWordAtBoundary(out string word)
        {
            if (currentWord.Length == 0)
            {
                word = null;
                return false;
            }

            word = currentWord.ToString();
            return true;
        }

        public void Clear()
        {
            currentWord.Clear();
        }

        public void RemoveLastChar()
        {
            if (currentWord.Length > 0)
            {
                currentWord.Length--;
            }
        }

        public char GetLastChar()
        {
            if (currentWord.Length > 0)
            {
                return currentWord[currentWord.Length - 1];
            }
            return '\0';
        }

        public bool IsEmpty => currentWord.Length == 0;
    }
}
