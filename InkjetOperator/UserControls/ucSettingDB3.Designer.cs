using System.Windows.Forms;

namespace InkjetOperator
{
    partial class ucSettingDB3
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblPath;
        private TextBox txtDbPath;
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
            lblPath = new Label();
            txtDbPath = new TextBox();
            btnBrowse = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(30, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(316, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Browse Database File";
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.Location = new Point(40, 100);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(131, 20);
            lblPath.TabIndex = 1;
            lblPath.Text = "Browse Database :";
            // 
            // txtDbPath
            // 
            txtDbPath.Location = new Point(200, 95);
            txtDbPath.Name = "txtDbPath";
            txtDbPath.Size = new Size(342, 27);
            txtDbPath.TabIndex = 2;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(548, 95);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(40, 30);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "📁";
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.BackColor = Color.FromArgb(150, 190, 120);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
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
            btnCancel.BackColor = Color.Red;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(954, 833);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(114, 53);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // ucSettingDB3
            // 
            Controls.Add(lblTitle);
            Controls.Add(lblPath);
            Controls.Add(txtDbPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Name = "ucSettingDB3";
            Size = new Size(1152, 939);
            ResumeLayout(false);
            PerformLayout();
        }
        private Button btnBrowse;
    }
}