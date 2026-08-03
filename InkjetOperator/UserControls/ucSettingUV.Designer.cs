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
        private Label lblUv1FolderCap;
        private TextBox txtUv1Folder;
        private Button btnBrowse1;
        private Label lblUv1Status;

        private GroupBox grpUv2;
        private Label lblUv2Dot;
        private Label lblUv2IpCap;
        private TextBox txtUv2Ip;
        private Label lblUv2Colon;
        private TextBox txtUv2Port;
        private Label lblUv2FolderCap;
        private TextBox txtUv2Folder;
        private Button btnBrowse2;
        private Label lblUv2Status;

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
            lblUv1FolderCap = new Label();
            txtUv1Folder = new TextBox();
            btnBrowse1 = new Button();
            lblUv1Status = new Label();
            grpUv2 = new GroupBox();
            lblUv2Dot = new Label();
            lblUv2IpCap = new Label();
            txtUv2Ip = new TextBox();
            lblUv2Colon = new Label();
            txtUv2Port = new TextBox();
            lblUv2FolderCap = new Label();
            txtUv2Folder = new TextBox();
            btnBrowse2 = new Button();
            lblUv2Status = new Label();
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
            lblTitle.Size = new Size(347, 51);
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
            grpUv1.Controls.Add(lblUv1FolderCap);
            grpUv1.Controls.Add(txtUv1Folder);
            grpUv1.Controls.Add(btnBrowse1);
            grpUv1.Controls.Add(lblUv1Status);
            grpUv1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpUv1.Location = new Point(40, 90);
            grpUv1.Name = "grpUv1";
            grpUv1.Size = new Size(1050, 180);
            grpUv1.TabIndex = 1;
            grpUv1.TabStop = false;
            grpUv1.Text = "UV1 — MK063 (Plate)";
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
            lblUv1IpCap.Size = new Size(112, 28);
            lblUv1IpCap.TabIndex = 1;
            lblUv1IpCap.Text = "IP Address :";
            // 
            // txtUv1Ip
            // 
            txtUv1Ip.Font = new Font("Segoe UI", 10F);
            txtUv1Ip.Location = new Point(170, 47);
            txtUv1Ip.Name = "txtUv1Ip";
            txtUv1Ip.Size = new Size(220, 34);
            txtUv1Ip.TabIndex = 2;
            // 
            // lblUv1Colon
            // 
            lblUv1Colon.AutoSize = true;
            lblUv1Colon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUv1Colon.Location = new Point(396, 48);
            lblUv1Colon.Name = "lblUv1Colon";
            lblUv1Colon.Size = new Size(21, 32);
            lblUv1Colon.TabIndex = 3;
            lblUv1Colon.Text = ":";
            // 
            // txtUv1Port
            // 
            txtUv1Port.Font = new Font("Segoe UI", 10F);
            txtUv1Port.Location = new Point(415, 47);
            txtUv1Port.Name = "txtUv1Port";
            txtUv1Port.Size = new Size(90, 34);
            txtUv1Port.TabIndex = 4;
            // 
            // lblUv1FolderCap
            // 
            lblUv1FolderCap.AutoSize = true;
            lblUv1FolderCap.Font = new Font("Segoe UI", 10F);
            lblUv1FolderCap.Location = new Point(14, 103);
            lblUv1FolderCap.Name = "lblUv1FolderCap";
            lblUv1FolderCap.Size = new Size(190, 28);
            lblUv1FolderCap.TabIndex = 5;
            lblUv1FolderCap.Text = "UV Software Folder :";
            // 
            // txtUv1Folder
            // 
            txtUv1Folder.Font = new Font("Segoe UI", 10F);
            txtUv1Folder.Location = new Point(225, 97);
            txtUv1Folder.Name = "txtUv1Folder";
            txtUv1Folder.ReadOnly = true;
            txtUv1Folder.Size = new Size(605, 34);
            txtUv1Folder.TabIndex = 6;
            // 
            // btnBrowse1
            // 
            btnBrowse1.Font = new Font("Segoe UI", 10F);
            btnBrowse1.Location = new Point(840, 95);
            btnBrowse1.Name = "btnBrowse1";
            btnBrowse1.Size = new Size(105, 34);
            btnBrowse1.TabIndex = 7;
            btnBrowse1.Text = "Browse...";
            btnBrowse1.Click += btnBrowse1_Click;
            // 
            // lblUv1Status
            // 
            lblUv1Status.AutoSize = true;
            lblUv1Status.Font = new Font("Segoe UI", 9F);
            lblUv1Status.Location = new Point(210, 135);
            lblUv1Status.Name = "lblUv1Status";
            lblUv1Status.Size = new Size(0, 25);
            lblUv1Status.TabIndex = 8;
            // 
            // grpUv2
            // 
            grpUv2.Controls.Add(lblUv2Dot);
            grpUv2.Controls.Add(lblUv2IpCap);
            grpUv2.Controls.Add(txtUv2Ip);
            grpUv2.Controls.Add(lblUv2Colon);
            grpUv2.Controls.Add(txtUv2Port);
            grpUv2.Controls.Add(lblUv2FolderCap);
            grpUv2.Controls.Add(txtUv2Folder);
            grpUv2.Controls.Add(btnBrowse2);
            grpUv2.Controls.Add(lblUv2Status);
            grpUv2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpUv2.Location = new Point(40, 290);
            grpUv2.Name = "grpUv2";
            grpUv2.Size = new Size(1050, 180);
            grpUv2.TabIndex = 2;
            grpUv2.TabStop = false;
            grpUv2.Text = "UV2 — MK067 (Shim)";
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
            lblUv2IpCap.Size = new Size(112, 28);
            lblUv2IpCap.TabIndex = 1;
            lblUv2IpCap.Text = "IP Address :";
            // 
            // txtUv2Ip
            // 
            txtUv2Ip.Font = new Font("Segoe UI", 10F);
            txtUv2Ip.Location = new Point(170, 47);
            txtUv2Ip.Name = "txtUv2Ip";
            txtUv2Ip.Size = new Size(220, 34);
            txtUv2Ip.TabIndex = 2;
            // 
            // lblUv2Colon
            // 
            lblUv2Colon.AutoSize = true;
            lblUv2Colon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblUv2Colon.Location = new Point(396, 48);
            lblUv2Colon.Name = "lblUv2Colon";
            lblUv2Colon.Size = new Size(21, 32);
            lblUv2Colon.TabIndex = 3;
            lblUv2Colon.Text = ":";
            // 
            // txtUv2Port
            // 
            txtUv2Port.Font = new Font("Segoe UI", 10F);
            txtUv2Port.Location = new Point(415, 47);
            txtUv2Port.Name = "txtUv2Port";
            txtUv2Port.Size = new Size(90, 34);
            txtUv2Port.TabIndex = 4;
            // 
            // lblUv2FolderCap
            // 
            lblUv2FolderCap.AutoSize = true;
            lblUv2FolderCap.Font = new Font("Segoe UI", 10F);
            lblUv2FolderCap.Location = new Point(20, 107);
            lblUv2FolderCap.Name = "lblUv2FolderCap";
            lblUv2FolderCap.Size = new Size(190, 28);
            lblUv2FolderCap.TabIndex = 5;
            lblUv2FolderCap.Text = "UV Software Folder :";
            // 
            // txtUv2Folder
            // 
            txtUv2Folder.Font = new Font("Segoe UI", 10F);
            txtUv2Folder.Location = new Point(225, 95);
            txtUv2Folder.Name = "txtUv2Folder";
            txtUv2Folder.ReadOnly = true;
            txtUv2Folder.Size = new Size(605, 34);
            txtUv2Folder.TabIndex = 6;
            // 
            // btnBrowse2
            // 
            btnBrowse2.Font = new Font("Segoe UI", 10F);
            btnBrowse2.Location = new Point(840, 95);
            btnBrowse2.Name = "btnBrowse2";
            btnBrowse2.Size = new Size(105, 34);
            btnBrowse2.TabIndex = 7;
            btnBrowse2.Text = "Browse...";
            btnBrowse2.Click += btnBrowse2_Click;
            // 
            // lblUv2Status
            // 
            lblUv2Status.AutoSize = true;
            lblUv2Status.Font = new Font("Segoe UI", 9F);
            lblUv2Status.Location = new Point(210, 135);
            lblUv2Status.Name = "lblUv2Status";
            lblUv2Status.Size = new Size(0, 25);
            lblUv2Status.TabIndex = 8;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 9F);
            lblHint.ForeColor = Color.DimGray;
            lblHint.Location = new Point(43, 485);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(1040, 25);
            lblHint.TabIndex = 3;
            lblHint.Text = "เลือกโฟลเดอร์ซอฟต์แวร์ UV (เช่น uvinkjet-250702-new) — ระบบจะตรวจสอบ database/sys/CPI.db3 และ document/default.uvdx ให้อัตโนมัติ";
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
            AutoScaleMode = AutoScaleMode.None;
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
