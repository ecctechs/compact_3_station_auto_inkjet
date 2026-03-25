namespace InkjetOperator
{
    partial class frmCreateJob
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtOrderNo;
        private TextBox txtCustomerName;
        private ComboBox cmbType;
        private NumericUpDown numQty;
        private Button btnCreate;
        private Button btnReset;
        private Label lblOrderNo;
        private Label lblCustomer;
        private Label lblType;
        private Label lblQty;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtOrderNo = new TextBox();
            txtCustomerName = new TextBox();
            cmbType = new ComboBox();
            numQty = new NumericUpDown();
            btnCreate = new Button();
            btnReset = new Button();
            lblOrderNo = new Label();
            lblCustomer = new Label();
            lblType = new Label();
            lblQty = new Label();
            ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
            SuspendLayout();
            // 
            // txtOrderNo
            // 
            txtOrderNo.Location = new Point(150, 25);
            txtOrderNo.Name = "txtOrderNo";
            txtOrderNo.Size = new Size(200, 27);
            txtOrderNo.TabIndex = 1;
            // 
            // txtCustomerName
            // 
            txtCustomerName.Location = new Point(150, 65);
            txtCustomerName.Name = "txtCustomerName";
            txtCustomerName.Size = new Size(300, 27);
            txtCustomerName.TabIndex = 3;
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.AddRange(new object[] { "กล่อง", "ชิ้น" });
            cmbType.Location = new Point(150, 105);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(200, 28);
            cmbType.TabIndex = 5;
            // 
            // numQty
            // 
            numQty.Location = new Point(150, 145);
            numQty.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numQty.Name = "numQty";
            numQty.Size = new Size(120, 27);
            numQty.TabIndex = 7;
            numQty.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnCreate
            // 
            btnCreate.Location = new Point(150, 200);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(75, 30);
            btnCreate.TabIndex = 8;
            btnCreate.Text = "Create Job";
            btnCreate.Click += btnCreate_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(280, 200);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(75, 30);
            btnReset.TabIndex = 9;
            btnReset.Text = "Reset";
            btnReset.Click += btnReset_Click;
            // 
            // lblOrderNo
            // 
            lblOrderNo.Location = new Point(30, 30);
            lblOrderNo.Name = "lblOrderNo";
            lblOrderNo.Size = new Size(100, 23);
            lblOrderNo.TabIndex = 0;
            lblOrderNo.Text = "Order No";
            // 
            // lblCustomer
            // 
            lblCustomer.Location = new Point(30, 70);
            lblCustomer.Name = "lblCustomer";
            lblCustomer.Size = new Size(100, 23);
            lblCustomer.TabIndex = 2;
            lblCustomer.Text = "Customer";
            // 
            // lblType
            // 
            lblType.Location = new Point(30, 110);
            lblType.Name = "lblType";
            lblType.Size = new Size(100, 23);
            lblType.TabIndex = 4;
            lblType.Text = "Type";
            // 
            // lblQty
            // 
            lblQty.Location = new Point(30, 150);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(100, 23);
            lblQty.TabIndex = 6;
            lblQty.Text = "Quantity";
            // 
            // frmCreateJob
            // 
            ClientSize = new Size(500, 270);
            Controls.Add(lblOrderNo);
            Controls.Add(txtOrderNo);
            Controls.Add(lblCustomer);
            Controls.Add(txtCustomerName);
            Controls.Add(lblType);
            Controls.Add(cmbType);
            Controls.Add(lblQty);
            Controls.Add(numQty);
            Controls.Add(btnCreate);
            Controls.Add(btnReset);
            Name = "frmCreateJob";
            Text = "Create Job";
            ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}