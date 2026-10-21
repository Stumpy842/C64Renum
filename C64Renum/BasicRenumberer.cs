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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace C64Renum
{
    internal static class BasicRenumberer
    {
        private static readonly int lineLength = 40; // maximum line length for display in the form
        private static string badLine = "";
        private static int myMaxLineLength;

        internal class RenumberResult
        {
            /// <summary>
            /// End address (>=0) on success; -1 to -3 on error.
            /// </summary>
            public int Addr { get; set; }

            /// <summary>
            /// Optional error message when Addr == -1 to -3.
            /// </summary>
            public string? Message { get; set; }

            /// <summary>
            /// Resulting memory buffer (may be null on failure).
            /// </summary>
            public byte[]? Mem { get; set; }
        }

        internal class Chunk
        {
            public string? Type { get; set; } = "";
            public List<byte>? ValBlob { get; set; }
            public string? ValTarget { get; set; }
        }

        internal class Line
        {
            public string? Ln { get; set; }
            public List<Chunk>? Chunks { get; set; }
        }

        private static Form1? myForm;

        // renumber BASIC in provided memory array byte[]
        // processes GOTO, GOSUB, GO TO, ON GOTO|GOSUB|GO TO, THEN
        // New cancellable async API
        internal static Task<RenumberResult> RenumberBasicAsync(byte[] mem, int startNo, int step, Form1 form, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();
                myForm = form;
                myMaxLineLength = myForm.optMaxLineLength;
                Dictionary<string, int> lineNoTable = [];
                List<Line> lines = [];
                int maxLineNo = 63999;
                int basicStart;
                int mStart;
                int addr = 2;
                int lineNo = startNo;
                int maxMem = mem.Length;
                List<Chunk>? chunks;
                List<byte>? blob;
                List<byte> bytes = [];

                // scans for a jump target,
                // generates a target entry and a new blob in current chunks,
                // any leading blanks are added to the current blob
                void ParseLineTarget(bool maybeList)
                {
                    token.ThrowIfCancellationRequested();
                    while (addr < maxMem && mem[addr] == 0x20)
                    {
                        token.ThrowIfCancellationRequested();
                        blob!.Add(0x20);
                        addr++;
                    }
                    // parse ASCII to string, ignoring any blanks
                    string n = "";
                    if (addr < maxMem)
                    {
                        byte c = mem[addr];
                        while ((c >= 0x30 && c <= 0x39) || c == 0x20)
                        {
                            token.ThrowIfCancellationRequested();
                            if (c != 0x20) n += (char)c;
                            addr++;
                            if (addr >= maxMem) break;
                            c = mem[addr];
                        }

                        // if we found a number, push chunks and open a new blob
                        if (n != "")
                        {
                            chunks!.Add(new Chunk { Type = "blob", ValBlob = [.. blob] });
                            chunks.Add(new Chunk { Type = "target", ValTarget = n });
                            blob.Clear();
                            // scan for any comma (ON GOTO|GOSUB|GO TO)
                            if (maybeList && addr < maxMem && mem[addr] == 0x2C)
                            {
                                token.ThrowIfCancellationRequested();
                                blob.Add(0x2C);
                                addr++;
                                ParseLineTarget(true);
                            }
                        }
                    }
                }

                // parse and split lines to chunks of blobs and line targets
                // stores parsed chunks in array lines,
                // generates a reference of old and new line numbers in lineNoTable.
                void ParseSplit()
                {
                    while (addr < maxMem)
                    {
                        token.ThrowIfCancellationRequested();
                        int lineLink = mem[addr++] + (mem[addr++] << 8);
                        if (lineLink == 0) break;
                        chunks = [];
                        blob = [];
                        string ln = (mem[addr++] + (mem[addr++] << 8)).ToString();
                        byte b = mem[addr++];
                        Line line = new()
                        {
                            Ln = ln,
                            Chunks = chunks
                        };
                        lineNoTable[ln] = lineNo;
                        lineNo += step;
                        while (b != 0)
                        {
                            token.ThrowIfCancellationRequested();
                            blob.Add(b);
                            switch (b)
                            {
                                case 0x8F: // REM (read rest of line)
                                    while (addr < maxMem && mem[addr] != 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        blob.Add(mem[addr++]);
                                    }
                                    break;
                                case 0x22: // quote (read up to next quote)
                                    if (addr < maxMem)
                                    {
                                        b = mem[addr++];
                                        while (b != 0)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            blob.Add(b);
                                            if (b == 0x22) break;
                                            if (addr >= maxMem) break;
                                            b = mem[addr++];
                                        }
                                        if (b == 0) addr--;
                                        break;
                                    }
                                    break;
                                case 0x89: // GOTO
                                case 0x8D: // GOSUB
                                    ParseLineTarget(true);
                                    break;
                                case 0xA7: // THEN
                                    ParseLineTarget(false);
                                    break;
                                case 0xCB: // GO (read ahead and test for TO)
                                    int t = addr;
                                    while (t < maxMem && mem[t] == 0x20) t++; // skip any blanks
                                    if (t >= maxMem || mem[t] != 0xA4) break;
                                    while (addr <= t)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        blob.Add(mem[addr++]);
                                    }
                                    ParseLineTarget(true);
                                    break;
                            }
                            if (addr >= maxMem) break;
                            b = mem[addr++];
                        }
                        if (blob.Count > 0) chunks.Add(new Chunk { Type = "blob", ValBlob = [.. blob] });
                        lines.Add(line);
                    }
                }

                //////////////////////////////////////////////////////////////////////////////////////
                // ReassembleAsync (local function inside RenumberBasicAsync, uses outer variables) //
                //////////////////////////////////////////////////////////////////////////////////////
                async Task<int> ReassembleAsync()
                {
                    void AppendRTB(string p)
                    {
                        if (form.rtb.InvokeRequired)
                        {
                            form.rtb.Invoke(new Action(() =>
                            { form.rtb.AppendText(p + Environment.NewLine); }));
                        }
                        else
                        { form.rtb.AppendText(p  + Environment.NewLine); }
                    }

                    token.ThrowIfCancellationRequested();
                    int cOut; // character count for line wrapping
                    Line currLine;
                    int linkAddr;
                    int mLink;
                    string pString;
                    string lineNoStr;
                    PRB _PRB = new(myForm!, token);

                    for (int i = 0, max = lines.Count; i < max; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        currLine = lines[i];
                        pString = "";
                        cOut = 0;
                        if (currLine.Ln is null || !lineNoTable.TryGetValue(currLine.Ln, out int currNo))
                        {
                            return -2; // Line number is null or not found in line number table
                        }
                        linkAddr = bytes.Count;
                        bytes.Add(0); // placeholder for line link
                        bytes.Add(0); // placeholder for line link
                        bytes.Add((byte)(currNo & 0xFF));
                        bytes.Add((byte)((currNo >> 8) & 0xFF));
                        // add the new line number to the printable string for display in the form
                        lineNoStr = currNo.ToString() + " ";
                        foreach (char c in lineNoStr)
                        {
                            token.ThrowIfCancellationRequested();
                            pString += c;
                            cOut++;
                            if (myForm!.optWrap && (cOut >= lineLength))
                            {
                                pString += '\n';
                                cOut = 0;
                            }
                        }
                        // transfer chunks to memory
                        if (currLine.Chunks is null)
                        {
                            badLine = currNo.ToString();
                            return -3; // Chunks list is null in line {currLine.Ln}
                        }
                        for (int j = 0; j < currLine.Chunks.Count; j++)
                        {
                            token.ThrowIfCancellationRequested();
                            Chunk chunk = currLine.Chunks[j];
                            if (chunk.Type == "blob")
                            {
                                List<byte>? chunkBlob = chunk.ValBlob;
                                if (chunkBlob is not null)
                                {
                                    bytes.AddRange(chunkBlob);
                                    // add the blob to the printable string for display in the form
                                    pString += _PRB.PrtBlob(chunkBlob, lineLength, ref cOut);
                                }
                            }
                            else if (chunk.Type == "target")
                            {
                                string s = "";
                                if (chunk.ValTarget != null)
                                {
                                    if (lineNoTable.TryGetValue(chunk.ValTarget, out int n))
                                    {
                                        s = n.ToString();
                                    }
                                    else
                                    {
                                        s = chunk.ValTarget;
                                    }
                                }
                                // Do we need to handle case of chunk.ValTarget being null?
                                // For now, we will just add an empty string if it is null.


                                foreach (char c in s)
                                {
                                    bytes.Add((byte)c);
                                    // add the target to the printable string for display in the form
                                    pString += c;
                                    cOut++;
                                    if (myForm!.optWrap && (cOut >= lineLength))
                                    {
                                        pString += '\n';
                                        cOut = 0;
                                    }
                                }
                            }
                        }

                        // --- BEGIN LINE LIMIT CODE CHECK ---
                        // 1. Character Check: Count raw text characters excluding display-only formatting newlines
                        int totalCharacters = pString.Replace("\n", "").Length;
                        if (totalCharacters > myMaxLineLength)
                        {
                            badLine = currNo.ToString();
                            return -4; // Code -4: Character limit exceeded
                        }

                        // 2. Byte Check: Measure the line's memory footprint
                        // Standard overhead: 2 bytes (link) + 2 bytes (line number) + 1 byte (trailing zero) = 5 bytes
                        int totalLineBytes = bytes.Count - linkAddr + 1; // +1 accounts for the trailing EOL zero byte
                        if (totalLineBytes > myMaxLineLength + 5)
                        {
                            badLine = currNo.ToString();
                            return -5; // Code -5: Byte limit exceeded
                        }
                        // --- END LINE LIMIT CODE CHECK ---

                        bytes.Add(0); // EOL
                        AppendRTB(pString);
                        // fix up the line link
                        mLink = bytes.Count + mStart;
                        bytes[linkAddr] = (byte)(mLink & 0xFF);
                        bytes[linkAddr + 1] = (byte)((mLink >> 8) & 0xFF);

                        if (form.optDemo) // Delay for demonstration purposes
                        { await Task.Delay(form.optDemoSpeed * 250); }
                    }
                    // end of code (empty line link)
                    AppendRTB("---- End of BASIC Program ----");
                    bytes.AddRange([0, 0]);
                    return bytes.Count;
                }

                ///////////////////////////////
                // Main function starts here //
                ///////////////////////////////
                basicStart = mem[0] + (mem[1] << 8);
                mStart = basicStart - 2;
                if (form.rtb.InvokeRequired)
                {
                    form.rtb.Invoke(new Action(() =>
                    { form.rtb.Clear(); }));
                }
                else
                { form.rtb.Clear(); }

                ParseSplit();
                // Check if we are still in the range of legal line numbers
                if (lines.Count > 0)
                {
                    string? lastLineNo = lines[^1].Ln;
                    if (lastLineNo != null && lineNoTable.TryGetValue(lastLineNo, out int topLineNo))
                    {
                        if (topLineNo > maxLineNo)
                        {
                            return new RenumberResult
                            {
                                Addr = -1,
                                Message = $"Out of range. Top line number is {topLineNo} ({maxLineNo} allowed).",
                                Mem = null
                            };
                        }
                    }
                }

                bytes.AddRange(mem[0], mem[1]);
                int endAddr = await ReassembleAsync();
                string msg = endAddr switch
                {
                    -2 => "Line number is null or not found in line number table.",
                    -3 => $"Chunks list is null in line {badLine}.",
                    -4 => $"Line {badLine} exceeds the maximum text length of {myMaxLineLength} characters.",
                    -5 => $"Line {badLine} layout demands more bytes than the allowed memory ceiling.",
                    _ => string.Empty // The default case (covers success codes > 0)
                };

                return new RenumberResult
                {
                    Addr = endAddr,
                    Mem = endAddr > 0 ? [.. bytes] : null,
                    Message = msg
                };
            }, token);
        }
    }
}

