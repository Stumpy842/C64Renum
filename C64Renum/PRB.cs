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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    internal class PRB
    {
        private Form1? myForm;
        private CancellationToken myToken;

        internal PRB(Form1 inForm, CancellationToken inToken)
        {
            myForm = inForm;
            myToken = inToken;
        }

        internal string PrtBlob(List<byte> chunkBlob, int length, ref int count)
        {
            int localCount = count;

            void AddNL(ref List<char> pChars)
            {
                localCount++;
                if (myForm!.optWrap && (localCount >= length))
                {
                    pChars.Add('\n');
                    localCount = 0;
                }
            }

            List<char> printableChars = [];
            bool inQuote = false;
            foreach (byte b in chunkBlob)
            {
                myToken.ThrowIfCancellationRequested();
                // Handle case for string inside quotes, which can contain any byte value,
                // including tokens and non-printable characters.
                if (b == 0x22) // quote character
                {
                    inQuote = !inQuote;
                    printableChars.Add((char)b);
                    AddNL(ref printableChars);
                    continue;
                }
                if (inQuote)
                {
                    if (b < 0x20) // special case for control characters inside quotes
                    {
                        printableChars.Add((char)(b + 0xEE80));
                        AddNL(ref printableChars);
                        continue;
                    }
                    if (b > 0x7F) // Control characters above 0x7F inside quotes
                    {
                        printableChars.Add((char)(b + 0xEE40));
                        AddNL(ref printableChars);
                        continue;
                    }
                    printableChars.Add((char)b);
                    AddNL(ref printableChars);
                    continue;
                }

                if (b >= 0x20 && b <= 0x7F) // printable ASCII range
                {
                    printableChars.Add((char)b);
                    AddNL(ref printableChars);
                }
                else
                {
                    string tkn;
                    if ((b >= 0x80 && b <= 0xCB) || b == 0xFF) // Token range
                    {
                        if (b == 0xFF) { tkn = "π"; }
                        else { tkn = Tokens.Tk[b - 0x80]; }

                        for (int i = 0; i < tkn.Length; i++)
                        {
                            printableChars.Add(tkn[i]);
                            AddNL(ref printableChars);
                        }
                        continue;
                    }
                    else // Non-printable character
                    {
                        printableChars.Add('?');
                        AddNL(ref printableChars);
                    }
                }
            }
            count = localCount;
            return new string([.. printableChars]);
        }
    }
}
