namespace BotClickApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnEditPattern;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnEditPattern = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtLot = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.Lot = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(351, 54);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 40);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start Bot";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnEditPattern
            // 
            this.btnEditPattern.Location = new System.Drawing.Point(351, 6);
            this.btnEditPattern.Name = "btnEditPattern";
            this.btnEditPattern.Size = new System.Drawing.Size(120, 40);
            this.btnEditPattern.TabIndex = 6;
            this.btnEditPattern.Text = "Edit Pattern";
            this.btnEditPattern.UseVisualStyleBackColor = true;
            this.btnEditPattern.Click += new System.EventHandler(this.btnEditPattern_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(30, 127);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 423);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // txtLot
            // 
            this.txtLot.Location = new System.Drawing.Point(120, 31);
            this.txtLot.Name = "txtLot";
            this.txtLot.Size = new System.Drawing.Size(194, 22);
            this.txtLot.TabIndex = 2;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(120, 72);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(194, 22);
            this.txtName.TabIndex = 3;
            // 
            // Lot
            // 
            this.Lot.AutoSize = true;
            this.Lot.Location = new System.Drawing.Point(39, 37);
            this.Lot.Name = "Lot";
            this.Lot.Size = new System.Drawing.Size(25, 16);
            this.Lot.TabIndex = 4;
            this.Lot.Text = "Lot";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Name";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Lot);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtLot);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnEditPattern);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bot Click + Capture";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtLot;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label Lot;
        private System.Windows.Forms.Label label2;
    }
}