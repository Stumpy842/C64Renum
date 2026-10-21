namespace C64Renum
{
    partial class TokensHelp
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
            lbTokens = new Label();
            btClose = new Button();
            label2 = new Label();
            lbReserved = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(205, 19);
            label1.Name = "label1";
            label1.Size = new Size(152, 21);
            label1.TabIndex = 0;
            label1.Text = "C64 BASIC Tokens";
            // 
            // lbTokens
            // 
            lbTokens.BorderStyle = BorderStyle.FixedSingle;
            lbTokens.Font = new Font("Segoe UI Symbol", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbTokens.Location = new Point(25, 50);
            lbTokens.Name = "lbTokens";
            lbTokens.Size = new Size(512, 149);
            lbTokens.TabIndex = 1;
            lbTokens.Text = "Tokens";
            // 
            // btClose
            // 
            btClose.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btClose.Location = new Point(241, 324);
            btClose.Name = "btClose";
            btClose.Size = new Size(80, 40);
            btClose.TabIndex = 2;
            btClose.Text = "Close";
            btClose.UseVisualStyleBackColor = true;
            btClose.Click += btClose_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(182, 236);
            label2.Name = "label2";
            label2.Size = new Size(198, 21);
            label2.TabIndex = 0;
            label2.Text = "C64 Reserved Keywords";
            // 
            // lbReserved
            // 
            lbReserved.BorderStyle = BorderStyle.FixedSingle;
            lbReserved.Font = new Font("Segoe UI Symbol", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbReserved.Location = new Point(25, 267);
            lbReserved.Name = "lbReserved";
            lbReserved.Size = new Size(512, 28);
            lbReserved.TabIndex = 1;
            lbReserved.Text = "Reserved";
            // 
            // TokensHelp
            // 
            AcceptButton = btClose;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(562, 387);
            Controls.Add(btClose);
            Controls.Add(lbReserved);
            Controls.Add(lbTokens);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TokensHelp";
            Text = "Tokens Help";
            Load += TokensHelp_Load;
            KeyDown += TokensHelp_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label lbTokens;
        private Button btClose;
        private Label label2;
        private Label lbReserved;
    }
}