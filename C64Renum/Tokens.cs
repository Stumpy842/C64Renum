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
    internal class Tokens
    {
        public static string[] Tk = [ "END", "FOR", "NEXT", "DATA", "INPUT#", "INPUT", "DIM", "READ", "LET", "GOTO", "RUN", "IF",
            "RESTORE", "GOSUB", "RETURN", "REM", "STOP", "ON", "WAIT", "LOAD", "SAVE", "VERIFY", "DEF", "POKE", "PRINT#", "PRINT",
            "CONT", "LIST", "CLR", "CMD", "SYS", "OPEN", "CLOSE", "GET", "NEW", "TAB(", "TO", "FN", "SPC(", "THEN", "NOT", "STEP",
            "+", "−", "*", "/", "↑", "AND", "OR", ">", "=", "<", "SGN", "INT", "ABS", "USR", "FRE", "POS", "SQR", "RND", "LOG",
            "EXP", "COS", "SIN", "TAN", "ATN", "PEEK", "LEN", "STR$", "VAL", "ASC", "CHR$", "LEFT$", "RIGHT$", "MID$", "GO", "π" ];
            //  "π" = 0xFF (special case)

        public static string[] ResV = ["ST", "TI", "TI$"];

        public static int GetTokenExpandedLength(byte token)
        {
            if (token == 0xFF) return 1; // Special case for "π"
            token -= 0X80; // Convert to zero based index
            if (token < 0 || token > Tk.Length - 2) return 4; // Subtract and extra 1 to exclude "π", return conservative value
            return Tk[token].Length; // Works since Tk stores tokens in order of their byte value 0x80 to 0xCB
        }

        internal static void DisplayC64Characters(RichTextBox _rtb)
        {
            // Print Standard ASCII Range
            _rtb.AppendText("--- Standard ASCII Range ---\n");
            int count = 0;
            for (int i = 0x20; i <= 0x7E; i++)
            {
                _rtb.AppendText(((char)i).ToString());
                count++;
                if (count % 32 == 0) _rtb.AppendText("\n");
            }
            _rtb.AppendText("\n\n");

            // Print C64 PETSCII Mapping (Private Use Area)
            _rtb.AppendText("--- C64 PETSCII Mapping (Private Use Area) ---\n");
            count = 0;
            for (int i = 0xE000; i <= 0xE1FF; i++)
            {
                _rtb.AppendText(((char)i).ToString());
                count++;
                if (count % 32 == 0) _rtb.AppendText("\n");
            }
        }
    }
}

