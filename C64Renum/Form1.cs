/*
C64Renum C64 Basic program renumbering utility
© Steven J. Stover, 2026

This program is free software: you can redistribute it and/or modify it under the terms of the
GNU Affero General Public License as published by the Free Software Foundation, either version 3 
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even
the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General
Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program.
If not, see <https://www.gnu.org/licenses/>.
*/
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static C64Renum.BasicRenumberer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace C64Renum
{

    public partial class Form1 : Form
    {
        internal static string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        internal static string myPath = Application.StartupPath;
        internal static string AppName = Assembly.GetEntryAssembly()!.GetName().Name!;
        internal static string AppVersion = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
        private string inFile = string.Empty;
        private string outFile = string.Empty;
        private int start;
        private int step;
        private readonly int wTime = 10;

        // Settings variables
        private string optInFilePath = string.Empty;
        private string optOutFilePath = string.Empty;
        private bool optOutUseInputFolder;
        private bool optDeleteOnCancel;
        private bool optNoPromptOverwrite;
        internal bool optWrap;
        internal bool optDemo;
        internal int optDemoSpeed;

        // C64 Pro Mono font
        // Keep the PrivateFontCollection alive for the lifetime of the form
        private readonly PrivateFontCollection pfc = new();
        private Font? c64Font;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        internal const int PFM_LINESPACING = 0x00000100;
        internal const int SCF_SELECTION = 0x0001;
        internal const int EM_SETPARAFORMAT = 0x447;


        public Form1()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.C64Renum;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFontFromResource();
            this.Text = $"{AppName} v{AppVersion}";
            btInputFile.Enabled = true;
            btProcess.Enabled = false;
            btCancel.Enabled = false;
            lbInputFile.Text = string.Empty;
            LoadSettings();
            rtb.Font = c64Font;
            rtb.WordWrap = false;
            rtb.HideSelection = false;
            SetSelectionLineSpacing(4, 280); // Adjust line height of RichTextBox rtb for better readability
            rtb.Clear();
            DisplayC64Characters();
        }

        private void LoadFontFromResource()
        {
            // 1. Locate the embedded resource. 
            string resourceName = "C64Renum.C64_Pro_Mono-STYLE.ttf";

            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception($"Resource '{resourceName}' not found. Check your namespace and filename.");
                }

                // 2. Read the stream into a byte array
                byte[] fontData = new byte[stream.Length];
                stream.ReadExactly(fontData, 0, (int)stream.Length);

                // 3. Allocate memory for the font and copy data
                IntPtr fontPtr = Marshal.AllocHGlobal(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                // 4. Add the font to your private collection
                pfc.AddMemoryFont(fontPtr, fontData.Length);

                // 5. Native Win32 call required so Windows can track the font properly
                uint dummy = 0;
                AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);

                // Free the unmanaged memory
                Marshal.FreeHGlobal(fontPtr);
            }
            // 6. Instantiate the font object (use the first family in the collection)
            c64Font = new Font(pfc.Families[0], 12, FontStyle.Regular);
        }

        private void DisplayC64Characters()
        {
            // Print Standard ASCII Range
            rtb.AppendText("--- Standard ASCII Range ---\n");
            for (int i = 0x20; i <= 0x7E; i++)
            {
                rtb.AppendText(((char)i).ToString());
                if ((i - 0x20 + 1) % 32 == 0) rtb.AppendText("\n");
            }
            rtb.AppendText("\n\n");

            // Print C64 PETSCII Mapping (Private Use Area)
            rtb.AppendText("--- C64 PETSCII Mapping (Private Use Area) ---\n");
            int count = 0;
            for (int i = 0xE000; i <= 0xE1FF; i++)
            {
                rtb.AppendText(((char)i).ToString());
                count++;
                if (count % 32 == 0) rtb.AppendText("\n");
            }
        }

        // Required P/Invoke declaration for memory font registration
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        // Clean up font collection when form closes
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            c64Font?.Dispose();
            pfc?.Dispose();
        }

        public void SetSelectionLineSpacing(byte bLineSpacingRule, int dyLineSpacing)
        {
            PARAFORMAT2 format = new();
            format.cbSize = Marshal.SizeOf(format);
            format.dwMask = PFM_LINESPACING;
            format.dyLineSpacing = dyLineSpacing;
            format.bLineSpacingRule = bLineSpacingRule;
            SendMessage(rtb.Handle, EM_SETPARAFORMAT, SCF_SELECTION, ref format);
        }


        private void LoadSettings()
        {
            // Load settings from the application configuration file
            Settings.Load();
            optInFilePath = Settings.CurrentSettings.InputFilePath;
            optOutFilePath = Settings.CurrentSettings.OutputFilePath;
            optOutUseInputFolder = Settings.CurrentSettings.OutUseInputFolder;
            optDeleteOnCancel = Settings.CurrentSettings.DeleteOnCancel;
            optNoPromptOverwrite = Settings.CurrentSettings.NoPromptOverwrite;
            rtb.BackColor = Settings.CurrentSettings.Background;
            rtb.ForeColor = Settings.CurrentSettings.Foreground;
            optWrap = Settings.CurrentSettings.WrapText;
            optDemo = Settings.CurrentSettings.DemoMode;
            optDemoSpeed = Settings.CurrentSettings.DemoSpeed;
        }

        private bool GetFilename(ref string fname, bool inp = true)
        {
            string path = optOutUseInputFolder ? optInFilePath : inp ? optInFilePath : optOutFilePath;
            if (!Directory.Exists(path))
            {
                path = defaultPath;
                Tools.TimedMessage("Invalid Input Folder name in Settings", "Error", this, MessageBoxIcon.Error, wTime);
            }
            using OpenFileDialog ofd = new()
            {
                DefaultExt = "prg",
                InitialDirectory = path,
                Filter = "prg files (*.prg)|*.prg",
                RestoreDirectory = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fname = ofd.FileName;
                return true;
            }
            else
            {
                fname = string.Empty;
                return false;
            }
        }

        private void btInputFile_Click(object sender, EventArgs e)
        {
            if (GetFilename(ref inFile))
            {
                lbInputFile.Text = inFile;
                GetOutfileName();
                if (CkProcess()) { ActiveControl = btProcess; }
            }
            else
            {
                lbInputFile.Text = string.Empty;
                tbOutputFile.Text = string.Empty;
                btProcess.Enabled = false;
            }
        }

        private void btOutputFile_Click(object sender, EventArgs e)
        {
            if (GetFilename(ref outFile, false))
            {
                tbOutputFile.Text = outFile;
            }
            else
            {
                outFile = string.Empty;
                tbOutputFile.Text = string.Empty;
            }
        }

        private void GetOutfileName()
        {
            if (!string.IsNullOrEmpty(inFile))
            {
                string folder = optOutUseInputFolder ? Path.GetDirectoryName(inFile)! : optOutFilePath;
                string name = Path.GetFileNameWithoutExtension(inFile) + "-renum.prg";
                outFile = Path.Combine(folder, name);
                tbOutputFile.Text = outFile;
            }
            else
            {
                outFile = string.Empty;
                tbOutputFile.Text = string.Empty;
            }
        }

        private void ShowSettingsWindow(bool Show = true)
        {
            using SettingsWindow settingsWindow = new(Show) { KeyPreview = true };
            if (settingsWindow.ShowDialog(this) == DialogResult.OK)
            {
                LoadSettings();
                GetOutfileName();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettingsWindow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tbOutputFile_TextChanged(object sender, EventArgs e)
        {
            btProcess.Enabled = CkProcess();
        }

        private bool CkProcess(bool can = false)
        {
            btProcess.Enabled = !string.IsNullOrEmpty(tbOutputFile.Text) && !string.IsNullOrEmpty(lbInputFile.Text)
                && StartStepOk() && (lbInputFile.Text != tbOutputFile.Text);
            if (can) { btCancel.Enabled = false; }
            return btProcess.Enabled;
        }

        private bool StartStepOk()
        {
            return int.TryParse(tbStart.Text, out start) && int.TryParse(tbStep.Text, out step) && step > 0;
        }

        private static bool ssKeydown(KeyEventArgs e)
        {
            if (e.Modifiers > 0)
            {
                e.SuppressKeyPress = true;
                return false;
            }
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                    {
                        e.SuppressKeyPress = true;
                        return false;
                    }
                }
            }
            return true;
        }

        private void tbStartStop_KeyDown(object sender, KeyEventArgs e)
        {
            ssKeydown(e);
        }

        private void tbStartStop_TextChanged(object sender, EventArgs e)
        {
            CkProcess();
        }

        private CancellationTokenSource? _renumberCts;

        private async void btProcess_Click(object sender, EventArgs e)
        {
            btProcess.Enabled = false;
            btCancel.Enabled = true;

            if (!File.Exists(inFile))
            {
                Tools.TimedMessage($"Input file {inFile} does not exist", "Error", this);
                CkProcess(true);
                return;
            }

            if (inFile.Equals(outFile, StringComparison.OrdinalIgnoreCase))
            {
                Tools.TimedMessage("Input and output files cannot be the same", "Error", this);
                CkProcess(true);
                return;
            }

            if (File.Exists(outFile) && !optNoPromptOverwrite)
            {
                DialogResult result = MessageBox.Show($"Output file {outFile} already exists. Overwrite?", "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    CkProcess(true);
                    return;
                }
            }

            byte[] fileBytes;
            try
            {
                fileBytes = File.ReadAllBytes(inFile);
            }
            catch (Exception ex)
            {
                Tools.TimedMessage($"Error reading input file {inFile}: {ex.Message}", "Error", this);
                CkProcess(true);
                return;
            }

            _renumberCts?.Dispose();
            _renumberCts = new CancellationTokenSource();
            CancellationToken token = _renumberCts.Token;

            try
            {
                RenumberResult res = await BasicRenumberer.RenumberBasicAsync(fileBytes, start, step, this, token);
                if (res.Addr >= 0)
                {
                    try
                    {
                        fileBytes = res.Mem!;
                        // Fix output file length to match new end address
                        Array.Resize(ref fileBytes, res.Addr);
                        File.WriteAllBytes(outFile, fileBytes);
                        Tools.TimedMessage($"Renumbering complete. Output file: {outFile}", "Success", this,
                            MessageBoxIcon.Information, wTime);
                    }
                    catch (Exception ex)
                    {
                        Tools.TimedMessage($"Error writing output file {outFile}: {ex.Message}", "Error", this,
                            MessageBoxIcon.Error, wTime);
                        TryDeleteOutfile();
                    }
                }
                else
                {
                    Tools.TimedMessage($"Renumbering failed: {res.Message}", "Error", this,
                        MessageBoxIcon.Error, wTime);
                    TryDeleteOutfile();
                }
            }
            catch (OperationCanceledException)
            {
                Tools.TimedMessage("Renumbering cancelled", "Info", this, MessageBoxIcon.Warning, wTime);
                TryDeleteOutfile();
            }
            catch (Exception ex)
            {
                Tools.TimedMessage($"Error: {ex.Message}", "Error", this, MessageBoxIcon.Error, wTime);
                TryDeleteOutfile();
            }
            finally
            {
                _renumberCts?.Dispose();
                _renumberCts = null;
                btCancel.Enabled = false;
                CkProcess();
            }
        }

        private void TryDeleteOutfile()
        {
            if (optDeleteOnCancel)
            {
                try
                {
                    if (File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }
                }
                catch (Exception ex)
                {
                    Tools.TimedMessage($"Error deleting output file {outFile}: {ex.Message}", "Error", this,
                        MessageBoxIcon.Error, wTime);
                }
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using About aboutWindow = new() { KeyPreview = true };
            aboutWindow.ShowDialog(this);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            _renumberCts?.Cancel();
            btCancel.Enabled = false;
            CkProcess();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);
    }
}
