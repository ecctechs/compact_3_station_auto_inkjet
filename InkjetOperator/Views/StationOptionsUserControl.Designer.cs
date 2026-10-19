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
        lblSectionOptions = new AntdUI.Label();
        lblSectionTools = new AntdUI.Label();
        divRow1 = new AntdUI.Divider();
        divRow2 = new AntdUI.Divider();
        divRow3 = new AntdUI.Divider();
        divRow4 = new AntdUI.Divider();
        divRow5 = new AntdUI.Divider();
        divRow6 = new AntdUI.Divider();
        divRow7 = new AntdUI.Divider();
        divRow8 = new AntdUI.Divider();
        divRow9 = new AntdUI.Divider();
        divRow10 = new AntdUI.Divider();
        divRow11 = new AntdUI.Divider();
        tlpRemoteSend = new System.Windows.Forms.TableLayoutPanel();
        lblRemoteSendHeading = new AntdUI.Label();
        lblRemoteSendHelp = new AntdUI.Label();
        chkManualRemoteSend = new AntdUI.Switch();
        tlpProcessTabs = new System.Windows.Forms.TableLayoutPanel();
        lblProcessTabsHeading = new AntdUI.Label();
        lblProcessTabsHelp = new AntdUI.Label();
        flpProcessTabs = new System.Windows.Forms.FlowLayoutPanel();
        rdoProcessTabsStations = new AntdUI.Radio();
        rdoProcessTabsDev = new AntdUI.Radio();
        rdoProcessTabsOff = new AntdUI.Radio();
        tlpMockup = new System.Windows.Forms.TableLayoutPanel();
        lblMockupHeading = new AntdUI.Label();
        lblMockupHelp = new AntdUI.Label();
        chkMockupStatus = new AntdUI.Switch();
        tlpAllOrNothing = new System.Windows.Forms.TableLayoutPanel();
        lblAllOrNothingHeading = new AntdUI.Label();
        lblAllOrNothingHelp = new AntdUI.Label();
        chkAllOrNothing = new AntdUI.Switch();
        tlpUv2Pick = new System.Windows.Forms.TableLayoutPanel();
        lblUv2PickHeading = new AntdUI.Label();
        lblUv2PickHelp = new AntdUI.Label();
        chkUv2Pick = new AntdUI.Switch();
        tlpUvRepeat = new System.Windows.Forms.TableLayoutPanel();
        lblUvRepeatHeading = new AntdUI.Label();
        lblUvRepeatHelp = new AntdUI.Label();
        chkUvRepeat = new AntdUI.Switch();
        tlpScanTab = new System.Windows.Forms.TableLayoutPanel();
        lblScanTabHeading = new AntdUI.Label();
        lblScanTabHelp = new AntdUI.Label();
        chkScanTab = new AntdUI.Switch();
        tlpReset = new System.Windows.Forms.TableLayoutPanel();
        lblResetHeading = new AntdUI.Label();
        lblResetHelp = new AntdUI.Label();
        btnResetRuntime = new AntdUI.Button();
        tlpJobData = new System.Windows.Forms.TableLayoutPanel();
        lblJobDataHeading = new AntdUI.Label();
        lblJobDataHelp = new AntdUI.Label();
        flpJobData = new System.Windows.Forms.FlowLayoutPanel();
        btnSaveJobData = new AntdUI.Button();
        btnImportJobData = new AntdUI.Button();
        btnClearJobData = new AntdUI.Button();
        tlpOptionsRoot.SuspendLayout();
        tlpRemoteSend.SuspendLayout();
        tlpProcessTabs.SuspendLayout();
        flpProcessTabs.SuspendLayout();
        tlpMockup.SuspendLayout();
        tlpAllOrNothing.SuspendLayout();
        tlpUv2Pick.SuspendLayout();
        tlpUvRepeat.SuspendLayout();
        tlpScanTab.SuspendLayout();
        tlpReset.SuspendLayout();
        tlpJobData.SuspendLayout();
        flpJobData.SuspendLayout();
        SuspendLayout();
        //
        // tlpOptionsRoot
        //
        tlpOptionsRoot.ColumnCount = 1;
        tlpOptionsRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Controls.Add(lblOptionsTitle, 0, 0);
        tlpOptionsRoot.Controls.Add(lblSectionOptions, 0, 1);
        tlpOptionsRoot.Controls.Add(divRow1, 0, 2);
        tlpOptionsRoot.Controls.Add(tlpRemoteSend, 0, 3);
        tlpOptionsRoot.Controls.Add(divRow2, 0, 4);
        tlpOptionsRoot.Controls.Add(tlpProcessTabs, 0, 5);
        tlpOptionsRoot.Controls.Add(divRow3, 0, 6);
        tlpOptionsRoot.Controls.Add(tlpMockup, 0, 7);
        tlpOptionsRoot.Controls.Add(divRow4, 0, 8);
        tlpOptionsRoot.Controls.Add(tlpAllOrNothing, 0, 9);
        tlpOptionsRoot.Controls.Add(divRow5, 0, 10);
        tlpOptionsRoot.Controls.Add(tlpUv2Pick, 0, 11);
        tlpOptionsRoot.Controls.Add(divRow6, 0, 12);
        tlpOptionsRoot.Controls.Add(tlpUvRepeat, 0, 13);
        tlpOptionsRoot.Controls.Add(divRow7, 0, 14);
        tlpOptionsRoot.Controls.Add(tlpScanTab, 0, 15);
        tlpOptionsRoot.Controls.Add(divRow8, 0, 16);
        tlpOptionsRoot.Controls.Add(lblSectionTools, 0, 17);
        tlpOptionsRoot.Controls.Add(divRow9, 0, 18);
        tlpOptionsRoot.Controls.Add(tlpReset, 0, 19);
        tlpOptionsRoot.Controls.Add(divRow10, 0, 20);
        tlpOptionsRoot.Controls.Add(tlpJobData, 0, 21);
        tlpOptionsRoot.Controls.Add(divRow11, 0, 22);
        tlpOptionsRoot.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpOptionsRoot.Location = new System.Drawing.Point(32, 32);
        tlpOptionsRoot.Margin = new System.Windows.Forms.Padding(0);
        tlpOptionsRoot.Name = "tlpOptionsRoot";
        tlpOptionsRoot.RowCount = 24;
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 184F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 184F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
        tlpOptionsRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpOptionsRoot.Size = new System.Drawing.Size(1216, 1465);
        tlpOptionsRoot.TabIndex = 0;
        //
        // lblOptionsTitle
        //
        lblOptionsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOptionsTitle.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
        lblOptionsTitle.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblOptionsTitle.Margin = new System.Windows.Forms.Padding(0);
        lblOptionsTitle.Name = "lblOptionsTitle";
        lblOptionsTitle.Size = new System.Drawing.Size(1216, 66);
        lblOptionsTitle.TabIndex = 1;
        lblOptionsTitle.Text = "ตัวเลือกหน้างาน";
        //
        // lblSectionOptions
        //
        lblSectionOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        lblSectionOptions.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblSectionOptions.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
        lblSectionOptions.Margin = new System.Windows.Forms.Padding(0);
        lblSectionOptions.Name = "lblSectionOptions";
        lblSectionOptions.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
        lblSectionOptions.Size = new System.Drawing.Size(1216, 48);
        lblSectionOptions.TabIndex = 2;
        lblSectionOptions.Text = "ตั้งค่าการทำงาน · เซฟทันทีที่กด";
        lblSectionOptions.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // tlpRemoteSend
        //
        tlpRemoteSend.ColumnCount = 2;
        tlpRemoteSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpRemoteSend.Controls.Add(lblRemoteSendHeading, 0, 0);
        tlpRemoteSend.Controls.Add(lblRemoteSendHelp, 0, 1);
        tlpRemoteSend.Controls.Add(chkManualRemoteSend, 1, 0);
        tlpRemoteSend.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        tlpRemoteSend.Name = "tlpRemoteSend";
        tlpRemoteSend.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpRemoteSend.RowCount = 2;
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpRemoteSend.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpRemoteSend.SetRowSpan(chkManualRemoteSend, 2);
        tlpRemoteSend.Size = new System.Drawing.Size(1216, 116);
        tlpRemoteSend.TabIndex = 3;
        //
        // lblRemoteSendHeading
        //
        lblRemoteSendHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblRemoteSendHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblRemoteSendHeading.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHeading.Name = "lblRemoteSendHeading";
        lblRemoteSendHeading.Size = new System.Drawing.Size(1076, 36);
        lblRemoteSendHeading.TabIndex = 4;
        lblRemoteSendHeading.Text = "ปุ่ม \"ขอให้ ST1 ส่ง\" ในหน้า Order Detail (ST3)";
        //
        // lblRemoteSendHelp
        //
        lblRemoteSendHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblRemoteSendHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblRemoteSendHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblRemoteSendHelp.Margin = new System.Windows.Forms.Padding(0);
        lblRemoteSendHelp.Name = "lblRemoteSendHelp";
        lblRemoteSendHelp.Size = new System.Drawing.Size(1076, 52);
        lblRemoteSendHelp.TabIndex = 5;
        lblRemoteSendHelp.Text = "ปุ่มสำรองตอนปุ่มกดหน้างานหรือ PLC ใช้ไม่ได้ ขึ้นเฉพาะงานที่ส่งขั้นแรกไปแล้วและยังเหลือขั้นถัดไป\r\nกดเมื่อชิ้นงานอยู่ในเครื่องพร้อมพิมพ์แล้วเท่านั้น";
        lblRemoteSendHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkManualRemoteSend
        //
        chkManualRemoteSend.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkManualRemoteSend.CheckedText = "เปิด";
        chkManualRemoteSend.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkManualRemoteSend.Margin = new System.Windows.Forms.Padding(0);
        chkManualRemoteSend.Name = "chkManualRemoteSend";
        chkManualRemoteSend.Size = new System.Drawing.Size(84, 36);
        chkManualRemoteSend.TabIndex = 6;
        chkManualRemoteSend.UnCheckedText = "ปิด";
        //
        // tlpProcessTabs
        //
        tlpProcessTabs.ColumnCount = 2;
        tlpProcessTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 340F));
        tlpProcessTabs.Controls.Add(lblProcessTabsHeading, 0, 0);
        tlpProcessTabs.Controls.Add(lblProcessTabsHelp, 0, 1);
        tlpProcessTabs.Controls.Add(flpProcessTabs, 1, 0);
        tlpProcessTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpProcessTabs.Margin = new System.Windows.Forms.Padding(0);
        tlpProcessTabs.Name = "tlpProcessTabs";
        tlpProcessTabs.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpProcessTabs.RowCount = 2;
        tlpProcessTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpProcessTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpProcessTabs.SetRowSpan(flpProcessTabs, 2);
        tlpProcessTabs.Size = new System.Drawing.Size(1216, 116);
        tlpProcessTabs.TabIndex = 7;
        //
        // lblProcessTabsHeading
        //
        lblProcessTabsHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcessTabsHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblProcessTabsHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblProcessTabsHeading.Margin = new System.Windows.Forms.Padding(0);
        lblProcessTabsHeading.Name = "lblProcessTabsHeading";
        lblProcessTabsHeading.Size = new System.Drawing.Size(876, 36);
        lblProcessTabsHeading.TabIndex = 8;
        lblProcessTabsHeading.Text = "ตัวกรอง In-line / Off-line";
        //
        // lblProcessTabsHelp
        //
        lblProcessTabsHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblProcessTabsHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblProcessTabsHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblProcessTabsHelp.Margin = new System.Windows.Forms.Padding(0);
        lblProcessTabsHelp.Name = "lblProcessTabsHelp";
        lblProcessTabsHelp.Size = new System.Drawing.Size(876, 52);
        lblProcessTabsHelp.TabIndex = 9;
        lblProcessTabsHelp.Text = "dropdown กรองงานตาม Process seq ในหน้า Order List\r\nเห็นผลภายในไม่กี่วินาที ไม่ต้องเปิดโปรแกรมใหม่";
        lblProcessTabsHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // flpProcessTabs
        //
        flpProcessTabs.Anchor = System.Windows.Forms.AnchorStyles.Right;
        flpProcessTabs.AutoSize = true;
        flpProcessTabs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        flpProcessTabs.Controls.Add(rdoProcessTabsStations);
        flpProcessTabs.Controls.Add(rdoProcessTabsDev);
        flpProcessTabs.Controls.Add(rdoProcessTabsOff);
        flpProcessTabs.Margin = new System.Windows.Forms.Padding(0);
        flpProcessTabs.Name = "flpProcessTabs";
        flpProcessTabs.Size = new System.Drawing.Size(330, 36);
        flpProcessTabs.TabIndex = 10;
        flpProcessTabs.WrapContents = false;
        //
        // rdoProcessTabsStations
        //
        rdoProcessTabsStations.Font = new System.Drawing.Font("Segoe UI", 13F);
        rdoProcessTabsStations.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsStations.Margin = new System.Windows.Forms.Padding(0);
        rdoProcessTabsStations.Name = "rdoProcessTabsStations";
        rdoProcessTabsStations.Size = new System.Drawing.Size(130, 36);
        rdoProcessTabsStations.TabIndex = 0;
        rdoProcessTabsStations.Text = "ST1 + ST3";
        //
        // rdoProcessTabsDev
        //
        rdoProcessTabsDev.Font = new System.Drawing.Font("Segoe UI", 13F);
        rdoProcessTabsDev.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsDev.Margin = new System.Windows.Forms.Padding(0);
        rdoProcessTabsDev.Name = "rdoProcessTabsDev";
        rdoProcessTabsDev.Size = new System.Drawing.Size(130, 36);
        rdoProcessTabsDev.TabIndex = 1;
        rdoProcessTabsDev.Text = "เฉพาะ Dev";
        //
        // rdoProcessTabsOff
        //
        rdoProcessTabsOff.Font = new System.Drawing.Font("Segoe UI", 13F);
        rdoProcessTabsOff.ForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
        rdoProcessTabsOff.Margin = new System.Windows.Forms.Padding(0);
        rdoProcessTabsOff.Name = "rdoProcessTabsOff";
        rdoProcessTabsOff.Size = new System.Drawing.Size(70, 36);
        rdoProcessTabsOff.TabIndex = 2;
        rdoProcessTabsOff.Text = "ปิด";
        //
        // tlpMockup
        //
        tlpMockup.ColumnCount = 2;
        tlpMockup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMockup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpMockup.Controls.Add(lblMockupHeading, 0, 0);
        tlpMockup.Controls.Add(lblMockupHelp, 0, 1);
        tlpMockup.Controls.Add(chkMockupStatus, 1, 0);
        tlpMockup.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpMockup.Margin = new System.Windows.Forms.Padding(0);
        tlpMockup.Name = "tlpMockup";
        tlpMockup.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpMockup.RowCount = 2;
        tlpMockup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpMockup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpMockup.SetRowSpan(chkMockupStatus, 2);
        tlpMockup.Size = new System.Drawing.Size(1216, 116);
        tlpMockup.TabIndex = 11;
        //
        // lblMockupHeading
        //
        lblMockupHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMockupHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblMockupHeading.ForeColor = System.Drawing.Color.FromArgb(217, 119, 6);
        lblMockupHeading.Margin = new System.Windows.Forms.Padding(0);
        lblMockupHeading.Name = "lblMockupHeading";
        lblMockupHeading.Size = new System.Drawing.Size(1076, 36);
        lblMockupHeading.TabIndex = 12;
        lblMockupHeading.Text = "Mockup สถานะเชื่อมต่อ (ถ่ายรูปคู่มือ)";
        //
        // lblMockupHelp
        //
        lblMockupHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblMockupHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblMockupHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblMockupHelp.Margin = new System.Windows.Forms.Padding(0);
        lblMockupHelp.Name = "lblMockupHelp";
        lblMockupHelp.Size = new System.Drawing.Size(1076, 52);
        lblMockupHelp.TabIndex = 13;
        lblMockupHelp.Text = "ทุกหน้าขึ้นว่าเชื่อมต่อได้หมดโดยไม่ต่อเครื่องจริง · ไม่แตะการส่งงาน PLC หรือเครื่องจริง\r\nถ่ายเสร็จต้องกลับมาปิด ห้ามเปิดค้างไว้ที่เครื่องหน้างาน";
        lblMockupHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkMockupStatus
        //
        chkMockupStatus.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkMockupStatus.CheckedText = "เปิด";
        chkMockupStatus.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkMockupStatus.Margin = new System.Windows.Forms.Padding(0);
        chkMockupStatus.Name = "chkMockupStatus";
        chkMockupStatus.Size = new System.Drawing.Size(84, 36);
        chkMockupStatus.TabIndex = 14;
        chkMockupStatus.UnCheckedText = "ปิด";
        //
        // tlpAllOrNothing
        //
        tlpAllOrNothing.ColumnCount = 2;
        tlpAllOrNothing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAllOrNothing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpAllOrNothing.Controls.Add(lblAllOrNothingHeading, 0, 0);
        tlpAllOrNothing.Controls.Add(lblAllOrNothingHelp, 0, 1);
        tlpAllOrNothing.Controls.Add(chkAllOrNothing, 1, 0);
        tlpAllOrNothing.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpAllOrNothing.Margin = new System.Windows.Forms.Padding(0);
        tlpAllOrNothing.Name = "tlpAllOrNothing";
        tlpAllOrNothing.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpAllOrNothing.RowCount = 2;
        tlpAllOrNothing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpAllOrNothing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpAllOrNothing.SetRowSpan(chkAllOrNothing, 2);
        tlpAllOrNothing.Size = new System.Drawing.Size(1216, 116);
        tlpAllOrNothing.TabIndex = 15;
        //
        // lblAllOrNothingHeading
        //
        lblAllOrNothingHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAllOrNothingHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblAllOrNothingHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblAllOrNothingHeading.Margin = new System.Windows.Forms.Padding(0);
        lblAllOrNothingHeading.Name = "lblAllOrNothingHeading";
        lblAllOrNothingHeading.Size = new System.Drawing.Size(1076, 36);
        lblAllOrNothingHeading.TabIndex = 16;
        lblAllOrNothingHeading.Text = "ส่งไม่ครบ ไม่ส่งเลย (งานที่ใช้หลายเครื่อง)";
        //
        // lblAllOrNothingHelp
        //
        lblAllOrNothingHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblAllOrNothingHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblAllOrNothingHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblAllOrNothingHelp.Margin = new System.Windows.Forms.Padding(0);
        lblAllOrNothingHelp.Name = "lblAllOrNothingHelp";
        lblAllOrNothingHelp.Size = new System.Drawing.Size(1076, 52);
        lblAllOrNothingHelp.TabIndex = 17;
        lblAllOrNothingHelp.Text = "เครื่องไหนส่งไม่ผ่าน ไม่ส่งเครื่องอื่นต่อ ล้างคิว ตีงานกลับเป็นรอเริ่ม แล้วบอกชื่อเครื่องที่ต้องตรวจ\r\nต้องเปิดที่เครื่อง ST1 · เครื่องที่รับงานไปก่อนแล้ว ระบบจะเตือนชื่อให้";
        lblAllOrNothingHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkAllOrNothing
        //
        chkAllOrNothing.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkAllOrNothing.CheckedText = "เปิด";
        chkAllOrNothing.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkAllOrNothing.Margin = new System.Windows.Forms.Padding(0);
        chkAllOrNothing.Name = "chkAllOrNothing";
        chkAllOrNothing.Size = new System.Drawing.Size(84, 36);
        chkAllOrNothing.TabIndex = 18;
        chkAllOrNothing.UnCheckedText = "ปิด";
        //
        // tlpUv2Pick
        //
        tlpUv2Pick.ColumnCount = 2;
        tlpUv2Pick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2Pick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpUv2Pick.Controls.Add(lblUv2PickHeading, 0, 0);
        tlpUv2Pick.Controls.Add(lblUv2PickHelp, 0, 1);
        tlpUv2Pick.Controls.Add(chkUv2Pick, 1, 0);
        tlpUv2Pick.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUv2Pick.Margin = new System.Windows.Forms.Padding(0);
        tlpUv2Pick.Name = "tlpUv2Pick";
        tlpUv2Pick.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpUv2Pick.RowCount = 2;
        tlpUv2Pick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpUv2Pick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUv2Pick.SetRowSpan(chkUv2Pick, 2);
        tlpUv2Pick.Size = new System.Drawing.Size(1216, 116);
        tlpUv2Pick.TabIndex = 19;
        //
        // lblUv2PickHeading
        //
        lblUv2PickHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2PickHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblUv2PickHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblUv2PickHeading.Margin = new System.Windows.Forms.Padding(0);
        lblUv2PickHeading.Name = "lblUv2PickHeading";
        lblUv2PickHeading.Size = new System.Drawing.Size(1076, 36);
        lblUv2PickHeading.TabIndex = 20;
        lblUv2PickHeading.Text = "รุ่นย่อย UV2 ให้ ST3 เลือก (งาน 11 / 12)";
        //
        // lblUv2PickHelp
        //
        lblUv2PickHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUv2PickHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUv2PickHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblUv2PickHelp.Margin = new System.Windows.Forms.Padding(0);
        lblUv2PickHelp.Name = "lblUv2PickHelp";
        lblUv2PickHelp.Size = new System.Drawing.Size(1076, 52);
        lblUv2PickHelp.TabIndex = 21;
        lblUv2PickHelp.Text = "ST1 ไม่ถามรุ่นย่อย UV2 · คนที่ ST3 กด เลือกรุ่น UV2 ในแถวงานตอนชิ้นงานมาถึง\r\nต้องเปิดที่เครื่อง ST1 · เครื่อง ST3 ต้องตั้งโฟลเดอร์โปรแกรม UV2 ไว้";
        lblUv2PickHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkUv2Pick
        //
        chkUv2Pick.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkUv2Pick.CheckedText = "เปิด";
        chkUv2Pick.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkUv2Pick.Margin = new System.Windows.Forms.Padding(0);
        chkUv2Pick.Name = "chkUv2Pick";
        chkUv2Pick.Size = new System.Drawing.Size(84, 36);
        chkUv2Pick.TabIndex = 22;
        chkUv2Pick.UnCheckedText = "ปิด";
        //
        // tlpUvRepeat
        //
        tlpUvRepeat.ColumnCount = 2;
        tlpUvRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUvRepeat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpUvRepeat.Controls.Add(lblUvRepeatHeading, 0, 0);
        tlpUvRepeat.Controls.Add(lblUvRepeatHelp, 0, 1);
        tlpUvRepeat.Controls.Add(chkUvRepeat, 1, 0);
        tlpUvRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpUvRepeat.Margin = new System.Windows.Forms.Padding(0);
        tlpUvRepeat.Name = "tlpUvRepeat";
        tlpUvRepeat.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpUvRepeat.RowCount = 2;
        tlpUvRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpUvRepeat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpUvRepeat.SetRowSpan(chkUvRepeat, 2);
        tlpUvRepeat.Size = new System.Drawing.Size(1216, 116);
        tlpUvRepeat.TabIndex = 23;
        //
        // lblUvRepeatHeading
        //
        lblUvRepeatHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUvRepeatHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblUvRepeatHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblUvRepeatHeading.Margin = new System.Windows.Forms.Padding(0);
        lblUvRepeatHeading.Name = "lblUvRepeatHeading";
        lblUvRepeatHeading.Size = new System.Drawing.Size(1076, 36);
        lblUvRepeatHeading.TabIndex = 24;
        lblUvRepeatHeading.Text = "พิมพ์รุ่นย่อย UV เพิ่มให้งานใบเดิม";
        //
        // lblUvRepeatHelp
        //
        lblUvRepeatHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblUvRepeatHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblUvRepeatHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblUvRepeatHelp.Margin = new System.Windows.Forms.Padding(0);
        lblUvRepeatHelp.Name = "lblUvRepeatHelp";
        lblUvRepeatHelp.Size = new System.Drawing.Size(1076, 52);
        lblUvRepeatHelp.TabIndex = 25;
        lblUvRepeatHelp.Text = "ระหว่างงานถือเครื่อง UV แถวงานมีปุ่ม เปลี่ยนรุ่น เลือกรุ่นที่ยังไม่พิมพ์แล้วส่งเข้าเครื่องทันที\r\nปุ่ม UV1 ขึ้นที่ ST1 · UV2 ขึ้นที่ ST3 · ยังเหลือรุ่น = ปุ่มจบงานสีส้ม กดจบได้แต่เตือนก่อน";
        lblUvRepeatHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkUvRepeat
        //
        chkUvRepeat.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkUvRepeat.CheckedText = "เปิด";
        chkUvRepeat.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkUvRepeat.Margin = new System.Windows.Forms.Padding(0);
        chkUvRepeat.Name = "chkUvRepeat";
        chkUvRepeat.Size = new System.Drawing.Size(84, 36);
        chkUvRepeat.TabIndex = 26;
        chkUvRepeat.UnCheckedText = "ปิด";
        //
        // tlpScanTab
        //
        tlpScanTab.ColumnCount = 2;
        tlpScanTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpScanTab.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
        tlpScanTab.Controls.Add(lblScanTabHeading, 0, 0);
        tlpScanTab.Controls.Add(lblScanTabHelp, 0, 1);
        tlpScanTab.Controls.Add(chkScanTab, 1, 0);
        tlpScanTab.Dock = System.Windows.Forms.DockStyle.Fill;
        tlpScanTab.Margin = new System.Windows.Forms.Padding(0);
        tlpScanTab.Name = "tlpScanTab";
        tlpScanTab.Padding = new System.Windows.Forms.Padding(0, 14, 0, 14);
        tlpScanTab.RowCount = 2;
        tlpScanTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpScanTab.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpScanTab.SetRowSpan(chkScanTab, 2);
        tlpScanTab.Size = new System.Drawing.Size(1216, 116);
        tlpScanTab.TabIndex = 27;
        //
        // lblScanTabHeading
        //
        lblScanTabHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblScanTabHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblScanTabHeading.ForeColor = System.Drawing.Color.FromArgb(36, 71, 101);
        lblScanTabHeading.Margin = new System.Windows.Forms.Padding(0);
        lblScanTabHeading.Name = "lblScanTabHeading";
        lblScanTabHeading.Size = new System.Drawing.Size(1076, 36);
        lblScanTabHeading.TabIndex = 28;
        lblScanTabHeading.Text = "แท็บ Scan Barcode ที่ ST1";
        //
        // lblScanTabHelp
        //
        lblScanTabHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblScanTabHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblScanTabHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblScanTabHelp.Margin = new System.Windows.Forms.Padding(0);
        lblScanTabHelp.Name = "lblScanTabHelp";
        lblScanTabHelp.Size = new System.Drawing.Size(1076, 52);
        lblScanTabHelp.TabIndex = 29;
        lblScanTabHelp.Text = "เพิ่มแท็บ Scan Barcode ให้เครื่อง ST1\r\nตั้งที่เครื่อง ST1 เอง · เห็นผลหลังเปิดโปรแกรมใหม่";
        lblScanTabHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // chkScanTab
        //
        chkScanTab.Anchor = System.Windows.Forms.AnchorStyles.Right;
        chkScanTab.CheckedText = "เปิด";
        chkScanTab.Font = new System.Drawing.Font("Segoe UI", 11F);
        chkScanTab.Margin = new System.Windows.Forms.Padding(0);
        chkScanTab.Name = "chkScanTab";
        chkScanTab.Size = new System.Drawing.Size(84, 36);
        chkScanTab.TabIndex = 30;
        chkScanTab.UnCheckedText = "ปิด";
        //
        // lblSectionTools
        //
        lblSectionTools.Dock = System.Windows.Forms.DockStyle.Fill;
        lblSectionTools.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        lblSectionTools.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblSectionTools.Margin = new System.Windows.Forms.Padding(0);
        lblSectionTools.Name = "lblSectionTools";
        lblSectionTools.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
        lblSectionTools.Size = new System.Drawing.Size(1216, 72);
        lblSectionTools.TabIndex = 31;
        lblSectionTools.Text = "เครื่องมือทดสอบ · ย้อนกลับไม่ได้ และมีผลกับทุกเครื่องที่ต่อ backend เดียวกัน";
        lblSectionTools.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
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
        tlpReset.Padding = new System.Windows.Forms.Padding(0, 14, 0, 16);
        tlpReset.RowCount = 3;
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpReset.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        tlpReset.Size = new System.Drawing.Size(1216, 184);
        tlpReset.TabIndex = 32;
        //
        // lblResetHeading
        //
        lblResetHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblResetHeading.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblResetHeading.Margin = new System.Windows.Forms.Padding(0);
        lblResetHeading.Name = "lblResetHeading";
        lblResetHeading.Size = new System.Drawing.Size(1216, 36);
        lblResetHeading.TabIndex = 33;
        lblResetHeading.Text = "รีเซ็ตสถานะงานทั้งหมด";
        //
        // lblResetHelp
        //
        lblResetHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblResetHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblResetHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblResetHelp.Margin = new System.Windows.Forms.Padding(0);
        lblResetHelp.Name = "lblResetHelp";
        lblResetHelp.Size = new System.Drawing.Size(1216, 70);
        lblResetHelp.TabIndex = 34;
        lblResetHelp.Text = "ลบคิวเครื่อง ประวัติคำสั่งที่ส่งเข้าเครื่อง และคำขอที่ ST3 ฝากไว้ · ทุกงานกลับเป็นรอเริ่ม\r\nไม่แตะตัวงาน pattern ข้อความ UV แผนการผลิต และค่าแคลมป์";
        lblResetHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // btnResetRuntime
        //
        btnResetRuntime.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnResetRuntime.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        btnResetRuntime.ForeColor = System.Drawing.Color.White;
        btnResetRuntime.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        btnResetRuntime.Name = "btnResetRuntime";
        btnResetRuntime.Radius = 8;
        btnResetRuntime.Size = new System.Drawing.Size(260, 48);
        btnResetRuntime.TabIndex = 35;
        btnResetRuntime.Text = "รีเซ็ตกลับเป็นค่าเริ่มต้น";
        btnResetRuntime.Type = AntdUI.TTypeMini.Error;
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
        tlpJobData.Padding = new System.Windows.Forms.Padding(0, 14, 0, 16);
        tlpJobData.RowCount = 3;
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tlpJobData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
        tlpJobData.Size = new System.Drawing.Size(1216, 184);
        tlpJobData.TabIndex = 36;
        //
        // lblJobDataHeading
        //
        lblJobDataHeading.Dock = System.Windows.Forms.DockStyle.Fill;
        lblJobDataHeading.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        lblJobDataHeading.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        lblJobDataHeading.Margin = new System.Windows.Forms.Padding(0);
        lblJobDataHeading.Name = "lblJobDataHeading";
        lblJobDataHeading.Size = new System.Drawing.Size(1216, 36);
        lblJobDataHeading.TabIndex = 37;
        lblJobDataHeading.Text = "ชุดข้อมูลงานทดสอบ";
        //
        // lblJobDataHelp
        //
        lblJobDataHelp.Dock = System.Windows.Forms.DockStyle.Fill;
        lblJobDataHelp.Font = new System.Drawing.Font("Segoe UI", 11F);
        lblJobDataHelp.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
        lblJobDataHelp.Margin = new System.Windows.Forms.Padding(0);
        lblJobDataHelp.Name = "lblJobDataHelp";
        lblJobDataHelp.Size = new System.Drawing.Size(1216, 70);
        lblJobDataHelp.TabIndex = 38;
        lblJobDataHelp.Text = "บันทึก: เก็บงานทั้งหมดเป็นไฟล์ .json ใน C:\\ProgramData\\CompactInkjet\\test-data · นำเข้า: แทนที่งานทั้งหมดด้วยไฟล์ · ล้าง: ลบงานทั้งหมด\r\nค่าตั้ง (register map PLC, Setting) ไม่ถูกแตะ · กดบันทึกไว้ก่อนล้างหรือนำเข้าเสมอ";
        lblJobDataHelp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        //
        // flpJobData
        //
        flpJobData.Controls.Add(btnSaveJobData);
        flpJobData.Controls.Add(btnImportJobData);
        flpJobData.Controls.Add(btnClearJobData);
        flpJobData.Dock = System.Windows.Forms.DockStyle.Fill;
        flpJobData.Margin = new System.Windows.Forms.Padding(0);
        flpJobData.Name = "flpJobData";
        flpJobData.Size = new System.Drawing.Size(1216, 48);
        flpJobData.TabIndex = 39;
        flpJobData.WrapContents = false;
        //
        // btnSaveJobData
        //
        btnSaveJobData.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        btnSaveJobData.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        btnSaveJobData.Name = "btnSaveJobData";
        btnSaveJobData.Radius = 8;
        btnSaveJobData.Size = new System.Drawing.Size(260, 48);
        btnSaveJobData.TabIndex = 0;
        btnSaveJobData.Text = "บันทึกข้อมูลงานตอนนี้";
        btnSaveJobData.Type = AntdUI.TTypeMini.Primary;
        //
        // btnImportJobData
        //
        btnImportJobData.BorderWidth = 2F;
        btnImportJobData.DefaultBorderColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnImportJobData.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        btnImportJobData.ForeColor = System.Drawing.Color.FromArgb(245, 34, 45);
        btnImportJobData.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        btnImportJobData.Name = "btnImportJobData";
        btnImportJobData.Radius = 8;
        btnImportJobData.Size = new System.Drawing.Size(260, 48);
        btnImportJobData.TabIndex = 1;
        btnImportJobData.Text = "นำเข้าจากไฟล์...";
        //
        // btnClearJobData
        //
        btnClearJobData.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        btnClearJobData.ForeColor = System.Drawing.Color.White;
        btnClearJobData.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        btnClearJobData.Name = "btnClearJobData";
        btnClearJobData.Radius = 8;
        btnClearJobData.Size = new System.Drawing.Size(260, 48);
        btnClearJobData.TabIndex = 2;
        btnClearJobData.Text = "ล้างข้อมูลงานทั้งหมด";
        btnClearJobData.Type = AntdUI.TTypeMini.Error;
        //
        // divRow1
        //
        divRow1.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow1.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow1.Margin = new System.Windows.Forms.Padding(0);
        divRow1.Name = "divRow1";
        divRow1.Size = new System.Drawing.Size(1216, 9);
        divRow1.TabIndex = 40;
        divRow1.Thickness = 1F;
        //
        // divRow2
        //
        divRow2.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow2.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow2.Margin = new System.Windows.Forms.Padding(0);
        divRow2.Name = "divRow2";
        divRow2.Size = new System.Drawing.Size(1216, 9);
        divRow2.TabIndex = 41;
        divRow2.Thickness = 1F;
        //
        // divRow3
        //
        divRow3.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow3.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow3.Margin = new System.Windows.Forms.Padding(0);
        divRow3.Name = "divRow3";
        divRow3.Size = new System.Drawing.Size(1216, 9);
        divRow3.TabIndex = 42;
        divRow3.Thickness = 1F;
        //
        // divRow4
        //
        divRow4.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow4.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow4.Margin = new System.Windows.Forms.Padding(0);
        divRow4.Name = "divRow4";
        divRow4.Size = new System.Drawing.Size(1216, 9);
        divRow4.TabIndex = 43;
        divRow4.Thickness = 1F;
        //
        // divRow5
        //
        divRow5.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow5.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow5.Margin = new System.Windows.Forms.Padding(0);
        divRow5.Name = "divRow5";
        divRow5.Size = new System.Drawing.Size(1216, 9);
        divRow5.TabIndex = 44;
        divRow5.Thickness = 1F;
        //
        // divRow6
        //
        divRow6.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow6.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow6.Margin = new System.Windows.Forms.Padding(0);
        divRow6.Name = "divRow6";
        divRow6.Size = new System.Drawing.Size(1216, 9);
        divRow6.TabIndex = 45;
        divRow6.Thickness = 1F;
        //
        // divRow7
        //
        divRow7.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow7.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow7.Margin = new System.Windows.Forms.Padding(0);
        divRow7.Name = "divRow7";
        divRow7.Size = new System.Drawing.Size(1216, 9);
        divRow7.TabIndex = 46;
        divRow7.Thickness = 1F;
        //
        // divRow8
        //
        divRow8.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow8.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow8.Margin = new System.Windows.Forms.Padding(0);
        divRow8.Name = "divRow8";
        divRow8.Size = new System.Drawing.Size(1216, 9);
        divRow8.TabIndex = 47;
        divRow8.Thickness = 1F;
        //
        // divRow9
        //
        divRow9.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow9.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow9.Margin = new System.Windows.Forms.Padding(0);
        divRow9.Name = "divRow9";
        divRow9.Size = new System.Drawing.Size(1216, 9);
        divRow9.TabIndex = 48;
        divRow9.Thickness = 1F;
        //
        // divRow10
        //
        divRow10.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow10.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow10.Margin = new System.Windows.Forms.Padding(0);
        divRow10.Name = "divRow10";
        divRow10.Size = new System.Drawing.Size(1216, 9);
        divRow10.TabIndex = 49;
        divRow10.Thickness = 1F;
        //
        // divRow11
        //
        divRow11.ColorSplit = System.Drawing.Color.FromArgb(229, 229, 229);
        divRow11.Dock = System.Windows.Forms.DockStyle.Fill;
        divRow11.Margin = new System.Windows.Forms.Padding(0);
        divRow11.Name = "divRow11";
        divRow11.Size = new System.Drawing.Size(1216, 9);
        divRow11.TabIndex = 50;
        divRow11.Thickness = 1F;
        //
        // StationOptionsUserControl
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        Controls.Add(tlpOptionsRoot);
        Name = "StationOptionsUserControl";
        Padding = new System.Windows.Forms.Padding(32);
        Size = new System.Drawing.Size(1280, 1529);
        flpJobData.ResumeLayout(false);
        tlpJobData.ResumeLayout(false);
        tlpReset.ResumeLayout(false);
        tlpScanTab.ResumeLayout(false);
        tlpUvRepeat.ResumeLayout(false);
        tlpUv2Pick.ResumeLayout(false);
        tlpAllOrNothing.ResumeLayout(false);
        tlpMockup.ResumeLayout(false);
        flpProcessTabs.ResumeLayout(false);
        tlpProcessTabs.ResumeLayout(false);
        tlpRemoteSend.ResumeLayout(false);
        tlpOptionsRoot.ResumeLayout(false);
        tlpProcessTabs.PerformLayout();
        flpProcessTabs.PerformLayout();
        tlpJobData.PerformLayout();
        flpJobData.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tlpOptionsRoot;
    private AntdUI.Label lblOptionsTitle;
    private AntdUI.Label lblSectionOptions;
    private AntdUI.Label lblSectionTools;
    private AntdUI.Divider divRow1;
    private AntdUI.Divider divRow2;
    private AntdUI.Divider divRow3;
    private AntdUI.Divider divRow4;
    private AntdUI.Divider divRow5;
    private AntdUI.Divider divRow6;
    private AntdUI.Divider divRow7;
    private AntdUI.Divider divRow8;
    private AntdUI.Divider divRow9;
    private AntdUI.Divider divRow10;
    private AntdUI.Divider divRow11;
    private System.Windows.Forms.TableLayoutPanel tlpRemoteSend;
    private AntdUI.Label lblRemoteSendHeading;
    private AntdUI.Label lblRemoteSendHelp;
    private AntdUI.Switch chkManualRemoteSend;
    private System.Windows.Forms.TableLayoutPanel tlpProcessTabs;
    private AntdUI.Label lblProcessTabsHeading;
    private AntdUI.Label lblProcessTabsHelp;
    private System.Windows.Forms.FlowLayoutPanel flpProcessTabs;
    private AntdUI.Radio rdoProcessTabsStations;
    private AntdUI.Radio rdoProcessTabsDev;
    private AntdUI.Radio rdoProcessTabsOff;
    private System.Windows.Forms.TableLayoutPanel tlpMockup;
    private AntdUI.Label lblMockupHeading;
    private AntdUI.Label lblMockupHelp;
    private AntdUI.Switch chkMockupStatus;
    private System.Windows.Forms.TableLayoutPanel tlpAllOrNothing;
    private AntdUI.Label lblAllOrNothingHeading;
    private AntdUI.Label lblAllOrNothingHelp;
    private AntdUI.Switch chkAllOrNothing;
    private System.Windows.Forms.TableLayoutPanel tlpUv2Pick;
    private AntdUI.Label lblUv2PickHeading;
    private AntdUI.Label lblUv2PickHelp;
    private AntdUI.Switch chkUv2Pick;
    private System.Windows.Forms.TableLayoutPanel tlpUvRepeat;
    private AntdUI.Label lblUvRepeatHeading;
    private AntdUI.Label lblUvRepeatHelp;
    private AntdUI.Switch chkUvRepeat;
    private System.Windows.Forms.TableLayoutPanel tlpScanTab;
    private AntdUI.Label lblScanTabHeading;
    private AntdUI.Label lblScanTabHelp;
    private AntdUI.Switch chkScanTab;
    private System.Windows.Forms.TableLayoutPanel tlpReset;
    private AntdUI.Label lblResetHeading;
    private AntdUI.Label lblResetHelp;
    private AntdUI.Button btnResetRuntime;
    private System.Windows.Forms.TableLayoutPanel tlpJobData;
    private AntdUI.Label lblJobDataHeading;
    private AntdUI.Label lblJobDataHelp;
    private System.Windows.Forms.FlowLayoutPanel flpJobData;
    private AntdUI.Button btnSaveJobData;
    private AntdUI.Button btnImportJobData;
    private AntdUI.Button btnClearJobData;
}
