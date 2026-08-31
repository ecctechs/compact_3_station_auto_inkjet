namespace InkjetOperator.Views;

partial class EditPatternUserControl
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
        tlpEditPatternRoot = new System.Windows.Forms.TableLayoutPanel();
        pnlRulesList = new AntdUI.Panel();
        tlpRulesList = new System.Windows.Forms.TableLayoutPanel();
        btnNewPattern = new AntdUI.Button();
        lblRulesHeader = new AntdUI.Label();
        lstPatterns = new System.Windows.Forms.ListBox();
        btnDeletePattern = new AntdUI.Button();
        pnlRuleDetail = new AntdUI.Panel();
        tlpRuleDetail = new System.Windows.Forms.TableLayoutPanel();
        tlpRuleForm = new System.Windows.Forms.TableLayoutPanel();
        lblRuleName = new AntdUI.Label();
        lblDescription = new AntdUI.Label();
        lblLotTest = new AntdUI.Label();
        lblBlockText = new AntdUI.Label();
        txtPatternName = new AntdUI.Input();
        txtDescription = new AntdUI.Input();
        txtLotTest = new AntdUI.Input();
        txtBlockText = new AntdUI.Input();
        tlpTransformHeader = new System.Windows.Forms.TableLayoutPanel();
        lblTransformTitle = new AntdUI.Label();
        btnAddRule = new AntdUI.Button();
        tblRules = new AntdUI.Table();
        tlpPreview = new System.Windows.Forms.TableLayoutPanel();
        lblPreviewCaption = new AntdUI.Label();
        lblPreview = new AntdUI.Label();
        flpDetailActions = new System.Windows.Forms.FlowLayoutPanel();
        btnSaveRule = new AntdUI.Button();
        btnCancel = new AntdUI.Button();
        tlpEditPatternRoot.SuspendLayout();
        pnlRulesList.SuspendLayout();
        tlpRulesList.SuspendLayout();
        pnlRuleDetail.SuspendLayout();
        tlpRuleDetail.SuspendLayout();
        tlpRuleForm.SuspendLayout();
        tlpTransformHeader.SuspendLayout();
        tlpPreview.SuspendLayout();
        flpDetailActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpEditPatternRoot
        //
        tlpEditPatternRoot.BackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        tlpEditPatternRoot.ColumnCount = 2;
        tlpEditPatternRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
        tlpEditPatternRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpEditPatternRoot.Controls.Add(pnlRulesList, 0, 0);
        tlpEditPatternRoot.Controls.Add(pnlRuleDetail, 1, 0);
        tlpEditPatternRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpEditPatternRoot.Location = new System.Drawing.Point(0, 0);
        tlpEditPatternRoot.Name = "tlpEditPatternRoot";
        tlpEditPatternRoot.Padding = new System.Windows.Forms.Padding(32);
        tlpEditPatternRoot.RowCount = 1;
        tlpEditPatternRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpEditPatternRoot.Size = new System.Drawing.Size(1500, 975);
        tlpEditPatternRoot.TabIndex = 0;
        //
        // pnlRulesList
        //
        pnlRulesList.Back = System.Drawing.Color.White;
        pnlRulesList.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlRulesList.BorderWidth = 2F;
        pnlRulesList.Controls.Add(tlpRulesList);
        pnlRulesList.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlRulesList.Location = new System.Drawing.Point(35, 35);
        pnlRulesList.Margin = new System.Windows.Forms.Padding(3, 3, 19, 3);
        pnlRulesList.Name = "pnlRulesList";
        pnlRulesList.Padding = new System.Windows.Forms.Padding(16);
        pnlRulesList.Radius = 16;
        pnlRulesList.Size = new System.Drawing.Size(332, 888);
        pnlRulesList.TabIndex = 0;
        //
        // tlpRulesList
        //
        tlpRulesList.BackColor = System.Drawing.Color.White;
        tlpRulesList.ColumnCount = 1;
        tlpRulesList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRulesList.Controls.Add(btnNewPattern, 0, 0);
        tlpRulesList.Controls.Add(lblRulesHeader, 0, 1);
        tlpRulesList.Controls.Add(lstPatterns, 0, 2);
        tlpRulesList.Controls.Add(btnDeletePattern, 0, 3);
        tlpRulesList.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRulesList.Location = new System.Drawing.Point(16, 16);
        tlpRulesList.Name = "tlpRulesList";
        tlpRulesList.RowCount = 4;
        tlpRulesList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
        tlpRulesList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpRulesList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRulesList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
        tlpRulesList.Size = new System.Drawing.Size(292, 848);
        tlpRulesList.TabIndex = 0;
        //
        // btnNewPattern
        //
        btnNewPattern.DefaultBorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnNewPattern.BorderWidth = 2F;
        btnNewPattern.Dock = System.Windows.Forms.DockStyle.Fill;
        btnNewPattern.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnNewPattern.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnNewPattern.Location = new System.Drawing.Point(3, 3);
        btnNewPattern.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
        btnNewPattern.Name = "btnNewPattern";
        btnNewPattern.Radius = 8;
        btnNewPattern.Size = new System.Drawing.Size(285, 54);
        btnNewPattern.TabIndex = 0;
        btnNewPattern.Text = "+ New Rule";
        btnNewPattern.Type = AntdUI.TTypeMini.Default;
        //
        // lblRulesHeader
        //
        lblRulesHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRulesHeader.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold);
        lblRulesHeader.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
        lblRulesHeader.Location = new System.Drawing.Point(3, 55);
        lblRulesHeader.Name = "lblRulesHeader";
        lblRulesHeader.Size = new System.Drawing.Size(285, 35);
        lblRulesHeader.TabIndex = 1;
        lblRulesHeader.Text = "RULES";
        lblRulesHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lstPatterns
        //
        lstPatterns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        lstPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
        lstPatterns.Font = new System.Drawing.Font("Segoe UI", 15F);
        lstPatterns.FormattingEnabled = true;
        lstPatterns.IntegralHeight = false;
        lstPatterns.ItemHeight = 28;
        lstPatterns.Location = new System.Drawing.Point(3, 89);
        lstPatterns.Name = "lstPatterns";
        lstPatterns.Size = new System.Drawing.Size(285, 660);
        lstPatterns.TabIndex = 2;
        //
        // btnDeletePattern
        //
        btnDeletePattern.DefaultBorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
        btnDeletePattern.BorderWidth = 2F;
        btnDeletePattern.Dock = System.Windows.Forms.DockStyle.Fill;
        btnDeletePattern.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnDeletePattern.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        btnDeletePattern.Location = new System.Drawing.Point(3, 629);
        btnDeletePattern.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
        btnDeletePattern.Name = "btnDeletePattern";
        btnDeletePattern.Radius = 8;
        btnDeletePattern.Size = new System.Drawing.Size(285, 54);
        btnDeletePattern.TabIndex = 3;
        btnDeletePattern.Text = "Delete";
        btnDeletePattern.Type = AntdUI.TTypeMini.Default;
        //
        // pnlRuleDetail
        //
        pnlRuleDetail.Back = System.Drawing.Color.White;
        pnlRuleDetail.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlRuleDetail.BorderWidth = 2F;
        pnlRuleDetail.Controls.Add(tlpRuleDetail);
        pnlRuleDetail.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlRuleDetail.Location = new System.Drawing.Point(323, 35);
        pnlRuleDetail.Margin = new System.Windows.Forms.Padding(3);
        pnlRuleDetail.Name = "pnlRuleDetail";
        pnlRuleDetail.Padding = new System.Windows.Forms.Padding(24);
        pnlRuleDetail.Radius = 16;
        pnlRuleDetail.Size = new System.Drawing.Size(1052, 888);
        pnlRuleDetail.TabIndex = 1;
        //
        // tlpRuleDetail
        //
        tlpRuleDetail.BackColor = System.Drawing.Color.White;
        tlpRuleDetail.ColumnCount = 1;
        tlpRuleDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRuleDetail.Controls.Add(tlpRuleForm, 0, 0);
        tlpRuleDetail.Controls.Add(tlpTransformHeader, 0, 1);
        tlpRuleDetail.Controls.Add(tblRules, 0, 2);
        tlpRuleDetail.Controls.Add(tlpPreview, 0, 3);
        tlpRuleDetail.Controls.Add(flpDetailActions, 0, 4);
        tlpRuleDetail.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRuleDetail.Location = new System.Drawing.Point(24, 24);
        tlpRuleDetail.Name = "tlpRuleDetail";
        tlpRuleDetail.RowCount = 5;
        tlpRuleDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 280F));
        tlpRuleDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
        tlpRuleDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRuleDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
        tlpRuleDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));
        tlpRuleDetail.Size = new System.Drawing.Size(992, 828);
        tlpRuleDetail.TabIndex = 0;
        //
        // tlpRuleForm
        //
        tlpRuleForm.BackColor = System.Drawing.Color.White;
        tlpRuleForm.ColumnCount = 2;
        tlpRuleForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 162F));
        tlpRuleForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRuleForm.Controls.Add(lblRuleName, 0, 0);
        tlpRuleForm.Controls.Add(lblDescription, 0, 1);
        tlpRuleForm.Controls.Add(lblLotTest, 0, 2);
        tlpRuleForm.Controls.Add(lblBlockText, 0, 3);
        tlpRuleForm.Controls.Add(txtPatternName, 1, 0);
        tlpRuleForm.Controls.Add(txtDescription, 1, 1);
        tlpRuleForm.Controls.Add(txtLotTest, 1, 2);
        tlpRuleForm.Controls.Add(txtBlockText, 1, 3);
        tlpRuleForm.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRuleForm.Location = new System.Drawing.Point(0, 0);
        tlpRuleForm.Margin = new System.Windows.Forms.Padding(0);
        tlpRuleForm.Name = "tlpRuleForm";
        tlpRuleForm.RowCount = 4;
        tlpRuleForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpRuleForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpRuleForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpRuleForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        tlpRuleForm.Size = new System.Drawing.Size(992, 280);
        tlpRuleForm.TabIndex = 0;
        //
        // lblRuleName
        //
        lblRuleName.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRuleName.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblRuleName.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblRuleName.Location = new System.Drawing.Point(3, 0);
        lblRuleName.Name = "lblRuleName";
        lblRuleName.Size = new System.Drawing.Size(155, 70);
        lblRuleName.TabIndex = 0;
        lblRuleName.Text = "Rule Name :";
        lblRuleName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDescription
        //
        lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
        lblDescription.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblDescription.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblDescription.Location = new System.Drawing.Point(3, 56);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new System.Drawing.Size(155, 70);
        lblDescription.TabIndex = 1;
        lblDescription.Text = "Description :";
        lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblLotTest
        //
        lblLotTest.Dock = System.Windows.Forms.DockStyle.Fill;
        lblLotTest.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblLotTest.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblLotTest.Location = new System.Drawing.Point(3, 112);
        lblLotTest.Name = "lblLotTest";
        lblLotTest.Size = new System.Drawing.Size(155, 70);
        lblLotTest.TabIndex = 2;
        lblLotTest.Text = "Lot Test :";
        lblLotTest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblBlockText
        //
        lblBlockText.Dock = System.Windows.Forms.DockStyle.Fill;
        lblBlockText.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblBlockText.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblBlockText.Location = new System.Drawing.Point(3, 168);
        lblBlockText.Name = "lblBlockText";
        lblBlockText.Size = new System.Drawing.Size(155, 70);
        lblBlockText.TabIndex = 3;
        lblBlockText.Text = "Block Text :";
        lblBlockText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // txtPatternName
        //
        txtPatternName.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtPatternName.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtPatternName.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtPatternName.Location = new System.Drawing.Point(133, 8);
        txtPatternName.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtPatternName.Name = "txtPatternName";
        txtPatternName.Radius = 6;
        txtPatternName.Size = new System.Drawing.Size(822, 50);
        txtPatternName.TabIndex = 4;
        //
        // txtDescription
        //
        txtDescription.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtDescription.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtDescription.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtDescription.Location = new System.Drawing.Point(133, 64);
        txtDescription.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtDescription.Name = "txtDescription";
        txtDescription.Radius = 6;
        txtDescription.Size = new System.Drawing.Size(822, 50);
        txtDescription.TabIndex = 5;
        //
        // txtLotTest
        //
        txtLotTest.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtLotTest.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtLotTest.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtLotTest.Location = new System.Drawing.Point(133, 120);
        txtLotTest.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtLotTest.Name = "txtLotTest";
        txtLotTest.Radius = 6;
        txtLotTest.Size = new System.Drawing.Size(822, 50);
        txtLotTest.TabIndex = 6;
        //
        // txtBlockText
        //
        txtBlockText.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        txtBlockText.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtBlockText.Font = new System.Drawing.Font("Segoe UI", 15F);
        txtBlockText.Location = new System.Drawing.Point(133, 176);
        txtBlockText.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
        txtBlockText.Name = "txtBlockText";
        txtBlockText.Radius = 6;
        txtBlockText.Size = new System.Drawing.Size(822, 50);
        txtBlockText.TabIndex = 7;
        //
        // tlpTransformHeader
        //
        tlpTransformHeader.BackColor = System.Drawing.Color.White;
        tlpTransformHeader.ColumnCount = 2;
        tlpTransformHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransformHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 188F));
        tlpTransformHeader.Controls.Add(lblTransformTitle, 0, 0);
        tlpTransformHeader.Controls.Add(btnAddRule, 1, 0);
        tlpTransformHeader.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTransformHeader.Location = new System.Drawing.Point(0, 224);
        tlpTransformHeader.Margin = new System.Windows.Forms.Padding(0);
        tlpTransformHeader.Name = "tlpTransformHeader";
        tlpTransformHeader.RowCount = 1;
        tlpTransformHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTransformHeader.Size = new System.Drawing.Size(992, 70);
        tlpTransformHeader.TabIndex = 1;
        //
        // lblTransformTitle
        //
        lblTransformTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTransformTitle.Font = new System.Drawing.Font("Segoe UI", 17.5F, System.Drawing.FontStyle.Bold);
        lblTransformTitle.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        lblTransformTitle.Location = new System.Drawing.Point(3, 0);
        lblTransformTitle.Name = "lblTransformTitle";
        lblTransformTitle.Size = new System.Drawing.Size(798, 70);
        lblTransformTitle.TabIndex = 0;
        lblTransformTitle.Text = "Transform Rules";
        lblTransformTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnAddRule
        //
        btnAddRule.Anchor = System.Windows.Forms.AnchorStyles.Right;
        btnAddRule.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnAddRule.ForeColor = System.Drawing.Color.White;
        btnAddRule.Location = new System.Drawing.Point(647, 6);
        btnAddRule.Name = "btnAddRule";
        btnAddRule.Radius = 8;
        btnAddRule.Size = new System.Drawing.Size(180, 55);
        btnAddRule.TabIndex = 1;
        btnAddRule.Text = "+ Add Rule";
        btnAddRule.Type = AntdUI.TTypeMini.Primary;
        //
        // tblRules
        //
        tblRules.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        tblRules.Bordered = true;
        tblRules.ColumnBack = System.Drawing.Color.FromArgb(30, 30, 30);
        tblRules.ColumnFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        tblRules.ColumnFore = System.Drawing.Color.White;
        tblRules.Dock = System.Windows.Forms.DockStyle.Fill;
        tblRules.EditMode = AntdUI.TEditMode.Click;
        tblRules.EmptyText = "No transform rules";
        tblRules.Font = new System.Drawing.Font("Segoe UI", 14F);
        tblRules.Location = new System.Drawing.Point(0, 284);
        tblRules.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
        tblRules.Name = "tblRules";
        tblRules.Radius = 8;
        tblRules.RowHeight = 58;
        tblRules.Size = new System.Drawing.Size(992, 308);
        tblRules.TabIndex = 2;
        //
        // tlpPreview
        //
        tlpPreview.BackColor = System.Drawing.Color.FromArgb(240, 245, 251);
        tlpPreview.ColumnCount = 2;
        tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
        tlpPreview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpPreview.Controls.Add(lblPreviewCaption, 0, 0);
        tlpPreview.Controls.Add(lblPreview, 1, 0);
        tlpPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpPreview.Location = new System.Drawing.Point(0, 534);
        tlpPreview.Margin = new System.Windows.Forms.Padding(0);
        tlpPreview.Name = "tlpPreview";
        tlpPreview.RowCount = 1;
        tlpPreview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpPreview.Size = new System.Drawing.Size(992, 75);
        tlpPreview.TabIndex = 3;
        //
        // lblPreviewCaption
        //
        lblPreviewCaption.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPreviewCaption.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblPreviewCaption.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblPreviewCaption.Location = new System.Drawing.Point(12, 0);
        lblPreviewCaption.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
        lblPreviewCaption.Name = "lblPreviewCaption";
        lblPreviewCaption.Size = new System.Drawing.Size(122, 75);
        lblPreviewCaption.TabIndex = 0;
        lblPreviewCaption.Text = "Preview :";
        lblPreviewCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblPreview
        //
        lblPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPreview.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        lblPreview.ForeColor = System.Drawing.Color.FromArgb(21, 87, 36);
        lblPreview.Location = new System.Drawing.Point(113, 0);
        lblPreview.Name = "lblPreview";
        lblPreview.Size = new System.Drawing.Size(848, 75);
        lblPreview.TabIndex = 1;
        lblPreview.Text = "";
        lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // flpDetailActions
        //
        flpDetailActions.BackColor = System.Drawing.Color.White;
        flpDetailActions.Controls.Add(btnSaveRule);
        flpDetailActions.Controls.Add(btnCancel);
        flpDetailActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpDetailActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpDetailActions.Location = new System.Drawing.Point(0, 594);
        flpDetailActions.Margin = new System.Windows.Forms.Padding(0);
        flpDetailActions.Name = "flpDetailActions";
        flpDetailActions.Size = new System.Drawing.Size(992, 85);
        flpDetailActions.TabIndex = 4;
        flpDetailActions.WrapContents = false;
        //
        // btnSaveRule
        //
        btnSaveRule.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        btnSaveRule.ForeColor = System.Drawing.Color.White;
        btnSaveRule.Location = new System.Drawing.Point(634, 12);
        btnSaveRule.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnSaveRule.Name = "btnSaveRule";
        btnSaveRule.Radius = 8;
        btnSaveRule.Size = new System.Drawing.Size(192, 55);
        btnSaveRule.TabIndex = 0;
        btnSaveRule.Text = "Save Rule";
        btnSaveRule.Type = AntdUI.TTypeMini.Primary;
        //
        // btnCancel
        //
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 15F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnCancel.Location = new System.Drawing.Point(474, 12);
        btnCancel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(192, 55);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Cancel";
        btnCancel.Type = AntdUI.TTypeMini.Default;
        //
        // EditPatternUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        Controls.Add(tlpEditPatternRoot);
        MinimumSize = new System.Drawing.Size(940, 620);
        Name = "EditPatternUserControl";
        Size = new System.Drawing.Size(1500, 975);
        tlpEditPatternRoot.ResumeLayout(false);
        pnlRulesList.ResumeLayout(false);
        tlpRulesList.ResumeLayout(false);
        pnlRuleDetail.ResumeLayout(false);
        tlpRuleDetail.ResumeLayout(false);
        tlpRuleForm.ResumeLayout(false);
        tlpTransformHeader.ResumeLayout(false);
        tlpPreview.ResumeLayout(false);
        flpDetailActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpEditPatternRoot;
    private AntdUI.Panel pnlRulesList;
    private System.Windows.Forms.TableLayoutPanel tlpRulesList;
    private AntdUI.Button btnNewPattern;
    private AntdUI.Label lblRulesHeader;
    private System.Windows.Forms.ListBox lstPatterns;
    private AntdUI.Button btnDeletePattern;
    private AntdUI.Panel pnlRuleDetail;
    private System.Windows.Forms.TableLayoutPanel tlpRuleDetail;
    private System.Windows.Forms.TableLayoutPanel tlpRuleForm;
    private AntdUI.Label lblRuleName;
    private AntdUI.Label lblDescription;
    private AntdUI.Label lblLotTest;
    private AntdUI.Label lblBlockText;
    private AntdUI.Input txtPatternName;
    private AntdUI.Input txtDescription;
    private AntdUI.Input txtLotTest;
    private AntdUI.Input txtBlockText;
    private System.Windows.Forms.TableLayoutPanel tlpTransformHeader;
    private AntdUI.Label lblTransformTitle;
    private AntdUI.Button btnAddRule;
    private AntdUI.Table tblRules;
    private System.Windows.Forms.TableLayoutPanel tlpPreview;
    private AntdUI.Label lblPreviewCaption;
    private AntdUI.Label lblPreview;
    private System.Windows.Forms.FlowLayoutPanel flpDetailActions;
    private AntdUI.Button btnSaveRule;
    private AntdUI.Button btnCancel;
}
