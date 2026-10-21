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
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static C64Renum.BasicRenumberer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace C64Renum
{

    public partial class Form1 : Form
    {
        internal static string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        internal static string myPath = Application.StartupPath;
        internal static string AppName = Assembly.GetEntryAssembly()!.GetName().Name!;
        internal static string AppVersion = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
        internal string inFile = string.Empty;
        internal string outFile = string.Empty;
        private int start;
        private int step;
        internal readonly int wTime = 10;
        internal bool _processing = false;
        internal bool _refproc = false;
        //internal PRB _PRB;

        // Settings variables
        private string optInFilePath = string.Empty;
        private string optOutFilePath = string.Empty;
        private bool optOutUseInputFolder;
        private bool optDeleteOnCancel;
        internal bool optNoPromptOverwrite;
        internal bool optWrap;
        internal bool optDemo;
        internal int optDemoSpeed;
        internal bool optCloseOnEsc;
        internal bool optClearOnInvalid;
        internal int optMaxVarLength;
        internal bool optEnableWarnings;
        public Mode optMode;
        internal bool optCloseRefOnEsc;
        internal int optMaxLineLength;
        internal bool optOverwriteOnTrim;
        internal int optStartLineNo;
        internal int optStepIncNo;
        internal bool optClrOutFilePath;

        public enum Mode { Renumber, Refactor }

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
            //_PRB = new PRB(this, CancellationToken.None);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
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
            Tokens.DisplayC64Characters(rtb);
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

        // Required P/Invoke declaration for memory font registration
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        // Clean up font collection when form closes
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Settings.Save(this);
            base.OnFormClosing(e);
            _reCts?.Dispose();
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
            optMode = Settings.CurrentSettings.rMode;
            optCloseOnEsc = Settings.CurrentSettings.CloseOnEscape;
            optClearOnInvalid = Settings.CurrentSettings.ClearOnInvalid;
            optMaxVarLength = Settings.CurrentSettings.MaxVarLength;
            optEnableWarnings = Settings.CurrentSettings.EnableWarnings;
            optCloseRefOnEsc = Settings.CurrentSettings.CloseRefOnEsc;
            optMaxLineLength = (int)Settings.CurrentSettings.LineLength;
            optOverwriteOnTrim = Settings.CurrentSettings.OverwriteOnTrim;
            optStartLineNo = Settings.CurrentSettings.StartLineNo;
            optStepIncNo = Settings.CurrentSettings.StepIncNo;
            tbStart.Text = optStartLineNo.ToString();
            tbStep.Text = optStepIncNo.ToString();
            optClrOutFilePath = Settings.CurrentSettings.ClrOutFilePath;

            ckMode();
        }

        private void ckMode()
        {
            renumberToolStripMenuItem.Checked = optMode == Mode.Renumber;
            refactorToolStripMenuItem.Checked = optMode == Mode.Refactor;
            SwapUI();
        }

        private void SwapUI()
        {
            bool r = optMode == Mode.Renumber;
            lbStart.Visible = r;
            tbStart.Visible = r;
            lbStep.Visible = r;
            tbStep.Visible = r;
            btProcess.Text = r ? "Process" : "Scan";
            lbMode.Text = optMode == Mode.Renumber ? "Renumber Mode" : "Refactor Mode";
            tbOutputFile.Text = "";
            GetOutfileName(out _);
            CkProcess();
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
            tbOutputFile.Text = "";
            if (GetFilename(ref inFile))
            {
                lbInputFile.Text = inFile;
                GetOutfileName(out _);
                if (CkProcess()) { ActiveControl = btProcess; }
            }
            else
            {
                lbInputFile.Text = string.Empty;
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

        private bool GetOutfileName(out string err)
        {
            err = string.Empty;
            bool ret = false;
            if (!string.IsNullOrWhiteSpace(inFile))
            {
                try
                {
                    string folder = optOutUseInputFolder ? Path.GetDirectoryName(inFile)! : optOutFilePath;
                    string name = Path.GetFileName(tbOutputFile.Text);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Path.GetFileNameWithoutExtension(inFile) +
                            (optMode == Mode.Renumber ? "-renum.prg" : "-refact.prg");
                    }
                    else
                    {
                        try
                        {
                            folder = Path.GetDirectoryName(tbOutputFile.Text)!;
                        }
                        catch (Exception ex)
                        {
                            Tools.TimedMessage($"Error: {ex.Message}", "Error", this, MessageBoxIcon.Error, wTime);
                            // Just leave folder as-is
                        }
                    }

                    string path = Path.Combine(folder, name);
                    if (FileValidation.IsValidFullPath(path, out err))
                    {
                        outFile = path;
                        tbOutputFile.Text = path;
                        ret = true;
                    }
                    else
                    {
                        if (optClrOutFilePath)
                        {
                            outFile = string.Empty;
                            tbOutputFile.Text = string.Empty;
                        }
                        ret = false;
                    }
                }
                catch (Exception ex)
                {
                    if (optClrOutFilePath)
                    {
                        outFile = string.Empty;
                        tbOutputFile.Text = string.Empty;
                    }
                    else { Tools.TimedMessage(ex.Message, "Error", this, MessageBoxIcon.Error, wTime); }
                    ret = false;
                }
            }
            else
            {
                outFile = string.Empty;
                tbOutputFile.Text = string.Empty;
                ret = false;
            }
            return ret;
        }

        private void ShowSettingsWindow(bool Show = true)
        {
            using SettingsWindow settingsWindow = new(this, Show) { KeyPreview = true };
            if (settingsWindow.ShowDialog(this) == DialogResult.OK)
            {
                LoadSettings();
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
            CkProcess(false, false);
        }

        internal void EndProc(bool can = false)
        {
            _processing = _refproc;
            CkProcess(can);
        }

        private bool CkProcess(bool can = false, bool focusProc = true)
        {
            bool allGood = !string.IsNullOrEmpty(tbOutputFile.Text) && !string.IsNullOrEmpty(lbInputFile.Text)
                && StartStepOk() && (lbInputFile.Text != tbOutputFile.Text) && !_processing;
            btCancel.Enabled = can;
            btProcess.Enabled = allGood;
            menuStrip1.Enabled = !_processing;
            lbMode.Enabled = !_processing;
            if (focusProc)
            {
                btProcess.Focus();
            }
            return allGood;
        }

        private bool StartStepOk()
        {
            if (optMode == Mode.Refactor) return true;
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

        private void tbStartStep_KeyDown(object sender, KeyEventArgs e)
        {
            ssKeydown(e);
        }

        private void tbStartStep_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(tbStart.Text, out int StartLineNo)) optStartLineNo = StartLineNo;
            if (int.TryParse(tbStep.Text, out int StepIncNo)) optStepIncNo = StepIncNo;
            CkProcess(false, false);
        }

        private CancellationTokenSource? _reCts;
        internal CancellationToken token;

        private async void btProcess_Click(object sender, EventArgs e)
        {
            _processing = true;
            CkProcess(true);

            if (!File.Exists(inFile))
            {
                Tools.TimedMessage($"Input file {inFile} does not exist", "Error", this, MessageBoxIcon.Error, wTime);
                EndProc();
                return;
            }

            if (!GetOutfileName(out string me))
            {
                Tools.TimedMessage(me, "Error", this, MessageBoxIcon.Error, wTime);
                EndProc();
                return;
            }

            if (inFile.Equals(outFile, StringComparison.OrdinalIgnoreCase))
            {
                Tools.TimedMessage("Input and output files cannot be the same", "Error", this, MessageBoxIcon.Error, wTime);
                EndProc();
                return;
            }

            if (File.Exists(outFile) && !optNoPromptOverwrite)
            {
                DialogResult result = MessageBox.Show($"Output file {outFile} already exists. Overwrite?",
                    "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    EndProc();
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
                Tools.TimedMessage($"Error reading input file {inFile}.\n{ex.Message}", "Error", this, MessageBoxIcon.Error, wTime);
                EndProc();
                return;
            }

            _reCts?.Dispose();
            _reCts = new CancellationTokenSource();
            token = _reCts.Token;

            if (optMode == Mode.Refactor)
            {
                GoRefactor(fileBytes);
                return;
            }

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
                        Tools.TimedMessage($"Error writing output file {outFile}.\n{ex.Message}", "Error", this,
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
                Tools.TimedMessage("Renumbering cancelled", "Info", this, MessageBoxIcon.Information, wTime);
                TryDeleteOutfile();
            }
            catch (Exception ex)
            {
                Tools.TimedMessage($"Error: {ex.Message}", "Error", this, MessageBoxIcon.Error, wTime);
                TryDeleteOutfile();
            }
            finally
            {
                _reCts?.Dispose();
                _reCts = null;
                EndProc();
            }
        }

        private void GoRefactor(byte[] fBytes)
        {
            using RefactorWindow refactorWindow = new(fBytes, this, _reCts!) { KeyPreview = true };
            DialogResult grResult = refactorWindow.ShowDialog(this);
            Settings.Load(); // Reload settings in case they were changed in the RefactorWindow
            if (grResult == DialogResult.Abort)
            {
                _refproc = false;
                _reCts?.Dispose();
                _reCts = null;
            }
            EndProc(_refproc);
            return;
        }

        internal void TryDeleteOutfile()
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
                    Tools.TimedMessage($"Error deleting output file {outFile}.\n{ex.Message}", "Error", this,
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
            _reCts?.Cancel();
            _refproc = false;
            EndProc();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

        private void renumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optMode = Mode.Renumber;
            ckMode();
        }

        private void refactorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optMode = Mode.Refactor;
            ckMode();
        }

        private void lbMode_DoubleClick(object sender, EventArgs e)
        {
            optMode = optMode == Mode.Renumber ? Mode.Refactor : Mode.Renumber;
            ckMode();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (optCloseOnEsc && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                this.Close();
            }
        }

        private void c64TokensToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using TokensHelp tkh = new();
            tkh.ShowDialog();
        }
    }
}
