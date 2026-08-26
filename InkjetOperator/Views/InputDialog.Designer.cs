namespace InkjetOperator.Views;

partial class InputDialog
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
        tlpDialogRoot = new System.Windows.Forms.TableLayoutPanel();
        tlpTitleBar = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        btnTitleClose = new System.Windows.Forms.Button();
        tlpBody = new System.Windows.Forms.TableLayoutPanel();
        lblPrompt = new AntdUI.Label();
        txtValue = new AntdUI.Input();
        flpActions = new System.Windows.Forms.FlowLayoutPanel();
        btnCancel = new AntdUI.Button();
        btnOk = new AntdUI.Button();
        tlpDialogRoot.SuspendLayout();
        tlpTitleBar.SuspendLayout();
        tlpBody.SuspendLayout();
        flpActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpDialogRoot
        //
        tlpDialogRoot.ColumnCount = 1;
        tlpDialogRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Controls.Add(tlpTitleBar, 0, 0);
        tlpDialogRoot.Controls.Add(tlpBody, 0, 1);
        tlpDialogRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpDialogRoot.Location = new System.Drawing.Point(0, 0);
        tlpDialogRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpDialogRoot.Name = "tlpDialogRoot";
        tlpDialogRoot.RowCount = 2;
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
        tlpDialogRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpDialogRoot.Size = new System.Drawing.Size(506, 267);
        tlpDialogRoot.TabIndex = 0;
        //
        // tlpTitleBar
        //
        tlpTitleBar.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        tlpTitleBar.ColumnCount = 2;
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
        tlpTitleBar.Controls.Add(lblTitle, 0, 0);
        tlpTitleBar.Controls.Add(btnTitleClose, 1, 0);
        tlpTitleBar.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTitleBar.Location = new System.Drawing.Point(0, 0);
        tlpTitleBar.Margin = new System.Windows.Forms.Padding(0);
        tlpTitleBar.Name = "tlpTitleBar";
        tlpTitleBar.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
        tlpTitleBar.RowCount = 1;
        tlpTitleBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBar.Size = new System.Drawing.Size(506, 60);
        tlpTitleBar.TabIndex = 0;
        //
        // lblTitle
        //
        lblTitle.BackColor = System.Drawing.Color.Transparent;
        lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F);
        lblTitle.ForeColor = System.Drawing.Color.White;
        lblTitle.Location = new System.Drawing.Point(19, 0);
        lblTitle.Margin = new System.Windows.Forms.Padding(3, 0, 8, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(438, 60);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Title";
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnTitleClose
        //
        btnTitleClose.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnTitleClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        btnTitleClose.Dock = System.Windows.Forms.DockStyle.Fill;
        btnTitleClose.FlatAppearance.BorderSize = 0;
        btnTitleClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnTitleClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnTitleClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnTitleClose.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnTitleClose.ForeColor = System.Drawing.Color.White;
        btnTitleClose.Location = new System.Drawing.Point(392, 0);
        btnTitleClose.Margin = new System.Windows.Forms.Padding(0);
        btnTitleClose.Name = "btnTitleClose";
        btnTitleClose.Size = new System.Drawing.Size(55, 60);
        btnTitleClose.TabIndex = 1;
        btnTitleClose.TabStop = false;
        btnTitleClose.Text = "✕";
        btnTitleClose.UseVisualStyleBackColor = false;
        //
        // tlpBody
        //
        tlpBody.BackColor = System.Drawing.Color.White;
        tlpBody.ColumnCount = 1;
        tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBody.Controls.Add(lblPrompt, 0, 0);
        tlpBody.Controls.Add(txtValue, 0, 1);
        tlpBody.Controls.Add(flpActions, 0, 2);
        tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpBody.Location = new System.Drawing.Point(0, 52);
        tlpBody.Margin = new System.Windows.Forms.Padding(0);
        tlpBody.Name = "tlpBody";
        tlpBody.Padding = new System.Windows.Forms.Padding(24, 20, 24, 20);
        tlpBody.RowCount = 3;
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
        tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpBody.Size = new System.Drawing.Size(506, 207);
        tlpBody.TabIndex = 1;
        //
        // lblPrompt
        //
        lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
        lblPrompt.Font = new System.Drawing.Font("Segoe UI", 14F);
        lblPrompt.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblPrompt.Location = new System.Drawing.Point(27, 20);
        lblPrompt.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        lblPrompt.Name = "lblPrompt";
        lblPrompt.Size = new System.Drawing.Size(444, 30);
        lblPrompt.TabIndex = 0;
        lblPrompt.Text = "Prompt";
        lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // txtValue
        //
        txtValue.BorderColor = System.Drawing.Color.FromArgb(91, 155, 213);
        txtValue.BorderWidth = 1.5F;
        txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
        txtValue.Font = new System.Drawing.Font("Segoe UI", 14F);
        txtValue.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        txtValue.Location = new System.Drawing.Point(27, 54);
        txtValue.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
        txtValue.Name = "txtValue";
        txtValue.Radius = 8;
        txtValue.Size = new System.Drawing.Size(444, 53);
        txtValue.TabIndex = 0;
        //
        // flpActions
        //
        flpActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        flpActions.Controls.Add(btnCancel);
        flpActions.Controls.Add(btnOk);
        flpActions.Location = new System.Drawing.Point(27, 116);
        flpActions.Margin = new System.Windows.Forms.Padding(3, 16, 3, 0);
        flpActions.Name = "flpActions";
        flpActions.Size = new System.Drawing.Size(444, 51);
        flpActions.TabIndex = 1;
        flpActions.WrapContents = false;
        //
        // btnCancel
        //
        btnCancel.BorderWidth = 2F;
        btnCancel.DefaultBack = System.Drawing.Color.White;
        btnCancel.DefaultBorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 14F);
        btnCancel.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        btnCancel.Location = new System.Drawing.Point(266, 0);
        btnCancel.Margin = new System.Windows.Forms.Padding(0);
        btnCancel.Name = "btnCancel";
        btnCancel.Radius = 8;
        btnCancel.Size = new System.Drawing.Size(138, 51);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Cancel";
        //
        // btnOk
        //
        btnOk.DefaultBack = System.Drawing.Color.FromArgb(91, 155, 213);
        btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
        btnOk.Font = new System.Drawing.Font("Segoe UI", 14F);
        btnOk.ForeColor = System.Drawing.Color.White;
        btnOk.Location = new System.Drawing.Point(134, 0);
        btnOk.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        btnOk.Name = "btnOk";
        btnOk.Radius = 8;
        btnOk.Size = new System.Drawing.Size(138, 51);
        btnOk.TabIndex = 1;
        btnOk.Text = "OK";
        btnOk.Type = AntdUI.TTypeMini.Primary;
        //
        // InputDialog
        //
        AcceptButton = btnOk;
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        BorderColor = System.Drawing.Color.FromArgb(175, 200, 224);
        BorderWidth = 1;
        CancelButton = btnCancel;
        ClientSize = new System.Drawing.Size(440, 232);
        Controls.Add(tlpDialogRoot);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "InputDialog";
        Radius = 8;
        Shadow = 12;
        ShowInTaskbar = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "InputDialog";
        tlpDialogRoot.ResumeLayout(false);
        tlpTitleBar.ResumeLayout(false);
        tlpBody.ResumeLayout(false);
        flpActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpDialogRoot;
    private System.Windows.Forms.TableLayoutPanel tlpTitleBar;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnTitleClose;
    private System.Windows.Forms.TableLayoutPanel tlpBody;
    private AntdUI.Label lblPrompt;
    private AntdUI.Input txtValue;
    private System.Windows.Forms.FlowLayoutPanel flpActions;
    private AntdUI.Button btnCancel;
    private AntdUI.Button btnOk;
}
