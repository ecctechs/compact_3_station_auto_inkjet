namespace InkjetOperator.UserControls
{
    partial class ucST3
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnRunBot = new Button();
            dgvList = new DataGridView();
            idDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            barcodeRawDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            orderNoDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            customerNameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            typeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            qtyDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            patternIdDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            lotNumberDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            patternNoErpDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            statusDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            errorMessageDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            warningDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            attemptDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            stationDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            createdByDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            createdAtDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            updatedAtDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            bindingSource1 = new BindingSource(components);
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // btnRunBot
            // 
            btnRunBot.BackColor = Color.FromArgb(40, 160, 80);
            btnRunBot.FlatAppearance.BorderSize = 0;
            btnRunBot.FlatStyle = FlatStyle.Flat;
            btnRunBot.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnRunBot.ForeColor = Color.White;
            btnRunBot.Location = new Point(339, 664);
            btnRunBot.Name = "btnRunBot";
            btnRunBot.Size = new Size(404, 55);
            btnRunBot.TabIndex = 1;
            btnRunBot.Text = "Send Job To Station 1";
            btnRunBot.UseVisualStyleBackColor = false;
            // 
            // dgvList
            // 
            dgvList.AllowUserToAddRows = false;
            dgvList.AutoGenerateColumns = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvList.ColumnHeadersHeight = 29;
            dgvList.Columns.AddRange(new DataGridViewColumn[] { idDataGridViewTextBoxColumn, barcodeRawDataGridViewTextBoxColumn, orderNoDataGridViewTextBoxColumn, customerNameDataGridViewTextBoxColumn, typeDataGridViewTextBoxColumn, qtyDataGridViewTextBoxColumn, patternIdDataGridViewTextBoxColumn, lotNumberDataGridViewTextBoxColumn, patternNoErpDataGridViewTextBoxColumn, statusDataGridViewTextBoxColumn, errorMessageDataGridViewTextBoxColumn, warningDataGridViewTextBoxColumn, attemptDataGridViewTextBoxColumn, stationDataGridViewTextBoxColumn, createdByDataGridViewTextBoxColumn, createdAtDataGridViewTextBoxColumn, updatedAtDataGridViewTextBoxColumn });
            dgvList.DataSource = bindingSource1;
            dgvList.Location = new Point(43, 48);
            dgvList.Name = "dgvList";
            dgvList.ReadOnly = true;
            dgvList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvList.Size = new Size(1062, 569);
            dgvList.TabIndex = 2;
            // 
            // idDataGridViewTextBoxColumn
            // 
            idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            idDataGridViewTextBoxColumn.HeaderText = "Id";
            idDataGridViewTextBoxColumn.MinimumWidth = 6;
            idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            idDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // barcodeRawDataGridViewTextBoxColumn
            // 
            barcodeRawDataGridViewTextBoxColumn.DataPropertyName = "BarcodeRaw";
            barcodeRawDataGridViewTextBoxColumn.HeaderText = "BarcodeRaw";
            barcodeRawDataGridViewTextBoxColumn.MinimumWidth = 6;
            barcodeRawDataGridViewTextBoxColumn.Name = "barcodeRawDataGridViewTextBoxColumn";
            barcodeRawDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // orderNoDataGridViewTextBoxColumn
            // 
            orderNoDataGridViewTextBoxColumn.DataPropertyName = "OrderNo";
            orderNoDataGridViewTextBoxColumn.HeaderText = "OrderNo";
            orderNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            orderNoDataGridViewTextBoxColumn.Name = "orderNoDataGridViewTextBoxColumn";
            orderNoDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // customerNameDataGridViewTextBoxColumn
            // 
            customerNameDataGridViewTextBoxColumn.DataPropertyName = "CustomerName";
            customerNameDataGridViewTextBoxColumn.HeaderText = "CustomerName";
            customerNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            customerNameDataGridViewTextBoxColumn.Name = "customerNameDataGridViewTextBoxColumn";
            customerNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            typeDataGridViewTextBoxColumn.HeaderText = "Type";
            typeDataGridViewTextBoxColumn.MinimumWidth = 6;
            typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // qtyDataGridViewTextBoxColumn
            // 
            qtyDataGridViewTextBoxColumn.DataPropertyName = "Qty";
            qtyDataGridViewTextBoxColumn.HeaderText = "Qty";
            qtyDataGridViewTextBoxColumn.MinimumWidth = 6;
            qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            qtyDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // patternIdDataGridViewTextBoxColumn
            // 
            patternIdDataGridViewTextBoxColumn.DataPropertyName = "PatternId";
            patternIdDataGridViewTextBoxColumn.HeaderText = "PatternId";
            patternIdDataGridViewTextBoxColumn.MinimumWidth = 6;
            patternIdDataGridViewTextBoxColumn.Name = "patternIdDataGridViewTextBoxColumn";
            patternIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // lotNumberDataGridViewTextBoxColumn
            // 
            lotNumberDataGridViewTextBoxColumn.DataPropertyName = "LotNumber";
            lotNumberDataGridViewTextBoxColumn.HeaderText = "LotNumber";
            lotNumberDataGridViewTextBoxColumn.MinimumWidth = 6;
            lotNumberDataGridViewTextBoxColumn.Name = "lotNumberDataGridViewTextBoxColumn";
            lotNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // patternNoErpDataGridViewTextBoxColumn
            // 
            patternNoErpDataGridViewTextBoxColumn.DataPropertyName = "PatternNoErp";
            patternNoErpDataGridViewTextBoxColumn.HeaderText = "PatternNoErp";
            patternNoErpDataGridViewTextBoxColumn.MinimumWidth = 6;
            patternNoErpDataGridViewTextBoxColumn.Name = "patternNoErpDataGridViewTextBoxColumn";
            patternNoErpDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            statusDataGridViewTextBoxColumn.HeaderText = "Status";
            statusDataGridViewTextBoxColumn.MinimumWidth = 6;
            statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            statusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // errorMessageDataGridViewTextBoxColumn
            // 
            errorMessageDataGridViewTextBoxColumn.DataPropertyName = "ErrorMessage";
            errorMessageDataGridViewTextBoxColumn.HeaderText = "ErrorMessage";
            errorMessageDataGridViewTextBoxColumn.MinimumWidth = 6;
            errorMessageDataGridViewTextBoxColumn.Name = "errorMessageDataGridViewTextBoxColumn";
            errorMessageDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // warningDataGridViewTextBoxColumn
            // 
            warningDataGridViewTextBoxColumn.DataPropertyName = "Warning";
            warningDataGridViewTextBoxColumn.HeaderText = "Warning";
            warningDataGridViewTextBoxColumn.MinimumWidth = 6;
            warningDataGridViewTextBoxColumn.Name = "warningDataGridViewTextBoxColumn";
            warningDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // attemptDataGridViewTextBoxColumn
            // 
            attemptDataGridViewTextBoxColumn.DataPropertyName = "Attempt";
            attemptDataGridViewTextBoxColumn.HeaderText = "Attempt";
            attemptDataGridViewTextBoxColumn.MinimumWidth = 6;
            attemptDataGridViewTextBoxColumn.Name = "attemptDataGridViewTextBoxColumn";
            attemptDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // stationDataGridViewTextBoxColumn
            // 
            stationDataGridViewTextBoxColumn.DataPropertyName = "Station";
            stationDataGridViewTextBoxColumn.HeaderText = "Station";
            stationDataGridViewTextBoxColumn.MinimumWidth = 6;
            stationDataGridViewTextBoxColumn.Name = "stationDataGridViewTextBoxColumn";
            stationDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // createdByDataGridViewTextBoxColumn
            // 
            createdByDataGridViewTextBoxColumn.DataPropertyName = "CreatedBy";
            createdByDataGridViewTextBoxColumn.HeaderText = "CreatedBy";
            createdByDataGridViewTextBoxColumn.MinimumWidth = 6;
            createdByDataGridViewTextBoxColumn.Name = "createdByDataGridViewTextBoxColumn";
            createdByDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // createdAtDataGridViewTextBoxColumn
            // 
            createdAtDataGridViewTextBoxColumn.DataPropertyName = "CreatedAt";
            createdAtDataGridViewTextBoxColumn.HeaderText = "CreatedAt";
            createdAtDataGridViewTextBoxColumn.MinimumWidth = 6;
            createdAtDataGridViewTextBoxColumn.Name = "createdAtDataGridViewTextBoxColumn";
            createdAtDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // updatedAtDataGridViewTextBoxColumn
            // 
            updatedAtDataGridViewTextBoxColumn.DataPropertyName = "UpdatedAt";
            updatedAtDataGridViewTextBoxColumn.HeaderText = "UpdatedAt";
            updatedAtDataGridViewTextBoxColumn.MinimumWidth = 6;
            updatedAtDataGridViewTextBoxColumn.Name = "updatedAtDataGridViewTextBoxColumn";
            updatedAtDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bindingSource1
            // 
            bindingSource1.DataSource = typeof(PrintJob);
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // ucST3
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvList);
            Controls.Add(btnRunBot);
            Name = "ucST3";
            Size = new Size(1151, 759);
            ((System.ComponentModel.ISupportInitialize)dgvList).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnRunBot;
        private DataGridView dgvList;
        private DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn barcodeRawDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn orderNoDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn customerNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn patternIdDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn lotNumberDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn patternNoErpDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn errorMessageDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn warningDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn attemptDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn stationDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn createdByDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn createdAtDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn updatedAtDataGridViewTextBoxColumn;
        private BindingSource bindingSource1;
        private System.Windows.Forms.Timer timer1;
    }
}
