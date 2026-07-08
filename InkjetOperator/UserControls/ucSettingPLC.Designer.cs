namespace InkjetOperator
{
    partial class ucSettingPLC
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
            pnlMainContent = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            pnlPlc = new Panel();
            txtPlc001Port = new TextBox();
            lblPlcPort = new Label();
            lblPlc001Status = new Label();
            btnEditPlc001 = new Button();
            lblPlc001 = new Label();
            lblPlcTitle = new Label();
            txtPlc001Ip = new TextBox();
            lblPlcIp = new Label();
            pnlMainContent.SuspendLayout();
            pnlPlc.SuspendLayout();
            SuspendLayout();
            //
            // pnlMainContent
            //
            pnlMainContent.BackColor = Color.White;
            pnlMainContent.Controls.Add(btnCancel);
            pnlMainContent.Controls.Add(btnSave);
            pnlMainContent.Controls.Add(pnlPlc);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(0, 0);
            pnlMainContent.Margin = new Padding(4, 5, 4, 5);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Padding = new Padding(29, 34, 29, 34);
            pnlMainContent.Size = new Size(1152, 939);
            pnlMainContent.TabIndex = 0;
            //
            // btnCancel
            //
            btnCancel.BackColor = Color.FromArgb(150, 150, 150);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(955, 873);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(114, 53);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            //
            // btnSave
            //
            btnSave.BackColor = Color.FromArgb(70, 130, 180);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(814, 873);
            btnSave.Margin = new Padding(4, 5, 4, 5);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(114, 53);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            //
            // pnlPlc
            //
            pnlPlc.BackColor = Color.White;
            pnlPlc.BorderStyle = BorderStyle.FixedSingle;
            pnlPlc.Controls.Add(txtPlc001Port);
            pnlPlc.Controls.Add(lblPlcPort);
            pnlPlc.Controls.Add(lblPlc001Status);
            pnlPlc.Controls.Add(btnEditPlc001);
            pnlPlc.Controls.Add(lblPlc001);
            pnlPlc.Controls.Add(lblPlcTitle);
            pnlPlc.Controls.Add(txtPlc001Ip);
            pnlPlc.Controls.Add(lblPlcIp);
            pnlPlc.Location = new Point(85, 39);
            pnlPlc.Margin = new Padding(4, 5, 4, 5);
            pnlPlc.Name = "pnlPlc";
            pnlPlc.Size = new Size(433, 260);
            pnlPlc.TabIndex = 2;
            //
            // txtPlc001Port
            //
            txtPlc001Port.Location = new Point(129, 188);
            txtPlc001Port.Margin = new Padding(4, 5, 4, 5);
            txtPlc001Port.Name = "txtPlc001Port";
            txtPlc001Port.Size = new Size(83, 31);
            txtPlc001Port.TabIndex = 7;
            //
            // lblPlcPort
            //
            lblPlcPort.AutoSize = true;
            lblPlcPort.Location = new Point(21, 191);
            lblPlcPort.Margin = new Padding(4, 0, 4, 0);
            lblPlcPort.Name = "lblPlcPort";
            lblPlcPort.Size = new Size(53, 25);
            lblPlcPort.TabIndex = 6;
            lblPlcPort.Text = "Port :";
            //
            // lblPlc001Status
            //
            lblPlc001Status.BackColor = Color.FromArgb(100, 200, 100);
            lblPlc001Status.BorderStyle = BorderStyle.FixedSingle;
            lblPlc001Status.Location = new Point(21, 84);
            lblPlc001Status.Margin = new Padding(4, 0, 4, 0);
            lblPlc001Status.Name = "lblPlc001Status";
            lblPlc001Status.Size = new Size(21, 23);
            lblPlc001Status.TabIndex = 0;
            //
            // btnEditPlc001
            //
            btnEditPlc001.FlatAppearance.BorderSize = 0;
            btnEditPlc001.FlatStyle = FlatStyle.Flat;
            btnEditPlc001.Font = new Font("Segoe UI", 8F);
            btnEditPlc001.Location = new Point(158, 75);
            btnEditPlc001.Margin = new Padding(4, 5, 4, 5);
            btnEditPlc001.Name = "btnEditPlc001";
            btnEditPlc001.Size = new Size(36, 41);
            btnEditPlc001.TabIndex = 1;
            btnEditPlc001.Text = "✎";
            //
            // lblPlc001
            //
            lblPlc001.BackColor = Color.Black;
            lblPlc001.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPlc001.ForeColor = Color.White;
            lblPlc001.Location = new Point(50, 75);
            lblPlc001.Margin = new Padding(4, 0, 4, 0);
            lblPlc001.Name = "lblPlc001";
            lblPlc001.Padding = new Padding(8, 4, 8, 4);
            lblPlc001.Size = new Size(100, 41);
            lblPlc001.TabIndex = 2;
            lblPlc001.Text = "PLC-001";
            lblPlc001.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblPlcTitle
            //
            lblPlcTitle.AutoSize = true;
            lblPlcTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblPlcTitle.Location = new Point(14, 16);
            lblPlcTitle.Margin = new Padding(4, 0, 4, 0);
            lblPlcTitle.Name = "lblPlcTitle";
            lblPlcTitle.Size = new Size(64, 38);
            lblPlcTitle.TabIndex = 3;
            lblPlcTitle.Text = "PLC";
            //
            // txtPlc001Ip
            //
            txtPlc001Ip.Location = new Point(129, 136);
            txtPlc001Ip.Margin = new Padding(4, 5, 4, 5);
            txtPlc001Ip.Name = "txtPlc001Ip";
            txtPlc001Ip.Size = new Size(284, 31);
            txtPlc001Ip.TabIndex = 4;
            //
            // lblPlcIp
            //
            lblPlcIp.AutoSize = true;
            lblPlcIp.Location = new Point(21, 141);
            lblPlcIp.Margin = new Padding(4, 0, 4, 0);
            lblPlcIp.Name = "lblPlcIp";
            lblPlcIp.Size = new Size(106, 25);
            lblPlcIp.TabIndex = 5;
            lblPlcIp.Text = "IP Address :";
            //
            // ucSettingPLC
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMainContent);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ucSettingPLC";
            Size = new Size(1152, 939);
            pnlMainContent.ResumeLayout(false);
            pnlPlc.ResumeLayout(false);
            pnlPlc.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlMainContent;

        // PLC
        private System.Windows.Forms.Panel pnlPlc;
        private System.Windows.Forms.Label lblPlcTitle;
        private System.Windows.Forms.Label lblPlc001Status;
        private System.Windows.Forms.Label lblPlc001;
        private System.Windows.Forms.Button btnEditPlc001;
        private System.Windows.Forms.Label lblPlcIp;
        private System.Windows.Forms.TextBox txtPlc001Ip;
        private System.Windows.Forms.Label lblPlcPort;
        private System.Windows.Forms.TextBox txtPlc001Port;

        // Buttons
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
