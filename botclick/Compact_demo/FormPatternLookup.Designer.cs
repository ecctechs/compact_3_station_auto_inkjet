using System.Windows.Forms;

namespace BotClickApp
{
    partial class FormPatternLookup
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblPattern;
        private TextBox txtPattern;
        private Button btnEnter;
        private Button btnEditPattern;
        private GroupBox grpProgram;
        private Label lblProgramName;
        private TextBox txtProgramName;
        private Label lblMk1ProgramNo;
        private TextBox txtMk1ProgramNo;
        private GroupBox grpBlocks;
        private DataGridView dgvBlocks;
        private GroupBox grpMisc;
        private Label lblHeight;
        private TextBox txtHeight;
        private Label lblWidth;
        private TextBox txtWidth;
        private Label lblTriggerDelay;
        private TextBox txtTriggerDelay;
        private Label lblPostAct;
        private TextBox txtPostAct;
        private Label lblDelay;
        private TextBox txtDelay;
        private Button btnClose;

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
            this.lblPattern = new System.Windows.Forms.Label();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.btnEnter = new System.Windows.Forms.Button();
            this.btnEditPattern = new System.Windows.Forms.Button();
            this.grpProgram = new System.Windows.Forms.GroupBox();
            this.lblProgramName = new System.Windows.Forms.Label();
            this.txtProgramName = new System.Windows.Forms.TextBox();
            this.lblMk1ProgramNo = new System.Windows.Forms.Label();
            this.txtMk1ProgramNo = new System.Windows.Forms.TextBox();
            this.grpBlocks = new System.Windows.Forms.GroupBox();
            this.dgvBlocks = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpMisc = new System.Windows.Forms.GroupBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblWidth = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.lblTriggerDelay = new System.Windows.Forms.Label();
            this.txtTriggerDelay = new System.Windows.Forms.TextBox();
            this.lblPostAct = new System.Windows.Forms.Label();
            this.txtPostAct = new System.Windows.Forms.TextBox();
            this.lblDelay = new System.Windows.Forms.Label();
            this.txtDelay = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpProgram.SuspendLayout();
            this.grpBlocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlocks)).BeginInit();
            this.grpMisc.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPattern
            // 
            this.lblPattern.AutoSize = true;
            this.lblPattern.Location = new System.Drawing.Point(16, 14);
            this.lblPattern.Name = "lblPattern";
            this.lblPattern.Size = new System.Drawing.Size(76, 16);
            this.lblPattern.TabIndex = 0;
            this.lblPattern.Text = "Pattern No :";
            // 
            // txtPattern
            // 
            this.txtPattern.Location = new System.Drawing.Point(106, 11);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(360, 22);
            this.txtPattern.TabIndex = 1;
            // 
            // btnEnter
            // 
            this.btnEnter.Location = new System.Drawing.Point(480, 8);
            this.btnEnter.Name = "btnEnter";
            this.btnEnter.Size = new System.Drawing.Size(100, 26);
            this.btnEnter.TabIndex = 2;
            this.btnEnter.Text = "Enter";
            this.btnEnter.UseVisualStyleBackColor = true;
            // 
            // btnEditPattern
            // 
            this.btnEditPattern.Location = new System.Drawing.Point(590, 8);
            this.btnEditPattern.Name = "btnEditPattern";
            this.btnEditPattern.Size = new System.Drawing.Size(110, 26);
            this.btnEditPattern.TabIndex = 7;
            this.btnEditPattern.Text = "Edit Pattern";
            this.btnEditPattern.UseVisualStyleBackColor = true;
            // 
            // grpProgram
            // 
            this.grpProgram.Controls.Add(this.lblProgramName);
            this.grpProgram.Controls.Add(this.txtProgramName);
            this.grpProgram.Controls.Add(this.lblMk1ProgramNo);
            this.grpProgram.Controls.Add(this.txtMk1ProgramNo);
            this.grpProgram.Location = new System.Drawing.Point(16, 46);
            this.grpProgram.Name = "grpProgram";
            this.grpProgram.Size = new System.Drawing.Size(688, 120);
            this.grpProgram.TabIndex = 3;
            this.grpProgram.TabStop = false;
            this.grpProgram.Text = "Program";
            // 
            // lblProgramName
            // 
            this.lblProgramName.AutoSize = true;
            this.lblProgramName.Location = new System.Drawing.Point(12, 26);
            this.lblProgramName.Name = "lblProgramName";
            this.lblProgramName.Size = new System.Drawing.Size(105, 16);
            this.lblProgramName.TabIndex = 0;
            this.lblProgramName.Text = "program_name :";
            // 
            // txtProgramName
            // 
            this.txtProgramName.Location = new System.Drawing.Point(130, 23);
            this.txtProgramName.Name = "txtProgramName";
            this.txtProgramName.ReadOnly = true;
            this.txtProgramName.Size = new System.Drawing.Size(540, 22);
            this.txtProgramName.TabIndex = 1;
            // 
            // lblMk1ProgramNo
            // 
            this.lblMk1ProgramNo.AutoSize = true;
            this.lblMk1ProgramNo.Location = new System.Drawing.Point(6, 59);
            this.lblMk1ProgramNo.Name = "lblMk1ProgramNo";
            this.lblMk1ProgramNo.Size = new System.Drawing.Size(118, 16);
            this.lblMk1ProgramNo.TabIndex = 4;
            this.lblMk1ProgramNo.Text = "mk1_program_no :";
            // 
            // txtMk1ProgramNo
            // 
            this.txtMk1ProgramNo.Location = new System.Drawing.Point(130, 53);
            this.txtMk1ProgramNo.Name = "txtMk1ProgramNo";
            this.txtMk1ProgramNo.ReadOnly = true;
            this.txtMk1ProgramNo.Size = new System.Drawing.Size(200, 22);
            this.txtMk1ProgramNo.TabIndex = 5;
            // 
            // grpBlocks
            // 
            this.grpBlocks.Controls.Add(this.dgvBlocks);
            this.grpBlocks.Location = new System.Drawing.Point(16, 176);
            this.grpBlocks.Name = "grpBlocks";
            this.grpBlocks.Size = new System.Drawing.Size(850, 220);
            this.grpBlocks.TabIndex = 4;
            this.grpBlocks.TabStop = false;
            this.grpBlocks.Text = "MK1 Blocks (1-5)";
            // 
            // dgvBlocks
            // 
            this.dgvBlocks.AllowUserToAddRows = false;
            this.dgvBlocks.AllowUserToDeleteRows = false;
            this.dgvBlocks.ColumnHeadersHeight = 29;
            this.dgvBlocks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.colResult});
            this.dgvBlocks.Location = new System.Drawing.Point(12, 22);
            this.dgvBlocks.Name = "dgvBlocks";
            this.dgvBlocks.RowHeadersVisible = false;
            this.dgvBlocks.RowHeadersWidth = 51;
            this.dgvBlocks.Size = new System.Drawing.Size(820, 186);
            this.dgvBlocks.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Block";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Text";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "X";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Width = 60;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Y";
            this.dataGridViewTextBoxColumn4.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.Width = 60;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Size";
            this.dataGridViewTextBoxColumn5.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Width = 125;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Scale";
            this.dataGridViewTextBoxColumn6.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Width = 125;
            // 
            // colResult
            // 
            this.colResult.HeaderText = "Result";
            this.colResult.MinimumWidth = 6;
            this.colResult.Name = "colResult";
            this.colResult.ReadOnly = true;
            this.colResult.Width = 150;
            // 
            // grpMisc
            // 
            this.grpMisc.Controls.Add(this.lblHeight);
            this.grpMisc.Controls.Add(this.txtHeight);
            this.grpMisc.Controls.Add(this.lblWidth);
            this.grpMisc.Controls.Add(this.txtWidth);
            this.grpMisc.Controls.Add(this.lblTriggerDelay);
            this.grpMisc.Controls.Add(this.txtTriggerDelay);
            this.grpMisc.Controls.Add(this.lblPostAct);
            this.grpMisc.Controls.Add(this.txtPostAct);
            this.grpMisc.Controls.Add(this.lblDelay);
            this.grpMisc.Controls.Add(this.txtDelay);
            this.grpMisc.Location = new System.Drawing.Point(16, 404);
            this.grpMisc.Name = "grpMisc";
            this.grpMisc.Size = new System.Drawing.Size(688, 110);
            this.grpMisc.TabIndex = 5;
            this.grpMisc.TabStop = false;
            this.grpMisc.Text = "Other MK1 Settings";
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(12, 26);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(81, 16);
            this.lblHeight.TabIndex = 0;
            this.lblHeight.Text = "mk1_height :";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(110, 23);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.ReadOnly = true;
            this.txtHeight.Size = new System.Drawing.Size(90, 22);
            this.txtHeight.TabIndex = 1;
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(210, 26);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(75, 16);
            this.lblWidth.TabIndex = 2;
            this.lblWidth.Text = "mk1_width :";
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(290, 23);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.ReadOnly = true;
            this.txtWidth.Size = new System.Drawing.Size(90, 22);
            this.txtWidth.TabIndex = 3;
            // 
            // lblTriggerDelay
            // 
            this.lblTriggerDelay.AutoSize = true;
            this.lblTriggerDelay.Location = new System.Drawing.Point(390, 26);
            this.lblTriggerDelay.Name = "lblTriggerDelay";
            this.lblTriggerDelay.Size = new System.Drawing.Size(124, 16);
            this.lblTriggerDelay.TabIndex = 4;
            this.lblTriggerDelay.Text = "mk1_trigger_delay :";
            // 
            // txtTriggerDelay
            // 
            this.txtTriggerDelay.Location = new System.Drawing.Point(520, 23);
            this.txtTriggerDelay.Name = "txtTriggerDelay";
            this.txtTriggerDelay.ReadOnly = true;
            this.txtTriggerDelay.Size = new System.Drawing.Size(150, 22);
            this.txtTriggerDelay.TabIndex = 5;
            // 
            // lblPostAct
            // 
            this.lblPostAct.AutoSize = true;
            this.lblPostAct.Location = new System.Drawing.Point(12, 56);
            this.lblPostAct.Name = "lblPostAct";
            this.lblPostAct.Size = new System.Drawing.Size(96, 16);
            this.lblPostAct.TabIndex = 6;
            this.lblPostAct.Text = "mk1_post_act :";
            // 
            // txtPostAct
            // 
            this.txtPostAct.Location = new System.Drawing.Point(114, 53);
            this.txtPostAct.Name = "txtPostAct";
            this.txtPostAct.ReadOnly = true;
            this.txtPostAct.Size = new System.Drawing.Size(180, 22);
            this.txtPostAct.TabIndex = 7;
            // 
            // lblDelay
            // 
            this.lblDelay.AutoSize = true;
            this.lblDelay.Location = new System.Drawing.Point(300, 56);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(79, 16);
            this.lblDelay.TabIndex = 8;
            this.lblDelay.Text = "mk1_delay :";
            // 
            // txtDelay
            // 
            this.txtDelay.Location = new System.Drawing.Point(385, 53);
            this.txtDelay.Name = "txtDelay";
            this.txtDelay.ReadOnly = true;
            this.txtDelay.Size = new System.Drawing.Size(120, 22);
            this.txtDelay.TabIndex = 9;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(604, 520);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 26);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FormPatternLookup
            // 
            this.ClientSize = new System.Drawing.Size(880, 560);
            this.Controls.Add(this.lblPattern);
            this.Controls.Add(this.txtPattern);
            this.Controls.Add(this.btnEnter);
            this.Controls.Add(this.btnEditPattern);
            this.Controls.Add(this.grpProgram);
            this.Controls.Add(this.grpBlocks);
            this.Controls.Add(this.grpMisc);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormPatternLookup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pattern Lookup";
            this.grpProgram.ResumeLayout(false);
            this.grpProgram.PerformLayout();
            this.grpBlocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlocks)).EndInit();
            this.grpMisc.ResumeLayout(false);
            this.grpMisc.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn colResult;
    }
}
