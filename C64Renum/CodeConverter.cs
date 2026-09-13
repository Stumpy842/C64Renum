using System;
using System.Collections.Generic;

internal static class BasicRenumberer
{
    private static Dictionary<string, int> lineNoTable = new Dictionary<string, int>();
    private static List<Dictionary<string, object>> lines = new List<Dictionary<string, object>>();
    private static int maxLineNo = 36999;
    // Dummy variables to make the code compile, assuming they exist in the wider scope of the original code
    private static int basicStart = 0;
    private static int startNo = 0;
    private static int step = 10;
    private static byte[] mem = new byte[0];

    // renumber BASIC in provided memory slice (0..max)
    // processes GOTO, GOSUB, GO TO, ON GOTO|GOSUB|GO TO, THEN
    public static void RenumberBasic(byte[] mem, int basicStart, int startNo, int step)
    {
        var lineNoTable = new Dictionary<int, int>();
        var lines = new List<object>(); // Placeholder for line chunks
        int maxLineNo = 36999;
        ParseSplit(); // Assuming ParseSplit is a method that populates lines and lineNoTable
        ReassembleLines();
    


    // parse and split lines to chunks of blobs and line targets
    // stores parsed chunks in array lines,
    // generates a reference of old and new line numbers in lineNoTable.
    static void ParseSplit()
    {
        int addr = basicStart,
            lineNo = startNo,
            maxMem = mem.Length;
        List<Dictionary<string, object>> chunks = null;
        List<byte> blob = null;

        // scans for a jump target,
        // generates a target entry and a new blob in current chunks,
        // any leading blanks are added to the current blob
        void ParseLineTarget(bool maybeList)
        {
            while (addr < mem.Length && mem[addr] == 0x20)
            {
                blob.Add(0x20);
                addr++;
            }
            // parse ASCII to string, ignoring any blanks
            string n = "";
            int c = addr < mem.Length ? mem[addr] : 0;
            while ((c >= 0x30 && c <= 0x39) || c == 0x20)
            {
                if (c != 0x20) n += (char)c;
                addr++;
                c = addr < mem.Length ? mem[addr] : 0;
            }
            // if we found a number, push chunks and open a new blob
            if (n != "")
            {
                chunks.Add(new Dictionary<string, object> { { "type", "blob" }, { "val", blob } });
                chunks.Add(new Dictionary<string, object> { { "type", "target" }, { "val", n } });
                blob = new List<byte>();
                // scan for any comma (ON GOTO|GOSUB|GO TO)
                if (maybeList && c == 0x2C)
                {
                    blob.Add(0x2C);
                    addr++;
                    ParseLineTarget(true);
                }
            }
        }

        while (addr < maxMem)
        {
            if (addr + 1 >= maxMem) break;
            int lineLink = mem[addr++] + (mem[addr++] << 8);
            if (lineLink == 0) break;
            chunks = new List<Dictionary<string, object>>();
            blob = new List<byte>();

            if (addr + 1 >= maxMem) break;
            string ln = (mem[addr++] + (mem[addr++] << 8)).ToString();

            if (addr >= maxMem) break;
            byte b = mem[addr++];

            var line = new Dictionary<string, object>
            {
                { "ln", ln },
                { "chunks", chunks }
            };
            lineNoTable[ln] = lineNo;
            lineNo += step;

            while (b != 0)
            {
                blob.Add(b);
                switch (b)
                {
                    case 0x8F: // REM (read rest of line)
                        while (addr < maxMem && mem[addr] != 0) blob.Add(mem[addr++]);
                        break;
                    case 0x22: // quote (read up to next quote)
                        if (addr < maxMem)
                        {
                            int c = mem[addr++];
                            while (c != 0)
                            {
                                blob.Add((byte)c);
                                if (c == 0x22) break;
                                c = addr < maxMem ? mem[addr++] : 0;
                            }
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
                        while (addr <= t) blob.Add(mem[addr++]);
                        ParseLineTarget(true);
                        break;
                }
                b = addr < maxMem ? mem[addr++] : (byte)0;
            }
            if (blob.Count > 0) chunks.Add(new Dictionary<string, object> { { "type", "blob" }, { "val", blob } });
            lines.Add(line);
            addr = lineLink;
        }
    }

    public class Chunk
    {
        public string Type;
        public byte[] Val; // For 'blob'
        public string TargetVal; // For 'target'
    }

    public class Line
    {
        public string Ln;
        public List<Chunk> Chunks;
    }

    // reassamble BASIC code from line chunks using new line numbers
    // from lineNoTable
    static int ReassembleLines()
    {
        int addr = BasicStart;
        for (int i = 0, max = Lines.Count; i < max; i++)
        {
            Line currLine = Lines[i];
            int currNo = LineNoTable[currLine.Ln];
            int linkAddr = addr;
            Mem[addr++] = 0;
            Mem[addr++] = 0;
            Mem[addr++] = (byte)(currNo & 0xFF);
            Mem[addr++] = (byte)((currNo >> 8) & 0xFF);
            // transfer chunks to memory
            for (int j = 0; j < currLine.Chunks.Count; j++)
            {
                Chunk chunk = currLine.Chunks[j];
                if (chunk.Type == "blob")
                {
                    byte[] blob = chunk.Val;
                    for (int k = 0; k < blob.Length; k++)
                        Mem[addr++] = blob[k];
                }
                else if (chunk.Type == "target")
                {
                    string s = "";
                    if (chunk.TargetVal != null)
                    {
                        if (LineNoTable.TryGetValue(chunk.TargetVal, out int n))
                        {
                            s = n.ToString();
                        }
                        else
                        {
                            s = chunk.TargetVal;
                        }
                    }
                    for (int k = 0; k < s.Length; k++)
                        Mem[addr++] = (byte)s[k];
                }
            }
            Mem[addr++] = 0; //EOL
            // fix up the line link
            Mem[linkAddr++] = (byte)(addr & 0xFF);
            Mem[linkAddr] = (byte)((addr >> 8) & 0xFF);
        }
        // end of code (empty line link)
        Mem[addr++] = 0;
        Mem[addr++] = 0;
        return addr;
    }
}
