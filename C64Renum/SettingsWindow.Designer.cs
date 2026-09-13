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
            tbrSpeed = new TrackBar();
            cbDemoMode = new CheckBox();
            cbWrap = new CheckBox();
            cbNoPromptOverwrite = new CheckBox();
            cbDeleteOnCancel = new CheckBox();
            groupBox2 = new GroupBox();
            label5 = new Label();
            label4 = new Label();
            lbBackground = new Label();
            lbForeground = new Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbrSpeed).BeginInit();
            groupBox2.SuspendLayout();
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
            btInputFolder.UseVisualStyleBackColor = true;
            btInputFolder.Click += btInputFolder_Click;
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
            btOutputFolder.UseVisualStyleBackColor = true;
            btOutputFolder.Click += btOutputFolder_Click;
            // 
            // cbOutUseInputFolder
            // 
            cbOutUseInputFolder.AutoSize = true;
            cbOutUseInputFolder.Location = new Point(126, 78);
            cbOutUseInputFolder.Name = "cbOutUseInputFolder";
            cbOutUseInputFolder.Size = new Size(112, 19);
            cbOutUseInputFolder.TabIndex = 4;
            cbOutUseInputFolder.Text = "Use &Input Folder";
            cbOutUseInputFolder.UseVisualStyleBackColor = true;
            cbOutUseInputFolder.CheckedChanged += cbOutUseInputFolder_CheckedChanged;
            // 
            // btSave
            // 
            btSave.Location = new Point(222, 302);
            btSave.Name = "btSave";
            btSave.Size = new Size(80, 40);
            btSave.TabIndex = 9;
            btSave.Text = "&Save";
            btSave.UseVisualStyleBackColor = true;
            btSave.Click += btSave_Click;
            // 
            // btCancel
            // 
            btCancel.Location = new Point(353, 302);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(80, 40);
            btCancel.TabIndex = 10;
            btCancel.Text = "&Cancel";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tbrSpeed);
            groupBox1.Controls.Add(cbDemoMode);
            groupBox1.Controls.Add(cbWrap);
            groupBox1.Controls.Add(cbNoPromptOverwrite);
            groupBox1.Controls.Add(cbDeleteOnCancel);
            groupBox1.Location = new Point(12, 143);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(290, 132);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Program Options";
            // 
            // tbrSpeed
            // 
            tbrSpeed.AutoSize = false;
            tbrSpeed.BackColor = SystemColors.Control;
            tbrSpeed.LargeChange = 1;
            tbrSpeed.Location = new Point(135, 92);
            tbrSpeed.Maximum = 6;
            tbrSpeed.Minimum = 1;
            tbrSpeed.Name = "tbrSpeed";
            tbrSpeed.Size = new Size(104, 29);
            tbrSpeed.TabIndex = 4;
            tbrSpeed.Value = 1;
            // 
            // cbDemoMode
            // 
            cbDemoMode.AutoSize = true;
            cbDemoMode.Location = new Point(6, 97);
            cbDemoMode.Name = "cbDemoMode";
            cbDemoMode.Size = new Size(127, 19);
            cbDemoMode.TabIndex = 3;
            cbDemoMode.Text = "Demo (&slow) mode";
            cbDemoMode.UseVisualStyleBackColor = true;
            cbDemoMode.CheckedChanged += cbDemoMode_CheckedChanged;
            // 
            // cbWrap
            // 
            cbWrap.AutoSize = true;
            cbWrap.Location = new Point(6, 72);
            cbWrap.Name = "cbWrap";
            cbWrap.Size = new Size(216, 19);
            cbWrap.TabIndex = 2;
            cbWrap.Text = "&Wrap output display at 40 chars/line";
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
            // groupBox2
            // 
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(lbBackground);
            groupBox2.Controls.Add(lbForeground);
            groupBox2.Location = new Point(325, 143);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(287, 132);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Output Display Colors";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(92, 90);
            label5.Name = "label5";
            label5.Size = new Size(95, 15);
            label5.TabIndex = 3;
            label5.Text = "Text &Background";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(92, 39);
            label4.Name = "label4";
            label4.Size = new Size(93, 15);
            label4.TabIndex = 1;
            label4.Text = "Text &Foreground";
            // 
            // lbBackground
            // 
            lbBackground.BackColor = SystemColors.ControlDark;
            lbBackground.Location = new Point(17, 77);
            lbBackground.Name = "lbBackground";
            lbBackground.Size = new Size(40, 40);
            lbBackground.TabIndex = 2;
            lbBackground.Click += lbBackground_Click;
            // 
            // lbForeground
            // 
            lbForeground.BackColor = SystemColors.ControlDark;
            lbForeground.Location = new Point(17, 26);
            lbForeground.Name = "lbForeground";
            lbForeground.Size = new Size(40, 40);
            lbForeground.TabIndex = 0;
            lbForeground.Click += lbForeground_Click;
            // 
            // SettingsWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(655, 367);
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
        internal Label lbForeground;
        internal Label lbBackground;
        internal CheckBox cbWrap;
        internal CheckBox cbDemoMode;
        internal TrackBar tbrSpeed;
    }
}