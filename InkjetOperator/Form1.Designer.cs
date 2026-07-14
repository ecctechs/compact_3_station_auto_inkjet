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
            button1 = new Button();
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
            pnlMenu.Controls.Add(button1);
            pnlMenu.Controls.Add(btnBot);
            pnlMenu.Controls.Add(lblLanguage);
            pnlMenu.Controls.Add(picLogo);
            pnlMenu.Controls.Add(btnSetting);
            pnlMenu.Controls.Add(btnEdit);
            pnlMenu.Controls.Add(btnOrder);
            pnlMenu.Controls.Add(btnInput);
            pnlMenu.Dock = DockStyle.Top;
            pnlMenu.Location = new Point(0, 0);
            pnlMenu.Margin = new Padding(4, 5, 4, 5);
            pnlMenu.Name = "pnlMenu";
            pnlMenu.Size = new Size(1440, 98);
            pnlMenu.TabIndex = 1;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(160, 160, 160);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            button1.ForeColor = Color.White;
            button1.Location = new Point(832, 16);
            button1.Margin = new Padding(4, 5, 4, 5);
            button1.Name = "button1";
            button1.Size = new Size(178, 66);
            button1.TabIndex = 5;
            button1.Text = "Job ST3";
            button1.UseVisualStyleBackColor = false;
            // 
            // btnBot
            // 
            btnBot.BackColor = Color.FromArgb(160, 160, 160);
            btnBot.FlatAppearance.BorderSize = 0;
            btnBot.FlatStyle = FlatStyle.Flat;
            btnBot.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnBot.ForeColor = Color.White;
            btnBot.Location = new Point(620, 16);
            btnBot.Margin = new Padding(4, 5, 4, 5);
            btnBot.Name = "btnBot";
            btnBot.Size = new Size(186, 66);
            btnBot.TabIndex = 4;
            btnBot.Text = "Bot UV";
            btnBot.UseVisualStyleBackColor = false;
            // 
            // lblLanguage
            // 
            lblLanguage.AutoSize = true;
            lblLanguage.BackColor = Color.Black;
            lblLanguage.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblLanguage.ForeColor = Color.White;
            lblLanguage.Location = new Point(1371, 25);
            lblLanguage.Margin = new Padding(4, 0, 4, 0);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Padding = new Padding(8, 4, 8, 4);
            lblLanguage.Size = new Size(55, 36);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "EN";
            // 
            // picLogo
            // 
            picLogo.ErrorImage = null;
            picLogo.Image = (Image)resources.GetObject("picLogo.Image");
            picLogo.InitialImage = (Image)resources.GetObject("picLogo.InitialImage");
            picLogo.Location = new Point(1203, 4);
            picLogo.Margin = new Padding(4, 5, 4, 5);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(160, 90);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 1;
            picLogo.TabStop = false;
            // 
            // btnSetting
            // 
            btnSetting.BackColor = Color.FromArgb(160, 160, 160);
            btnSetting.FlatAppearance.BorderSize = 0;
            btnSetting.FlatStyle = FlatStyle.Flat;
            btnSetting.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnSetting.ForeColor = Color.White;
            btnSetting.Location = new Point(1031, 16);
            btnSetting.Margin = new Padding(4, 5, 4, 5);
            btnSetting.Name = "btnSetting";
            btnSetting.Size = new Size(186, 66);
            btnSetting.TabIndex = 3;
            btnSetting.Text = "Setting";
            btnSetting.UseVisualStyleBackColor = false;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.FromArgb(160, 160, 160);
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnEdit.ForeColor = Color.White;
            btnEdit.Location = new Point(414, 16);
            btnEdit.Margin = new Padding(4, 5, 4, 5);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(186, 66);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "Edit Pattern";
            btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnOrder
            // 
            btnOrder.BackColor = Color.FromArgb(160, 160, 160);
            btnOrder.FlatAppearance.BorderSize = 0;
            btnOrder.FlatStyle = FlatStyle.Flat;
            btnOrder.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnOrder.ForeColor = Color.White;
            btnOrder.Location = new Point(214, 16);
            btnOrder.Margin = new Padding(4, 5, 4, 5);
            btnOrder.Name = "btnOrder";
            btnOrder.Size = new Size(186, 66);
            btnOrder.TabIndex = 1;
            btnOrder.Text = "Order List";
            btnOrder.UseVisualStyleBackColor = false;
            // 
            // btnInput
            // 
            btnInput.BackColor = Color.FromArgb(108, 147, 204);
            btnInput.FlatAppearance.BorderSize = 0;
            btnInput.FlatStyle = FlatStyle.Flat;
            btnInput.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnInput.ForeColor = Color.White;
            btnInput.Location = new Point(14, 16);
            btnInput.Margin = new Padding(4, 5, 4, 5);
            btnInput.Name = "btnInput";
            btnInput.Size = new Size(186, 66);
            btnInput.TabIndex = 0;
            btnInput.Text = "Input Order";
            btnInput.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.White;
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 98);
            pnlContent.Margin = new Padding(4, 5, 4, 5);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1440, 1176);
            pnlContent.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1440, 1274);
            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);
            Margin = new Padding(4, 5, 4, 5);
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
        private Button button1;
    }
}