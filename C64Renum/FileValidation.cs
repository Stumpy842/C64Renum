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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C64Renum
{
    public static class FileValidation
    {
        // Windows legacy path limit constant (MAX_PATH)
        private const int MaxPathLength = 260;

        // Initialized once when the application loads for maximum performance
        private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
        { "CON", "PRN", "AUX", "NUL",
           "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
           "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Validates a single file name component (e.g., "file.txt").
        /// </summary>
        public static bool IsValidName(string n, ref string errMsg)
        {
            // 1. Reject null, empty, or pure whitespace strings
            if (string.IsNullOrWhiteSpace(n))
            {
                errMsg = "Empty output filename.";
                return false;
            }

            // 2. Reject if any operating system invalid characters are found
            if (n.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                errMsg = $"Invalid output filename '{n}'.";
                return false;
            }

            // 3. Extract the name without extension
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(n);

            // 4. TryGetValue searches case-insensitively and extracts the exact string from the set if found
            if (ReservedNames.TryGetValue(nameWithoutExtension, out string? match))
            {
                errMsg = $"Output filename contains reserved system name '{match}'.";
                return false;
            }

            // Passed all standalone name checks!
            return true;
        }

        /// <summary>
        /// Validates the entire output path for invalid directory/file characters, 
        /// reserved names, and the Windows 260-character limit.
        /// </summary>

        public static bool IsValidFullPath(string fullPath, out string errMsg)
        {
            errMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                errMsg = "Empty output pathname.";
                return false;
            }

            // 1. Strictly check the length of exactly what the user typed
            if (fullPath.Length >= MaxPathLength)
            {
                errMsg = $"Output pathname too long (> {MaxPathLength} characters).";
                return false;
            }

            try
            {
                // 2. Optional: Enforce that the user must provide a root drive (e.g., C:\...)
                // Remove this check if you actually want to allow relative paths.
                if (!Path.IsPathRooted(fullPath))
                {
                    errMsg = "Relative pathnames not allowed.";
                    return false;
                }

                // 3. Extract the parts strictly by parsing the raw string
                string directory = Path.GetDirectoryName(fullPath)!;
                string fileName = Path.GetFileName(fullPath);

                // 4. Validate directory characters
                if (!string.IsNullOrEmpty(directory) && directory.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    errMsg = $"Invalid output pathname '{directory}'.";
                    return false;
                }

                // 5. Validate the filename using our robust method
                return IsValidName(fileName, ref errMsg);
            }
            catch (Exception)
            {
                // Catches catastrophic structural path failures (like embedded null characters)
                errMsg = $"Invalid output pathname '{fullPath}'.";
                return false;
            }
        }
    }
}
