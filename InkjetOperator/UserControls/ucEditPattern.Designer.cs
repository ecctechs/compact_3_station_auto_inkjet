namespace InkjetOperator
{
    partial class ucEditPattern
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstPatterns = new System.Windows.Forms.ListBox();
            this.pnlLeftBottom = new System.Windows.Forms.Panel();
            this.btnDelPattern = new System.Windows.Forms.Button();
            this.btnNewPattern = new System.Windows.Forms.Button();
            this.lblPreview = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvRules = new System.Windows.Forms.DataGridView();
            this.colFrom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTransform = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colParameter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAddRule = new System.Windows.Forms.Button();
            this.txtBlockText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBarcodeTest = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPatternName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlLeftBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRules)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Panel1.Controls.Add(this.lstPatterns);
            this.splitContainer1.Panel1.Controls.Add(this.pnlLeftBottom);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(10);
            this.splitContainer1.Panel2.Controls.Add(this.lblPreview);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.dgvRules);
            this.splitContainer1.Panel2.Controls.Add(this.btnSave);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddRule);
            this.splitContainer1.Panel2.Controls.Add(this.txtBlockText);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.txtBarcodeTest);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.txtDescription);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.txtPatternName);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10);
            this.splitContainer1.Size = new System.Drawing.Size(900, 545);
            this.splitContainer1.SplitterDistance = 220;
            this.splitContainer1.TabIndex = 0;
            // 
            // lstPatterns
            // 
            this.lstPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPatterns.FormattingEnabled = true;
            this.lstPatterns.ItemHeight = 15;
            this.lstPatterns.Location = new System.Drawing.Point(10, 10);
            this.lstPatterns.Name = "lstPatterns";
            this.lstPatterns.Size = new System.Drawing.Size(200, 485);
            this.lstPatterns.TabIndex = 0;
            this.lstPatterns.SelectedIndexChanged += new System.EventHandler(this.lstPatterns_SelectedIndexChanged);
            // 
            // pnlLeftBottom
            // 
            this.pnlLeftBottom.Controls.Add(this.btnDelPattern);
            this.pnlLeftBottom.Controls.Add(this.btnNewPattern);
            this.pnlLeftBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftBottom.Location = new System.Drawing.Point(10, 495);
            this.pnlLeftBottom.Name = "pnlLeftBottom";
            this.pnlLeftBottom.Size = new System.Drawing.Size(200, 40);
            // 
            // btnDelPattern
            // 
            this.btnDelPattern.Location = new System.Drawing.Point(54, 10);
            this.btnDelPattern.Name = "btnDelPattern";
            this.btnDelPattern.Size = new System.Drawing.Size(45, 25);
            this.btnDelPattern.Text = "Del";
            this.btnDelPattern.Click += new System.EventHandler(this.btnDeletePattern_Click);
            // 
            // btnNewPattern
            // 
            this.btnNewPattern.Location = new System.Drawing.Point(3, 10);
            this.btnNewPattern.Name = "btnNewPattern";
            this.btnNewPattern.Size = new System.Drawing.Size(45, 25);
            this.btnNewPattern.Text = "New";
            this.btnNewPattern.Click += new System.EventHandler(this.btnAddPattern_Click);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblPreview.ForeColor = System.Drawing.Color.Blue;
            this.lblPreview.Location = new System.Drawing.Point(115, 145);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(24, 21);
            this.lblPreview.Text = "---";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 15);
            this.label5.Text = "Preview:";
            // 
            // dgvRules
            // 
            this.dgvRules.AllowUserToAddRows = false;
            this.dgvRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRules.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFrom,
            this.colTo,
            this.colTransform,
            this.colParameter,
            this.colDelete});
            this.dgvRules.Location = new System.Drawing.Point(15, 180);
            this.dgvRules.Name = "dgvRules";
            this.dgvRules.Size = new System.Drawing.Size(645, 300);
            this.dgvRules.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRules_CellContentClick);
            this.dgvRules.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRules_CellValueChanged);
            // 
            // colFrom
            // 
            this.colFrom.HeaderText = "From";
            this.colFrom.Name = "colFrom";
            this.colFrom.Width = 60;
            // 
            // colTo
            // 
            this.colTo.HeaderText = "To";
            this.colTo.Name = "colTo";
            this.colTo.Width = 60;
            // 
            // colTransform
            // 
            this.colTransform.HeaderText = "Transform Rule";
            this.colTransform.Name = "colTransform";
            this.colTransform.Width = 150;
            // 
            // colParameter
            // 
            this.colParameter.HeaderText = "Value";
            this.colParameter.Name = "colParameter";
            this.colParameter.Width = 120;
            // 
            // colDelete
            // 
            this.colDelete.HeaderText = "Del";
            this.colDelete.Name = "colDelete";
            this.colDelete.Text = "Remove";
            this.colDelete.UseColumnTextForButtonValue = true;
            this.colDelete.Width = 80;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.PaleGreen;
            this.btnSave.Location = new System.Drawing.Point(560, 495);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.Text = "Save Rule";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAddRule
            // 
            this.btnAddRule.Location = new System.Drawing.Point(15, 495);
            this.btnAddRule.Name = "btnAddRule";
            this.btnAddRule.Size = new System.Drawing.Size(100, 35);
            this.btnAddRule.Text = "Add Rule";
            this.btnAddRule.Click += new System.EventHandler(this.btnAddRule_Click);
            // 
            // txtBlockText
            // 
            this.txtBlockText.Location = new System.Drawing.Point(115, 110);
            this.txtBlockText.Name = "txtBlockText";
            this.txtBlockText.Size = new System.Drawing.Size(300, 23);
            this.txtBlockText.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 113);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 15);
            this.label4.Text = "Block text:";
            // 
            // txtBarcodeTest
            // 
            this.txtBarcodeTest.Location = new System.Drawing.Point(115, 80);
            this.txtBarcodeTest.Name = "txtBarcodeTest";
            this.txtBarcodeTest.Size = new System.Drawing.Size(300, 23);
            this.txtBarcodeTest.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 15);
            this.label3.Text = "Lot test:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(115, 50);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(300, 23);
            this.txtDescription.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 15);
            this.label2.Text = "Description:";
            // 
            // txtPatternName
            // 
            this.txtPatternName.Location = new System.Drawing.Point(115, 20);
            this.txtPatternName.Name = "txtPatternName";
            this.txtPatternName.Size = new System.Drawing.Size(300, 23);
            this.txtPatternName.TextChanged += new System.EventHandler(this.InputChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 15);
            this.label1.Text = "Rule name:";
            // 
            // ucEditPattern
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 545);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ucEditPattern";
            this.Text = "Pattern Editor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlLeftBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRules)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstPatterns;
        private System.Windows.Forms.Panel pnlLeftBottom;
        private System.Windows.Forms.Button btnDelPattern;
        private System.Windows.Forms.Button btnNewPattern;
        private System.Windows.Forms.TextBox txtPatternName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBarcodeTest;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBlockText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgvRules;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAddRule;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFrom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTo;
        private System.Windows.Forms.DataGridViewComboBoxColumn colTransform;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter;
        private System.Windows.Forms.DataGridViewButtonColumn colDelete;
    }
}