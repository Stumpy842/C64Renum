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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    public partial class RefactorWindow : Form
    {
        private readonly string instTextFilename = "Instructions.txt"; // Name of embedded instructions text file
        internal byte[]? myBytes;
        internal Dictionary<string, string> myVarsDict = [];
        // Stores: [First 2 Characters, Assigned Highlight Color]
        private readonly Dictionary<string, Color> _prefixColors = [];
        private readonly Form1? myForm;
        private readonly CancellationTokenSource? cts;
        private int _lastRowIndex = -1;
        private readonly int myWaitTime;
        // Options variables
        private readonly int myMaxVarLength;
        private readonly bool myEnableWarnings;
        private readonly bool myCloseRefOnEsc;
        private bool myOverwriteOnTrim;

        // Variables for cell flashing code
        private System.Windows.Forms.Timer? _flashTimer;
        private TextBox? _flashingTextBox = null;
        private Color _originalCellColor;
        private static readonly Color _collisionColor = Color.Yellow; // Color alert for collisions
        private static readonly Color _mirrorColor = Color.LightCyan; // Visual anchor for safe C64 mirrors
        private static readonly string collisionColorName = _collisionColor.Name.ToLower(); // Lowercase for display
        private static readonly string mirrorColorName = _mirrorColor.Name.ToLower(); // Lowercase for display

        public RefactorWindow(byte[] bytesIn, Form1 formIn, CancellationTokenSource inCts)
        {
            InitializeComponent();
            myBytes = bytesIn;
            myForm = formIn;
            cts = inCts;
            myMaxVarLength = myForm.optMaxVarLength;
            myEnableWarnings = myForm.optEnableWarnings;
            myCloseRefOnEsc = myForm.optCloseRefOnEsc;
            myOverwriteOnTrim = myForm.optOverwriteOnTrim;
            myWaitTime = myForm.wTime;
            this.Icon = Properties.Resources.C64Renum;
            this.Text += $" - {myForm.inFile}";

            // Maximizes rendering performance and stops grid flickering during scrolling
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dgv, true, null);

            // Set properties of DataGridView dgv
            dgv.ColumnCount = 2;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.Columns[0].Name = "KeyColumn";
            dgv.Columns[0].HeaderText = "Old Name";
            dgv.Columns[1].Name = "ValueColumn";
            dgv.Columns[1].HeaderText = "New Name";
            dgv.Columns[0].ReadOnly = true;
            dgv.Columns[0].DefaultCellStyle.SelectionBackColor = dgv.Columns[0].DefaultCellStyle.BackColor;
            dgv.Columns[0].DefaultCellStyle.SelectionForeColor = dgv.Columns[0].DefaultCellStyle.ForeColor;

            // Set sorting behavior
            dgv.Columns[0].SortMode = DataGridViewColumnSortMode.Programmatic;
            dgv.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;

            // Set tooltips for the headers to explain the custom sorting and double-click actions
            dgv.ShowCellToolTips = true;
            dgv.Columns[0].HeaderCell.ToolTipText = "Single click to sort using native C64 variable order rules.";
            dgv.Columns[1].HeaderCell.ToolTipText = "Single click to sort alphabetically.\nDouble click to toggle Trim overwrite mode";
            toolTip1.SetToolTip(btTrim, $"Double click the '{dgv.Columns[1].HeaderText}' column\nheader to toggle Trim overwrite mode");
        }

        private async void RefactorWindow_Load(object sender, EventArgs e)
        {
            void PopulateAndLockGrid()
            {
                // 1. Suspend layout and logic processing
                dgv.SuspendLayout();

                dgv.Columns[1].HeaderText += $"  (Max:{myMaxVarLength}   Trim:" +
                    (myOverwriteOnTrim ? "Over" : "Trim") + ")";

                // Create an in-memory list of rows to add all at once
                List<DataGridViewRow> rowsToAdd = [];

                foreach (KeyValuePair<string, string> kvp in myVarsDict)
                {
                    // Create the row object without adding it to the grid yet
                    DataGridViewRow row = new();
                    row.CreateCells(dgv, kvp.Key, kvp.Value);
                    rowsToAdd.Add(row);

                    string upperKey = kvp.Key.ToUpper().Replace(" ", "");

                    // --- FIXED SYSTEM REGISTER INTERCEPT LOOP ---
                    bool isReservedST = false;
                    bool isReservedTI = false;

                    if (upperKey.StartsWith("ST") || upperKey.StartsWith("TI"))
                    {
                        bool isString = upperKey.Contains('$');
                        bool isInteger = upperKey.Contains('%');
                        bool isArray = upperKey.Contains('(');

                        if (upperKey.StartsWith("ST"))
                        {
                            if (!isString && !isInteger && !isArray) isReservedST = true;
                        }
                        else if (upperKey.StartsWith("TI"))
                        {
                            if (!isInteger && !isArray)
                            {
                                if (isString)
                                {
                                    if (upperKey == "TI$") isReservedTI = true;
                                }
                                else
                                {
                                    isReservedTI = true;
                                }
                            }
                        }
                    }

                    if (isReservedST || isReservedTI)
                    {
                        row.Cells[0].Style.BackColor = Color.FromArgb(139, 0, 0);
                        row.Cells[0].Style.ForeColor = Color.White;

                        row.Cells[1].ReadOnly = true;
                        row.Cells[1].Style.BackColor = Color.FromArgb(225, 230, 235);
                    }
                }

                // Add all rows in a single batch operation
                dgv.Rows.AddRange([.. rowsToAdd]);

                // 2. Identify duplicate groups based SOLELY on the immutable Column 0 values
                _prefixColors.Clear();
                var colZeroSignatures = dgv.Rows
                    .Cast<DataGridViewRow>()
                    .Select(row => ComputeSignatureFromCellText(row.Cells[0].Value?.ToString()))
                    .Where(sig => !string.IsNullOrEmpty(sig))
                    .Select(sig => sig!)
                    .ToList();

                var duplicateSignatures = colZeroSignatures
                    .GroupBy(sig => sig)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                for (int i = 0; i < duplicateSignatures.Count; i++)
                {
                    string signature = duplicateSignatures[i];
                    double hue = (i * 0.618033988749895 * 360.0) % 360.0;
                    double saturation = 0.75;
                    double lightness = signature.EndsWith('(') ? 0.65 : 0.85;

                    _prefixColors[signature] = Tools.ColorFromHsl(hue, saturation, lightness);
                }

                // 3. Apply standard group color assignments to Column 0 permanently
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells[1].ReadOnly) continue;

                    string signature = ComputeSignatureFromCellText(row.Cells[0].Value?.ToString()) ?? string.Empty;
                    if (_prefixColors.TryGetValue(signature, out Color assignedColor))
                    {
                        row.Cells[0].Style.BackColor = assignedColor;
                        row.Cells[0].Style.ForeColor = Color.Black;
                    }
                }

                // 4. Resume layout and force a single redraw
                dgv.ResumeLayout(true);
            }

            void ShowInstructions()
            {
                bool gotText = Tools.GetTextFromResourceFile(instTextFilename, out string instText);
                if (!string.IsNullOrEmpty(instText))
                {
                    instText = instText.Replace("[collision]", collisionColorName).Replace("[mirror]", mirrorColorName)
                        .Replace("[old]", dgv.Columns[0].HeaderText).Replace("[new]", dgv.Columns[1].HeaderText);
                }
                lbInstruct.Text = gotText ? instText : $"Cannot read embedded file '{instTextFilename}'";
            }

            actionPanel.Enabled = false;
            dgv.Enabled = false;
            this.UseWaitCursor = true;
            ShowInstructions();

            if (myBytes != null)
            {
                try
                {
                    // Scan the BASIC memory for all variable names and populate the dictionary
                    Refactorer.RefactorResult res = await Refactorer.ScanBasicVarsAsync(myBytes, myVarsDict!, myForm!, cts!.Token);
                    if (!string.IsNullOrEmpty(res.Message))
                    {
                        Tools.TimedMessage(res.Message, "Error", this, MessageBoxIcon.Error, myWaitTime);
                        this.Close();
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    Tools.TimedMessage("Refactoring canceled", "Info", this, MessageBoxIcon.Warning, myWaitTime);
                    this.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Tools.TimedMessage($"Error: {ex.Message}", "Error", this, MessageBoxIcon.Error, myWaitTime);
                    this.Close();
                    return;
                }

                myVarsDict = DictionarySortUtils.SortByKeyFirstTwoAlnumThenSuffix(myVarsDict)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                PopulateAndLockGrid();

                dgv.Enabled = true;
                actionPanel.Enabled = true;
                this.UseWaitCursor = false;
                dgv.Focus();

                // Initial rendering sweep for Column 1
                InitializeFlashTimer();
            }
            else
            {
                this.UseWaitCursor = false;
                Tools.TimedMessage("No target memory data was provided to refactor.", "Error",this, MessageBoxIcon.Error, myWaitTime);
                this.Close();
            }            
        }

        private bool WarnOnCollisions() // Return true if var name collisions found
        {
            // Step 1: Complete UI Style Reset Sweep
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.Cells[1].ReadOnly)
                {
                    row.Cells[1].Style.BackColor = Color.FromArgb(225, 230, 235);
                    continue;
                }

                row.Cells[1].Style.BackColor = dgv.DefaultCellStyle.BackColor;
                row.Cells[1].Style.ForeColor = dgv.DefaultCellStyle.ForeColor;
            }

            bool hasTrueCollisions = false;
            HashSet<int> trueCollidingRowIndices = [];
            HashSet<int> mirrorRowIndices = [];

            // Step 2: Cross-examine every editable row against the entire grid context
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow rowA = dgv.Rows[i];
                if (rowA.IsNewRow || rowA.Cells[1].ReadOnly) continue;

                object? cellOneValueA = rowA.Cells[1].Value;

                // If Column 1 is empty, it cannot be highlighted as a collision source.
                if (cellOneValueA == null || string.IsNullOrWhiteSpace(cellOneValueA.ToString())) continue;

                string finalNameA = cellOneValueA.ToString()!.Trim().ToUpper().Replace(" ", "");

                // --- FIXED SYSTEM INTERCEPT CHECK FOR ROW A ---
                bool isInvalidSystemA = false;
                if (finalNameA.StartsWith("ST") || finalNameA.StartsWith("TI"))
                {
                    bool isString = finalNameA.Contains('$');
                    bool isInteger = finalNameA.Contains('%');
                    bool isArray = finalNameA.Contains('(');

                    if (finalNameA.StartsWith("ST"))
                    {
                        if (!isString && !isInteger && !isArray) isInvalidSystemA = true;
                    }
                    else if (finalNameA.StartsWith("TI"))
                    {
                        if (!isInteger && !isArray)
                        {
                            if (isString)
                            {
                                if (finalNameA == "TI$") isInvalidSystemA = true;
                            }
                            else
                            {
                                isInvalidSystemA = true;
                            }
                        }
                    }
                }

                if (isInvalidSystemA)
                {
                    trueCollidingRowIndices.Add(i);
                    hasTrueCollisions = true;
                    continue; // Block immediately as a true Yellow error
                }

                string? signatureA = ComputeSignatureFromCellText(finalNameA);
                if (string.IsNullOrEmpty(signatureA)) continue;

                // Isolate Row A's native, original identity from Column 0
                string origNameA = rowA.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
                string? origSignatureA = ComputeSignatureFromCellText(origNameA);

                bool rowAHasTrueCollision = false;
                bool rowAHasMirrorRelation = false;

                // Check rowA against every other single rowB in the entire grid configuration
                for (int j = 0; j < dgv.Rows.Count; j++)
                {
                    if (i == j) continue; // Do not check a row against itself

                    DataGridViewRow rowB = dgv.Rows[j];
                    if (rowB.IsNewRow) continue;

                    object? cellOneValueB = rowB.Cells[1].Value;
                    bool isColumnOneActiveB = cellOneValueB != null && !string.IsNullOrWhiteSpace(cellOneValueB.ToString());

                    string finalNameB;
                    if (isColumnOneActiveB)
                    {
                        finalNameB = cellOneValueB!.ToString()!.Trim();
                    }
                    else if (rowB.Cells[1].ReadOnly)
                    {
                        finalNameB = rowB.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
                    }
                    else
                    {
                        continue; // Row B is editable but currently blanked out. Skip it!
                    }

                    string? signatureB = ComputeSignatureFromCellText(finalNameB);

                    // Match Found!
                    if (signatureA == signatureB)
                    {
                        // Isolate Row B's original native identity from Column 0
                        string origNameB = rowB.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
                        string? origSignatureB = ComputeSignatureFromCellText(origNameB);

                        if (origSignatureA == origSignatureB)
                        {
                            // Case A: They were already matching variables in C64 memory (Safe Mirror)
                            rowAHasMirrorRelation = true;
                        }
                        else
                        {
                            // Case B: They were completely different, meaning this is a new conflict (True Error)
                            rowAHasTrueCollision = true;
                        }
                    }
                }

                // Assign to the highest priority paint classification bucket
                if (rowAHasTrueCollision)
                {
                    trueCollidingRowIndices.Add(i);
                    hasTrueCollisions = true;
                }
                else if (rowAHasMirrorRelation)
                {
                    mirrorRowIndices.Add(i);
                }
            }

            // Step 3: Direct Painting Execution
            // Stencil soft LightCyan backgrounds onto verified native mirror rows
            foreach (int rowIndex in mirrorRowIndices)
            {
                dgv.Rows[rowIndex].Cells[1].Style.BackColor = _mirrorColor;
                dgv.Rows[rowIndex].Cells[1].Style.ForeColor = Color.Black;
            }

            // Stencil Yellow backgrounds onto verified active conflict rows (overwrites mirrors if a row has both)
            foreach (int rowIndex in trueCollidingRowIndices)
            {
                dgv.Rows[rowIndex].Cells[1].Style.BackColor = _collisionColor;
                dgv.Rows[rowIndex].Cells[1].Style.ForeColor = Color.Black;
            }

            // Step 4: Enforce default profiles on any safe/empty rows
            ApplyDynamicColumnOneColors();

            return hasTrueCollisions;
        }

        private void ApplyDynamicColumnOneColors()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                // State 1: Protect structurally locked greyed-out fields (ST, TI)
                if (row.Cells[1].ReadOnly) continue;

                object? cellOneVal = row.Cells[1].Value;
                bool isColumnOneActive = cellOneVal != null && !string.IsNullOrWhiteSpace(cellOneVal.ToString());

                if (!isColumnOneActive)
                {
                    row.Cells[1].Style.BackColor = dgv.DefaultCellStyle.BackColor;
                    row.Cells[1].Style.ForeColor = dgv.DefaultCellStyle.ForeColor;
                    continue;
                }

                // State 2: Protect valid collision highlights set during Step 3 of the analyzer
                if (row.Cells[1].Style.BackColor == _collisionColor) continue;
                if (row.Cells[1].Style.BackColor == _mirrorColor) continue; // --- FIX: Protect Cyan profiles too! ---

                // State 3: Safe / Default Presentation
                row.Cells[1].Style.BackColor = dgv.DefaultCellStyle.BackColor;
                row.Cells[1].Style.ForeColor = dgv.DefaultCellStyle.ForeColor;
            }
        }

        private void dgv_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var row = dgv.Rows[e.RowIndex];
                var cell = row.Cells[e.ColumnIndex];

                // SAVE VAULT: Preserve the true original string (with suffix) in the cell's Tag
                cell.Tag = cell.Value?.ToString();

                string currentVal = cell.Value?.ToString()?.Trim() ?? string.Empty;

                // --- FIX: Explicitly target Column 0 index safely ---
                string columnZeroText = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;

                // Extract true terminal suffixes explicitly, checking longer multi-character strings first
                string suffix = string.Empty;
                if (columnZeroText.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) suffix = "[DUMMY]";
                else if (columnZeroText.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) suffix = "[FN]";
                else if (columnZeroText.EndsWith('(')) suffix = "(";
                else if (columnZeroText.EndsWith('$')) suffix = "$";
                else if (columnZeroText.EndsWith('%')) suffix = "%";

                // Strip suffix for a clean raw text editing presentation
                if (!string.IsNullOrEmpty(suffix) && currentVal.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string cleanBase = currentVal[..^suffix.Length].Trim();
                    cell.Value = cleanBase;
                }
            }
        }

        private void editingTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void dgv_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox editingTextBox)
            {
                // ALWAYS detach your custom handler first to prevent infinite accumulation/leaking
                editingTextBox.KeyPress -= editingTextBox_KeyPress;
                editingTextBox.KeyDown -= editingTextBox_KeyDown;

                // Target column index 1
                if (dgv.CurrentCell != null && dgv.CurrentCell.ColumnIndex == 1)
                {
                    // Disable the right-click menu entirely
                    editingTextBox.ContextMenuStrip = new ContextMenuStrip(); // Assigns an empty menu, blocking the default one

                    // Enforce your maximum character length safely
                    editingTextBox.MaxLength = myMaxVarLength;

                    // Force the text box to display and save everything as uppercase natively
                    editingTextBox.CharacterCasing = CharacterCasing.Upper;

                    // Attach the keypress handling logic safely for column 1 only
                    editingTextBox.KeyPress += editingTextBox_KeyPress;
                    editingTextBox.KeyDown += editingTextBox_KeyDown;
                }
                else
                {
                    // For any other column, reset the MaxLength to standard defaults 
                    // so column 1's limits don't leak over to them.
                    editingTextBox.MaxLength = 32767;
                }
            }
        }

        private void dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                // --- ESCAPE KEY GUARD CLAUSE ---
                if (_escapeKeyPressed) return;

                var row = dgv.Rows[e.RowIndex];
                if (row.IsNewRow) return;

                // CRITICAL GUARD: Skip validation entirely if this cell is structurally ReadOnly
                if (row.Cells[1].ReadOnly) return;

                // 1. EXTRACT IMMUTABLE IDENTITY CONTEXT FROM COLUMN 0
                string varNameContext = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(varNameContext)) return;

                // FormattedValue contains exactly what is written in the editing textbox right now
                string rawProposed = e.FormattedValue?.ToString()?.Trim() ?? string.Empty;

                // If they empty out the cell completely, that's a valid action (reverts back to Col 0 behavior)
                if (string.IsNullOrEmpty(rawProposed)) return;

                // --- PRE-EMPTIVE MOUSE HOVER FOR ESCAPE PIPELINES ---
                Point clientMousePos = PointToClient(Cursor.Position);
                bool isClickingExitButton =
                    (btCancel != null && btCancel.Visible && btCancel.Bounds.Contains(clientMousePos)) ||
                    (btReset != null && btReset.Visible && btReset.Bounds.Contains(clientMousePos));

                if (isClickingExitButton)
                {
                    dgv.CancelEdit(); // Safe rollback
                    e.Cancel = false; // Release the grid focus lock
                    return;
                }

                // --- FIX: Extract original suffix type to pass as context metadata ---
                string suffixContext = string.Empty;
                if (varNameContext.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) suffixContext = "[DUMMY]";
                else if (varNameContext.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) suffixContext = "[FN]";
                else if (varNameContext.EndsWith('(')) suffixContext = "(";
                else if (varNameContext.EndsWith('$')) suffixContext = "$";
                else if (varNameContext.EndsWith('%')) suffixContext = "%";

                // 2. EVALUATE PROPOSED VALUE AGAINST STRICT C64 RULE MATRIX
                string k = InvalidValue(rawProposed, suffixContext, out CollisionSource matchType);

                if (!string.IsNullOrEmpty(k))
                {
                    string ms = $"The value '{rawProposed}' ";
                    ms += matchType switch
                    {
                        CollisionSource.Keyword => "contains the reserved Commodore keyword",
                        CollisionSource.SystemReserved => "matches a reserved system variable",
                        _ => "violates C64 naming rules",
                    };

                    // Fire clean, non-blocking timed warning alert window
                    Tools.TimedMessage<TokensHelp>($"{ms} [{k}].\n\nPlease correct your entry before proceeding.",
                        "Validation Error", this, MessageBoxIcon.Warning);

                    e.Cancel = true; // Lock focus securely inside this text editing cell
                }
            }
        }

        private static string InvalidValue(string proposedValue, string suffixContext, out CollisionSource source)
        {
            source = CollisionSource.None;
            if (string.IsNullOrEmpty(proposedValue)) return "";

            // 1. Collapse interior spaces for strict C64 parsing equivalence
            string valUpper = proposedValue.ToUpper().Replace(" ", "", StringComparison.Ordinal);
            string suffixUpper = suffixContext.ToUpper().Replace(" ", "");

            // Track data types across both parameters
            bool isString = valUpper.Contains('$') || suffixUpper.Contains('$') || suffixUpper == "$";
            bool isInteger = valUpper.Contains('%') || suffixUpper.Contains('%') || suffixUpper == "%";
            bool isArray = valUpper.Contains('(') || suffixUpper.Contains('(') || suffixUpper == "(";

            // Strip out complex custom suffixes if they are embedded (for Trim automated loop execution)
            if (valUpper.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase))
            {
                valUpper = valUpper[..^7];
                isString = false; isInteger = false; isArray = false; // Reset type flags for custom local vars
            }
            else if (valUpper.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase))
            {
                valUpper = valUpper[..^4];
                isString = false; isInteger = false; isArray = false; // Reset type flags for custom local functions
            }
            else
            {
                // Clean off legacy single-character symbols so valUpper isolated down to its raw alphanumeric root
                if (valUpper.EndsWith('$') || valUpper.EndsWith('%') || valUpper.EndsWith('('))
                {
                    valUpper = valUpper[..^1];
                }
            }

            if (string.IsNullOrEmpty(valUpper)) return "";

            // =========================================================================
            // 2. FIXED SYSTEM REGISTER PREFIX GUARD
            // Uses the combined type detection vectors to approve legal variations.
            // =========================================================================
            if (valUpper.StartsWith("ST") || valUpper.StartsWith("TI"))
            {
                if (valUpper.StartsWith("ST"))
                {
                    // ST only blocks the float space. If it has a verified type extension, it is legal!
                    if (!isString && !isInteger && !isArray)
                    {
                        source = CollisionSource.SystemReserved;
                        return "ST";
                    }
                }
                else if (valUpper.StartsWith("TI"))
                {
                    // TI owns float (TI) and string (TI$).
                    if (!isInteger && !isArray)
                    {
                        if (isString)
                        {
                            // Reassemble full text string to verify literal "TI$" system slot matching
                            string reconstructed = valUpper;
                            if (!reconstructed.EndsWith('$')) reconstructed += "$";

                            if (reconstructed == "TI$")
                            {
                                source = CollisionSource.SystemReserved;
                                return "TI$";
                            }
                        }
                        else
                        {
                            source = CollisionSource.SystemReserved;
                            return "TI";
                        }
                    }
                }
            }

            // =========================================================================
            // 3. GREEDY KEYWORD SCANNING
            // =========================================================================
            string keywordCheckTarget = valUpper.Replace("(", "", StringComparison.Ordinal);

            foreach (string t in Tokens.Tk)
            {
                if (keywordCheckTarget.Contains(t, StringComparison.OrdinalIgnoreCase))
                {
                    source = CollisionSource.Keyword;
                    return t;
                }
            }

            return "";
        }

        private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                var row = dgv.Rows[e.RowIndex];
                if (row.IsNewRow) return;

                var cell = row.Cells[e.ColumnIndex];

                // Capture and immediately clear your low-level form flag
                bool escaped = _escapeKeyPressed;
                _escapeKeyPressed = false;

                if (escaped)
                {
                    // RESTORE VAULT: WinForms rolled back to the stripped value, 
                    // so we override it with our saved copy containing the true original suffix.
                    if (cell.Tag != null)
                    {
                        cell.Value = cell.Tag.ToString();
                    }
                    cell.Tag = null; // Clear vault
                    return;
                }

                // NORMAL EXIT PIPELINE
                cell.Tag = null; // Clear vault for normal exits too

                string currentText = cell.Value?.ToString()?.Trim() ?? string.Empty;

                // --- FIX: Only perform suffix restoration if the user actually typed a name base! ---
                // If currentText is empty, we leave it empty so it can fall back to Column 0.
                if (!string.IsNullOrEmpty(currentText))
                {
                    string columnZeroText = row.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;

                    string suffix = string.Empty;
                    if (columnZeroText.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) suffix = "[DUMMY]";
                    else if (columnZeroText.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) suffix = "[FN]";
                    else if (columnZeroText.EndsWith('(')) suffix = "(";
                    else if (columnZeroText.EndsWith('$')) suffix = "$";
                    else if (columnZeroText.EndsWith('%')) suffix = "%";

                    if (!string.IsNullOrEmpty(suffix) && !currentText.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        cell.Value = currentText + suffix;
                    }
                }

                // --- FIX: Wrap collision updates in BeginInvoke ---
                // This lets the grid completely close out the cell edit state and commit 
                // the empty value string to data matrices before updating or clearing colors!
                this.BeginInvoke(new Action(() =>
                {
                    WarnOnCollisions();
                }));
            }
        }

        private void dgv_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure we are operating within valid data bounds
            if (e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count) return;

            //var row = dgv.Rows[e.RowIndex];

            // --- READONLY CHECK & DIRECTIONAL LOOK-AHEAD SCAN ---
            // Corrected: Explicitly index column 1 to check its read-only assignment
            if (dgv.Rows[e.RowIndex].Cells[1].ReadOnly)
            {
                // Determine direction: default to scanning Down (1) unless we came from a lower row index
                int direction = (e.RowIndex >= _lastRowIndex) ? 1 : -1;

                this.BeginInvoke(new Action(() =>
                {
                    int targetRowIndex = -1;

                    // Scan the grid in the direction of travel to find the next editable cell
                    for (int i = e.RowIndex + direction; i >= 0 && i < dgv.Rows.Count; i += direction)
                    {
                        if (!dgv.Rows[i].Cells[1].ReadOnly && !dgv.Rows[i].IsNewRow)
                        {
                            targetRowIndex = i;
                            break;
                        }
                    }

                    // Scenario A: An editable cell was found -> Focus its column 1 text field
                    if (targetRowIndex != -1 && targetRowIndex < dgv.Rows.Count)
                    {
                        _lastRowIndex = targetRowIndex;
                        dgv.CurrentCell = dgv.Rows[targetRowIndex].Cells[1];
                    }
                    // Scenario B: Hit a dead end (top headerText row or bottom row) 
                    else
                    {
                        // If we have a valid previous position, bounce focus back to it to prevent toggling loops
                        if (_lastRowIndex >= 0 && _lastRowIndex < dgv.Rows.Count && !dgv.Rows[_lastRowIndex].Cells[1].ReadOnly)
                        {
                            dgv.CurrentCell = dgv.Rows[_lastRowIndex].Cells[1];
                        }
                        else
                        {
                            // absolute fallback if no rows are editable anywhere in the entire grid
                            _lastRowIndex = -1;
                            dgv.ClearSelection();
                            dgv.CurrentCell = null;
                        }
                    }
                }));
                return;
            }

            // --- STANDARD COLUMN 0 FORWARDING CASE ---
            if (e.ColumnIndex == 0 && dgv.Columns.Count > 1)
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (e.RowIndex < dgv.Rows.Count)
                    {
                        _lastRowIndex = e.RowIndex;
                        dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[1]; // Direct target to the value cell
                    }
                }));
                return;
            }

            // Track the index for any standard successful focus landing on column 1
            _lastRowIndex = e.RowIndex;
        }

        private void editingTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (sender is not TextBox editingTextBox) return;

            // Isolate the current row context metadata upfront
            string suffixContext = string.Empty;
            if (dgv.CurrentRow != null)
            {
                string columnZeroText = dgv.CurrentRow.Cells[0].Value?.ToString()?.Trim() ?? string.Empty;
                if (columnZeroText.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) suffixContext = "[DUMMY]";
                else if (columnZeroText.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) suffixContext = "[FN]";
                else if (columnZeroText.EndsWith('(')) suffixContext = "(";
                else if (columnZeroText.EndsWith('$')) suffixContext = "$";
                else if (columnZeroText.EndsWith('%')) suffixContext = "%";
            }

            // 1. Intercept Ctrl+V (ASCII value 22)
            if (e.KeyChar == (char)22)
            {
                e.Handled = true; // Block the native, unchecked paste operation

                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrEmpty(clipboardText)) return;

                // Calculate current text state before pasting
                string currentText = editingTextBox.Text;
                int selectionStart = editingTextBox.SelectionStart;
                int selectionLength = editingTextBox.SelectionLength;

                // Remove selected text if the user is overwriting an active selection
                string baseText = currentText.Remove(selectionStart, selectionLength);

                // Construct what the final string will look like if we let it paste
                string proposedText = baseText.Insert(selectionStart, clipboardText);

                // --- DEEP VERIFICATION SWEEP ---
                bool isValidPaste = true;

                // Rule A: Max Character Length Check
                if (proposedText.Length > this.myMaxVarLength)
                {
                    isValidPaste = false;
                }

                // Rule B: Syntax Check (First character must be alphabetic, rest must be alnum or spaces)
                if (isValidPaste && proposedText.Length > 0)
                {
                    if (!char.IsLetter(proposedText[0]))
                    {
                        isValidPaste = false;
                    }

                    for (int i = 1; i < proposedText.Length; i++)
                    {
                        if (!char.IsLetterOrDigit(proposedText[i]))
                        {
                            isValidPaste = false;
                            break; // Early exit optimization
                        }
                    }
                }

                // Rule C: Pre-emptively run the strict C64 token filter matrix!
                CollisionSource collisionSource = CollisionSource.None;
                if (isValidPaste && dgv.CurrentRow != null)
                {
                    // --- FIX: Pass the extracted row type suffix context to protect valid system edits ---
                    string validationResult = InvalidValue(proposedText, suffixContext, out collisionSource);

                    if (!string.IsNullOrEmpty(validationResult))
                    {
                        isValidPaste = false; // Block it immediately if it contains ST, TI, or keywords like OR
                    }
                }

                if (isValidPaste)
                {
                    // Paste passes validation! Insert it safely into the box.
                    editingTextBox.SelectedText = clipboardText;
                }
                else
                {
                    // Paste failed verification! Trigger error feedback animation sequence.
                    if (!this.myEnableWarnings) return;

                    // PRECISE EVALUATION: Pink strictly for length overflows, Blue for any character/syntax/token failures
                    Color targetAlertColor = Color.LightBlue;

                    if (proposedText.Length > this.myMaxVarLength)
                    {
                        targetAlertColor = Color.LightPink;
                    }
                    else if (collisionSource == CollisionSource.SystemReserved || collisionSource == CollisionSource.Keyword)
                    {
                        targetAlertColor = Color.LightBlue;
                    }

                    if (_flashingTextBox == null)
                    {
                        _flashingTextBox = editingTextBox;
                        _originalCellColor = editingTextBox.BackColor;
                    }
                    else
                    {
                        _flashTimer?.Stop();
                    }

                    editingTextBox.BackColor = targetAlertColor;
                    editingTextBox.Invalidate();
                    System.Media.SystemSounds.Beep.Play();
                    _flashTimer?.Start();
                }
                return;
            }

            // 2. Always allow standard control keys (like backspace) regardless of index position
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // 3. Continuous typing syntax evaluation rules
            bool isAllowedCharacter;
            if (editingTextBox.SelectionStart == 0)
            {
                isAllowedCharacter = char.IsLetter(e.KeyChar);
            }
            else
            {
                // Allow typing letter or digits
                isAllowedCharacter = char.IsLetterOrDigit(e.KeyChar);
            }

            bool isLengthLimitReached = editingTextBox.Text.Length >= this.myMaxVarLength;
            bool isSyntaxViolation = !isAllowedCharacter;
            bool isOverflowViolation = isAllowedCharacter && isLengthLimitReached;

            if (isSyntaxViolation || isOverflowViolation)
            {
                e.Handled = true;

                if (!this.myEnableWarnings) return;

                Color targetAlertColor = isSyntaxViolation ? Color.LightBlue : Color.LightPink;

                if (_flashingTextBox == null)
                {
                    _flashingTextBox = editingTextBox;
                    _originalCellColor = editingTextBox.BackColor;
                }
                else
                {
                    _flashTimer?.Stop();
                }

                editingTextBox.BackColor = targetAlertColor;
                editingTextBox.Invalidate();

                System.Media.SystemSounds.Beep.Play();
                _flashTimer?.Start();
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            // 1. Force the DataGridView to push any pending text edits into the validation engine
            dgv.EndEdit();

            // 2. Explicitly validate all child controls on the form
            if (this.ValidateChildren(ValidationConstraints.Enabled))
            {
                // This block will ONLY run if all grid cells pass your blacklist filter!
                SaveDataAndReturnToMain();
            }
            else
            {
                // Validation failed, and the focus is now securely locked back into the invalid cell.
                // We do not need a MessageBox here because your CellValidating event already showed one!
            }
        }

        private bool UpdateVarsDict()
        {
            myVarsDict.Clear();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                // Read cells safely using column indices 0 and 1
                object? cell0 = row.Cells[0].Value;
                object? cell1 = row.Cells[1].Value;

                string? key = cell0?.ToString();
                string? value = cell1?.ToString();

                // Rule 1: Skip if column 1 is null, empty, or whitespace
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                // Skip if key equals value (no change)
                if (string.Equals(key, value, StringComparison.Ordinal))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(key) && !myVarsDict.ContainsKey(key))
                {
                    myVarsDict.Add(key, value);
                }
            }
            return myVarsDict.Count > 0;
        }

        private async void SaveDataAndReturnToMain()
        {
            DialogResult rs;

            // Map grid changes to your tracking dictionary (preserving raw keys)
            if (!UpdateVarsDict())
            {
                rs = MessageBox.Show("No changes to process. Do you want to continue editing?", "Nothing To Do",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (rs == DialogResult.Yes) { return; }
                else { this.DialogResult = DialogResult.Abort; this.Close(); return; }
            }

            // Check for variable signature conflicts (soft pink alerts apply only to Column 1)
            if (WarnOnCollisions())
            {
                rs = MessageBox.Show("There are one or more new variable names which are\nduplicate conflict errors " +
                    $"(highlighted {collisionColorName} in the grid).\n\nDo you want to proceed with the refactor process anyway?",
                    "Warning! Duplicate Variable Names Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (rs != DialogResult.Yes)
                {
                    return; // Abort submission and stay in the grid editor safely
                }
            }

            this.Visible = false; // Hide the form while processing

            try
            {
                // Process any changed variables and write output file
                Refactorer.RefactorResult res = await Refactorer.ScanBasicVarsAsync(myBytes!, myVarsDict!, myForm!, cts!.Token, true);
                if (string.IsNullOrWhiteSpace(res.Message))
                {
                    string myOutFile = myForm?.outFile ?? "";
                    if (!string.IsNullOrEmpty(myOutFile))
                    {
                        try
                        {
                            File.WriteAllBytes(myOutFile, res.Mem!);
                        }
                        catch (Exception ex)
                        {
                            Tools.TimedMessage($"Failed to write output file '{myOutFile}'\n{ex.Message}", "File Write Error",
                                this,MessageBoxIcon.Error, myWaitTime);
                            this.DialogResult = DialogResult.Abort;
                            this.Close();
                            return;
                        }
                        this.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        // This should never happen because the main form should have already
                        // validated the output file path, but we check just in case.
                        Tools.TimedMessage("No output file path was specified.\nChanges will not be saved to disk.",
                            "Output Filename Missing", this, MessageBoxIcon.Error, myWaitTime);
                        this.DialogResult = DialogResult.Abort;
                        this.Close();
                        return;
                    }
                    Tools.TimedMessage($"Refactoring complete. Output file: {myOutFile}", "Success",
                        this, MessageBoxIcon.Information, myWaitTime);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    Tools.TimedMessage($"{res.Message}\nNo output file written.", "Refactoring Error", this,
                        MessageBoxIcon.Warning, myWaitTime);
                    this.DialogResult = DialogResult.Abort;
                    this.Close(); return;
                }
            }
            catch (OperationCanceledException)
            {
                Tools.TimedMessage("Refactoring canceled,\nNo output file written.", "Info", this,
                    MessageBoxIcon.Warning, myWaitTime);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Tools.TimedMessage($"An unexpected error occurred:\n{ex.Message}", "Error", this,
                    MessageBoxIcon.Error, myWaitTime);
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            myForm!._refproc = false;
            myForm.EndProc();
            Close();
        }

        private void RefactorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (myCloseRefOnEsc && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                myForm!._refproc = false;
                myForm.EndProc();
                this.Close();
            }
        }

        // Form-level field to hold the full string before editing begins
        private bool _escapeKeyPressed = false;
        // Reentrancy guard for trimming loop
        private bool _isTrimming = false;

        // Catch the ESC key before it tears down the editing session
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && dgv.EditingControl != null)
            {
                _escapeKeyPressed = true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public enum CollisionSource
        {
            None,
            Keyword,       // Matched Tokens.Tk
            SystemReserved // Matched Tokens.ResV
        }

        // Essential method for using the HelpTools class to display a Help window
        protected override void WndProc(ref Message m)
        {
            // The form intercepts the message stream here and hands it to the tool class
            // Simply specify your class name inside the angle brackets
            if (HelpTools.HandleHelpMessage<TokensHelp>(ref m, this))
            {
                return; // Short-circuit: The message was handled, so stop processing it
            }

            // 2. STABILIZE HEADER DOUBLE-CLICKS
            // 0x00A3 is the Windows message ID for WM_NCLBUTTONDBLCLK (Non-Client Left Button Double Click)
            // This fires when clicking headers, title bars, or borders.
            if (m.Msg == 0x00A3)
            {
                // Use a grid hit-test to see if the double-click happened on a column headerText
                Point clientPt = dgv.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hit = dgv.HitTest(clientPt.X, clientPt.Y);

                if (hit.Type == DataGridViewHitTestType.ColumnHeader && hit.ColumnIndex == 1)
                {
                    // Direct the click event directly to your handler method manually
                    ExecuteColumnOneHeaderDoubleClickAction();

                    // CRITICAL STEP: Set the message result to Handled (IntPtr.Zero) and return!
                    // This consumes Click #2 completely, stopping the sorting engine from breaking your layout rows.
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            // Let all other standard messages (move, resize, paint) pass through normally
            base.WndProc(ref m);
        }

        private void btTrim_Click(object sender, EventArgs e)
        {
            ExecuteTrimLoop();
        }

        private void ExecuteTrimLoop()
        {
            if (_isTrimming) return;
            _isTrimming = true;

            try
            {
                dgv.CurrentCell = null;
                bool anyChange = false;

                dgv.SuspendLayout();
                try
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells[1].ReadOnly) continue;

                        string cellZeroText = row.Cells[0].Value?.ToString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(cellZeroText)) continue;

                        string currentColumnOneText = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty;
                        bool isColumnOneEmpty = string.IsNullOrEmpty(currentColumnOneText);

                        string finalTrimmedValue;

                        // --- SIMPLIFIED PIPELINE ---
                        if (myOverwriteOnTrim || isColumnOneEmpty)
                        {
                            // Column 1 contents don't matter at all; generate replacement directly from Column 0
                            finalTrimmedValue = ComputeTrimmedValueFromColumnZero(cellZeroText);
                        }
                        else
                        {
                            // Overwrite is FALSE and Column 1 has a value. Check if it needs a self-trim.
                            string colOneSuffix = string.Empty;
                            string colOneTextWithoutSuffix = currentColumnOneText;

                            if (colOneTextWithoutSuffix.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) colOneSuffix = "[DUMMY]";
                            else if (colOneTextWithoutSuffix.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) colOneSuffix = "[FN]";
                            else if (colOneTextWithoutSuffix.EndsWith('(')) colOneSuffix = "(";
                            else if (colOneTextWithoutSuffix.EndsWith('$')) colOneSuffix = "$";
                            else if (colOneTextWithoutSuffix.EndsWith('%')) colOneSuffix = "%";

                            if (!string.IsNullOrEmpty(colOneSuffix))
                            {
                                colOneTextWithoutSuffix = colOneTextWithoutSuffix[..^colOneSuffix.Length];
                            }

                            string colOneCollapsedBase = colOneTextWithoutSuffix.Replace(" ", string.Empty);

                            // Skip processing if it's already within legal limits
                            if (colOneCollapsedBase.Length <= 2) continue;

                            // It's too long! Force trim it down using its own character base
                            string colOneTrimmedBase = colOneCollapsedBase[..2];
                            finalTrimmedValue = colOneTrimmedBase + colOneSuffix;
                        }

                        // Guard clause: Prevent compacting into a reserved C64 token/keyword
                        string keywordMatch = InvalidValue(finalTrimmedValue, "", out CollisionSource source);
                        if (!string.IsNullOrEmpty(keywordMatch)) continue;

                        // Apply change if it differs from what is currently visible in Column 1
                        if (!string.Equals(currentColumnOneText, finalTrimmedValue, StringComparison.Ordinal))
                        {
                            row.Cells[1].Value = finalTrimmedValue;
                            anyChange = true;
                        }
                    }
                }
                finally
                {
                    dgv.ResumeLayout();
                }

                if (anyChange)
                {
                    ApplyDynamicColumnOneColors(); // Refresh Column 1 graphics instantly
                    WarnOnCollisions();            // Soft-pink highlights for active naming errors
                }
            }
            finally
            {
                _isTrimming = false;
            }
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            ExecuteManualResetLoop();
        }

        private void ExecuteManualResetLoop()
        {
            // 1. Clear out active cell selections to avoid focus artifacts
            dgv.CurrentCell = null;

            dgv.SuspendLayout();
            try
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    // PROTECTIVE SYSTEM VARIABLES SHIELD
                    // Skip structurally locked, greyed-out system rows completely
                    if (row.Cells[1].ReadOnly) continue;

                    // 2. Wipe the cell text value back to empty
                    row.Cells[1].Value = string.Empty;

                    // 3. Reset Column 1 immediately to standard grid defaults
                    row.Cells[1].Style.BackColor = dgv.DefaultCellStyle.BackColor;
                    row.Cells[1].Style.ForeColor = dgv.DefaultCellStyle.ForeColor;
                }
            }
            finally
            {
                dgv.ResumeLayout();
            }

            // 4. Force a safety color brush refresh sweep across the UI grid
            ApplyDynamicColumnOneColors();
            WarnOnCollisions();
        }

        private void InitializeFlashTimer() // Call this initialization helper method in your Form_Load event handler
        {
            _flashTimer = new System.Windows.Forms.Timer
            { Interval = 200 }; // Flash duration in milliseconds
            _flashTimer.Tick += FlashTimer_Tick;
        }

        private void FlashTimer_Tick(object? sender, EventArgs e)
        {
            _flashTimer?.Stop();

            if (_flashingTextBox != null)
            {
                // Restore the original background color captured before the alert
                _flashingTextBox.BackColor = _originalCellColor;
                _flashingTextBox.Invalidate();
            }
            // Reset state tracking variables to accept the next error state cleanly
            _flashingTextBox = null;
            _originalCellColor = Color.Empty;
        }

        private static string ComputeTrimmedValueFromColumnZero(string cellZeroText)
        {
            if (string.IsNullOrEmpty(cellZeroText)) return string.Empty;

            // --- FIX: Extract true terminal suffixes explicitly before processing the base text string ---
            string suffix = string.Empty;
            string textWithoutSuffix = cellZeroText.Trim();

            if (textWithoutSuffix.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase)) suffix = "[DUMMY]";
            else if (textWithoutSuffix.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase)) suffix = "[FN]";
            else if (textWithoutSuffix.EndsWith('(')) suffix = "(";
            else if (textWithoutSuffix.EndsWith('$')) suffix = "$";
            else if (textWithoutSuffix.EndsWith('%')) suffix = "%";

            if (!string.IsNullOrEmpty(suffix))
            {
                textWithoutSuffix = textWithoutSuffix[..^suffix.Length];
            }

            // Now safely collapse all spacing characters from the true variable name segment
            string collapsedBase = textWithoutSuffix.Replace(" ", string.Empty);
            string trimmedBase = collapsedBase.Length >= 2 ? collapsedBase[..2] : collapsedBase;

            return trimmedBase + suffix;
        }

        // Compute canonical signature for duplicate grouping; returns null for system-reserved/empty
        private static string? ComputeSignatureFromCellText(string? rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return null;

            string collapsedKey = rawText.Replace(" ", "").ToUpper();

            // --- FIX 1: Run the protective intercept BEFORE slicing the string suffixes ---
            bool isReservedST = collapsedKey.StartsWith("ST")
                                && !collapsedKey.Contains('$')
                                && !collapsedKey.Contains('%')
                                && !collapsedKey.Contains('(');

            bool isReservedTI = collapsedKey.StartsWith("TI")
                                && !collapsedKey.Contains('%');

            if (isReservedST || isReservedTI) return null;

            // --- FIX 2: Explicitly extract custom multi-character type suffixes first ---
            string typeSuffix = "";
            bool isArray = false;
            string baseKey = collapsedKey;

            if (baseKey.EndsWith("[DUMMY]", StringComparison.OrdinalIgnoreCase))
            {
                typeSuffix = "[DUMMY]";
                baseKey = baseKey[..^7];
            }
            else if (baseKey.EndsWith("[FN]", StringComparison.OrdinalIgnoreCase))
            {
                typeSuffix = "[FN]";
                baseKey = baseKey[..^4];
            }
            else
            {
                // Fall back to scanning native legacy single-character Commodore tokens
                if (baseKey.EndsWith('('))
                {
                    isArray = true;
                    baseKey = baseKey[..^1];
                }

                if (baseKey.EndsWith('$'))
                {
                    typeSuffix = "$";
                    baseKey = baseKey[..^1];
                }
                else if (baseKey.EndsWith('%'))
                {
                    typeSuffix = "%";
                    baseKey = baseKey[..^1];
                }
            }

            // Isolate the matching unique identifier token signature
            string significantChars = baseKey.Length >= 2 ? baseKey[..2] : baseKey;
            string arrayIndicator = isArray ? "(" : "";

            return $"{significantChars}_{typeSuffix}{arrayIndicator}";
        }

        private void ExecuteColumnOneHeaderDoubleClickAction()
        {
            string oTrim = "Trim";
            string nTrim = "Over";
            if (myOverwriteOnTrim) { oTrim = "Over"; nTrim = "Trim"; }
            string headerText = dgv.Columns[1].HeaderText;
            int w = myOverwriteOnTrim ? 1 : 2;
            int c = 0;
            dgv.Columns[1].HeaderText = Regex.Replace(headerText, Regex.Escape(oTrim), m =>
            {
                c++;
                return (c == w) ? nTrim : m.Value;
            });
            myOverwriteOnTrim = !myOverwriteOnTrim;
            Settings.CurrentSettings.OverwriteOnTrim = myOverwriteOnTrim;
            myForm!.optOverwriteOnTrim = myOverwriteOnTrim;
            Settings.Save(myForm!, false);
        }

        private void dgv_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Target only the Column 1 headerText
            if (e.ColumnIndex == 1)
            {
                // TRUE DOUBLE-CLICK CAUGHT! 
                // Clear the pending single-click flag instantly so the sort code is completely bypassed
                _isDoubleClickPending = false;

                // Run your custom action safely on perfectly stable data rows
                ExecuteColumnOneHeaderDoubleClickAction();
            }
        }

        private bool _isDoubleClickPending = false;
        private bool _isColumnOneSortingAscending = true;
        private async void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // --- COLUMN 0: Sorts immediately with no intervention ---
            if (e.ColumnIndex == 0)
            {
                NativeSortColumnZero();
                return;
            }

            // --- COLUMN 1: Intercept single click sorting ---
            if (e.ColumnIndex == 1)
            {
                _isDoubleClickPending = true;

                // Wait a brief fraction of a second (aligned with OS double click speeds) 
                // to see if the double-click handler trips our flag
                await System.Threading.Tasks.Task.Delay(SystemInformation.DoubleClickTime, cts!.Token);

                if (_isDoubleClickPending)
                {
                    // No second click arrived! It's a true single click, safe to sort now.
                    _isDoubleClickPending = false;
                    NativeSortColumnOne();
                }
            }
        }

        private bool _isColumnZeroSortingAscending = true; // Tracks Column 0 sort toggle

        private void NativeSortColumnZero()
        {
            dgv.CurrentCell = null; // Clear active cell focus to avoid index glitches

            // 1. Pull the data grid's current rows back into a localized dictionary
            Dictionary<string, string> currentGridData = [];
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                string key = row.Cells[0].Value?.ToString() ?? string.Empty;
                string value = row.Cells[1].Value?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(key) && !currentGridData.ContainsKey(key))
                {
                    currentGridData.Add(key, value);
                }
            }

            // 2. Pass data to your custom C64 sorting routine
            var baseSorted = DictionarySortUtils.SortByKeyFirstTwoAlnumThenSuffix(currentGridData);

            List<KeyValuePair<string, string>> sortedPairs = [.. baseSorted];

            if (!_isColumnZeroSortingAscending)
            {
                sortedPairs.Reverse(); // Invert the list in-place safely without altering the type definition
            }

            // 3. Suspend drawing layouts and clear/reload the grid in the updated sequence
            dgv.SuspendLayout();
            try
            {
                dgv.Rows.Clear();

                foreach (var pair in sortedPairs)
                {
                    int rowIndex = dgv.Rows.Add(pair.Key, pair.Value);
                    DataGridViewRow row = dgv.Rows[rowIndex];

                    string upperKey = pair.Key.ToUpper().Replace(" ", "");

                    // --- FIXED SYSTEM REGISTER INTERCEPT LOOP ---
                    bool isReservedST = false;
                    bool isReservedTI = false;

                    if (upperKey.StartsWith("ST") || upperKey.StartsWith("TI"))
                    {
                        bool isString = upperKey.Contains('$');
                        bool isInteger = upperKey.Contains('%');
                        bool isArray = upperKey.Contains('(');

                        if (upperKey.StartsWith("ST"))
                        {
                            if (!isString && !isInteger && !isArray) isReservedST = true;
                        }
                        else if (upperKey.StartsWith("TI"))
                        {
                            if (!isInteger && !isArray)
                            {
                                if (isString)
                                {
                                    if (upperKey == "TI$") isReservedTI = true;
                                }
                                else
                                {
                                    isReservedTI = true;
                                }
                            }
                        }
                    }

                    if (isReservedST || isReservedTI)
                    {
                        row.Cells[0].Style.BackColor = Color.FromArgb(139, 0, 0); // Dark Red
                        row.Cells[0].Style.ForeColor = Color.White;                // White Text
                        row.Cells[1].ReadOnly = true;                             // Lock edit access
                        row.Cells[1].Style.BackColor = Color.FromArgb(225, 230, 235);
                    }
                    else
                    {
                        // Re-apply original immutable Golden Ratio background group coloring to Column 0
                        string signature = ComputeSignatureFromCellText(pair.Key) ?? string.Empty;
                        if (_prefixColors.TryGetValue(signature, out Color assignedColor))
                        {
                            row.Cells[0].Style.BackColor = assignedColor;
                            row.Cells[0].Style.ForeColor = Color.Black;
                        }
                    }
                }
            }
            finally
            {
                dgv.ResumeLayout();
            }

            // 4. Force a safety color brush refresh sequence across Column 1
            ApplyDynamicColumnOneColors();
            WarnOnCollisions();

            // 5. Toggle sorting direction tracking for your next headerText click event
            _isColumnZeroSortingAscending = !_isColumnZeroSortingAscending;
        }

        private void NativeSortColumnOne()
        {
            ListSortDirection direction = _isColumnOneSortingAscending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            dgv.Sort(dgv.Columns[1], direction);
            _isColumnOneSortingAscending = !_isColumnOneSortingAscending; // Toggle sort direction state
        }

        private DataGridViewCell? savedCurrentCell = null;
        private void dgv_Enter(object sender, EventArgs e)
        {
            if (savedCurrentCell != null && savedCurrentCell.RowIndex >= 0)
            {
                savedCurrentCell.Selected = true;
                dgv.CurrentCell = savedCurrentCell;
            }
        }

        private void dgv_Leave(object sender, EventArgs e)
        {
            // Save the currently active cell and clear the visual highlight
            savedCurrentCell = dgv.CurrentCell;
            dgv.ClearSelection();
        }
    }
}
