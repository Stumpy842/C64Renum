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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    internal class Refactorer
    {
        private static Form1? form;
        private static readonly int lineLength = 40; // maximum line length for display in the form
        private static int myMaxLineLength;

        internal class RefactorResult
        {
            /// <summary>
            /// End address (>=0) on success; < 0 on error.
            /// </summary>
            public int Addr { get; set; }

            /// <summary>
            /// Optional error message when Addr < 0.
            /// </summary>
            public string? Message { get; set; }

            /// <summary>
            /// Resulting memory buffer (may be null on failure).
            /// </summary>
            public byte[]? Mem { get; set; }

            /// <summary>
            /// Resulting variable pairs (may be null on failure).
            /// </summary>
            public Dictionary<string, string>? VarPairs { get; set; }
        }

        internal class Chunk
        {
            public string Type { get; set; } = "";
            public List<byte>? ValBlob { get; set; }
            //public string? ValTarget { get; set; }
        }

        internal class Line
        {
            public string? Ln { get; set; }
            public List<Chunk>? Chunks { get; set; }
        }

        // Error codes returned by ParseLines:
        internal static class pErr
        {
            internal static readonly Dictionary<int, string> parseErrors = new()
            {
                { 0, "" }, // 0 means no errors
                { 1, "No variables found in file" },
                { 2, "Unexpected end of file" },
                { 3, "Malformed double-quote expression" },
                { 4, "Malformed DEF FN expression" },
                { 5, "Malformed FN expression" },
                { 6, "Undefined FN" }, 
                { 7, "Invalid line number" },
                { 8, "Chunks list is null" },
                { 9, $"Maximum text length of {myMaxLineLength} exceeded" },
                { 10, $"Maximum byte count of {myMaxLineLength} exceeded"}
            };

            internal static string pStr(int errorCode)
            {
                return parseErrors.TryGetValue(errorCode, out var message) ? message : $"Unknown Error (Code {errorCode})";
            }
        }

        internal static Task<RefactorResult> ScanBasicVarsAsync(byte[] mem, Dictionary<string, string> varPairs,
            Form1 inForm, CancellationToken token, bool rn = false)
        {
            void AllOver()
            {
                form!._refproc = false;
                if (form!.InvokeRequired)
                {
                    form.Invoke(new Action(() =>
                    { form.EndProc(); }));
                }
                else
                { form.EndProc(); }
            }

            inForm._refproc = true;

            return Task.Run(async () =>
            {
                RefactorResult Done(string message = "", int a = 0, byte[]? m = null, Dictionary<string, string>? vp = null) =>
                new() { Message = message, Addr = a, Mem = m, VarPairs = vp ?? varPairs };

                form = inForm;
                myMaxLineLength = form.optMaxLineLength;
                int addr = 2;
                int maxMem = mem.Length;
                if (maxMem < 10) { AllOver();  return Done(pErr.pStr(2)); } // Invalid file
                string badLn = "";
                List<byte> blob = [];
                List<byte> bytes = [];

                // Move varPairs to a local variable so it can be captured by the local function
                // Dictionary<string, string> localVarPairs = varPairs;

                async Task<int> ParseLines()
                {
                    void Rename(string oldName, int vs)
                    {
                        if (!varPairs.TryGetValue(oldName, out string? newName))
                        { return; }
                        int idx = oldName.IndexOf('[');
                        int len = idx != -1 ? idx : oldName.Length;
                        if (idx != -1)
                        {
                            newName = newName[..newName.IndexOf('[')];
                        }
                        blob.RemoveRange(vs, len);
                        blob.AddRange(Encoding.ASCII.GetBytes(newName));
                    }

                    void AppendRTB(string p)
                    {
                        if (form!.rtb.InvokeRequired)
                        {
                            form.rtb.Invoke(new Action(() =>
                            { form.rtb.AppendText(p + Environment.NewLine); }));
                        }
                        else
                        { form.rtb.AppendText(p + Environment.NewLine); }
                    }

                    List<Line> lines = [];
                    List<Chunk>? chunks;
                    List<byte> v;
                    int t;
                    List<string> myFN = [];
                    bool inDEF; bool inNum; // In function, in number
                    bool gotE; bool gotPM; // 'E' as scientific notation, + or - 
                    string vDummy;
                    int vStart;

                    while (addr < maxMem)
                    {
                        token.ThrowIfCancellationRequested();
                        int lineLink = mem[addr++] + (mem[addr++] << 8);
                        if (lineLink == 0) break;
                        chunks = [];
                        blob = [];
                        string ln = (mem[addr++] + (mem[addr++] << 8)).ToString();
                        Line line = new()
                        {
                            Ln = ln,
                            Chunks = chunks
                        };
                        //lineNoTable[ln] = lineNo;
                        //lineNo += step;
                        inDEF = false; inNum = false; gotE = false; gotPM = false;
                        vDummy = "";
                        v = [];
                        byte b = mem[addr++];
                        while (b != 0)
                        {
                            token.ThrowIfCancellationRequested();
                            blob.Add(b);
                            switch (b)
                            {
                                case 0x3A: // Full colon - Reset flags on ':' char
                                    inDEF = false; inNum = false; gotE = false; gotPM = false;
                                    vDummy = "";
                                    break;

                                case 0x83: //DATA (read to next ':' or EOL
                                    while (addr < maxMem && !(mem[addr] == 0x3A || mem[addr] == 0))
                                    {
                                        blob.Add(mem[addr++]);
                                    }
                                    break;
                                case 0x8F:// REM (read rest of line)
                                    while (addr < maxMem && mem[addr] != 0)
                                    {
                                        blob.Add(mem[addr++]);
                                    }
                                    break;

                                case 0x22: // Double quote (read up to next double quote)
                                    if (addr < maxMem)
                                    {
                                        b = mem[addr++];
                                        while (b != 0)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            blob.Add(b);
                                            if (b == 0x22) break;
                                            if (addr >= maxMem)
                                            { badLn = ln; return 3; } // EOF with missing double quote
                                            b = mem[addr++];
                                        }
                                        if (b == 0) addr--;
                                        break;
                                    }
                                    else { badLn = ln; return 2; } // Unexpected EOF

                                /*                            case 0x86: // DIM
                                                                t = addr;
                                                                while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                                                if (t >= maxMem || !(mem[t] >= 0x41 && mem[t] <= 0x5A)) return 5;
                                                                while (addr <= t) blob.Add(mem[addr++]);
                                                                // Found A-Z
                                                                v.Clear();
                                                                v.Add(mem[t]);
                                                                while (addr <= t) blob.Add(mem[addr++]);
                                                                b = mem[addr++];
                                                                while ((b >= 0x30 && b <= 0x39) || (b >= 0x41 && b <= 0x5A))
                                                                {
                                                                    // b is Alphanumeric
                                                                    v.Add(b);
                                                                    b = mem[addr++];
                                                                    blob.Add(b);
                                                                }
                                                                addr--;
                                                                //blob.RemoveAt(blob.Count - 1);
                                                                if (v.Count > 0)
                                                                {
                                                                    string k = Encoding.ASCII.GetString([.. v]) + "[ARRAY]";
                                                                    localVarPairs[k] = "[ARRAY]";
                                                                }
                                                                break;
                                */

                                case 0xA5: // FN - Syntax & undefined FN checking
                                    t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                    if (t >= maxMem || !(mem[t] >= 0x41 && mem[t] <= 0x5A))
                                    { badLn = ln; return 5; } // Error if not A-Z
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    // Found A-Z
                                    vStart = blob.Count - 1;
                                    v.Clear();
                                    v.Add(mem[t]);
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    b = mem[addr++];
                                    while ((b >= 0x30 && b <= 0x39) || (b >= 0x41 && b <= 0x5A) || b == 0x20)
                                    {
                                        // b is Alphanumeric or space
                                        v.Add(b);
                                        blob.Add(b);
                                        b = mem[addr++];
                                    }
                                    addr--;
                                    //blob.RemoveAt(blob.Count - 1);
                                    if (v.Count > 0)
                                    {
                                        string k = Encoding.ASCII.GetString([.. v]).Trim();
                                        if (rn) { Rename(k + "[FN]", vStart); }
                                        else
                                        {
                                            // check for FN used before being defined
                                            if (!myFN.Contains(k)) { badLn = ln; return 6; }
                                        }
                                        // k has already been added to localVarPairs in the DEF section
                                    }
                                    break;

                                case 0x96: // DEF (read ahead and test for FN)
                                    t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                    if (t >= maxMem || mem[t] != 0xA5)
                                    { badLn = ln; return 4; } // Error if not FN
                                                              // Found FN
                                    inDEF = true;
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    // Look for FN name
                                    t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                    if (t >= maxMem || !(mem[t] >= 0x41 && mem[t] <= 0x5A))
                                    { badLn = ln; return 4; } // Error if function name is not A-Z
                                                              // Found A-Z
                                    v.Clear();
                                    v.Add(mem[t]);
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    vStart = blob.Count - 1;
                                    b = mem[addr++];
                                    // Capture FN name
                                    while ((b >= 0x30 && b <= 0x39) || (b >= 0x41 && b <= 0x5A) || b == 0x20)
                                    {
                                        // b is Alphanumeric or space
                                        v.Add(b);
                                        blob.Add(b);
                                        b = mem[addr++];
                                    }
                                    addr--;
                                    //blob.RemoveAt(blob.Count - 1);
                                    if (v.Count > 0)
                                    {
                                        string k = Encoding.ASCII.GetString([.. v]).Trim();
                                        if (rn) { Rename(k + "[FN]", vStart); }
                                        else
                                        {
                                            // Add FN name + suffix to Dictionary
                                            varPairs[k + "[FN]"] = "";
                                            // Add FN name to list of functions
                                            if (!myFN.Contains(k)) myFN.Add(k);
                                        }
                                    }
                                    // Read ahead and test for '(' skipping blanks 
                                    t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                    if (t >= maxMem || mem[t] != 0x28)
                                    { badLn = ln; return 4; } // Error - Missing '('
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    // Look for Dummy var name
                                    t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // Skip any blanks
                                                                              // Must be A-Z
                                    if (t > maxMem || !(mem[t] >= 0x41 && mem[t] <= 0x5A))
                                    { badLn = ln; return 4; } // Error - Non-Alpha char after '('
                                                              // Found A-Z
                                    v.Clear();
                                    v.Add(mem[t]);
                                    while (addr <= t) blob.Add(mem[addr++]);
                                    vStart = blob.Count - 1;
                                    b = mem[addr++];
                                    // Capture Dummy var name
                                    while ((b >= 0x30 && b <= 0x39) || (b >= 0x41 && b <= 0x5A) || b == 0x20)
                                    {
                                        // b is Alphanumeric or space
                                        v.Add(b);
                                        b = mem[addr++];
                                        blob.Add(b);
                                    }
                                    addr--;
                                    if (v.Count > 0)
                                    {
                                        string k = Encoding.ASCII.GetString([.. v]).Trim();
                                        if (rn) { Rename(k + "[DUMMY]", vStart); }
                                        else
                                        {
                                            // Add Dummy var name + suffix to Dictionary
                                            varPairs[k + "[DUMMY]"] = "";
                                            // Save Dummy var name
                                            vDummy = k;
                                        }
                                    }
                                    break;

                                default: // Check for variable name and store it
                                    if (b >= 0x30 && b <= 0x39) // Check for number
                                    {
                                        // Found 0-9
                                        while ((b >= 0x30 && b <= 0x39) || b == 0x20 ||
                                            (gotE && (b == 0xAA || b == 0xAB)) || (inNum && b == 0x45))
                                        // While b is 0-9, Space, '+' or '-' after 'E', or 'E' in a number
                                        {
                                            inNum = true;
                                            if (b == 0x45)
                                            {
                                                if (gotE) break;
                                                gotE = true;
                                            }
                                            if (b == 0xAA || b == 0xAB) // + or - token
                                            {
                                                if (gotPM) break;
                                                gotPM = true;
                                            }
                                            b = mem[addr++];
                                            blob.Add(b);
                                        }
                                        blob.RemoveAt(blob.Count - 1);
                                        addr--;
                                        break;
                                    }

                                    // Not a number, reset flags
                                    inNum = false;
                                    gotE = false;
                                    gotPM = false;
                                    if (b >= 0x41 && b <= 0x5A) // Check for letter
                                    {
                                        // Found A-Z
                                        v.Clear();
                                        vStart = blob.Count - 1;
                                        while ((b >= 0x30 && b <= 0x39) || (b >= 0x41 && b <= 0x5A) || b == 0x20)
                                        {
                                            // b is Alphanumeric or Space
                                            v.Add(b);
                                            b = mem[addr++];
                                            blob.Add(b);
                                        }
                                        blob.RemoveAt(blob.Count - 1);
                                        while (addr < maxMem && b == 0x20)
                                        {
                                            blob.Add(b);
                                            b = mem[addr++];
                                        }
                                        // Check b for '$', '%' and '('
                                        if (b == 0x24 || b == 0x25 || b == 0x28)
                                        {
                                            v.Add(b);
                                            blob.Add(b);
                                            b = mem[addr++];
                                        }
                                        if (v.Count > 0)
                                        {
                                            string k = Encoding.ASCII.GetString([.. v]).Trim();
                                            if (rn) { Rename(k, vStart); }
                                            else
                                            {
                                                // If in a line with a DEF FN, Check for match with local Dummy var, skip if found
                                                if (!(inDEF && string.Equals(k, vDummy, StringComparison.OrdinalIgnoreCase)))
                                                { varPairs[k] = ""; }
                                            }
                                        }
                                        addr--;
                                    }
                                    break;
                            } // End switch (b)

                            if (addr >= maxMem) { badLn = ln; return 2; }
                            b = mem[addr++];
                        } // End while (b != 0)

                        if (blob.Count > 0)
                        {
                            chunks.Add(new Chunk { Type = "blob", ValBlob = [.. blob] });
                        }
                        lines.Add(line);
                    } // End while (addr < maxMem)

                    if (rn)
                    {
                        PRB _PRB = new(form!, token);
                        bytes.AddRange(mem[0], mem[1]); // Add BASIC start address to bytes
                        int mStart = mem[0] + (mem[1] << 8) - 2;
                        int cOut; // character count for line wrapping
                        Line aline;
                        int linkAddr;
                        int nextLineAddr;
                        int lastGoodLineNo = 0;
                        string pString;
                        string lineNoStr;
                        for (int i = 0, max = lines.Count; i < max; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            aline = lines[i];
                            pString = "";
                            cOut = 0;
                            linkAddr = bytes.Count;
                            bytes.AddRange([0, 0]); // Placeholder for link address
                            if (!int.TryParse(aline.Ln, out int lineNo))
                            {
                                badLn = lastGoodLineNo.ToString();
                                return 7; // Invalid line number
                            }
                            lastGoodLineNo = lineNo;
                            bytes.Add((byte)(lineNo & 0xFF));
                            bytes.Add((byte)((lineNo >> 8) & 0xFF));
                            // add the new line number to the printable string for display in the form
                            lineNoStr = lineNo.ToString() + " ";
                            foreach (char c in lineNoStr)
                            {
                                token.ThrowIfCancellationRequested();
                                pString += c;
                                cOut++;
                                if (form!.optWrap && (cOut >= lineLength))
                                {
                                    pString += '\n';
                                    cOut = 0;
                                }
                            }
                            if (aline.Chunks is null)
                            {
                                badLn = lineNo.ToString();
                                return 8; // Chunks list is null
                            }
                            foreach (var chunk in aline.Chunks!)
                            {
                                token.ThrowIfCancellationRequested();
                                if (chunk.Type == "blob" && chunk.ValBlob != null)
                                {
                                    bytes.AddRange(chunk.ValBlob);
                                    // add the blob to the printable string for display in the form
                                    pString += _PRB.PrtBlob(chunk.ValBlob, lineLength, ref cOut);
                                }
                            }

                            // --- BEGIN LINE LIMIT CODE CHECK ---
                            // 1. Character Check: Count raw text characters excluding display-only formatting newlines
                            int totalCharacters = pString.Replace("\n", "").Length;
                            if (totalCharacters > myMaxLineLength)
                            {
                                badLn = lineNo.ToString();
                                return 9; // Character limit exceeded
                            }

                            // 2. Byte Check: Measure the line's memory footprint
                            // Standard overhead: 2 bytes (link) + 2 bytes (line number) + 1 byte (trailing zero) = 5 bytes
                            int totalLineBytes = bytes.Count - linkAddr + 1; // +1 accounts for the trailing EOL zero byte
                            if (totalLineBytes > myMaxLineLength + 5)
                            {
                                badLn = lineNo.ToString();
                                return 10; // Byte limit exceeded
                            }
                            // --- END LINE LIMIT CODE CHECK ---

                            bytes.Add(0); // EOL marker for BASIC line
                            AppendRTB(pString);
                            // Update link address
                            nextLineAddr = bytes.Count + mStart;
                            bytes[linkAddr] = (byte)(nextLineAddr & 0xFF);
                            bytes[linkAddr + 1] = (byte)((nextLineAddr >> 8) & 0xFF);

                            if (form!.optDemo) // Delay for demonstration purposes
                            { await Task.Delay(form.optDemoSpeed * 250, token); }
                        }

                        AppendRTB("---- End of BASIC Program ----");
                        bytes.AddRange([0, 0]); // Add final 0, 0 to indicate end of BASIC program
                        return 0; // Success
                    } // End if (rn)

                    return (varPairs == null || varPairs.Count < 1) ? 1 : 0;
                }

                ///////////////////////////////
                // Main function starts here //
                ///////////////////////////////
                if (rn)
                {
                    if (form.rtb.InvokeRequired)
                    {
                        form.rtb.Invoke(new Action(() =>
                        { form.rtb.Clear(); }));
                    }
                    else
                    { form.rtb.Clear(); }
                }

                int pResult = await ParseLines();
                AllOver();
                return Done(pErr.pStr(pResult) + (badLn != "" ? $" in line {badLn}." : ""), addr, [.. bytes], varPairs);
            }, token);
        }
    }
}
