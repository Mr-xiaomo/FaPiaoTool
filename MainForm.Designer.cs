namespace FaPiaoTool
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstInvoices = new System.Windows.Forms.ListBox();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.cmbLayout = new System.Windows.Forms.ComboBox();
            this.lblLayout = new System.Windows.Forms.Label();
            this.btnMerge = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(20, 20);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(120, 35);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "添加发票";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);

            // lblLayout — placed right after btnAdd
            this.lblLayout.AutoSize = true;
            this.lblLayout.Location = new System.Drawing.Point(160, 29);
            this.lblLayout.Name = "lblLayout";
            this.lblLayout.Size = new System.Drawing.Size(68, 16);
            this.lblLayout.TabIndex = 5;
            this.lblLayout.Text = "排版版式：";

            // cmbLayout — placed right after lblLayout
            this.cmbLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayout.FormattingEnabled = true;
            this.cmbLayout.Items.AddRange(new object[] {
                "单张（1张/页）",
                "双张（2张/页）",
                "四张（4张/页）"});
            this.cmbLayout.Location = new System.Drawing.Point(235, 25);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.Size = new System.Drawing.Size(180, 24);
            this.cmbLayout.TabIndex = 6;

            // lstInvoices — MultiExtended for batch selection, 10 items visible
            this.lstInvoices.FormattingEnabled = true;
            this.lstInvoices.IntegralHeight = false;
            this.lstInvoices.ItemHeight = 16;
            this.lstInvoices.Location = new System.Drawing.Point(20, 70);
            this.lstInvoices.Name = "lstInvoices";
            this.lstInvoices.ScrollAlwaysVisible = true;
            this.lstInvoices.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstInvoices.Size = new System.Drawing.Size(420, 200);
            this.lstInvoices.TabIndex = 1;

            // btnMoveUp
            this.btnMoveUp.Location = new System.Drawing.Point(460, 70);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(75, 35);
            this.btnMoveUp.TabIndex = 2;
            this.btnMoveUp.Text = "上移";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);

            // btnMoveDown
            this.btnMoveDown.Location = new System.Drawing.Point(460, 115);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(75, 35);
            this.btnMoveDown.TabIndex = 3;
            this.btnMoveDown.Text = "下移";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(460, 160);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 35);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);

            // btnReset — placed below delete
            this.btnReset.Location = new System.Drawing.Point(460, 205);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 35);
            this.btnReset.TabIndex = 10;
            this.btnReset.Text = "重置";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);

            // btnMerge
            this.btnMerge.Location = new System.Drawing.Point(20, 290);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(140, 40);
            this.btnMerge.TabIndex = 7;
            this.btnMerge.Text = "合并并保存";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.BtnMerge_Click);

            // progressBar1
            this.progressBar1.Location = new System.Drawing.Point(180, 290);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(355, 23);
            this.progressBar1.TabIndex = 8;

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(180, 318);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 16);
            this.lblStatus.TabIndex = 9;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 350);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.cmbLayout);
            this.Controls.Add(this.lblLayout);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnMoveUp);
            this.Controls.Add(this.lstInvoices);
            this.Controls.Add(this.btnAdd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "发票合并工具";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListBox lstInvoices;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ComboBox cmbLayout;
        private System.Windows.Forms.Label lblLayout;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblStatus;
    }
}