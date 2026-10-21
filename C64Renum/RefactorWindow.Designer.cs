
namespace C64Renum
{
    partial class RefactorWindow
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
            dgv = new DataGridView();
            label1 = new Label();
            btSave = new Button();
            btCancel = new Button();
            btReset = new Button();
            lbInstruct = new Label();
            btTrim = new Button();
            toolTip1 = new ToolTip(components);
            actionPanel = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Dock = DockStyle.Left;
            dgv.Location = new Point(0, 0);
            dgv.Name = "dgv";
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.Size = new Size(458, 624);
            dgv.StandardTab = true;
            dgv.TabIndex = 0;
            dgv.CellBeginEdit += dgv_CellBeginEdit;
            dgv.CellEndEdit += dgv_CellEndEdit;
            dgv.CellEnter += dgv_CellEnter;
            dgv.CellValidating += dgv_CellValidating;
            dgv.ColumnHeaderMouseClick += dgv_ColumnHeaderMouseClick;
            dgv.ColumnHeaderMouseDoubleClick += dgv_ColumnHeaderMouseDoubleClick;
            dgv.EditingControlShowing += dgv_EditingControlShowing;
            dgv.Enter += dgv_Enter;
            dgv.Leave += dgv_Leave;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(484, 12);
            label1.Name = "label1";
            label1.Size = new Size(294, 17);
            label1.TabIndex = 1;
            label1.Text = "Cannot  rename system variables ST, TI  or TI$";
            // 
            // btSave
            // 
            btSave.Location = new Point(3, 3);
            btSave.Name = "btSave";
            btSave.Size = new Size(80, 30);
            btSave.TabIndex = 0;
            btSave.Text = "&Save";
            btSave.UseVisualStyleBackColor = true;
            btSave.Click += btSave_Click;
            // 
            // btCancel
            // 
            btCancel.CausesValidation = false;
            btCancel.Location = new Point(126, 3);
            btCancel.Name = "btCancel";
            btCancel.Size = new Size(80, 30);
            btCancel.TabIndex = 1;
            btCancel.Text = "&Cancel";
            btCancel.UseVisualStyleBackColor = true;
            btCancel.Click += btCancel_Click;
            // 
            // btReset
            // 
            btReset.CausesValidation = false;
            btReset.Location = new Point(126, 48);
            btReset.Name = "btReset";
            btReset.Size = new Size(80, 30);
            btReset.TabIndex = 3;
            btReset.Text = "&Reset";
            btReset.UseVisualStyleBackColor = true;
            btReset.Click += btReset_Click;
            // 
            // lbInstruct
            // 
            lbInstruct.AutoEllipsis = true;
            lbInstruct.Location = new Point(483, 130);
            lbInstruct.Name = "lbInstruct";
            lbInstruct.Size = new Size(296, 487);
            lbInstruct.TabIndex = 3;
            lbInstruct.Text = "Instructions";
            // 
            // btTrim
            // 
            btTrim.Location = new Point(3, 48);
            btTrim.Name = "btTrim";
            btTrim.Size = new Size(80, 30);
            btTrim.TabIndex = 2;
            btTrim.Text = "&Trim";
            btTrim.UseVisualStyleBackColor = true;
            btTrim.Click += btTrim_Click;
            // 
            // actionPanel
            // 
            actionPanel.Controls.Add(btSave);
            actionPanel.Controls.Add(btCancel);
            actionPanel.Controls.Add(btReset);
            actionPanel.Controls.Add(btTrim);
            actionPanel.Location = new Point(527, 40);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(209, 81);
            actionPanel.TabIndex = 2;
            // 
            // RefactorWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ClientSize = new Size(800, 624);
            Controls.Add(actionPanel);
            Controls.Add(lbInstruct);
            Controls.Add(label1);
            Controls.Add(dgv);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RefactorWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Refactor Variables";
            Load += RefactorWindow_Load;
            KeyDown += RefactorWindow_KeyDown;
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            actionPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgv;
        private Label label1;
        private Button btSave;
        private Button btCancel;
        private Button btReset;
        internal Label lbInstruct;
        private Button btTrim;
        private ToolTip toolTip1;
        private Panel actionPanel;
    }
}