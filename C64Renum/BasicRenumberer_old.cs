using System;
using System.Collections.Generic;
using System.Text;
//using System.Text.RegularExpressions;

public class BasicRenumberer
{
    public struct RenumberResult
    {
        public int Addr { get; set; }
        public string Message { get; set; }
    }

    public struct LineInfo
    {
        public int Ln { get; set; }
        public byte[] Data { get; set; }
    }

    public RenumberResult RenumberBasic(byte[] mem, int basicStart, int startNo, int step)
    {
        int maxLineNo = 36999;
        List<LineInfo> lines = new List<LineInfo>();
        Dictionary<int, int> lineNoTable = new Dictionary<int, int>();
        int currentLineNo = startNo;

        // Helper to parse and split the memory into lines
        // This assumes a standard C64/VIC20 BASIC memory format:
        // [next_addr_low][next_addr_high][line_no_low][line_no_high][tokens...][00]
        void ParseSplit()
        {
            int ptr = basicStart;
            while (ptr < mem.Length)
            {
                int nextAddr = mem[ptr] + (mem[ptr + 1] << 8);
                if (nextAddr == 0) break;

                int lineNo = mem[ptr + 2] + (mem[ptr + 3] << 8);
                
                // Find the end of the line (null terminator)
                int endPtr = ptr + 4;
                while (endPtr < mem.Length && mem[endPtr] != 0)
                {
                    endPtr++;
                }

                int dataLen = endPtr - (ptr + 4);
                byte[] lineData = new byte[dataLen];
                Array.Copy(mem, ptr + 4, lineData, 0, dataLen);

                lines.Add(new LineInfo { Ln = lineNo, Data = lineData });
                lineNoTable[lineNo] = currentLineNo;
                
                currentLineNo += step;
                ptr = nextAddr;
            }
        }

        // Helper to reassemble lines back into memory with updated references
        int ReassembleLines()
        {
            int ptr = basicStart;
            for (int i = 0; i < lines.Count; i++)
            {
                LineInfo line = lines[i];
                int newLineNo = lineNoTable[line.Ln];
                
                // Calculate next address
                int nextAddr = ptr + 4 + line.Data.Length + 1;
                
                // Write next address (link pointer)
                if (i == lines.Count - 1)
                {
                    mem[ptr] = (byte)(nextAddr & 0xFF);
                    mem[ptr + 1] = (byte)((nextAddr >> 8) & 0xFF);
                }
                else
                {
                    mem[ptr] = (byte)(nextAddr & 0xFF);
                    mem[ptr + 1] = (byte)((nextAddr >> 8) & 0xFF);
                }

                // Write new line number
                mem[ptr + 2] = (byte)(newLineNo & 0xFF);
                mem[ptr + 3] = (byte)((newLineNo >> 8) & 0xFF);

                // Write line data (with reference updates)
                // Note: In a real implementation, GOTO/GOSUB targets in line.Data 
                // would be updated here using lineNoTable.
                for (int j = 0; j < line.Data.Length; j++)
                {
                    mem[ptr + 4 + j] = line.Data[j];
                }

                // Write null terminator
                mem[ptr + 4 + line.Data.Length] = 0;
                
                ptr = nextAddr;
            }

            // Write end of program marker (00 00)
            mem[ptr] = 0;
            mem[ptr + 1] = 0;
            
            return ptr + 2;
        }

        ParseSplit();

        // check, if we are still in the range of legal line numbers
        if (lines.Count > 0)
        {
            int topLineNo = lineNoTable[lines[lines.Count - 1].Ln];
            // we could throw an exception as well…
            if (topLineNo > maxLineNo)
            {
                return new RenumberResult
                {
                    Addr = -1,
                    Message = "Out of range. Top line number is " + topLineNo +
                              " (" + maxLineNo + " allowed)."
                };
            }
        }

        int endAddr = ReassembleLines();
        return new RenumberResult
        {
            Addr = endAddr
        };
    }
}
