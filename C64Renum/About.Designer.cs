namespace C64Renum
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            lbTitle = new Label();
            btOK = new Button();
            lbInfo = new Label();
            lbLicense = new Label();
            panel1 = new Panel();
            lbPageLink = new Label();
            lbDisclaimer = new Label();
            pbLogo = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
            SuspendLayout();
            // 
            // lbTitle
            // 
            lbTitle.AutoSize = true;
            lbTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbTitle.Location = new Point(73, 15);
            lbTitle.Name = "lbTitle";
            lbTitle.Size = new Size(57, 21);
            lbTitle.TabIndex = 0;
            lbTitle.Text = "label1";
            lbTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btOK
            // 
            btOK.Location = new Point(59, 231);
            btOK.Name = "btOK";
            btOK.Size = new Size(80, 40);
            btOK.TabIndex = 1;
            btOK.Text = "OK";
            btOK.UseVisualStyleBackColor = true;
            btOK.Click += btOK_Click;
            // 
            // lbInfo
            // 
            lbInfo.AutoSize = true;
            lbInfo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbInfo.Location = new Point(73, 50);
            lbInfo.Name = "lbInfo";
            lbInfo.Size = new Size(43, 17);
            lbInfo.TabIndex = 2;
            lbInfo.Text = "label1";
            lbInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbLicense
            // 
            lbLicense.AutoSize = true;
            lbLicense.Location = new Point(73, 122);
            lbLicense.Name = "lbLicense";
            lbLicense.Size = new Size(38, 15);
            lbLicense.TabIndex = 3;
            lbLicense.Text = "label1";
            // 
            // panel1
            // 
            panel1.Controls.Add(lbPageLink);
            panel1.Controls.Add(lbDisclaimer);
            panel1.Controls.Add(btOK);
            panel1.Controls.Add(lbLicense);
            panel1.Controls.Add(lbTitle);
            panel1.Controls.Add(lbInfo);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(412, 287);
            panel1.TabIndex = 4;
            // 
            // lbPageLink
            // 
            lbPageLink.AutoSize = true;
            lbPageLink.Cursor = Cursors.Hand;
            lbPageLink.ForeColor = SystemColors.HotTrack;
            lbPageLink.Location = new Point(73, 95);
            lbPageLink.Name = "lbPageLink";
            lbPageLink.Size = new Size(38, 15);
            lbPageLink.TabIndex = 5;
            lbPageLink.Text = "label1";
            lbPageLink.Click += lbPageLink_Click;
            // 
            // lbDisclaimer
            // 
            lbDisclaimer.AutoSize = true;
            lbDisclaimer.Location = new Point(73, 190);
            lbDisclaimer.Name = "lbDisclaimer";
            lbDisclaimer.Size = new Size(38, 15);
            lbDisclaimer.TabIndex = 4;
            lbDisclaimer.Text = "label1";
            // 
            // pbLogo
            // 
            pbLogo.Image = (Image)resources.GetObject("pbLogo.Image");
            pbLogo.Location = new Point(418, 15);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new Size(256, 256);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 5;
            pbLogo.TabStop = false;
            // 
            // About
            // 
            AcceptButton = btOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 287);
            Controls.Add(pbLogo);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "About";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            Load += About_Load;
            KeyDown += About_KeyDown;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label lbTitle;
        private Button btOK;
        private Label lbInfo;
        private Label lbLicense;
        private Panel panel1;
        private PictureBox pbLogo;
        private Label lbDisclaimer;
        private Label lbPageLink;
    }
}