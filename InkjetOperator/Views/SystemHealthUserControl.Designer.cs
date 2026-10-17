namespace InkjetOperator.Views;

partial class SystemHealthUserControl
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
        tlpHealthRoot = new System.Windows.Forms.TableLayoutPanel();
        lblHealthTitle = new AntdUI.Label();
        lblHealthSummary = new AntdUI.Label();
        flpHealthActions = new System.Windows.Forms.FlowLayoutPanel();
        btnRestartBackend = new AntdUI.Button();
        lblRestartHelp = new AntdUI.Label();
        btnOpenAppLog = new AntdUI.Button();
        btnOpenBackendLog = new AntdUI.Button();
        tblHealth = new AntdUI.Table();
        tlpHealthRoot.SuspendLayout();
        flpHealthActions.SuspendLayout();
        SuspendLayout();
        //
        // tlpHealthRoot
        //
        tlpHealthRoot.ColumnCount = 1;
        tlpHealthRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHealthRoot.Controls.Add(lblHealthTitle, 0, 0);
        tlpHealthRoot.Controls.Add(lblHealthSummary, 0, 1);
        tlpHealthRoot.Controls.Add(flpHealthActions, 0, 2);
        tlpHealthRoot.Controls.Add(tblHealth, 0, 3);
        tlpHealthRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpHealthRoot.Location = new System.Drawing.Point(32, 32);
        tlpHealthRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpHealthRoot.Name = "tlpHealthRoot";
        tlpHealthRoot.RowCount = 4;
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        tlpHealthRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpHealthRoot.Size = new System.Drawing.Size(1216, 736);
        tlpHealthRoot.TabIndex = 0;
        //
        // lblHealthTitle
        //
        lblHealthTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHealthTitle.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
        lblHealthTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblHealthTitle.Location = new System.Drawing.Point(0, 0);
        lblHealthTitle.Margin = new System.Windows.Forms.Padding(0);
        lblHealthTitle.Name = "lblHealthTitle";
        lblHealthTitle.Size = new System.Drawing.Size(1216, 54);
        lblHealthTitle.TabIndex = 0;
        lblHealthTitle.Text = "สถานะระบบ";
        //
        // lblHealthSummary
        //
        lblHealthSummary.Dock = System.Windows.Forms.DockStyle.Fill;
        lblHealthSummary.Font = new System.Drawing.Font("Segoe UI", 15F);
        lblHealthSummary.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblHealthSummary.Location = new System.Drawing.Point(0, 54);
        lblHealthSummary.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        lblHealthSummary.Name = "lblHealthSummary";
        lblHealthSummary.Size = new System.Drawing.Size(1216, 32);
        lblHealthSummary.TabIndex = 1;
        lblHealthSummary.Text = "กำลังตรวจสอบ...";
        //
        // flpHealthActions
        //
        flpHealthActions.Controls.Add(btnRestartBackend);
        flpHealthActions.Controls.Add(lblRestartHelp);
        flpHealthActions.Controls.Add(btnOpenAppLog);
        flpHealthActions.Controls.Add(btnOpenBackendLog);
        flpHealthActions.Dock = System.Windows.Forms.DockStyle.Fill;
        flpHealthActions.Location = new System.Drawing.Point(0, 98);
        flpHealthActions.Margin = new System.Windows.Forms.Padding(0);
        flpHealthActions.Name = "flpHealthActions";
        flpHealthActions.Size = new System.Drawing.Size(1216, 64);
        flpHealthActions.TabIndex = 2;
        flpHealthActions.WrapContents = false;
        //
        // btnRestartBackend — ขอบแดง: ทุกสถานีต่อ backend ไม่ได้ช่วงรีสตาร์ท
        //
        btnRestartBackend.BorderWidth = 2F;
        btnRestartBackend.DefaultBorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnRestartBackend.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnRestartBackend.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnRestartBackend.Location = new System.Drawing.Point(0, 0);
        btnRestartBackend.Margin = new System.Windows.Forms.Padding(0, 0, 16, 12);
        btnRestartBackend.Name = "btnRestartBackend";
        btnRestartBackend.Radius = 8;
        btnRestartBackend.Size = new System.Drawing.Size(260, 52);
        btnRestartBackend.TabIndex = 0;
        btnRestartBackend.Text = "รีสตาร์ท backend";
        //
        // lblRestartHelp
        //
        lblRestartHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblRestartHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblRestartHelp.Location = new System.Drawing.Point(276, 0);
        lblRestartHelp.Margin = new System.Windows.Forms.Padding(0);
        lblRestartHelp.Name = "lblRestartHelp";
        lblRestartHelp.Size = new System.Drawing.Size(540, 52);
        lblRestartHelp.TabIndex = 1;
        lblRestartHelp.Text = "ใช้หลังอัปเดตไฟล์ backend · ระหว่างรีสตาร์ททุกสถานีต่อ backend ไม่ได้\r\nอย่ากดตอนกำลังส่งงาน · กดได้เฉพาะเครื่องที่ backend อยู่";
        //
        // btnOpenAppLog — เปิดโฟลเดอร์ log ของโปรแกรมเครื่องนี้ (ทุกข้อความที่ขึ้นบนจอ ผลส่งงาน error)
        //
        btnOpenAppLog.BorderWidth = 2F;
        btnOpenAppLog.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnOpenAppLog.Font = new System.Drawing.Font("Segoe UI", 13F);
        btnOpenAppLog.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnOpenAppLog.IconSvg = "FolderOpenOutlined";
        btnOpenAppLog.Margin = new System.Windows.Forms.Padding(16, 0, 0, 12);
        btnOpenAppLog.Name = "btnOpenAppLog";
        btnOpenAppLog.Radius = 8;
        btnOpenAppLog.Size = new System.Drawing.Size(180, 52);
        btnOpenAppLog.TabIndex = 2;
        btnOpenAppLog.Text = "log โปรแกรม";
        //
        // btnOpenBackendLog — เปิดโฟลเดอร์ log ของ backend (เฉพาะเครื่องที่ backend อยู่)
        //
        btnOpenBackendLog.BorderWidth = 2F;
        btnOpenBackendLog.DefaultBorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnOpenBackendLog.Font = new System.Drawing.Font("Segoe UI", 13F);
        btnOpenBackendLog.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        btnOpenBackendLog.IconSvg = "FolderOpenOutlined";
        btnOpenBackendLog.Margin = new System.Windows.Forms.Padding(12, 0, 0, 12);
        btnOpenBackendLog.Name = "btnOpenBackendLog";
        btnOpenBackendLog.Radius = 8;
        btnOpenBackendLog.Size = new System.Drawing.Size(180, 52);
        btnOpenBackendLog.TabIndex = 3;
        btnOpenBackendLog.Text = "log backend";
        //
        // tblHealth
        //
        tblHealth.Dock = System.Windows.Forms.DockStyle.Fill;
        tblHealth.EmptyText = "กำลังตรวจสอบ...";
        tblHealth.Font = new System.Drawing.Font("Segoe UI", 14F);
        tblHealth.Location = new System.Drawing.Point(0, 162);
        tblHealth.Margin = new System.Windows.Forms.Padding(0);
        tblHealth.Name = "tblHealth";
        tblHealth.Radius = 10;
        tblHealth.RowHeight = 46;
        tblHealth.Size = new System.Drawing.Size(1216, 574);
        tblHealth.TabIndex = 3;
        //
        // SystemHealthUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpHealthRoot);
        Name = "SystemHealthUserControl";
        Padding = new System.Windows.Forms.Padding(32);
        Size = new System.Drawing.Size(1280, 800);
        tlpHealthRoot.ResumeLayout(false);
        flpHealthActions.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpHealthRoot;
    private AntdUI.Label lblHealthTitle;
    private AntdUI.Label lblHealthSummary;
    private AntdUI.Table tblHealth;
    private System.Windows.Forms.FlowLayoutPanel flpHealthActions;
    private AntdUI.Button btnRestartBackend;
    private AntdUI.Label lblRestartHelp;
    private AntdUI.Button btnOpenAppLog;
    private AntdUI.Button btnOpenBackendLog;
}
