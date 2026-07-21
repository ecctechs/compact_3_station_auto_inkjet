using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    partial class ucSettingUV
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;

        private GroupBox grpUv1;
        private Label lblUv1Dot;
        private Label lblUv1IpCap;
        private TextBox txtUv1Ip;
        private Label lblUv1Colon;
        private TextBox txtUv1Port;
        private Label lblUv1DbCap;
        private TextBox txtUv1Db;
        private Button btnBrowse1;

        private GroupBox grpUv2;
        private Label lblUv2Dot;
        private Label lblUv2IpCap;
        private TextBox txtUv2Ip;
        private Label lblUv2Colon;
        private TextBox txtUv2Port;
        private Label lblUv2DbCap;
        private TextBox txtUv2Db;
        private Button btnBrowse2;

        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            grpUv1 = new GroupBox();
            lblUv1Dot = new Label();
            lblUv1IpCap = new Label();
            txtUv1Ip = new TextBox();
            lblUv1Colon = new Label();
            txtUv1Port = new TextBox();
            lblUv1DbCap = new Label();
            txtUv1Db = new TextBox();
            btnBrowse1 = new Button();
            grpUv2 = new GroupBox();
            lblUv2Dot = new Label();
            lblUv2IpCap = new Label();
            txtUv2Ip = new TextBox();
            lblUv2Colon = new Label();
            txtUv2Port = new TextBox();
            lblUv2DbCap = new Label();
            txtUv2Db = new TextBox();
            btnBrowse2 = new Button();
            lblHint = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            grpUv1.SuspendLayout();
            grpUv2.SuspendLayout();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            lblTitle.Location = new Point(30, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 51);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "UV Printer Setting";
            //
            // grpUv1
            //
            grpUv1.Controls.Add(lblUv1Dot);
            grpUv1.Controls.Add(lblUv1IpCap);
            grpUv1.Controls.Add(txtUv1Ip);
            grpUv1.Controls.Add(lblUv1Colon);
            grpUv1.Controls.Add(txtUv1Port);
            grpUv1.Controls.Add(lblUv1DbCap);
            grpUv1.Controls.Add(txtUv1Db);
            grpUv1.Controls.Add(btnBrowse1);
            grpUv1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpUv1.Location = new Point(40, 90);
            grpUv1.Name = "grpUv1";
            grpUv1.Size = new Size(830, 150);
            grpUv1.TabIndex = 1;
            grpUv1.TabStop = false;
            grpUv1.Text = "UV1 — MK063";
            //
            // lblUv1Dot
            //
            lblUv1Dot.BackColor = Color.Gray;
            lblUv1Dot.BorderStyle = BorderStyle.FixedSingle;
            lblUv1Dot.Location = new Point(20, 52);
            lblUv1Dot.Name = "lblUv1Dot";
            lblUv1Dot.Size = new Size(18, 18);
            lblUv1Dot.TabIndex = 0;
            //
            // lblUv1IpCap
            //
            lblUv1IpCap.AutoSize = true;
            lblUv1IpCap.Font = new Font("Segoe UI", 10F);
            lblUv1IpCap.Location = new Point(50, 50);
            lblUv1IpCap.Name = "lblUv1IpCap";
            lblUv1IpCap.Size = new Size(100, 23);
            lblUv1IpCap.TabIndex = 1;
            lblUv1IpCap.Text = "IP Address :";
            //
            // txtUv1Ip
            //
            txtUv1Ip.Font = new Font("Segoe UI", 10F);
            txtUv1Ip.Location = new Point(160, 47);
            txtUv1Ip.Name = "txtUv1Ip";
            txtUv1Ip.Size = new Size(220, 30);
            txtUv1Ip.TabIndex = 2;
            //
            // lblUv1Colon
            //
            lblUv1Colon.AutoSize = true;
            lblUv1Colon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUv1Colon.Location = new Point(386, 48);
            lblUv1Colon.Name = "lblUv1Colon";
            lblUv1Colon.Size = new Size(15, 28);
            lblUv1Colon.TabIndex = 3;
            lblUv1Colon.Text = ":";
            //
            // txtUv1Port
            //
            txtUv1Port.Font = new Font("Segoe UI", 10F);
            txtUv1Port.Location = new Point(405, 47);
            txtUv1Port.Name = "txtUv1Port";
            txtUv1Port.Size = new Size(90, 30);
            txtUv1Port.TabIndex = 4;
            //
            // lblUv1DbCap
            //
            lblUv1DbCap.AutoSize = true;
            lblUv1DbCap.Font = new Font("Segoe UI", 10F);
            lblUv1DbCap.Location = new Point(50, 100);
            lblUv1DbCap.Name = "lblUv1DbCap";
            lblUv1DbCap.Size = new Size(80, 23);
            lblUv1DbCap.TabIndex = 5;
            lblUv1DbCap.Text = "CPI.db3 :";
            //
            // txtUv1Db
            //
            txtUv1Db.Font = new Font("Segoe UI", 10F);
            txtUv1Db.Location = new Point(160, 97);
            txtUv1Db.Name = "txtUv1Db";
            txtUv1Db.Size = new Size(535, 30);
            txtUv1Db.TabIndex = 6;
            //
            // btnBrowse1
            //
            btnBrowse1.Font = new Font("Segoe UI", 10F);
            btnBrowse1.Location = new Point(705, 95);
            btnBrowse1.Name = "btnBrowse1";
            btnBrowse1.Size = new Size(105, 34);
            btnBrowse1.TabIndex = 7;
            btnBrowse1.Text = "📁";
            btnBrowse1.Click += btnBrowse1_Click;
            //
            // grpUv2
            //
            grpUv2.Controls.Add(lblUv2Dot);
            grpUv2.Controls.Add(lblUv2IpCap);
            grpUv2.Controls.Add(txtUv2Ip);
            grpUv2.Controls.Add(lblUv2Colon);
            grpUv2.Controls.Add(txtUv2Port);
            grpUv2.Controls.Add(lblUv2DbCap);
            grpUv2.Controls.Add(txtUv2Db);
            grpUv2.Controls.Add(btnBrowse2);
            grpUv2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpUv2.Location = new Point(40, 260);
            grpUv2.Name = "grpUv2";
            grpUv2.Size = new Size(830, 150);
            grpUv2.TabIndex = 2;
            grpUv2.TabStop = false;
            grpUv2.Text = "UV2 — MK067";
            //
            // lblUv2Dot
            //
            lblUv2Dot.BackColor = Color.Gray;
            lblUv2Dot.BorderStyle = BorderStyle.FixedSingle;
            lblUv2Dot.Location = new Point(20, 52);
            lblUv2Dot.Name = "lblUv2Dot";
            lblUv2Dot.Size = new Size(18, 18);
            lblUv2Dot.TabIndex = 0;
            //
            // lblUv2IpCap
            //
            lblUv2IpCap.AutoSize = true;
            lblUv2IpCap.Font = new Font("Segoe UI", 10F);
            lblUv2IpCap.Location = new Point(50, 50);
            lblUv2IpCap.Name = "lblUv2IpCap";
            lblUv2IpCap.Size = new Size(100, 23);
            lblUv2IpCap.TabIndex = 1;
            lblUv2IpCap.Text = "IP Address :";
            //
            // txtUv2Ip
            //
            txtUv2Ip.Font = new Font("Segoe UI", 10F);
            txtUv2Ip.Location = new Point(160, 47);
            txtUv2Ip.Name = "txtUv2Ip";
            txtUv2Ip.Size = new Size(220, 30);
            txtUv2Ip.TabIndex = 2;
            //
            // lblUv2Colon
            //
            lblUv2Colon.AutoSize = true;
            lblUv2Colon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUv2Colon.Location = new Point(386, 48);
            lblUv2Colon.Name = "lblUv2Colon";
            lblUv2Colon.Size = new Size(15, 28);
            lblUv2Colon.TabIndex = 3;
            lblUv2Colon.Text = ":";
            //
            // txtUv2Port
            //
            txtUv2Port.Font = new Font("Segoe UI", 10F);
            txtUv2Port.Location = new Point(405, 47);
            txtUv2Port.Name = "txtUv2Port";
            txtUv2Port.Size = new Size(90, 30);
            txtUv2Port.TabIndex = 4;
            //
            // lblUv2DbCap
            //
            lblUv2DbCap.AutoSize = true;
            lblUv2DbCap.Font = new Font("Segoe UI", 10F);
            lblUv2DbCap.Location = new Point(50, 100);
            lblUv2DbCap.Name = "lblUv2DbCap";
            lblUv2DbCap.Size = new Size(80, 23);
            lblUv2DbCap.TabIndex = 5;
            lblUv2DbCap.Text = "CPI.db3 :";
            //
            // txtUv2Db
            //
            txtUv2Db.Font = new Font("Segoe UI", 10F);
            txtUv2Db.Location = new Point(160, 97);
            txtUv2Db.Name = "txtUv2Db";
            txtUv2Db.Size = new Size(535, 30);
            txtUv2Db.TabIndex = 6;
            //
            // btnBrowse2
            //
            btnBrowse2.Font = new Font("Segoe UI", 10F);
            btnBrowse2.Location = new Point(705, 95);
            btnBrowse2.Name = "btnBrowse2";
            btnBrowse2.Size = new Size(105, 34);
            btnBrowse2.TabIndex = 7;
            btnBrowse2.Text = "📁";
            btnBrowse2.Click += btnBrowse2_Click;
            //
            // lblHint
            //
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 9F);
            lblHint.ForeColor = Color.DimGray;
            lblHint.Location = new Point(43, 425);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(400, 23);
            lblHint.TabIndex = 3;
            lblHint.Text = "CPI.db3 รองรับ path ในเครื่อง (C:\\...) หรือ share ผ่านเครื่อง UV (\\\\ip\\share\\CPI.db3) — เว้นว่างช่องที่ไม่ใช้ได้";
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.BackColor = Color.FromArgb(150, 190, 120);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(812, 833);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(114, 53);
            btnSave.TabIndex = 4;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(150, 150, 150);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(954, 833);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(114, 53);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            //
            // ucSettingUV
            //
            Controls.Add(lblTitle);
            Controls.Add(grpUv1);
            Controls.Add(grpUv2);
            Controls.Add(lblHint);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Name = "ucSettingUV";
            Size = new Size(1152, 939);
            grpUv1.ResumeLayout(false);
            grpUv1.PerformLayout();
            grpUv2.ResumeLayout(false);
            grpUv2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
