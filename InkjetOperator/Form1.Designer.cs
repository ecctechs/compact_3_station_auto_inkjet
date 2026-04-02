namespace InkjetOperator
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pnlMenu = new Panel();
            btnBot = new Button();
            lblLanguage = new Label();
            picLogo = new PictureBox();
            btnSetting = new Button();
            btnEdit = new Button();
            btnOrder = new Button();
            btnInput = new Button();
            pnlContent = new Panel();
            pnlMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // pnlMenu
            // 
            pnlMenu.BackColor = Color.White;
            pnlMenu.Controls.Add(btnBot);
            pnlMenu.Controls.Add(lblLanguage);
            pnlMenu.Controls.Add(picLogo);
            pnlMenu.Controls.Add(btnSetting);
            pnlMenu.Controls.Add(btnEdit);
            pnlMenu.Controls.Add(btnOrder);
            pnlMenu.Controls.Add(btnInput);
            pnlMenu.Dock = DockStyle.Top;
            pnlMenu.Location = new Point(0, 0);
            pnlMenu.Name = "pnlMenu";
            pnlMenu.Size = new Size(1008, 60);
            pnlMenu.TabIndex = 1;
            // 
            // btnBot
            // 
            btnBot.BackColor = Color.FromArgb(160, 160, 160);
            btnBot.FlatAppearance.BorderSize = 0;
            btnBot.FlatStyle = FlatStyle.Flat;
            btnBot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBot.ForeColor = Color.White;
            btnBot.Location = new Point(578, 10);
            btnBot.Name = "btnBot";
            btnBot.Size = new Size(130, 40);
            btnBot.TabIndex = 4;
            btnBot.Text = "Bot UV";
            btnBot.UseVisualStyleBackColor = false;
            // 
            // lblLanguage
            // 
            lblLanguage.AutoSize = true;
            lblLanguage.BackColor = Color.Black;
            lblLanguage.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLanguage.ForeColor = Color.White;
            lblLanguage.Location = new Point(960, 15);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Padding = new Padding(5, 2, 5, 2);
            lblLanguage.Size = new Size(37, 23);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "EN";
            // 
            // picLogo
            // 
            picLogo.ErrorImage = null;
            picLogo.Image = (Image)resources.GetObject("picLogo.Image");
            picLogo.InitialImage = (Image)resources.GetObject("picLogo.InitialImage");
            picLogo.Location = new Point(874, 4);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(80, 50);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 1;
            picLogo.TabStop = false;
            // 
            // btnSetting
            // 
            btnSetting.BackColor = Color.FromArgb(160, 160, 160);
            btnSetting.FlatAppearance.BorderSize = 0;
            btnSetting.FlatStyle = FlatStyle.Flat;
            btnSetting.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSetting.ForeColor = Color.White;
            btnSetting.Location = new Point(430, 10);
            btnSetting.Name = "btnSetting";
            btnSetting.Size = new Size(130, 40);
            btnSetting.TabIndex = 3;
            btnSetting.Text = "Setting";
            btnSetting.UseVisualStyleBackColor = false;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.FromArgb(160, 160, 160);
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnEdit.ForeColor = Color.White;
            btnEdit.Location = new Point(290, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(130, 40);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "Edit Pattern";
            btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnOrder
            // 
            btnOrder.BackColor = Color.FromArgb(160, 160, 160);
            btnOrder.FlatAppearance.BorderSize = 0;
            btnOrder.FlatStyle = FlatStyle.Flat;
            btnOrder.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnOrder.ForeColor = Color.White;
            btnOrder.Location = new Point(150, 10);
            btnOrder.Name = "btnOrder";
            btnOrder.Size = new Size(130, 40);
            btnOrder.TabIndex = 1;
            btnOrder.Text = "Order List";
            btnOrder.UseVisualStyleBackColor = false;
            // 
            // btnInput
            // 
            btnInput.BackColor = Color.FromArgb(108, 147, 204);
            btnInput.FlatAppearance.BorderSize = 0;
            btnInput.FlatStyle = FlatStyle.Flat;
            btnInput.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnInput.ForeColor = Color.White;
            btnInput.Location = new Point(10, 10);
            btnInput.Name = "btnInput";
            btnInput.Size = new Size(130, 40);
            btnInput.TabIndex = 0;
            btnInput.Text = "Input Order";
            btnInput.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.White;
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 60);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1008, 704);
            pnlContent.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 764);
            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Inkjet Operator";
            pnlMenu.ResumeLayout(false);
            pnlMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlMenu;
        private System.Windows.Forms.Button btnInput;
        private System.Windows.Forms.Button btnOrder;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Panel pnlContent;
        private Button btnBot;
    }
}