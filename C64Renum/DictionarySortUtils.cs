/*
C64Renum C64 Basic program renumber/refactor utility
© 2026 Steven J. Stover

This program is free software: you can redistribute it and/or modify it under the terms of the
GNU Affero General Public License as published by the Free Software Foundation, either version 3 
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even
the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General
Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program.
If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace C64Renum
{
    internal static class DictionarySortUtils
    {
        // Returns a list of pairs sorted by the custom key comparison.
        // Keys that share the same first two alphanumeric characters are ordered
        // so entries whose character immediately after the second alphanumeric
        // is alphanumeric or end-of-string come before those whose character is non-alphanumeric (e.g. '$').
        public static List<KeyValuePair<string, string>> SortByKeyFirstTwoAlnumThenSuffix(Dictionary<string, string> input)
        {
            List<KeyValuePair<string, string>> list = [.. input];
            list.Sort((x, y) =>
            {
                int cmp = CompareKeys(x.Key, y.Key);
                if (cmp != 0) return cmp;
                // stable tie-breaker: full key ordinal-ignorecase then value
                int tie = string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
                if (tie != 0) return tie;
                return string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
            });
            return list;
        }

        private static int CompareKeys(string a, string b)
        {
            // 1. Compare the first alphanumeric character (e.g., 'A')
            string aFirst1 = GetFirstNAlnum(a, 1);
            string bFirst1 = GetFirstNAlnum(b, 1);
            int cmp = string.Compare(aFirst1, bFirst1, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;

            // 2. Extract the true full suffix type tag cleanly (Multi-character priority)
            string aSuffix = GetVariableSuffix(a);
            string bSuffix = GetVariableSuffix(b);

            // 3. Rank the suffix: Missing (0), '$' (1), '%' (2), '(' (3), '[FN]' (4), '[DUMMY]' (5), Others (6)
            int aSuffixRank = GetSuffixRank(aSuffix);
            int bSuffixRank = GetSuffixRank(bSuffix);

            if (aSuffixRank != bSuffixRank) return aSuffixRank - bSuffixRank;

            // 4. Fallback to full-key ordinal comparison if they share the same suffix type
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetVariableSuffix(string s)
        {
            // --- FIX: Handle multi-character structural tags first ---
            if (s.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) return "[DUMMY]";
            if (s.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) return "[FN]";

            // Find where the alphanumeric prefix ends to locate single-char legacy tokens
            for (int i = 0; i < s.Length; i++)
            {
                // If we hit a non-alphanumeric character, that is our suffix ($, %, (, etc.)
                if (!char.IsLetterOrDigit(s[i]))
                {
                    return s[i].ToString();
                }
            }
            return string.Empty; // No suffix found (plain variable like AB or ABC)
        }

        private static int GetSuffixRank(string suffix)
        {
            if (string.IsNullOrEmpty(suffix)) return 0; // No suffix comes first (e.g., AB, ABC)

            return suffix.ToUpperInvariant() switch
            {
                "$" => 1,       // String variables next (e.g., AB$)
                "%" => 2,       // Integer variables next (e.g., AB%)
                "(" => 3,       // Array variables next (e.g., AB()
                "[FN]" => 4,    // Function templates next (e.g., AB[FN])
                "[DUMMY]" => 5, // Local variables next (e.g., AB[DUMMY])
                _ => 6          // Any fallback/other unexpected symbols
            };
        }

        private static string GetFirstNAlnum(string s, int n)
        {
            StringBuilder sb = new(n);
            for (int i = 0; i < s.Length && sb.Length < n; i++)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
            }
            return sb.ToString();
        }

        // Returns index of the character immediately after the nth alphanumeric character.
        // If fewer than n alnum chars exist, returns s.Length.
        private static int IndexAfterNthAlnum(string s, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsLetterOrDigit(s[i]))
                {
                    count++;
                    if (count == n)
                    {
                        return i + 1;
                    }
                }
            }
            return s.Length;
        }

        // Returns true when the character at idx is present and is non-alphanumeric.
        // End-of-string is treated as not non-alphanumeric (preferred group).
        private static bool IsNonAlnumAt(string s, int idx)
        {
            if (idx >= s.Length) return false;
            return !char.IsLetterOrDigit(s[idx]);
        }
    }
}
