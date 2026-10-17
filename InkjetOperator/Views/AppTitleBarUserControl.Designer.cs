namespace InkjetOperator.Views;

partial class AppTitleBarUserControl
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppTitleBarUserControl));
        tlpTitleBarRoot = new System.Windows.Forms.TableLayoutPanel();
        picAppIcon = new System.Windows.Forms.PictureBox();
        lblAppTitle = new System.Windows.Forms.Label();
        btnMinimize = new System.Windows.Forms.Button();
        btnMaximize = new System.Windows.Forms.Button();
        btnClose = new System.Windows.Forms.Button();
        tlpTitleBarRoot.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picAppIcon).BeginInit();
        SuspendLayout();
        //
        // tlpTitleBarRoot
        //
        tlpTitleBarRoot.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        tlpTitleBarRoot.ColumnCount = 5;
        tlpTitleBarRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tlpTitleBarRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBarRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
        tlpTitleBarRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
        tlpTitleBarRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
        tlpTitleBarRoot.Controls.Add(picAppIcon, 0, 0);
        tlpTitleBarRoot.Controls.Add(lblAppTitle, 1, 0);
        tlpTitleBarRoot.Controls.Add(btnMinimize, 2, 0);
        tlpTitleBarRoot.Controls.Add(btnMaximize, 3, 0);
        tlpTitleBarRoot.Controls.Add(btnClose, 4, 0);
        tlpTitleBarRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpTitleBarRoot.Location = new System.Drawing.Point(0, 0);
        tlpTitleBarRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpTitleBarRoot.Name = "tlpTitleBarRoot";
        tlpTitleBarRoot.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
        tlpTitleBarRoot.RowCount = 1;
        tlpTitleBarRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpTitleBarRoot.Size = new System.Drawing.Size(1920, 40);
        tlpTitleBarRoot.TabIndex = 0;
        //
        // picAppIcon — ไอคอนโปรแกรมบนพื้นขาว: ในไอคอนมีส่วนสีน้ำเงินเข้มเดียวกับแถบหัว วางตรง ๆ จะกลืนหาย
        //
        picAppIcon.BackColor = System.Drawing.Color.White;
        picAppIcon.Image = (System.Drawing.Image)resources.GetObject("picAppIcon.Image");
        picAppIcon.Location = new System.Drawing.Point(14, 6);
        picAppIcon.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
        picAppIcon.Name = "picAppIcon";
        picAppIcon.Padding = new System.Windows.Forms.Padding(1);
        picAppIcon.Size = new System.Drawing.Size(28, 28);
        picAppIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        picAppIcon.TabIndex = 4;
        picAppIcon.TabStop = false;
        //
        // lblAppTitle
        //
        lblAppTitle.BackColor = System.Drawing.Color.Transparent;
        lblAppTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 12F);
        lblAppTitle.ForeColor = System.Drawing.Color.White;
        lblAppTitle.Location = new System.Drawing.Point(57, 0);
        lblAppTitle.Margin = new System.Windows.Forms.Padding(3, 0, 8, 0);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Size = new System.Drawing.Size(1300, 50);
        lblAppTitle.TabIndex = 0;
        lblAppTitle.Text = "Compact Inkjet";
        lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnMinimize
        //
        btnMinimize.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMinimize.Dock = System.Windows.Forms.DockStyle.Fill;
        btnMinimize.FlatAppearance.BorderSize = 0;
        btnMinimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnMinimize.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnMinimize.ForeColor = System.Drawing.Color.White;
        btnMinimize.Location = new System.Drawing.Point(1062, 0);
        btnMinimize.Margin = new System.Windows.Forms.Padding(0);
        btnMinimize.Name = "btnMinimize";
        btnMinimize.Size = new System.Drawing.Size(58, 50);
        btnMinimize.TabIndex = 1;
        btnMinimize.TabStop = false;
        btnMinimize.Text = "─";
        btnMinimize.UseVisualStyleBackColor = false;
        //
        // btnMaximize
        //
        btnMaximize.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnMaximize.Dock = System.Windows.Forms.DockStyle.Fill;
        btnMaximize.FlatAppearance.BorderSize = 0;
        btnMaximize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnMaximize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(91, 155, 213);
        btnMaximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnMaximize.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnMaximize.ForeColor = System.Drawing.Color.White;
        btnMaximize.Location = new System.Drawing.Point(1108, 0);
        btnMaximize.Margin = new System.Windows.Forms.Padding(0);
        btnMaximize.Name = "btnMaximize";
        btnMaximize.Size = new System.Drawing.Size(58, 50);
        btnMaximize.TabIndex = 2;
        btnMaximize.TabStop = false;
        btnMaximize.Text = "□";
        btnMaximize.Visible = false;
        btnMaximize.UseVisualStyleBackColor = false;
        //
        // btnClose
        //
        btnClose.BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnClose.Font = new System.Drawing.Font("Segoe UI", 12.5F);
        btnClose.ForeColor = System.Drawing.Color.White;
        btnClose.Location = new System.Drawing.Point(1154, 0);
        btnClose.Margin = new System.Windows.Forms.Padding(0);
        btnClose.Name = "btnClose";
        btnClose.Size = new System.Drawing.Size(58, 50);
        btnClose.TabIndex = 3;
        btnClose.TabStop = false;
        btnClose.Text = "✕";
        btnClose.UseVisualStyleBackColor = false;
        //
        // AppTitleBarUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.FromArgb(36, 71, 101);
        Controls.Add(tlpTitleBarRoot);
        Name = "AppTitleBarUserControl";
        Size = new System.Drawing.Size(1920, 40);
        tlpTitleBarRoot.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picAppIcon).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpTitleBarRoot;
    private System.Windows.Forms.PictureBox picAppIcon;
    private System.Windows.Forms.Label lblAppTitle;
    private System.Windows.Forms.Button btnMinimize;
    private System.Windows.Forms.Button btnMaximize;
    private System.Windows.Forms.Button btnClose;
}
