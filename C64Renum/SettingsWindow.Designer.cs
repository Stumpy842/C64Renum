namespace C64Renum
{
    partial class SettingsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            label1 = new Label();
            tbInputFolder = new TextBox();
            btInputFolder = new Button();
            label2 = new Label();
            tbOutputFolder = new TextBox();
            btOutputFolder = new Button();
            cbOutUseInputFolder = new CheckBox();
            btSave = new Button();
            btCancel = new Button();
            groupBox1 = new GroupBox();
            cbClrOutFilePath = new CheckBox();
            cbCloseOnEsc = new CheckBox();
            cbWrap = new CheckBox();
            cbNoPromptOverwrite = new CheckBox();
            cbDeleteOnCancel = new CheckBox();
            tbrSpeed = new TrackBar();
            cbDemoMode = new CheckBox();
            cbClearOnInvalid = new CheckBox();
            groupBox2 = new GroupBox();
            lbBackground = new FocusableLabel();
            lbForeground = new FocusableLabel();
            cbLineLength = new CheckBox();
            label5 = new Label();
            label4 = new Label();
            groupBox3 = new GroupBox();
            cbOverwriteOnTrim = new CheckBox();
            cbCloseRefOnEsc = new CheckBox();
            cbEnableWarnings = new CheckBox();
            label3 = new Label();
            nudMaxVarLength = new NumericUpDown();
            toolTip1 = new ToolTip(components);
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbrSpeed).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxVarLength).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 20);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 0;
            label1.Text = "Input Folder";
            // 
            // tbInputFolder
            // 
            tbInputFolder.Location = new Point(12, 38);
            tbInputFolder.Name = "tbInputFolder";
            tbInputFolder.PlaceholderText = "Input Folder";
            tbInputFolder.Size = new Size(600, 23);
            tbInputFolder.TabIndex = 1;
            tbInputFolder.KeyDown += tbInputFolder_KeyDown;
            // 
            // btInputFolder
            // 
            btInputFolder.AutoSize = true;
            btInputFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btInputFolder.Location = new Point(618, 36);
            btInputFolder.Name = "btInputFolder";
            btInputFolder.Size = new Size(26, 25);
            btInputFolder.TabIndex = 2;
            btInputFolder.Text = "...";
            toolTip1.SetToolTip(btInputFolder, "Browse to select Input Folder");
            btInputFolder.UseVisualStyleBackColor = true;
            btInputFolder.Click += btFolder_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 78);
            label2.Name = "label2";
            label2.Size = new Size(77, 15);
            label2.TabIndex = 3;
            label2.Text = "Ouput Folder";
            // 
            // tbOutputFolder
            // 
            tbOutputFolder.Location = new Point(12, 96);
            tbOutputFolder.Name = "tbOutputFolder";
            tbOutputFolder.PlaceholderText = "Output Folder";
            tbOutputFolder.Size = new Size(600, 23);
            tbOutputFolder.TabIndex = 5;
            tbOutputFolder.KeyDown += tbOutputFolder_KeyDown;
            // 
            // btOutputFolder
            // 
            btOutputFolder.AutoSize = true;
            btOutputFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btOutputFolder.Location = new Point(618, 94);
            btOutputFolder.Name = "btOutputFolder";
            btOutputFolder.Size = new Size(26, 25);
            btOutputFolder.TabIndex = 6;
            btOutputFolder.Text = "...";
            toolTip1.SetToolTip(btOutputFolder, "Browse to select Output Folder");
            btOutputFolder.UseVisualStyleBackColor = true;
            btOutputFolder.Click += btFolder_Click;
            // 
            // cbOutUseInputFolder
            // 
            cbOutUseInputFolder.AutoSize = true;
            cbOutUseInputFolder.Location = new Point(126, 78);
            cbOutUseInputFolder.Name = "cbOutUseInputFolder";
            cbOutUseInputFolder.Size = new Size(112, 19);
            cbOutUseInputFolder.TabIndex = 4;
            cbOutUseInputFolder.Text = "Use &Input Folder";
            toolTip1.SetToolTip(cbOutUseInputFolder, "Use the Input Folder as the Output Folder");
            cbOutUseInputFolder.UseVisualStyleBackColor = true;
            cbOutUseInputFolder.CheckedChanged += cbOutUseInputFolder_CheckedChanged;
            // 
            // btSave
            // 
            btSave.Location = new Point(222, 442);
            btSave.Name = "btSave";
            btSave.Size = new Size(80, 40);
            btSave.TabIndex = 10;
            btSave.Text = "&Save";
            btSave.UseVisualStyleBackColor = true;
            btSave.Click += btSave_Click;
            // 
            // btCancel
            // 
            btCancel.Location = new Point(353, 442);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(80, 40);
            btCancel.TabIndex = 11;
            btCancel.Text = "&Cancel";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cbClrOutFilePath);
            groupBox1.Controls.Add(cbCloseOnEsc);
            groupBox1.Controls.Add(cbWrap);
            groupBox1.Controls.Add(cbNoPromptOverwrite);
            groupBox1.Controls.Add(cbDeleteOnCancel);
            groupBox1.Location = new Point(22, 143);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(290, 155);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Program Options";
            // 
            // cbClrOutFilePath
            // 
            cbClrOutFilePath.AutoSize = true;
            cbClrOutFilePath.Location = new Point(6, 122);
            cbClrOutFilePath.Name = "cbClrOutFilePath";
            cbClrOutFilePath.Size = new Size(196, 19);
            cbClrOutFilePath.TabIndex = 4;
            cbClrOutFilePath.Text = "Clear Output File&name if invalid ";
            toolTip1.SetToolTip(cbClrOutFilePath, "Clears the Output Filename field if it\r\ncontains an invalid path or filename\r\n\r\n");
            cbClrOutFilePath.UseVisualStyleBackColor = true;
            // 
            // cbCloseOnEsc
            // 
            cbCloseOnEsc.AutoSize = true;
            cbCloseOnEsc.Location = new Point(6, 97);
            cbCloseOnEsc.Name = "cbCloseOnEsc";
            cbCloseOnEsc.Size = new Size(234, 19);
            cbCloseOnEsc.TabIndex = 3;
            cbCloseOnEsc.Text = "Close main &window on Escape keypress";
            cbCloseOnEsc.UseVisualStyleBackColor = true;
            // 
            // cbWrap
            // 
            cbWrap.AutoSize = true;
            cbWrap.Location = new Point(6, 72);
            cbWrap.Name = "cbWrap";
            cbWrap.Size = new Size(212, 19);
            cbWrap.TabIndex = 2;
            cbWrap.Text = "Wr&ap BASIC display at 40 chars/line";
            cbWrap.UseVisualStyleBackColor = true;
            // 
            // cbNoPromptOverwrite
            // 
            cbNoPromptOverwrite.AutoSize = true;
            cbNoPromptOverwrite.Location = new Point(6, 47);
            cbNoPromptOverwrite.Name = "cbNoPromptOverwrite";
            cbNoPromptOverwrite.Size = new Size(233, 19);
            cbNoPromptOverwrite.TabIndex = 1;
            cbNoPromptOverwrite.Text = "Do not prompt to &overwrite existing file";
            cbNoPromptOverwrite.UseVisualStyleBackColor = true;
            // 
            // cbDeleteOnCancel
            // 
            cbDeleteOnCancel.AutoSize = true;
            cbDeleteOnCancel.Location = new Point(6, 22);
            cbDeleteOnCancel.Name = "cbDeleteOnCancel";
            cbDeleteOnCancel.Size = new Size(215, 19);
            cbDeleteOnCancel.TabIndex = 0;
            cbDeleteOnCancel.Text = "&Delete output file on Cancel or Error";
            cbDeleteOnCancel.UseVisualStyleBackColor = true;
            // 
            // tbrSpeed
            // 
            tbrSpeed.AutoSize = false;
            tbrSpeed.BackColor = SystemColors.Control;
            tbrSpeed.LargeChange = 1;
            tbrSpeed.Location = new Point(460, 22);
            tbrSpeed.Maximum = 6;
            tbrSpeed.Minimum = 1;
            tbrSpeed.Name = "tbrSpeed";
            tbrSpeed.Size = new Size(104, 29);
            tbrSpeed.TabIndex = 5;
            toolTip1.SetToolTip(tbrSpeed, "< Faster --- Slower >");
            tbrSpeed.Value = 1;
            // 
            // cbDemoMode
            // 
            cbDemoMode.AutoSize = true;
            cbDemoMode.Location = new Point(331, 27);
            cbDemoMode.Name = "cbDemoMode";
            cbDemoMode.Size = new Size(127, 19);
            cbDemoMode.TabIndex = 4;
            cbDemoMode.Text = "Demo (slow) &mode";
            toolTip1.SetToolTip(cbDemoMode, "Slows down display of main window BASIC listing");
            cbDemoMode.UseVisualStyleBackColor = true;
            cbDemoMode.CheckedChanged += cbDemoMode_CheckedChanged;
            // 
            // cbClearOnInvalid
            // 
            cbClearOnInvalid.AutoSize = true;
            cbClearOnInvalid.Location = new Point(6, 22);
            cbClearOnInvalid.Name = "cbClearOnInvalid";
            cbClearOnInvalid.Size = new Size(184, 19);
            cbClearOnInvalid.TabIndex = 0;
            cbClearOnInvalid.Text = "Clear grid cell on invalid &value";
            toolTip1.SetToolTip(cbClearOnInvalid, "Automatically clears cell for reentry after invalid value");
            cbClearOnInvalid.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lbBackground);
            groupBox2.Controls.Add(lbForeground);
            groupBox2.Controls.Add(cbLineLength);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(tbrSpeed);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(cbDemoMode);
            groupBox2.Location = new Point(18, 320);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(615, 98);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Output Colors && Mode";
            // 
            // lbBackground
            // 
            lbBackground.BackColor = SystemColors.ControlDark;
            lbBackground.Location = new Point(186, 26);
            lbBackground.Name = "lbBackground";
            lbBackground.Size = new Size(40, 40);
            lbBackground.TabIndex = 2;
            toolTip1.SetToolTip(lbBackground, "Background color for main window BASIC output");
            lbBackground.Click += lbBackground_Click;
            // 
            // lbForeground
            // 
            lbForeground.BackColor = SystemColors.ControlDark;
            lbForeground.Location = new Point(46, 26);
            lbForeground.Name = "lbForeground";
            lbForeground.Size = new Size(40, 40);
            lbForeground.TabIndex = 0;
            toolTip1.SetToolTip(lbForeground, "Foreground color for main window BASIC output");
            lbForeground.Click += lbForeground_Click;
            // 
            // cbLineLength
            // 
            cbLineLength.AutoSize = true;
            cbLineLength.Location = new Point(331, 57);
            cbLineLength.Name = "cbLineLength";
            cbLineLength.Size = new Size(205, 19);
            cbLineLength.TabIndex = 6;
            cbLineLength.Text = "&Limit output lines to 80 characters";
            toolTip1.SetToolTip(cbLineLength, "Output files will be editable with a C64 or emulator if checked.\r\nLines may reach 250 characters if unchecked.\r\nApplies to both Renumber & Refactore modes.");
            cbLineLength.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(159, 73);
            label5.Name = "label5";
            label5.Size = new Size(95, 15);
            label5.TabIndex = 3;
            label5.Text = "Text &Background";
            toolTip1.SetToolTip(label5, "Background color for main window BASIC output");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 72);
            label4.Name = "label4";
            label4.Size = new Size(96, 15);
            label4.TabIndex = 1;
            label4.Text = "Text  &Foreground";
            toolTip1.SetToolTip(label4, "Foreground color for main window BASIC output");
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cbOverwriteOnTrim);
            groupBox3.Controls.Add(cbCloseRefOnEsc);
            groupBox3.Controls.Add(cbEnableWarnings);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(nudMaxVarLength);
            groupBox3.Controls.Add(cbClearOnInvalid);
            groupBox3.Location = new Point(343, 143);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(290, 155);
            groupBox3.TabIndex = 8;
            groupBox3.TabStop = false;
            groupBox3.Text = "Refactor Options";
            // 
            // cbOverwriteOnTrim
            // 
            cbOverwriteOnTrim.AutoSize = true;
            cbOverwriteOnTrim.Location = new Point(6, 122);
            cbOverwriteOnTrim.Name = "cbOverwriteOnTrim";
            cbOverwriteOnTrim.Size = new Size(182, 19);
            cbOverwriteOnTrim.TabIndex = 5;
            cbOverwriteOnTrim.Text = "Overwrite new values on &Trim";
            toolTip1.SetToolTip(cbOverwriteOnTrim, "Any new values will be replaced by a trimmed\r\nversion of the original value when checked");
            cbOverwriteOnTrim.UseVisualStyleBackColor = true;
            // 
            // cbCloseRefOnEsc
            // 
            cbCloseRefOnEsc.AutoSize = true;
            cbCloseRefOnEsc.Location = new Point(6, 97);
            cbCloseRefOnEsc.Name = "cbCloseRefOnEsc";
            cbCloseRefOnEsc.Size = new Size(203, 19);
            cbCloseRefOnEsc.TabIndex = 4;
            cbCloseRefOnEsc.Text = "Close &Refactor window on Escape";
            cbCloseRefOnEsc.UseVisualStyleBackColor = true;
            // 
            // cbEnableWarnings
            // 
            cbEnableWarnings.AutoSize = true;
            cbEnableWarnings.Location = new Point(6, 47);
            cbEnableWarnings.Name = "cbEnableWarnings";
            cbEnableWarnings.Size = new Size(198, 19);
            cbEnableWarnings.TabIndex = 1;
            cbEnableWarnings.Text = "&Enable warnings on invalid input";
            toolTip1.SetToolTip(cbEnableWarnings, "Flashes cell background on invalid input:\r\n     Pink = Too many characters\r\n     Blue = Invalid character in name");
            cbEnableWarnings.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(55, 74);
            label3.Name = "label3";
            label3.Size = new Size(142, 15);
            label3.TabIndex = 3;
            label3.Text = "Ma&ximum variable length";
            toolTip1.SetToolTip(label3, "Limit new variable names to this length");
            // 
            // nudMaxVarLength
            // 
            nudMaxVarLength.Location = new Point(6, 70);
            nudMaxVarLength.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            nudMaxVarLength.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            nudMaxVarLength.Name = "nudMaxVarLength";
            nudMaxVarLength.Size = new Size(43, 23);
            nudMaxVarLength.TabIndex = 2;
            nudMaxVarLength.TextAlign = HorizontalAlignment.Right;
            nudMaxVarLength.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // SettingsWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(654, 505);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(btCancel);
            Controls.Add(btSave);
            Controls.Add(cbOutUseInputFolder);
            Controls.Add(btOutputFolder);
            Controls.Add(btInputFolder);
            Controls.Add(tbOutputFolder);
            Controls.Add(tbInputFolder);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "SettingsWindow";
            Text = "SettingsWindow";
            KeyDown += SettingsWindow_KeyDown;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbrSpeed).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxVarLength).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button btInputFolder;
        private Label label2;
        private Button btOutputFolder;
        private Button btSave;
        private Button btCancel;
        internal TextBox tbInputFolder;
        internal TextBox tbOutputFolder;
        internal CheckBox cbOutUseInputFolder;
        private GroupBox groupBox1;
        internal CheckBox cbDeleteOnCancel;
        internal CheckBox cbNoPromptOverwrite;
        private GroupBox groupBox2;
        private Label label4;
        private Label label5;
        internal CheckBox cbWrap;
        internal CheckBox cbDemoMode;
        internal TrackBar tbrSpeed;
        internal CheckBox cbCloseOnEsc;
        internal CheckBox cbClearOnInvalid;
        private GroupBox groupBox3;
        internal NumericUpDown nudMaxVarLength;
        private Label label3;
        internal CheckBox cbEnableWarnings;
        internal CheckBox cbCloseRefOnEsc;
        private ToolTip toolTip1;
        internal CheckBox cbLineLength;
        internal CheckBox cbOverwriteOnTrim;
        internal FocusableLabel lbBackground;
        internal FocusableLabel lbForeground;
        internal CheckBox cbClrOutFilePath;
    }
}