namespace InkjetOperator
{
    partial class ucInputOrder
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlMain = new Panel();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnOK = new Button();
            pnlFormContainer = new Panel();
            tableLayoutPanel = new TableLayoutPanel();
            lblBarcode = new Label();
            txtBarcode = new TextBox();
            lblOrderNo = new Label();
            txtOrderNo = new TextBox();
            lblCustomerName = new Label();
            txtCustomerName = new TextBox();
            lblType = new Label();
            txtType = new TextBox();
            lblQty = new Label();
            txtQty = new TextBox();
            lblScanStatus = new Label();
            picBarcode = new PictureBox();
            lblTitle = new Label();
            pnlMain.SuspendLayout();
            pnlButtons.SuspendLayout();
            pnlFormContainer.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBarcode).BeginInit();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(108, 147, 204);
            pnlMain.Controls.Add(pnlButtons);
            pnlMain.Controls.Add(pnlFormContainer);
            pnlMain.Controls.Add(lblScanStatus);
            pnlMain.Controls.Add(picBarcode);
            pnlMain.Controls.Add(lblTitle);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(3, 4, 3, 4);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1152, 939);
            pnlMain.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = Color.Transparent;
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnOK);
            pnlButtons.Location = new Point(311, 690);
            pnlButtons.Margin = new Padding(3, 4, 3, 4);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(571, 80);
            pnlButtons.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(220, 80, 50);
            btnCancel.FlatAppearance.BorderSize = 3;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(320, 7);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(160, 67);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(165, 195, 130);
            btnOK.FlatAppearance.BorderSize = 3;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(91, 7);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(160, 67);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = false;
            // 
            // pnlFormContainer
            // 
            pnlFormContainer.BackColor = Color.White;
            pnlFormContainer.Controls.Add(tableLayoutPanel);
            pnlFormContainer.Location = new Point(311, 357);
            pnlFormContainer.Margin = new Padding(3, 4, 3, 4);
            pnlFormContainer.Name = "pnlFormContainer";
            pnlFormContainer.Padding = new Padding(23, 27, 23, 27);
            pnlFormContainer.Size = new Size(571, 320);
            pnlFormContainer.TabIndex = 1;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 171F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(lblBarcode, 0, 0);
            tableLayoutPanel.Controls.Add(txtBarcode, 1, 0);
            tableLayoutPanel.Controls.Add(lblOrderNo, 0, 1);
            tableLayoutPanel.Controls.Add(txtOrderNo, 1, 1);
            tableLayoutPanel.Controls.Add(lblCustomerName, 0, 2);
            tableLayoutPanel.Controls.Add(txtCustomerName, 1, 2);
            tableLayoutPanel.Controls.Add(lblType, 0, 3);
            tableLayoutPanel.Controls.Add(txtType, 1, 3);
            tableLayoutPanel.Controls.Add(lblQty, 0, 4);
            tableLayoutPanel.Controls.Add(txtQty, 1, 4);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(23, 27);
            tableLayoutPanel.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.Size = new Size(525, 266);
            tableLayoutPanel.TabIndex = 0;
            // 
            // lblBarcode
            // 
            lblBarcode.AutoSize = true;
            lblBarcode.Dock = DockStyle.Fill;
            lblBarcode.Font = new Font("Segoe UI", 11F);
            lblBarcode.Location = new Point(3, 0);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(165, 53);
            lblBarcode.TabIndex = 0;
            lblBarcode.Text = "Barcode :";
            lblBarcode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtBarcode
            // 
            txtBarcode.BackColor = Color.WhiteSmoke;
            txtBarcode.Dock = DockStyle.Fill;
            txtBarcode.Font = new Font("Consolas", 12F, FontStyle.Bold);
            txtBarcode.Location = new Point(174, 7);
            txtBarcode.Margin = new Padding(3, 7, 3, 7);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.Size = new Size(348, 31);
            txtBarcode.TabIndex = 0;
            // 
            // lblOrderNo
            // 
            lblOrderNo.AutoSize = true;
            lblOrderNo.Dock = DockStyle.Fill;
            lblOrderNo.Font = new Font("Segoe UI", 11F);
            lblOrderNo.Location = new Point(3, 53);
            lblOrderNo.Name = "lblOrderNo";
            lblOrderNo.Size = new Size(165, 53);
            lblOrderNo.TabIndex = 1;
            lblOrderNo.Text = "Order No :";
            lblOrderNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtOrderNo
            // 
            txtOrderNo.Dock = DockStyle.Fill;
            txtOrderNo.Font = new Font("Segoe UI", 11F);
            txtOrderNo.Location = new Point(174, 60);
            txtOrderNo.Margin = new Padding(3, 7, 3, 7);
            txtOrderNo.Name = "txtOrderNo";
            txtOrderNo.Size = new Size(348, 32);
            txtOrderNo.TabIndex = 1;
            // 
            // lblCustomerName
            // 
            lblCustomerName.AutoSize = true;
            lblCustomerName.Dock = DockStyle.Fill;
            lblCustomerName.Font = new Font("Segoe UI", 11F);
            lblCustomerName.Location = new Point(3, 106);
            lblCustomerName.Name = "lblCustomerName";
            lblCustomerName.Size = new Size(165, 53);
            lblCustomerName.TabIndex = 2;
            lblCustomerName.Text = "Customer Name :";
            lblCustomerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCustomerName
            // 
            txtCustomerName.Dock = DockStyle.Fill;
            txtCustomerName.Font = new Font("Segoe UI", 11F);
            txtCustomerName.Location = new Point(174, 113);
            txtCustomerName.Margin = new Padding(3, 7, 3, 7);
            txtCustomerName.Name = "txtCustomerName";
            txtCustomerName.Size = new Size(348, 32);
            txtCustomerName.TabIndex = 2;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Dock = DockStyle.Fill;
            lblType.Font = new Font("Segoe UI", 11F);
            lblType.Location = new Point(3, 159);
            lblType.Name = "lblType";
            lblType.Size = new Size(165, 53);
            lblType.TabIndex = 3;
            lblType.Text = "Type :";
            lblType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtType
            // 
            txtType.Dock = DockStyle.Fill;
            txtType.Font = new Font("Segoe UI", 11F);
            txtType.Location = new Point(174, 166);
            txtType.Margin = new Padding(3, 7, 3, 7);
            txtType.Name = "txtType";
            txtType.Size = new Size(348, 32);
            txtType.TabIndex = 3;
            // 
            // lblQty
            // 
            lblQty.AutoSize = true;
            lblQty.Dock = DockStyle.Fill;
            lblQty.Font = new Font("Segoe UI", 11F);
            lblQty.Location = new Point(3, 212);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(165, 54);
            lblQty.TabIndex = 4;
            lblQty.Text = "Qty :";
            lblQty.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtQty
            // 
            txtQty.Dock = DockStyle.Fill;
            txtQty.Font = new Font("Segoe UI", 11F);
            txtQty.Location = new Point(174, 219);
            txtQty.Margin = new Padding(3, 7, 3, 7);
            txtQty.Name = "txtQty";
            txtQty.Size = new Size(348, 32);
            txtQty.TabIndex = 4;
            // 
            // lblScanStatus
            // 
            lblScanStatus.AutoSize = true;
            lblScanStatus.BackColor = Color.Transparent;
            lblScanStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.Location = new Point(519, 299);
            lblScanStatus.Name = "lblScanStatus";
            lblScanStatus.Size = new Size(167, 28);
            lblScanStatus.TabIndex = 2;
            lblScanStatus.Text = "รอสแกนบาร์โค้ด...";
            lblScanStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // picBarcode
            // 
            picBarcode.BackColor = Color.FromArgb(230, 240, 250);
            picBarcode.Location = new Point(448, 133);
            picBarcode.Margin = new Padding(3, 4, 3, 4);
            picBarcode.Name = "picBarcode";
            picBarcode.Size = new Size(309, 131);
            picBarcode.TabIndex = 3;
            picBarcode.TabStop = false;
            picBarcode.Paint += picBarcode_Paint;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Black;
            lblTitle.Location = new Point(402, 24);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(412, 81);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "Scan Barcode";
            // 
            // ucInputOrder
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMain);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ucInputOrder";
            Size = new Size(1152, 939);
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlFormContainer.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picBarcode).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblScanStatus;
        private System.Windows.Forms.PictureBox picBarcode;
        private System.Windows.Forms.Panel pnlFormContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label lblBarcode;
        private System.Windows.Forms.TextBox txtBarcode;
        private System.Windows.Forms.Label lblOrderNo;
        private System.Windows.Forms.TextBox txtOrderNo;
        private System.Windows.Forms.Label lblCustomerName;
        private System.Windows.Forms.TextBox txtCustomerName;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.TextBox txtQty;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}