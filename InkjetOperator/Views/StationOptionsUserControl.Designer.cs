namespace InkjetOperator.Views;

partial class StationOptionsUserControl
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
        tlpOptionsRoot = new System.Windows.Forms.TableLayoutPanel();
        lblOptionsTitle = new AntdUI.Label();
        pnlRemoteSend = new AntdUI.Panel();
        lblRemoteSendHeading = new AntdUI.Label();
        tlpRemoteSend = new System.Windows.Forms.TableLayoutPanel();
        chkManualRemoteSend = new AntdUI.Checkbox();
        lblRemoteSendHelp = new AntdUI.Label();
        pnlProcessTabs = new AntdUI.Panel();
        tlpProcessTabs = new System.Windows.Forms.TableLayoutPanel();
        lblProcessTabsHeading = new AntdUI.Label();
        flpProcessTabs = new System.Windows.Forms.FlowLayoutPanel();
        rdoProcessTabsStations = new AntdUI.Radio();
        rdoProcessTabsDev = new AntdUI.Radio();
        rdoProcessTabsOff = new AntdUI.Radio();
        lblProcessTabsHelp = new AntdUI.Label();
        pnlMockup = new AntdUI.Panel();
        tlpMockup = new System.Windows.Forms.TableLayoutPanel();
        lblMockupHeading = new AntdUI.Label();
        chkMockupStatus = new AntdUI.Checkbox();
        lblMockupHelp = new AntdUI.Label();
        pnlReset = new AntdUI.Panel();
        tlpReset = new System.Windows.Forms.TableLayoutPanel();
        lblResetHeading = new AntdUI.Label();
        btnResetRuntime = new AntdUI.Button();
        lblResetHelp = new AntdUI.Label();
        tlpOptionsRoot.SuspendLayout();
        pnlRemoteSend.SuspendLayout();
        pnlProcessTabs.SuspendLayout();
        tlpProcessTabs.SuspendLayout();
        flpProcessTabs.SuspendLayout();
        pnlMockup.SuspendLayout();
        tlpMockup.SuspendLayout();
        pnlReset.SuspendLayout();
        tlpReset.SuspendLayout();
        tlpRemoteSend.SuspendLayout();
        SuspendLayout();
        //
        // tlpOptionsRoot
        //
        tlpOptionsRoot.ColumnCount = 1;
        tlpOptionsRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Controls.Add(lblOptionsTitle, 0, 0);
        tlpOptionsRoot.Controls.Add(pnlRemoteSend, 0, 1);
        tlpOptionsRoot.Controls.Add(pnlProcessTabs, 0, 2);
        tlpOptionsRoot.Controls.Add(pnlMockup, 0, 3);
        tlpOptionsRoot.Controls.Add(pnlReset, 0, 4);
        tlpOptionsRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOptionsRoot.Location = new System.Drawing.Point(32, 32);
        tlpOptionsRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpOptionsRoot.Name = "tlpOptionsRoot";
        tlpOptionsRoot.RowCount = 6;
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 232F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 214F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Size = new System.Drawing.Size(1216, 976);
        tlpOptionsRoot.TabIndex = 0;
        //
        // lblOptionsTitle
        //
        lblOptionsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOptionsTitle.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
        lblOptionsTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblOptionsTitle.Location = new System.Drawing.Point(0, 0);
        lblOptionsTitle.Margin = new System.Windows.Forms.Padding(0);
        lblOptionsTitle.Name = "lblOptionsTitle";
        lblOptionsTitle.Size = new System.Drawing.Size(1216, 66);
        lblOptionsTitle.TabIndex = 0;
        lblOptionsTitle.Text = "ตัวเลือกหน้างาน";
        //
        // pnlRemoteSend
        //
        pnlRemoteSend.Back = System.Drawing.Color.White;
        pnlRemoteSend.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlRemoteSend.BorderWidth = 2F;
        pnlRemoteSend.Controls.Add(tlpRemoteSend);
        pnlRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlRemoteSend.Location = new System.Drawing.Point(0, 66);
        pnlRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        pnlRemoteSend.Name = "pnlRemoteSend";
        pnlRemoteSend.Padding = new System.Windows.Forms.Padding(24);
        pnlRemoteSend.Radius = 10;
        pnlRemoteSend.Size = new System.Drawing.Size(1216, 210);
        pnlRemoteSend.TabIndex = 1;
        //
        // tlpRemoteSend
        //
        tlpRemoteSend.ColumnCount = 1;
        tlpRemoteSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.Controls.Add(lblRemoteSendHeading, 0, 0);
        tlpRemoteSend.Controls.Add(chkManualRemoteSend, 0, 1);
        tlpRemoteSend.Controls.Add(lblRemoteSendHelp, 0, 2);
        tlpRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRemoteSend.Location = new System.Drawing.Point(24, 24);
        tlpRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        tlpRemoteSend.Name = "tlpRemoteSend";
        tlpRemoteSend.RowCount = 3;
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.Size = new System.Drawing.Size(1168, 162);
        tlpRemoteSend.TabIndex = 0;
        //
        // pnlProcessTabs
        //
        pnlProcessTabs.Back = System.Drawing.Color.White;
        pnlProcessTabs.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlProcessTabs.BorderWidth = 2F;
        pnlProcessTabs.Controls.Add(tlpProcessTabs);
        pnlProcessTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlProcessTabs.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlProcessTabs.Name = "pnlProcessTabs";
        pnlProcessTabs.Padding = new System.Windows.Forms.Padding(24);
        pnlProcessTabs.Radius = 10;
        pnlProcessTabs.Size = new System.Drawing.Size(1216, 216);
        pnlProcessTabs.TabIndex = 2;
        //
        // tlpProcessTabs
        //
        tlpProcessTabs.ColumnCount = 1;
        tlpProcessTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessTabs.Controls.Add(lblProcessTabsHeading, 0, 0);
        tlpProcessTabs.Controls.Add(flpProcessTabs, 0, 1);
        tlpProcessTabs.Controls.Add(lblProcessTabsHelp, 0, 2);
        tlpProcessTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpProcessTabs.Margin = new System.Windows.Forms.Padding(0);
        tlpProcessTabs.Name = "tlpProcessTabs";
        tlpProcessTabs.RowCount = 3;
        tlpProcessTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpProcessTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpProcessTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessTabs.Size = new System.Drawing.Size(1168, 168);
        tlpProcessTabs.TabIndex = 0;
        //
        // lblProcessTabsHeading
        //
        lblProcessTabsHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcessTabsHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblProcessTabsHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblProcessTabsHeading.Margin = new System.Windows.Forms.Padding(0);
        lblProcessTabsHeading.Name = "lblProcessTabsHeading";
        lblProcessTabsHeading.Size = new System.Drawing.Size(1168, 42);
        lblProcessTabsHeading.TabIndex = 0;
        lblProcessTabsHeading.Text = "ตัวกรอง In-line / Off-line ในหน้า Order List";
        //
        // flpProcessTabs - สามตัวเลือกอยู่ใน parent เดียวกัน Radio จึงเลือกได้ทีละข้อเอง
        //
        flpProcessTabs.Controls.Add(rdoProcessTabsStations);
        flpProcessTabs.Controls.Add(rdoProcessTabsDev);
        flpProcessTabs.Controls.Add(rdoProcessTabsOff);
        flpProcessTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        flpProcessTabs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        flpProcessTabs.Name = "flpProcessTabs";
        flpProcessTabs.Size = new System.Drawing.Size(1168, 32);
        flpProcessTabs.TabIndex = 1;
        flpProcessTabs.WrapContents = false;
        //
        // rdoProcessTabsStations
        //
        rdoProcessTabsStations.Font = new System.Drawing.Font("Segoe UI", 15F);
        rdoProcessTabsStations.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsStations.Margin = new System.Windows.Forms.Padding(0, 0, 32, 0);
        rdoProcessTabsStations.Name = "rdoProcessTabsStations";
        rdoProcessTabsStations.Size = new System.Drawing.Size(300, 32);
        rdoProcessTabsStations.TabIndex = 0;
        rdoProcessTabsStations.Text = "โชว์ที่ ST1 และ ST3";
        //
        // rdoProcessTabsDev
        //
        rdoProcessTabsDev.Font = new System.Drawing.Font("Segoe UI", 15F);
        rdoProcessTabsDev.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsDev.Margin = new System.Windows.Forms.Padding(0, 0, 32, 0);
        rdoProcessTabsDev.Name = "rdoProcessTabsDev";
        rdoProcessTabsDev.Size = new System.Drawing.Size(300, 32);
        rdoProcessTabsDev.TabIndex = 1;
        rdoProcessTabsDev.Text = "เฉพาะโหมด Dev";
        //
        // rdoProcessTabsOff
        //
        rdoProcessTabsOff.Font = new System.Drawing.Font("Segoe UI", 15F);
        rdoProcessTabsOff.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsOff.Margin = new System.Windows.Forms.Padding(0);
        rdoProcessTabsOff.Name = "rdoProcessTabsOff";
        rdoProcessTabsOff.Size = new System.Drawing.Size(300, 32);
        rdoProcessTabsOff.TabIndex = 2;
        rdoProcessTabsOff.Text = "ปิดทั้งหมด";
        //
        // lblProcessTabsHelp
        //
        lblProcessTabsHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcessTabsHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblProcessTabsHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblProcessTabsHelp.Margin = new System.Windows.Forms.Padding(0);
        lblProcessTabsHelp.Name = "lblProcessTabsHelp";
        lblProcessTabsHelp.Size = new System.Drawing.Size(1168, 82);
        lblProcessTabsHelp.TabIndex = 2;
        lblProcessTabsHelp.Text = "เพิ่ม dropdown กรองงานเป็น In-line หรือ Off-line ในหน้า Order List ใช้ได้ทั้งแท็บ List และ History\r\nอ่านจากช่อง Process seq ของงานตรง ๆ โปรแกรมไม่ได้เปลี่ยนค่านี้เอง งานที่ช่องนี้ไม่ใช่ In-line หรือ Off-line เห็นเมื่อเลือก \"ทั้งหมด\"\r\nเปลี่ยนแล้วหน้า Order List เห็นผลภายในไม่กี่วินาที ไม่ต้องปิดเปิดโปรแกรม";
        //
        // pnlMockup — ขอบส้ม: เป็นของที่ต้องกลับมาปิด ไม่ใช่ตั้งทิ้งไว้
        //
        pnlMockup.Back = System.Drawing.Color.White;
        pnlMockup.BorderColor = System.Drawing.Color.FromArgb(217, 119, 6);
        pnlMockup.BorderWidth = 2F;
        pnlMockup.Controls.Add(tlpMockup);
        pnlMockup.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlMockup.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlMockup.Name = "pnlMockup";
        pnlMockup.Padding = new System.Windows.Forms.Padding(24);
        pnlMockup.Radius = 10;
        pnlMockup.Size = new System.Drawing.Size(1216, 224);
        pnlMockup.TabIndex = 3;
        //
        // tlpMockup
        //
        tlpMockup.ColumnCount = 1;
        tlpMockup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMockup.Controls.Add(lblMockupHeading, 0, 0);
        tlpMockup.Controls.Add(chkMockupStatus, 0, 1);
        tlpMockup.Controls.Add(lblMockupHelp, 0, 2);
        tlpMockup.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMockup.Margin = new System.Windows.Forms.Padding(0);
        tlpMockup.Name = "tlpMockup";
        tlpMockup.RowCount = 3;
        tlpMockup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpMockup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpMockup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMockup.Size = new System.Drawing.Size(1168, 176);
        tlpMockup.TabIndex = 0;
        //
        // lblMockupHeading
        //
        lblMockupHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMockupHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblMockupHeading.ForeColor = System.Drawing.Color.FromArgb(217, 119, 6);
        lblMockupHeading.Margin = new System.Windows.Forms.Padding(0);
        lblMockupHeading.Name = "lblMockupHeading";
        lblMockupHeading.Size = new System.Drawing.Size(1168, 42);
        lblMockupHeading.TabIndex = 0;
        lblMockupHeading.Text = "Mockup สถานะการเชื่อมต่อ (สำหรับถ่ายรูปคู่มือ)";
        //
        // chkMockupStatus
        //
        chkMockupStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        chkMockupStatus.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkMockupStatus.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkMockupStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkMockupStatus.Name = "chkMockupStatus";
        chkMockupStatus.Size = new System.Drawing.Size(1168, 32);
        chkMockupStatus.TabIndex = 1;
        chkMockupStatus.Text = "ให้ทุกหน้าแสดงสถานะเชื่อมต่อสำเร็จทั้งหมด";
        //
        // lblMockupHelp
        //
        lblMockupHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMockupHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMockupHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblMockupHelp.Margin = new System.Windows.Forms.Padding(0);
        lblMockupHelp.Name = "lblMockupHelp";
        lblMockupHelp.Size = new System.Drawing.Size(1168, 90);
        lblMockupHelp.TabIndex = 2;
        lblMockupHelp.Text = "ไฟสถานะ ป้ายผลตรวจ และบรรทัดในกล่องผลการทำงานของทุกหน้า ขึ้นว่าเชื่อมต่อได้ทั้งหมด โดยไม่ได้ต่อเครื่องจริง ใช้ถ่ายรูปทำคู่มือ\r\nมีผลทุกสถานีบนเครื่องนี้ (Scan Barcode · ST1 · ST3) เปลี่ยน MENU_LEVEL ไปถ่ายหน้าของสถานีอื่นได้ ค่ายังค้างอยู่\r\nไม่แตะการส่งงาน การเขียนค่าเข้า PLC หรือการสั่งเครื่องจริง — ของพวกนั้นยังรายงานผลจริงเสมอ\r\nถ่ายเสร็จต้องกลับมาปิดที่หน้านี้ (MENU_LEVEL 99) ห้ามเปิดค้างไว้ที่เครื่องหน้างาน";
        //
        // pnlReset
        //
        pnlReset.Back = System.Drawing.Color.White;
        pnlReset.BorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        pnlReset.BorderWidth = 2F;
        pnlReset.Controls.Add(tlpReset);
        pnlReset.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlReset.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlReset.Name = "pnlReset";
        pnlReset.Padding = new System.Windows.Forms.Padding(24);
        pnlReset.Radius = 10;
        pnlReset.Size = new System.Drawing.Size(1216, 198);
        pnlReset.TabIndex = 4;
        //
        // tlpReset
        //
        tlpReset.ColumnCount = 1;
        tlpReset.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpReset.Controls.Add(lblResetHeading, 0, 0);
        tlpReset.Controls.Add(lblResetHelp, 0, 1);
        tlpReset.Controls.Add(btnResetRuntime, 0, 2);
        tlpReset.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpReset.Margin = new System.Windows.Forms.Padding(0);
        tlpReset.Name = "tlpReset";
        tlpReset.RowCount = 3;
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpReset.Size = new System.Drawing.Size(1168, 150);
        tlpReset.TabIndex = 0;
        //
        // lblResetHeading
        //
        lblResetHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblResetHeading.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblResetHeading.Margin = new System.Windows.Forms.Padding(0);
        lblResetHeading.Name = "lblResetHeading";
        lblResetHeading.Size = new System.Drawing.Size(1168, 42);
        lblResetHeading.TabIndex = 0;
        lblResetHeading.Text = "รีเซ็ตกลับเป็นค่าเริ่มต้น";
        //
        // lblResetHelp
        //
        lblResetHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblResetHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblResetHelp.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblResetHelp.Name = "lblResetHelp";
        lblResetHelp.Size = new System.Drawing.Size(1168, 44);
        lblResetHelp.TabIndex = 1;
        lblResetHelp.Text = "ล้างทุกอย่างที่บอกว่างานเดินไปถึงไหนแล้ว แล้วให้ทุกใบกลับไปเป็นรอเริ่ม ใช้ตอนทดสอบเมื่ออยากเริ่มนับหนึ่งใหม่ทั้งกระดาน\r\nลบ: คิวเครื่องทุกแถว · ประวัติคำสั่งที่ส่งเข้าเครื่องทุกแถว · ธงคำขอที่ ST3 ฝากไว้\r\nไม่แตะ: ตัวงาน ข้อมูล pattern ข้อความ UV แผนการผลิต และค่าแคลมป์ ยังอยู่ครบเหมือนเดิม\r\nย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่ออยู่กับ backend เดียวกัน ไม่ใช่แค่เครื่องนี้";
        //
        // btnResetRuntime
        //
        btnResetRuntime.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnResetRuntime.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnResetRuntime.ForeColor = System.Drawing.Color.White;
        btnResetRuntime.Margin = new System.Windows.Forms.Padding(0);
        btnResetRuntime.Name = "btnResetRuntime";
        btnResetRuntime.Radius = 8;
        btnResetRuntime.Size = new System.Drawing.Size(300, 52);
        btnResetRuntime.TabIndex = 2;
        btnResetRuntime.Text = "รีเซ็ตกลับเป็นค่าเริ่มต้น";
        btnResetRuntime.Type = AntdUI.TTypeMini.Error;
        //
        // lblRemoteSendHeading
        //
        lblRemoteSendHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblRemoteSendHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblRemoteSendHeading.Location = new System.Drawing.Point(0, 0);
        lblRemoteSendHeading.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHeading.Name = "lblRemoteSendHeading";
        lblRemoteSendHeading.Size = new System.Drawing.Size(1168, 42);
        lblRemoteSendHeading.TabIndex = 0;
        lblRemoteSendHeading.Text = "ปุ่มสำรองของปุ่มกดหน้างาน (ST3)";
        //
        // chkManualRemoteSend
        //
        chkManualRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        chkManualRemoteSend.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkManualRemoteSend.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkManualRemoteSend.Location = new System.Drawing.Point(0, 42);
        chkManualRemoteSend.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkManualRemoteSend.Name = "chkManualRemoteSend";
        chkManualRemoteSend.Size = new System.Drawing.Size(1168, 32);
        chkManualRemoteSend.TabIndex = 1;
        chkManualRemoteSend.Text = "โชว์ปุ่ม \"ขอให้ ST1 ส่ง\" ในหน้า Order Detail";
        //
        // lblRemoteSendHelp
        //
        lblRemoteSendHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHelp.Font = new System.Drawing.Font("Segoe UI", 13F);
        lblRemoteSendHelp.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
        lblRemoteSendHelp.Location = new System.Drawing.Point(0, 86);
        lblRemoteSendHelp.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHelp.Name = "lblRemoteSendHelp";
        lblRemoteSendHelp.Size = new System.Drawing.Size(1168, 76);
        lblRemoteSendHelp.TabIndex = 2;
        lblRemoteSendHelp.Text = "ใช้แทนปุ่มกดหน้างานตอนปุ่มกดหรือ PLC ใช้ไม่ได้ ขึ้นเฉพาะเครื่องของ ST3 และเฉพาะงานที่ส่งขั้นแรกไปแล้วและยังเหลือขั้นถัดไป\r\nก่อนกด ต้องแน่ใจว่าชิ้นงานอยู่ในเครื่องพร้อมพิมพ์แล้ว เพราะการกดที่จอไม่ได้ยืนยันเรื่องนี้เหมือนการเดินไปกดปุ่มหน้าเครื่อง";
        lblRemoteSendHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // StationOptionsUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpOptionsRoot);
        Name = "StationOptionsUserControl";
        Padding = new System.Windows.Forms.Padding(32);
        Size = new System.Drawing.Size(1280, 1040);
        tlpOptionsRoot.ResumeLayout(false);
        tlpReset.ResumeLayout(false);
        flpProcessTabs.ResumeLayout(false);
        tlpProcessTabs.ResumeLayout(false);
        pnlProcessTabs.ResumeLayout(false);
        tlpMockup.ResumeLayout(false);
        pnlMockup.ResumeLayout(false);
        pnlReset.ResumeLayout(false);
        pnlRemoteSend.ResumeLayout(false);
        tlpRemoteSend.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOptionsRoot;
    private AntdUI.Label lblOptionsTitle;
    private AntdUI.Panel pnlRemoteSend;
    private AntdUI.Label lblRemoteSendHeading;
    private AntdUI.Panel pnlProcessTabs;
    private System.Windows.Forms.TableLayoutPanel tlpProcessTabs;
    private AntdUI.Label lblProcessTabsHeading;
    private System.Windows.Forms.FlowLayoutPanel flpProcessTabs;
    private AntdUI.Radio rdoProcessTabsStations;
    private AntdUI.Radio rdoProcessTabsDev;
    private AntdUI.Radio rdoProcessTabsOff;
    private AntdUI.Label lblProcessTabsHelp;
    private AntdUI.Panel pnlMockup;
    private System.Windows.Forms.TableLayoutPanel tlpMockup;
    private AntdUI.Label lblMockupHeading;
    private AntdUI.Checkbox chkMockupStatus;
    private AntdUI.Label lblMockupHelp;
    private AntdUI.Panel pnlReset;
    private System.Windows.Forms.TableLayoutPanel tlpReset;
    private AntdUI.Label lblResetHeading;
    private AntdUI.Button btnResetRuntime;
    private AntdUI.Label lblResetHelp;
    private System.Windows.Forms.TableLayoutPanel tlpRemoteSend;
    private AntdUI.Checkbox chkManualRemoteSend;
    private AntdUI.Label lblRemoteSendHelp;
}
