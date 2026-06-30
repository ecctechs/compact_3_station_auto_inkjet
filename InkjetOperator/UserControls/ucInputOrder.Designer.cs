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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucInputOrder));
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
            lblTitle = new Label();
            picBarcode = new PictureBox();
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
            pnlMain.Margin = new Padding(4, 5, 4, 5);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1440, 1174);
            pnlMain.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = Color.Transparent;
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnOK);
            pnlButtons.Location = new Point(389, 929);
            pnlButtons.Margin = new Padding(4, 5, 4, 5);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(714, 100);
            pnlButtons.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(220, 80, 50);
            btnCancel.FlatAppearance.BorderSize = 3;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(400, 9);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(200, 84);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click_1;
            // 
            // btnOK
            // 
            btnOK.BackColor = Color.FromArgb(165, 195, 130);
            btnOK.FlatAppearance.BorderSize = 3;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(114, 9);
            btnOK.Margin = new Padding(4, 5, 4, 5);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(200, 84);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += btnOK_Click_1;
            // 
            // pnlFormContainer
            // 
            pnlFormContainer.BackColor = Color.White;
            pnlFormContainer.Controls.Add(tableLayoutPanel);
            pnlFormContainer.Location = new Point(389, 446);
            pnlFormContainer.Margin = new Padding(4, 5, 4, 5);
            pnlFormContainer.Name = "pnlFormContainer";
            pnlFormContainer.Padding = new Padding(29, 34, 29, 34);
            pnlFormContainer.Size = new Size(714, 400);
            pnlFormContainer.TabIndex = 1;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 214F));
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
            tableLayoutPanel.Location = new Point(29, 34);
            tableLayoutPanel.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.Size = new Size(656, 332);
            tableLayoutPanel.TabIndex = 0;
            // 
            // lblBarcode
            // 
            lblBarcode.AutoSize = true;
            lblBarcode.Dock = DockStyle.Fill;
            lblBarcode.Font = new Font("Segoe UI", 11F);
            lblBarcode.Location = new Point(4, 0);
            lblBarcode.Margin = new Padding(4, 0, 4, 0);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(206, 66);
            lblBarcode.TabIndex = 0;
            lblBarcode.Text = "Barcode :";
            lblBarcode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtBarcode
            // 
            txtBarcode.BackColor = Color.WhiteSmoke;
            txtBarcode.Dock = DockStyle.Fill;
            txtBarcode.Font = new Font("Consolas", 12F, FontStyle.Bold);
            txtBarcode.Location = new Point(218, 9);
            txtBarcode.Margin = new Padding(4, 9, 4, 9);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.Size = new Size(434, 36);
            txtBarcode.TabIndex = 0;
            // 
            // lblOrderNo
            // 
            lblOrderNo.AutoSize = true;
            lblOrderNo.Dock = DockStyle.Fill;
            lblOrderNo.Font = new Font("Segoe UI", 11F);
            lblOrderNo.Location = new Point(4, 66);
            lblOrderNo.Margin = new Padding(4, 0, 4, 0);
            lblOrderNo.Name = "lblOrderNo";
            lblOrderNo.Size = new Size(206, 66);
            lblOrderNo.TabIndex = 1;
            lblOrderNo.Text = "Order No :";
            lblOrderNo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtOrderNo
            // 
            txtOrderNo.Dock = DockStyle.Fill;
            txtOrderNo.Font = new Font("Segoe UI", 11F);
            txtOrderNo.Location = new Point(218, 75);
            txtOrderNo.Margin = new Padding(4, 9, 4, 9);
            txtOrderNo.Name = "txtOrderNo";
            txtOrderNo.Size = new Size(434, 37);
            txtOrderNo.TabIndex = 1;
            // 
            // lblCustomerName
            // 
            lblCustomerName.AutoSize = true;
            lblCustomerName.Dock = DockStyle.Fill;
            lblCustomerName.Font = new Font("Segoe UI", 11F);
            lblCustomerName.Location = new Point(4, 132);
            lblCustomerName.Margin = new Padding(4, 0, 4, 0);
            lblCustomerName.Name = "lblCustomerName";
            lblCustomerName.Size = new Size(206, 66);
            lblCustomerName.TabIndex = 2;
            lblCustomerName.Text = "Customer Name :";
            lblCustomerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtCustomerName
            // 
            txtCustomerName.Dock = DockStyle.Fill;
            txtCustomerName.Font = new Font("Segoe UI", 11F);
            txtCustomerName.Location = new Point(218, 141);
            txtCustomerName.Margin = new Padding(4, 9, 4, 9);
            txtCustomerName.Name = "txtCustomerName";
            txtCustomerName.Size = new Size(434, 37);
            txtCustomerName.TabIndex = 2;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Dock = DockStyle.Fill;
            lblType.Font = new Font("Segoe UI", 11F);
            lblType.Location = new Point(4, 198);
            lblType.Margin = new Padding(4, 0, 4, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(206, 66);
            lblType.TabIndex = 3;
            lblType.Text = "Type :";
            lblType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtType
            // 
            txtType.Dock = DockStyle.Fill;
            txtType.Font = new Font("Segoe UI", 11F);
            txtType.Location = new Point(218, 207);
            txtType.Margin = new Padding(4, 9, 4, 9);
            txtType.Name = "txtType";
            txtType.Size = new Size(434, 37);
            txtType.TabIndex = 3;
            // 
            // lblQty
            // 
            lblQty.AutoSize = true;
            lblQty.Dock = DockStyle.Fill;
            lblQty.Font = new Font("Segoe UI", 11F);
            lblQty.Location = new Point(4, 264);
            lblQty.Margin = new Padding(4, 0, 4, 0);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(206, 68);
            lblQty.TabIndex = 4;
            lblQty.Text = "Qty :";
            lblQty.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtQty
            // 
            txtQty.Dock = DockStyle.Fill;
            txtQty.Font = new Font("Segoe UI", 11F);
            txtQty.Location = new Point(218, 273);
            txtQty.Margin = new Padding(4, 9, 4, 9);
            txtQty.Name = "txtQty";
            txtQty.Size = new Size(434, 37);
            txtQty.TabIndex = 4;
            // 
            // lblScanStatus
            // 
            lblScanStatus.AutoSize = true;
            lblScanStatus.BackColor = Color.Transparent;
            lblScanStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblScanStatus.ForeColor = Color.White;
            lblScanStatus.Location = new Point(649, 374);
            lblScanStatus.Margin = new Padding(4, 0, 4, 0);
            lblScanStatus.Name = "lblScanStatus";
            lblScanStatus.Size = new Size(203, 32);
            lblScanStatus.TabIndex = 2;
            lblScanStatus.Text = "รอสแกนบาร์โค้ด...";
            lblScanStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblScanStatus.Visible = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Black;
            lblTitle.Location = new Point(502, 30);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(495, 96);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "Scan Barcode";
            // 
            // picBarcode
            // 
            picBarcode.BackColor = Color.FromArgb(230, 240, 250);
            picBarcode.BackgroundImage = (Image)resources.GetObject("picBarcode.BackgroundImage");
            picBarcode.BackgroundImageLayout = ImageLayout.Stretch;
            picBarcode.Location = new Point(560, 169);
            picBarcode.Margin = new Padding(4, 5, 4, 5);
            picBarcode.Name = "picBarcode";
            picBarcode.Size = new Size(386, 168);
            picBarcode.SizeMode = PictureBoxSizeMode.Zoom;
            picBarcode.TabIndex = 3;
            picBarcode.TabStop = false;
            // 
            // ucInputOrder
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMain);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ucInputOrder";
            Size = new Size(1440, 1174);
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
        private PictureBox picBarcode;
    }
}