namespace BotClickApp
{
    partial class PatternEditorControl
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ListBox lstPatterns;
        private System.Windows.Forms.Button btnNewPattern;
        private System.Windows.Forms.Button btnDeletePattern;

        private System.Windows.Forms.Label lblPatternNameLabel;
        private System.Windows.Forms.Label lblDescriptionLabel;
        private System.Windows.Forms.Label lblBarcodeLabel;
        private System.Windows.Forms.Label lblBlockTextLabel;
        private System.Windows.Forms.TextBox block_text;

        private System.Windows.Forms.Label lblMk1ProgramLabel; // kept label but control removed

        private System.Windows.Forms.TextBox txtPatternName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtBarcodeTest;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.DataGridView dgvRules;
        private System.Windows.Forms.Button btnAddRule;
        private System.Windows.Forms.Button btnDeleteRule;
        private System.Windows.Forms.Button btnSavePattern;

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
            this.lstPatterns = new System.Windows.Forms.ListBox();
            this.btnNewPattern = new System.Windows.Forms.Button();
            this.btnDeletePattern = new System.Windows.Forms.Button();
            this.lblPatternNameLabel = new System.Windows.Forms.Label();
            this.lblDescriptionLabel = new System.Windows.Forms.Label();
            this.lblBarcodeLabel = new System.Windows.Forms.Label();
            this.lblBlockTextLabel = new System.Windows.Forms.Label();
            this.txtPatternName = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtBarcodeTest = new System.Windows.Forms.TextBox();
            this.block_text = new System.Windows.Forms.TextBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.dgvRules = new System.Windows.Forms.DataGridView();
            this.btnAddRule = new System.Windows.Forms.Button();
            this.btnDeleteRule = new System.Windows.Forms.Button();
            this.btnSavePattern = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRules)).BeginInit();
            this.SuspendLayout();
            // 
            // lstPatterns
            // 
            this.lstPatterns.FormattingEnabled = true;
            this.lstPatterns.ItemHeight = 16;
            this.lstPatterns.Location = new System.Drawing.Point(12, 12);
            this.lstPatterns.Name = "lstPatterns";
            this.lstPatterns.Size = new System.Drawing.Size(200, 388);
            this.lstPatterns.TabIndex = 0;
            // 
            // btnNewPattern
            // 
            this.btnNewPattern.Location = new System.Drawing.Point(12, 410);
            this.btnNewPattern.Name = "btnNewPattern";
            this.btnNewPattern.Size = new System.Drawing.Size(90, 30);
            this.btnNewPattern.TabIndex = 1;
            this.btnNewPattern.Text = "New";
            this.btnNewPattern.UseVisualStyleBackColor = true;
            // 
            // btnDeletePattern
            // 
            this.btnDeletePattern.Location = new System.Drawing.Point(122, 410);
            this.btnDeletePattern.Name = "btnDeletePattern";
            this.btnDeletePattern.Size = new System.Drawing.Size(90, 30);
            this.btnDeletePattern.TabIndex = 2;
            this.btnDeletePattern.Text = "Delete";
            this.btnDeletePattern.UseVisualStyleBackColor = true;
            // 
            // lblPatternNameLabel
            // 
            this.lblPatternNameLabel.AutoSize = true;
            this.lblPatternNameLabel.Location = new System.Drawing.Point(228, 15);
            this.lblPatternNameLabel.Name = "lblPatternNameLabel";
            this.lblPatternNameLabel.Size = new System.Drawing.Size(75, 16);
            this.lblPatternNameLabel.TabIndex = 13;
            this.lblPatternNameLabel.Text = "Rule name:";
            // 
            // lblDescriptionLabel
            // 
            this.lblDescriptionLabel.AutoSize = true;
            this.lblDescriptionLabel.Location = new System.Drawing.Point(228, 43);
            this.lblDescriptionLabel.Name = "lblDescriptionLabel";
            this.lblDescriptionLabel.Size = new System.Drawing.Size(78, 16);
            this.lblDescriptionLabel.TabIndex = 12;
            this.lblDescriptionLabel.Text = "Description:";
            // 
            // lblBarcodeLabel
            // 
            this.lblBarcodeLabel.AutoSize = true;
            this.lblBarcodeLabel.Location = new System.Drawing.Point(228, 71);
            this.lblBarcodeLabel.Name = "lblBarcodeLabel";
            this.lblBarcodeLabel.Size = new System.Drawing.Size(86, 16);
            this.lblBarcodeLabel.TabIndex = 22;
            this.lblBarcodeLabel.Text = "Barcode test:";
            // 
            // lblBlockTextLabel
            // 
            this.lblBlockTextLabel.AutoSize = true;
            this.lblBlockTextLabel.Location = new System.Drawing.Point(228, 96);
            this.lblBlockTextLabel.Name = "lblBlockTextLabel";
            this.lblBlockTextLabel.Size = new System.Drawing.Size(67, 16);
            this.lblBlockTextLabel.TabIndex = 23;
            this.lblBlockTextLabel.Text = "Block text:";
            // 
            // txtPatternName
            // 
            this.txtPatternName.Location = new System.Drawing.Point(323, 12);
            this.txtPatternName.Name = "txtPatternName";
            this.txtPatternName.Size = new System.Drawing.Size(565, 22);
            this.txtPatternName.TabIndex = 3;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(323, 40);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(565, 22);
            this.txtDescription.TabIndex = 4;
            // 
            // txtBarcodeTest
            // 
            this.txtBarcodeTest.Location = new System.Drawing.Point(323, 68);
            this.txtBarcodeTest.Name = "txtBarcodeTest";
            this.txtBarcodeTest.Size = new System.Drawing.Size(565, 22);
            this.txtBarcodeTest.TabIndex = 5;
            // 
            // block_text
            // 
            this.block_text.Location = new System.Drawing.Point(323, 92);
            this.block_text.Name = "block_text";
            this.block_text.Size = new System.Drawing.Size(565, 22);
            this.block_text.TabIndex = 6;
            // 
            // lblPreview
            // 
            this.lblPreview.Location = new System.Drawing.Point(228, 128);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(660, 23);
            this.lblPreview.TabIndex = 6;
            this.lblPreview.Text = "Preview: ";
            // 
            // dgvRules
            // 
            this.dgvRules.AllowUserToAddRows = false;
            this.dgvRules.AllowUserToDeleteRows = false;
            this.dgvRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRules.Location = new System.Drawing.Point(228, 156);
            this.dgvRules.Name = "dgvRules";
            this.dgvRules.RowHeadersWidth = 51;
            this.dgvRules.RowTemplate.Height = 24;
            this.dgvRules.Size = new System.Drawing.Size(660, 300);
            this.dgvRules.TabIndex = 7;
            // 
            // btnAddRule
            // 
            this.btnAddRule.Location = new System.Drawing.Point(228, 490);
            this.btnAddRule.Name = "btnAddRule";
            this.btnAddRule.Size = new System.Drawing.Size(100, 30);
            this.btnAddRule.TabIndex = 8;
            this.btnAddRule.Text = "Add Rule";
            this.btnAddRule.UseVisualStyleBackColor = true;
            // 
            // btnDeleteRule
            // 
            this.btnDeleteRule.Location = new System.Drawing.Point(338, 490);
            this.btnDeleteRule.Name = "btnDeleteRule";
            this.btnDeleteRule.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRule.TabIndex = 9;
            this.btnDeleteRule.Text = "Delete Rule";
            this.btnDeleteRule.UseVisualStyleBackColor = true;
            // 
            // btnSavePattern
            // 
            this.btnSavePattern.Location = new System.Drawing.Point(788, 490);
            this.btnSavePattern.Name = "btnSavePattern";
            this.btnSavePattern.Size = new System.Drawing.Size(100, 30);
            this.btnSavePattern.TabIndex = 10;
            this.btnSavePattern.Text = "Save Rule";
            this.btnSavePattern.UseVisualStyleBackColor = true;
            // 
            // PatternEditorControl
            // 
            this.Controls.Add(this.btnSavePattern);
            this.Controls.Add(this.btnDeleteRule);
            this.Controls.Add(this.btnAddRule);
            this.Controls.Add(this.dgvRules);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.block_text);
            this.Controls.Add(this.lblBlockTextLabel);
            this.Controls.Add(this.txtBarcodeTest);
            this.Controls.Add(this.lblBarcodeLabel);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescriptionLabel);
            this.Controls.Add(this.txtPatternName);
            this.Controls.Add(this.lblPatternNameLabel);
            this.Controls.Add(this.btnDeletePattern);
            this.Controls.Add(this.btnNewPattern);
            this.Controls.Add(this.lstPatterns);
            this.Name = "PatternEditorControl";
            this.Size = new System.Drawing.Size(900, 539);
            this.Load += new System.EventHandler(this.PatternEditorControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRules)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
