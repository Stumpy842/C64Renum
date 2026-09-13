namespace C64Renum
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btInputFile = new Button();
            btProcess = new Button();
            btCancel = new Button();
            toolTip1 = new ToolTip(components);
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem1 = new ToolStripMenuItem();
            label1 = new Label();
            tbOutputFile = new TextBox();
            btOutputFile = new Button();
            label2 = new Label();
            lbInputFile = new Label();
            rtb = new RichTextBox();
            label3 = new Label();
            tbStart = new TextBox();
            label4 = new Label();
            tbStep = new TextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btInputFile
            // 
            btInputFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btInputFile.Location = new Point(12, 48);
            btInputFile.Name = "btInputFile";
            btInputFile.Size = new Size(33, 23);
            btInputFile.TabIndex = 0;
            btInputFile.Text = "...";
            btInputFile.UseVisualStyleBackColor = true;
            btInputFile.Click += btInputFile_Click;
            // 
            // btProcess
            // 
            btProcess.Location = new Point(750, 90);
            btProcess.Name = "btProcess";
            btProcess.Size = new Size(80, 40);
            btProcess.TabIndex = 6;
            btProcess.Text = "Process";
            btProcess.UseVisualStyleBackColor = true;
            btProcess.Click += btProcess_Click;
            // 
            // btCancel
            // 
            btCancel.Location = new Point(877, 90);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(80, 40);
            btCancel.TabIndex = 7;
            btCancel.Text = "Cancel";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(973, 24);
            menuStrip1.TabIndex = 9;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, settingsToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(116, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += btInputFile_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(116, 22);
            settingsToolStripMenuItem.Text = "&Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(116, 22);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem1 });
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(44, 20);
            aboutToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem1
            // 
            aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            aboutToolStripMenuItem1.Size = new Size(107, 22);
            aboutToolStripMenuItem1.Text = "&About";
            aboutToolStripMenuItem1.Click += aboutToolStripMenuItem1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(51, 30);
            label1.Name = "label1";
            label1.Size = new Size(56, 15);
            label1.TabIndex = 10;
            label1.Text = "Input File";
            // 
            // tbOutputFile
            // 
            tbOutputFile.Location = new Point(51, 107);
            tbOutputFile.Name = "tbOutputFile";
            tbOutputFile.Size = new Size(623, 23);
            tbOutputFile.TabIndex = 3;
            tbOutputFile.TextChanged += tbOutputFile_TextChanged;
            // 
            // btOutputFile
            // 
            btOutputFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btOutputFile.Location = new Point(12, 107);
            btOutputFile.Name = "btOutputFile";
            btOutputFile.Size = new Size(33, 23);
            btOutputFile.TabIndex = 2;
            btOutputFile.Text = "...";
            btOutputFile.UseVisualStyleBackColor = true;
            btOutputFile.Click += btOutputFile_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(51, 89);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 11;
            label2.Text = "Ouput File";
            // 
            // lbInputFile
            // 
            lbInputFile.BackColor = SystemColors.Window;
            lbInputFile.Location = new Point(51, 55);
            lbInputFile.Name = "lbInputFile";
            lbInputFile.Size = new Size(623, 23);
            lbInputFile.TabIndex = 1;
            lbInputFile.Text = "InputFile";
            lbInputFile.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // rtb
            // 
            rtb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtb.BackColor = SystemColors.Window;
            rtb.Font = new Font("C64 Pro Mono", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rtb.ForeColor = SystemColors.Window;
            rtb.Location = new Point(12, 149);
            rtb.Name = "rtb";
            rtb.Size = new Size(945, 464);
            rtb.TabIndex = 8;
            rtb.Text = "";
            rtb.WordWrap = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(713, 60);
            label3.Name = "label3";
            label3.Size = new Size(31, 15);
            label3.TabIndex = 12;
            label3.Text = "Start";
            // 
            // tbStart
            // 
            tbStart.Location = new Point(750, 56);
            tbStart.Name = "tbStart";
            tbStart.Size = new Size(67, 23);
            tbStart.TabIndex = 4;
            tbStart.Text = "100";
            tbStart.TextAlign = HorizontalAlignment.Right;
            tbStart.TextChanged += tbStartStop_TextChanged;
            tbStart.KeyDown += tbStartStop_KeyDown;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(854, 60);
            label4.Name = "label4";
            label4.Size = new Size(30, 15);
            label4.TabIndex = 13;
            label4.Text = "Step";
            // 
            // tbStep
            // 
            tbStep.Location = new Point(890, 56);
            tbStep.Name = "tbStep";
            tbStep.Size = new Size(67, 23);
            tbStep.TabIndex = 5;
            tbStep.Text = "10";
            tbStep.TextAlign = HorizontalAlignment.Right;
            tbStep.TextChanged += tbStartStop_TextChanged;
            tbStep.KeyDown += tbStartStop_KeyDown;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(973, 625);
            Controls.Add(label4);
            Controls.Add(tbStep);
            Controls.Add(tbStart);
            Controls.Add(label3);
            Controls.Add(rtb);
            Controls.Add(lbInputFile);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btCancel);
            Controls.Add(btProcess);
            Controls.Add(btOutputFile);
            Controls.Add(btInputFile);
            Controls.Add(tbOutputFile);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "C64Renum";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btInputFile;
        private Button btProcess;
        private Button btCancel;
        private ToolTip toolTip1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private Label label1;
        private TextBox tbOutputFile;
        private Button btOutputFile;
        private Label label2;
        private Label lbInputFile;
        internal RichTextBox rtb;
        private Label label3;
        private TextBox tbStart;
        private Label label4;
        private TextBox tbStep;
        private ToolStripMenuItem aboutToolStripMenuItem1;
    }
}
