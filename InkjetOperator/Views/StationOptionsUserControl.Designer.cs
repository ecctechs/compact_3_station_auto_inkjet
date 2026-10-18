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
        pnlAllOrNothing = new AntdUI.Panel();
        tlpAllOrNothing = new System.Windows.Forms.TableLayoutPanel();
        lblAllOrNothingHeading = new AntdUI.Label();
        chkAllOrNothing = new AntdUI.Checkbox();
        lblAllOrNothingHelp = new AntdUI.Label();
        pnlUv2Pick = new AntdUI.Panel();
        pnlUvRepeat = new AntdUI.Panel();
        tlpUvRepeat = new System.Windows.Forms.TableLayoutPanel();
        lblUvRepeatHeading = new AntdUI.Label();
        chkUvRepeat = new AntdUI.Checkbox();
        lblUvRepeatHelp = new AntdUI.Label();
        pnlScanTab = new AntdUI.Panel();
        tlpScanTab = new System.Windows.Forms.TableLayoutPanel();
        lblScanTabHeading = new AntdUI.Label();
        chkScanTab = new AntdUI.Checkbox();
        lblScanTabHelp = new AntdUI.Label();
        tlpUv2Pick = new System.Windows.Forms.TableLayoutPanel();
        lblUv2PickHeading = new AntdUI.Label();
        chkUv2Pick = new AntdUI.Checkbox();
        lblUv2PickHelp = new AntdUI.Label();
        pnlReset = new AntdUI.Panel();
        pnlJobData = new AntdUI.Panel();
        tlpJobData = new System.Windows.Forms.TableLayoutPanel();
        lblJobDataHeading = new AntdUI.Label();
        lblJobDataHelp = new AntdUI.Label();
        flpJobData = new System.Windows.Forms.FlowLayoutPanel();
        btnSaveJobData = new AntdUI.Button();
        btnImportJobData = new AntdUI.Button();
        btnClearJobData = new AntdUI.Button();
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
        pnlAllOrNothing.SuspendLayout();
        tlpAllOrNothing.SuspendLayout();
        pnlUv2Pick.SuspendLayout();
        pnlUvRepeat.SuspendLayout();
        tlpUvRepeat.SuspendLayout();
        pnlScanTab.SuspendLayout();
        tlpScanTab.SuspendLayout();
        tlpUv2Pick.SuspendLayout();
        pnlReset.SuspendLayout();
        tlpReset.SuspendLayout();
        pnlJobData.SuspendLayout();
        tlpJobData.SuspendLayout();
        flpJobData.SuspendLayout();
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
        tlpOptionsRoot.Controls.Add(pnlAllOrNothing, 0, 4);
        tlpOptionsRoot.Controls.Add(pnlUv2Pick, 0, 5);
        tlpOptionsRoot.Controls.Add(pnlUvRepeat, 0, 6);
        tlpOptionsRoot.Controls.Add(pnlScanTab, 0, 7);
        tlpOptionsRoot.Controls.Add(pnlReset, 0, 8);
        tlpOptionsRoot.Controls.Add(pnlJobData, 0, 9);
        tlpOptionsRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOptionsRoot.Location = new System.Drawing.Point(32, 32);
        tlpOptionsRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpOptionsRoot.Name = "tlpOptionsRoot";
        tlpOptionsRoot.RowCount = 11;
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 232F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 286F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 246F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 202F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 202F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 214F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 262F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Size = new System.Drawing.Size(1216, 2174);
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
        // pnlAllOrNothing — ขอบน้ำเงิน: ตัวเลือกของการส่งงาน เปิดทิ้งไว้ใช้งานจริงได้
        //
        pnlAllOrNothing.Back = System.Drawing.Color.White;
        pnlAllOrNothing.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlAllOrNothing.BorderWidth = 2F;
        pnlAllOrNothing.Controls.Add(tlpAllOrNothing);
        pnlAllOrNothing.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlAllOrNothing.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlAllOrNothing.Name = "pnlAllOrNothing";
        pnlAllOrNothing.Padding = new System.Windows.Forms.Padding(24);
        pnlAllOrNothing.Radius = 10;
        pnlAllOrNothing.Size = new System.Drawing.Size(1216, 270);
        pnlAllOrNothing.TabIndex = 4;
        //
        // tlpAllOrNothing
        //
        tlpAllOrNothing.ColumnCount = 1;
        tlpAllOrNothing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAllOrNothing.Controls.Add(lblAllOrNothingHeading, 0, 0);
        tlpAllOrNothing.Controls.Add(chkAllOrNothing, 0, 1);
        tlpAllOrNothing.Controls.Add(lblAllOrNothingHelp, 0, 2);
        tlpAllOrNothing.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpAllOrNothing.Margin = new System.Windows.Forms.Padding(0);
        tlpAllOrNothing.Name = "tlpAllOrNothing";
        tlpAllOrNothing.RowCount = 3;
        tlpAllOrNothing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpAllOrNothing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpAllOrNothing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAllOrNothing.Size = new System.Drawing.Size(1168, 222);
        tlpAllOrNothing.TabIndex = 0;
        //
        // lblAllOrNothingHeading
        //
        lblAllOrNothingHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAllOrNothingHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblAllOrNothingHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblAllOrNothingHeading.Margin = new System.Windows.Forms.Padding(0);
        lblAllOrNothingHeading.Name = "lblAllOrNothingHeading";
        lblAllOrNothingHeading.Size = new System.Drawing.Size(1168, 42);
        lblAllOrNothingHeading.TabIndex = 0;
        lblAllOrNothingHeading.Text = "ส่งไม่ครบ ไม่ส่งเลย (งานที่ใช้หลายเครื่อง)";
        //
        // chkAllOrNothing
        //
        chkAllOrNothing.Dock = System.Windows.Forms.DockStyle.Fill;
        chkAllOrNothing.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkAllOrNothing.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkAllOrNothing.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkAllOrNothing.Name = "chkAllOrNothing";
        chkAllOrNothing.Size = new System.Drawing.Size(1168, 32);
        chkAllOrNothing.TabIndex = 1;
        chkAllOrNothing.Text = "เครื่องไหนส่งไม่ผ่าน ตีทั้งงานกลับเป็นรอเริ่ม ไม่ค้างคิว";
        //
        // lblAllOrNothingHelp
        //
        lblAllOrNothingHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAllOrNothingHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblAllOrNothingHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblAllOrNothingHelp.Margin = new System.Windows.Forms.Padding(0);
        lblAllOrNothingHelp.Name = "lblAllOrNothingHelp";
        lblAllOrNothingHelp.Size = new System.Drawing.Size(1168, 136);
        lblAllOrNothingHelp.TabIndex = 2;
        lblAllOrNothingHelp.Text = "ใช้กับงานที่ส่งหลายเครื่อง (MK1 + MK2, UV1 + UV2, MK + UV2) — เครื่องไหนต่อไม่ติดหรือส่งไม่ผ่าน (รวมส่งไปแล้วไม่ได้คำตอบ) ไม่ส่งเครื่องไหนต่อ ล้างคิวของงาน ตีงานกลับเป็นรอเริ่ม แล้วบอกชื่อเครื่องที่ต้องไปตรวจ\r\nไม่มีคิวค้าง \"รอตรวจผล\" อีก คิวที่ค้างอยู่แล้ว ปลดได้ด้วยปุ่มหน้าเครื่อง หรือปุ่มยกเลิก / จบงาน · เครื่องไม่ว่างยังเข้าคิวรอเหมือนเดิม\r\nข้อแลก: เครื่องที่รับงานไปก่อนอีกเครื่องจะล้ม ได้ข้อมูลไปแล้วจริง ระบบเตือนชื่อเครื่องนั้นให้ กดเริ่มงานใหม่จะส่งทับ\r\nต้องเปิดที่เครื่อง ST1 (ST1 เป็นคนส่งทุกงาน รวมงานที่กดจาก ST3) และต้องใช้ backend รุ่นที่รองรับ";
        //
        // pnlUv2Pick — ขอบน้ำเงิน: ตัวเลือกของการส่งงาน เปิดทิ้งไว้ใช้งานจริงได้
        //
        pnlUv2Pick.Back = System.Drawing.Color.White;
        pnlUv2Pick.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlUv2Pick.BorderWidth = 2F;
        pnlUv2Pick.Controls.Add(tlpUv2Pick);
        pnlUv2Pick.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlUv2Pick.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlUv2Pick.Name = "pnlUv2Pick";
        pnlUv2Pick.Padding = new System.Windows.Forms.Padding(24);
        pnlUv2Pick.Radius = 10;
        pnlUv2Pick.Size = new System.Drawing.Size(1216, 230);
        pnlUv2Pick.TabIndex = 5;
        //
        // tlpUv2Pick
        //
        tlpUv2Pick.ColumnCount = 1;
        tlpUv2Pick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2Pick.Controls.Add(lblUv2PickHeading, 0, 0);
        tlpUv2Pick.Controls.Add(chkUv2Pick, 0, 1);
        tlpUv2Pick.Controls.Add(lblUv2PickHelp, 0, 2);
        tlpUv2Pick.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUv2Pick.Margin = new System.Windows.Forms.Padding(0);
        tlpUv2Pick.Name = "tlpUv2Pick";
        tlpUv2Pick.RowCount = 3;
        tlpUv2Pick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpUv2Pick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUv2Pick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2Pick.Size = new System.Drawing.Size(1168, 182);
        tlpUv2Pick.TabIndex = 0;
        //
        // lblUv2PickHeading
        //
        lblUv2PickHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2PickHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblUv2PickHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblUv2PickHeading.Margin = new System.Windows.Forms.Padding(0);
        lblUv2PickHeading.Name = "lblUv2PickHeading";
        lblUv2PickHeading.Size = new System.Drawing.Size(1168, 42);
        lblUv2PickHeading.TabIndex = 0;
        lblUv2PickHeading.Text = "รุ่นย่อย UV2 ให้ ST3 เลือก (งาน 11 / 12)";
        //
        // chkUv2Pick
        //
        chkUv2Pick.Dock = System.Windows.Forms.DockStyle.Fill;
        chkUv2Pick.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkUv2Pick.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkUv2Pick.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkUv2Pick.Name = "chkUv2Pick";
        chkUv2Pick.Size = new System.Drawing.Size(1168, 32);
        chkUv2Pick.TabIndex = 1;
        chkUv2Pick.Text = "ST1 ไม่ถามรุ่นย่อย UV2 — คนที่ ST3 กด เลือกรุ่น UV2 ในแถวงานเอง";
        //
        // lblUv2PickHelp
        //
        lblUv2PickHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2PickHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv2PickHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblUv2PickHelp.Margin = new System.Windows.Forms.Padding(0);
        lblUv2PickHelp.Name = "lblUv2PickHelp";
        lblUv2PickHelp.Size = new System.Drawing.Size(1168, 96);
        lblUv2PickHelp.TabIndex = 2;
        lblUv2PickHelp.Text = "ปิด (ค่าเริ่มต้น): ST1 เลือกรุ่นทุกเครื่องตอนกดเริ่มงาน เหมือนเดิม\r\nเปิด: โปรแกรม UV2 ที่มีให้เลือกหลายไฟล์ ST1 ส่งเครื่องอื่นตามปกติ แต่ยังไม่จอง UV2 · แถวงานขึ้น Pick UV2 (ST3)\r\nคนที่ ST3 กด เลือกรุ่น UV2 ตอนชิ้นงานมาถึง เลือกแล้ว ST1 ส่งให้ตามคิว · ไฟล์เดียวส่งจาก ST1 ทันทีเหมือนเดิม\r\nต้องเปิดที่เครื่อง ST1 (คนกดเริ่มงาน) · เครื่อง ST3 ต้องตั้งโฟลเดอร์โปรแกรม UV2 ไว้";
        //
        // pnlUvRepeat
        //
        pnlUvRepeat.Back = System.Drawing.Color.White;
        pnlUvRepeat.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlUvRepeat.BorderWidth = 2F;
        pnlUvRepeat.Controls.Add(tlpUvRepeat);
        pnlUvRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlUvRepeat.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
        pnlUvRepeat.Name = "pnlUvRepeat";
        pnlUvRepeat.Padding = new System.Windows.Forms.Padding(24, 18, 24, 18);
        pnlUvRepeat.Radius = 10;
        pnlUvRepeat.TabIndex = 5;
        //
        // tlpUvRepeat
        //
        tlpUvRepeat.ColumnCount = 1;
        tlpUvRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUvRepeat.Controls.Add(lblUvRepeatHeading, 0, 0);
        tlpUvRepeat.Controls.Add(chkUvRepeat, 0, 1);
        tlpUvRepeat.Controls.Add(lblUvRepeatHelp, 0, 2);
        tlpUvRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUvRepeat.Margin = new System.Windows.Forms.Padding(0);
        tlpUvRepeat.Name = "tlpUvRepeat";
        tlpUvRepeat.RowCount = 3;
        tlpUvRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpUvRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpUvRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUvRepeat.Size = new System.Drawing.Size(1168, 150);
        tlpUvRepeat.TabIndex = 0;
        //
        // lblUvRepeatHeading
        //
        lblUvRepeatHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUvRepeatHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblUvRepeatHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblUvRepeatHeading.Margin = new System.Windows.Forms.Padding(0);
        lblUvRepeatHeading.Name = "lblUvRepeatHeading";
        lblUvRepeatHeading.Size = new System.Drawing.Size(1168, 42);
        lblUvRepeatHeading.TabIndex = 0;
        lblUvRepeatHeading.Text = "พิมพ์รุ่นย่อย UV เพิ่มให้งานใบเดิม";
        //
        // chkUvRepeat
        //
        chkUvRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
        chkUvRepeat.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkUvRepeat.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkUvRepeat.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkUvRepeat.Name = "chkUvRepeat";
        chkUvRepeat.Size = new System.Drawing.Size(1168, 32);
        chkUvRepeat.TabIndex = 1;
        chkUvRepeat.Text = "เปิดปุ่ม เลือกรุ่น UV1 / UV2 ในแถวงาน หลังพิมพ์รุ่นก่อนหน้าเสร็จ";
        //
        // lblUvRepeatHelp
        //
        lblUvRepeatHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUvRepeatHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUvRepeatHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblUvRepeatHelp.Margin = new System.Windows.Forms.Padding(0);
        lblUvRepeatHelp.Name = "lblUvRepeatHelp";
        lblUvRepeatHelp.Size = new System.Drawing.Size(1168, 64);
        lblUvRepeatHelp.TabIndex = 2;
        lblUvRepeatHelp.Text = "ปิด (ค่าเริ่มต้น): เลือกรุ่นย่อยครั้งเดียวตอนกดเริ่มงาน เหมือนเดิม\r\nเปิด: พิมพ์เสร็จและปล่อยเครื่องแล้ว ปุ่มเลือกรุ่นโผล่ในแถว เลือกได้เฉพาะรุ่นที่ยังไม่ได้พิมพ์ ครบทุกรุ่นแล้วปุ่มหาย\r\n\r\nปุ่ม UV1 ขึ้นที่ ST1 · ปุ่ม UV2 ขึ้นที่ ST3 · ปุ่มจบงานยังเป็นสีส้มตราบที่ยังเหลือรุ่น และกดจบได้ตลอดไม่ต้องพิมพ์ครบ\r\n";
        //
        // pnlScanTab
        //
        pnlScanTab.Back = System.Drawing.Color.White;
        pnlScanTab.BorderColor = System.Drawing.Color.FromArgb(36, 71, 101);
        pnlScanTab.BorderWidth = 2F;
        pnlScanTab.Controls.Add(tlpScanTab);
        pnlScanTab.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlScanTab.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
        pnlScanTab.Name = "pnlScanTab";
        pnlScanTab.Padding = new System.Windows.Forms.Padding(24, 18, 24, 18);
        pnlScanTab.Radius = 10;
        pnlScanTab.TabIndex = 6;
        //
        // tlpScanTab
        //
        tlpScanTab.ColumnCount = 1;
        tlpScanTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpScanTab.Controls.Add(lblScanTabHeading, 0, 0);
        tlpScanTab.Controls.Add(chkScanTab, 0, 1);
        tlpScanTab.Controls.Add(lblScanTabHelp, 0, 2);
        tlpScanTab.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpScanTab.Margin = new System.Windows.Forms.Padding(0);
        tlpScanTab.Name = "tlpScanTab";
        tlpScanTab.RowCount = 3;
        tlpScanTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpScanTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        tlpScanTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpScanTab.Size = new System.Drawing.Size(1168, 150);
        tlpScanTab.TabIndex = 0;
        //
        // lblScanTabHeading
        //
        lblScanTabHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblScanTabHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblScanTabHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblScanTabHeading.Margin = new System.Windows.Forms.Padding(0);
        lblScanTabHeading.Name = "lblScanTabHeading";
        lblScanTabHeading.Size = new System.Drawing.Size(1168, 42);
        lblScanTabHeading.TabIndex = 0;
        lblScanTabHeading.Text = "หน้า Scan Barcode ที่ ST1";
        //
        // chkScanTab
        //
        chkScanTab.Dock = System.Windows.Forms.DockStyle.Fill;
        chkScanTab.Font = new System.Drawing.Font("Segoe UI", 15F);
        chkScanTab.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        chkScanTab.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        chkScanTab.Name = "chkScanTab";
        chkScanTab.Size = new System.Drawing.Size(1168, 32);
        chkScanTab.TabIndex = 1;
        chkScanTab.Text = "เพิ่มแท็บ Scan Barcode ให้เครื่อง ST1";
        //
        // lblScanTabHelp
        //
        lblScanTabHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblScanTabHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblScanTabHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblScanTabHelp.Margin = new System.Windows.Forms.Padding(0);
        lblScanTabHelp.Name = "lblScanTabHelp";
        lblScanTabHelp.Size = new System.Drawing.Size(1168, 64);
        lblScanTabHelp.TabIndex = 2;
        lblScanTabHelp.Text = "ปิด (ค่าเริ่มต้น): ST1 เห็นแท็บ Order List / Setting เหมือนเดิม หน้า Scan Barcode มีเฉพาะสถานีสแกน\r\n\r\nเปิด: ST1 มีแท็บ Scan Barcode เพิ่มมาด้วย · ตั้งที่เครื่อง ST1 เอง เห็นผลหลังเปิดโปรแกรมใหม่";
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
        pnlReset.TabIndex = 6;
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
        // pnlJobData — ขอบแดง: ล้าง / นำเข้า แทนที่ข้อมูลงานทั้งระบบ ย้อนกลับไม่ได้
        //
        pnlJobData.Back = System.Drawing.Color.White;
        pnlJobData.BorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        pnlJobData.BorderWidth = 2F;
        pnlJobData.Controls.Add(tlpJobData);
        pnlJobData.Dock = System.Windows.Forms.DockStyle.Fill;
        pnlJobData.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
        pnlJobData.Name = "pnlJobData";
        pnlJobData.Padding = new System.Windows.Forms.Padding(24);
        pnlJobData.Radius = 10;
        pnlJobData.Size = new System.Drawing.Size(1216, 246);
        pnlJobData.TabIndex = 7;
        //
        // tlpJobData
        //
        tlpJobData.ColumnCount = 1;
        tlpJobData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpJobData.Controls.Add(lblJobDataHeading, 0, 0);
        tlpJobData.Controls.Add(lblJobDataHelp, 0, 1);
        tlpJobData.Controls.Add(flpJobData, 0, 2);
        tlpJobData.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpJobData.Margin = new System.Windows.Forms.Padding(0);
        tlpJobData.Name = "tlpJobData";
        tlpJobData.RowCount = 3;
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        tlpJobData.Size = new System.Drawing.Size(1168, 198);
        tlpJobData.TabIndex = 0;
        //
        // lblJobDataHeading
        //
        lblJobDataHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblJobDataHeading.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
        lblJobDataHeading.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblJobDataHeading.Margin = new System.Windows.Forms.Padding(0);
        lblJobDataHeading.Name = "lblJobDataHeading";
        lblJobDataHeading.Size = new System.Drawing.Size(1168, 42);
        lblJobDataHeading.TabIndex = 0;
        lblJobDataHeading.Text = "ชุดข้อมูลงานทดสอบ (บันทึก / นำเข้า / ล้าง)";
        //
        // lblJobDataHelp
        //
        lblJobDataHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblJobDataHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblJobDataHelp.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
        lblJobDataHelp.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblJobDataHelp.Name = "lblJobDataHelp";
        lblJobDataHelp.Size = new System.Drawing.Size(1168, 88);
        lblJobDataHelp.TabIndex = 1;
        lblJobDataHelp.Text = "บันทึก: เก็บข้อมูลงานทั้งหมดตอนนี้ (ตัวงาน pattern ข้อความ MK ข้อมูล UV แผนการผลิต ค่าแคลมป์) เป็นไฟล์ .json ใน C:\\ProgramData\\CompactInkjet\\test-data — คัดลอกไฟล์ไปเครื่องอื่นได้\r\nนำเข้า: เลือกไฟล์ แทนที่ข้อมูลงานทั้งหมดในฐาน · ทุกใบกลับเป็นรอเริ่ม ไม่มีคิว · id ตามไฟล์\r\nล้าง: ลบข้อมูลงานทั้งหมด id เริ่มใหม่ที่ 1 · ค่าตั้ง (register map PLC, Setting) ไม่ถูกแตะ\r\nมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน และย้อนกลับไม่ได้ — กดบันทึกไว้ก่อนล้างหรือนำเข้าเสมอ";
        //
        // flpJobData
        //
        flpJobData.AutoSize = true;
        flpJobData.Controls.Add(btnSaveJobData);
        flpJobData.Controls.Add(btnImportJobData);
        flpJobData.Controls.Add(btnClearJobData);
        flpJobData.Dock = System.Windows.Forms.DockStyle.Fill;
        flpJobData.Margin = new System.Windows.Forms.Padding(0);
        flpJobData.Name = "flpJobData";
        flpJobData.Size = new System.Drawing.Size(1168, 52);
        flpJobData.TabIndex = 2;
        flpJobData.WrapContents = false;
        //
        // btnSaveJobData
        //
        btnSaveJobData.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnSaveJobData.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
        btnSaveJobData.Name = "btnSaveJobData";
        btnSaveJobData.Radius = 8;
        btnSaveJobData.Size = new System.Drawing.Size(300, 52);
        btnSaveJobData.TabIndex = 0;
        btnSaveJobData.Text = "บันทึกข้อมูลงานตอนนี้";
        btnSaveJobData.Type = AntdUI.TTypeMini.Primary;
        //
        // btnImportJobData
        //
        btnImportJobData.DefaultBorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnImportJobData.BorderWidth = 2F;
        btnImportJobData.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnImportJobData.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnImportJobData.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
        btnImportJobData.Name = "btnImportJobData";
        btnImportJobData.Radius = 8;
        btnImportJobData.Size = new System.Drawing.Size(300, 52);
        btnImportJobData.TabIndex = 1;
        btnImportJobData.Text = "นำเข้าจากไฟล์...";
        //
        // btnClearJobData
        //
        btnClearJobData.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        btnClearJobData.ForeColor = System.Drawing.Color.White;
        btnClearJobData.Margin = new System.Windows.Forms.Padding(0);
        btnClearJobData.Name = "btnClearJobData";
        btnClearJobData.Radius = 8;
        btnClearJobData.Size = new System.Drawing.Size(300, 52);
        btnClearJobData.TabIndex = 2;
        btnClearJobData.Text = "ล้างข้อมูลงานทั้งหมด";
        btnClearJobData.Type = AntdUI.TTypeMini.Error;
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
        Size = new System.Drawing.Size(1280, 2238);
        tlpOptionsRoot.ResumeLayout(false);
        tlpReset.ResumeLayout(false);
        flpJobData.ResumeLayout(false);
        tlpJobData.ResumeLayout(false);
        tlpJobData.PerformLayout();
        pnlJobData.ResumeLayout(false);
        pnlJobData.PerformLayout();
        flpProcessTabs.ResumeLayout(false);
        tlpProcessTabs.ResumeLayout(false);
        pnlProcessTabs.ResumeLayout(false);
        tlpMockup.ResumeLayout(false);
        pnlMockup.ResumeLayout(false);
        tlpAllOrNothing.ResumeLayout(false);
        pnlAllOrNothing.ResumeLayout(false);
        tlpUv2Pick.ResumeLayout(false);
        pnlUvRepeat.ResumeLayout(false);
        tlpUvRepeat.ResumeLayout(false);
        pnlScanTab.ResumeLayout(false);
        tlpScanTab.ResumeLayout(false);
        pnlUv2Pick.ResumeLayout(false);
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
    private AntdUI.Panel pnlAllOrNothing;
    private System.Windows.Forms.TableLayoutPanel tlpAllOrNothing;
    private AntdUI.Label lblAllOrNothingHeading;
    private AntdUI.Checkbox chkAllOrNothing;
    private AntdUI.Label lblAllOrNothingHelp;
    private AntdUI.Panel pnlUv2Pick;
    private System.Windows.Forms.TableLayoutPanel tlpUv2Pick;
    private AntdUI.Label lblUv2PickHeading;
    private AntdUI.Checkbox chkUv2Pick;
    private AntdUI.Label lblUv2PickHelp;
    private AntdUI.Panel pnlUvRepeat;
    private System.Windows.Forms.TableLayoutPanel tlpUvRepeat;
    private AntdUI.Label lblUvRepeatHeading;
    private AntdUI.Checkbox chkUvRepeat;
    private AntdUI.Label lblUvRepeatHelp;
    private AntdUI.Panel pnlScanTab;
    private System.Windows.Forms.TableLayoutPanel tlpScanTab;
    private AntdUI.Label lblScanTabHeading;
    private AntdUI.Checkbox chkScanTab;
    private AntdUI.Label lblScanTabHelp;
    private AntdUI.Panel pnlReset;
    private System.Windows.Forms.TableLayoutPanel tlpReset;
    private AntdUI.Label lblResetHeading;
    private AntdUI.Button btnResetRuntime;
    private AntdUI.Label lblResetHelp;
    private AntdUI.Panel pnlJobData;
    private System.Windows.Forms.TableLayoutPanel tlpJobData;
    private AntdUI.Label lblJobDataHeading;
    private AntdUI.Label lblJobDataHelp;
    private System.Windows.Forms.FlowLayoutPanel flpJobData;
    private AntdUI.Button btnSaveJobData;
    private AntdUI.Button btnImportJobData;
    private AntdUI.Button btnClearJobData;
    private System.Windows.Forms.TableLayoutPanel tlpRemoteSend;
    private AntdUI.Checkbox chkManualRemoteSend;
    private AntdUI.Label lblRemoteSendHelp;
}
