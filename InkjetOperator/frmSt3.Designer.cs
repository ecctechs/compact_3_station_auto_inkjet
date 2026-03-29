namespace InkjetOperator
{
    partial class frmSt3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dataGridView1 = new DataGridView();
            idDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            barcodeRawDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            patternIdDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            lotNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            statusDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            errorMessageDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            createdByDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            attemptDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            warningDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            printJobBindingSource = new BindingSource(components);
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)printJobBindingSource).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { idDataGridViewTextBoxColumn, barcodeRawDataGridViewTextBoxColumn, patternIdDataGridViewTextBoxColumn, lotNumberDataGridViewTextBoxColumn, statusDataGridViewTextBoxColumn, errorMessageDataGridViewTextBoxColumn, createdByDataGridViewTextBoxColumn, attemptDataGridViewTextBoxColumn, warningDataGridViewTextBoxColumn });
            dataGridView1.DataSource = printJobBindingSource;
            dataGridView1.Location = new Point(24, 42);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(749, 355);
            dataGridView1.TabIndex = 0;
            // 
            // idDataGridViewTextBoxColumn
            // 
            idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            idDataGridViewTextBoxColumn.HeaderText = "Id";
            idDataGridViewTextBoxColumn.MinimumWidth = 6;
            idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            idDataGridViewTextBoxColumn.Width = 125;
            // 
            // barcodeRawDataGridViewTextBoxColumn
            // 
            barcodeRawDataGridViewTextBoxColumn.DataPropertyName = "BarcodeRaw";
            barcodeRawDataGridViewTextBoxColumn.HeaderText = "BarcodeRaw";
            barcodeRawDataGridViewTextBoxColumn.MinimumWidth = 6;
            barcodeRawDataGridViewTextBoxColumn.Name = "barcodeRawDataGridViewTextBoxColumn";
            barcodeRawDataGridViewTextBoxColumn.Width = 125;
            // 
            // patternIdDataGridViewTextBoxColumn
            // 
            patternIdDataGridViewTextBoxColumn.DataPropertyName = "PatternId";
            patternIdDataGridViewTextBoxColumn.HeaderText = "PatternId";
            patternIdDataGridViewTextBoxColumn.MinimumWidth = 6;
            patternIdDataGridViewTextBoxColumn.Name = "patternIdDataGridViewTextBoxColumn";
            patternIdDataGridViewTextBoxColumn.Width = 125;
            // 
            // lotNumberDataGridViewTextBoxColumn
            // 
            lotNumberDataGridViewTextBoxColumn.DataPropertyName = "LotNumber";
            lotNumberDataGridViewTextBoxColumn.HeaderText = "LotNumber";
            lotNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            lotNumberDataGridViewTextBoxColumn.Name = "lotNumberDataGridViewTextBoxColumn";
            lotNumberDataGridViewTextBoxColumn.Width = 125;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.Width = 125;
            // 
            // errorMessageDataGridViewTextBoxColumn
            // 
            errorMessageDataGridViewTextBoxColumn.DataPropertyName = "ErrorMessage";
            errorMessageDataGridViewTextBoxColumn.HeaderText = "ErrorMessage";
            errorMessageDataGridViewTextBoxColumn.MinimumWidth = 6;
            errorMessageDataGridViewTextBoxColumn.Name = "errorMessageDataGridViewTextBoxColumn";
            errorMessageDataGridViewTextBoxColumn.Width = 125;
            // 
            // createdByDataGridViewTextBoxColumn
            // 
            createdByDataGridViewTextBoxColumn.DataPropertyName = "CreatedBy";
            createdByDataGridViewTextBoxColumn.HeaderText = "CreatedBy";
            createdByDataGridViewTextBoxColumn.MinimumWidth = 6;
            createdByDataGridViewTextBoxColumn.Name = "createdByDataGridViewTextBoxColumn";
            createdByDataGridViewTextBoxColumn.Width = 125;
            // 
            // attemptDataGridViewTextBoxColumn
            // 
            attemptDataGridViewTextBoxColumn.DataPropertyName = "Attempt";
            attemptDataGridViewTextBoxColumn.HeaderText = "Attempt";
            attemptDataGridViewTextBoxColumn.MinimumWidth = 6;
            attemptDataGridViewTextBoxColumn.Name = "attemptDataGridViewTextBoxColumn";
            attemptDataGridViewTextBoxColumn.Width = 125;
            // 
            // warningDataGridViewTextBoxColumn
            // 
            warningDataGridViewTextBoxColumn.DataPropertyName = "Warning";
            warningDataGridViewTextBoxColumn.HeaderText = "Warning";
            warningDataGridViewTextBoxColumn.MinimumWidth = 6;
            warningDataGridViewTextBoxColumn.Name = "warningDataGridViewTextBoxColumn";
            warningDataGridViewTextBoxColumn.Width = 125;
            // 
            // printJobBindingSource
            // 
            printJobBindingSource.DataSource = typeof(Models.PrintJob);
            // 
            // button1
            // 
            button1.Location = new Point(638, 419);
            button1.Name = "button1";
            button1.Size = new Size(107, 39);
            button1.TabIndex = 1;
            button1.Text = "Send St1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmSt3
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 532);
            Controls.Add(button1);
            Controls.Add(dataGridView1);
            Name = "frmSt3";
            Text = "frmSt3";
            Load += frmSt3_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)printJobBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn barcodeRawDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn patternIdDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn lotNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn errorMessageDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn createdByDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn attemptDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn warningDataGridViewTextBoxColumn;
        private BindingSource printJobBindingSource;
        private Button button1;
    }
}