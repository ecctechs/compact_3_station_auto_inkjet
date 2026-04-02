namespace InkjetOperator
{
    partial class ucSetting
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            panelPcStation1 = new Panel();
            label1 = new Label();
            button1 = new Button();
            label2 = new Label();
            label3 = new Label();
            txtPcip = new TextBox();
            label4 = new Label();
            pnlMainContent = new Panel();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            pnlPlc = new Panel();
            lblPlc001Status = new Label();
            btnEditPlc001 = new Button();
            lblPlc001 = new Label();
            lblPlcTitle = new Label();
            txtPlc001Ip = new TextBox();
            lblPlcIp = new Label();
            pnlUvPrinters = new Panel();
            lblUv002Status = new Label();
            lblUv001Status = new Label();
            btnEditUv002 = new Button();
            btnEditUv001 = new Button();
            txtUv002Ip = new TextBox();
            lblUv002Ip = new Label();
            lblUv002 = new Label();
            txtUv001Ip = new TextBox();
            lblUv001Ip = new Label();
            lblUv001 = new Label();
            lblUvTitle = new Label();
            pnlMkPrinters = new Panel();
            lblMk061Status = new Label();
            lblMk060Status = new Label();
            lblMk059Status = new Label();
            lblMk058Status = new Label();
            btnEditMk061 = new Button();
            btnEditMk060 = new Button();
            btnEditMk059 = new Button();
            btnEditMk058 = new Button();
            txtMk061Baud = new TextBox();
            lblMk061Baud = new Label();
            txtMk061Com = new TextBox();
            lblMk061Com = new Label();
            lblMk061 = new Label();
            txtMk060Baud = new TextBox();
            lblMk060Baud = new Label();
            txtMk060Com = new TextBox();
            lblMk060Com = new Label();
            lblMk060 = new Label();
            txtMk059Baud = new TextBox();
            lblMk059Baud = new Label();
            txtMk059Com = new TextBox();
            lblMk059Com = new Label();
            lblMk059 = new Label();
            txtMk058Baud = new TextBox();
            lblMk058Baud = new Label();
            txtMk058Com = new TextBox();
            lblMk058Com = new Label();
            lblMk058 = new Label();
            lblMkTitle = new Label();
            panelPcStation1.SuspendLayout();
            pnlMainContent.SuspendLayout();
            pnlButtons.SuspendLayout();
            pnlPlc.SuspendLayout();
            pnlUvPrinters.SuspendLayout();
            pnlMkPrinters.SuspendLayout();
            SuspendLayout();
            // 
            // panelPcStation1
            // 
            panelPcStation1.BackColor = Color.White;
            panelPcStation1.BorderStyle = BorderStyle.FixedSingle;
            panelPcStation1.Controls.Add(label1);
            panelPcStation1.Controls.Add(button1);
            panelPcStation1.Controls.Add(label2);
            panelPcStation1.Controls.Add(label3);
            panelPcStation1.Controls.Add(txtPcip);
            panelPcStation1.Controls.Add(label4);
            panelPcStation1.Location = new Point(78, 223);
            panelPcStation1.Name = "panelPcStation1";
            panelPcStation1.Size = new Size(390, 161);
            panelPcStation1.TabIndex = 4;
            // 
            // label1
            // 
            label1.BackColor = Color.FromArgb(100, 200, 100);
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Location = new Point(15, 50);
            label1.Name = "label1";
            label1.Size = new Size(15, 15);
            label1.TabIndex = 0;
            // 
            // button1
            // 
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 8F);
            button1.Location = new Point(164, 46);
            button1.Name = "button1";
            button1.Size = new Size(25, 25);
            button1.TabIndex = 1;
            button1.Text = "✎";
            // 
            // label2
            // 
            label2.BackColor = Color.Black;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(35, 45);
            label2.Name = "label2";
            label2.Padding = new Padding(5, 2, 5, 2);
            label2.Size = new Size(124, 25);
            label2.TabIndex = 2;
            label2.Text = "IP Station 1";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label3.Location = new Point(10, 10);
            label3.Name = "label3";
            label3.Size = new Size(52, 25);
            label3.TabIndex = 3;
            label3.Text = "PC 2";
            // 
            // txtPcip
            // 
            txtPcip.Location = new Point(90, 82);
            txtPcip.Name = "txtPcip";
            txtPcip.Size = new Size(200, 23);
            txtPcip.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(15, 85);
            label4.Name = "label4";
            label4.Size = new Size(68, 15);
            label4.TabIndex = 5;
            label4.Text = "IP Address :";
            // 
            // pnlMainContent
            // 
            pnlMainContent.BackColor = Color.White;
            pnlMainContent.Controls.Add(panelPcStation1);
            pnlMainContent.Controls.Add(pnlButtons);
            pnlMainContent.Controls.Add(pnlPlc);
            pnlMainContent.Controls.Add(pnlUvPrinters);
            pnlMainContent.Controls.Add(pnlMkPrinters);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(0, 0);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Padding = new Padding(20, 20, 20, 20);
            pnlMainContent.Size = new Size(900, 600);
            pnlMainContent.TabIndex = 1;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = Color.Transparent;
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Location = new Point(379, 526);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(320, 50);
            pnlButtons.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(150, 150, 150);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(170, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 40);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(70, 130, 180);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(50, 5);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 40);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // pnlPlc
            // 
            pnlPlc.BackColor = Color.White;
            pnlPlc.BorderStyle = BorderStyle.FixedSingle;
            pnlPlc.Controls.Add(lblPlc001Status);
            pnlPlc.Controls.Add(btnEditPlc001);
            pnlPlc.Controls.Add(lblPlc001);
            pnlPlc.Controls.Add(lblPlcTitle);
            pnlPlc.Controls.Add(txtPlc001Ip);
            pnlPlc.Controls.Add(lblPlcIp);
            pnlPlc.Location = new Point(363, 329);
            pnlPlc.Name = "pnlPlc";
            pnlPlc.Size = new Size(304, 180);
            pnlPlc.TabIndex = 1;
            // 
            // lblPlc001Status
            // 
            lblPlc001Status.BackColor = Color.FromArgb(100, 200, 100);
            lblPlc001Status.BorderStyle = BorderStyle.FixedSingle;
            lblPlc001Status.Location = new Point(15, 50);
            lblPlc001Status.Name = "lblPlc001Status";
            lblPlc001Status.Size = new Size(15, 15);
            lblPlc001Status.TabIndex = 0;
            // 
            // btnEditPlc001
            // 
            btnEditPlc001.FlatAppearance.BorderSize = 0;
            btnEditPlc001.FlatStyle = FlatStyle.Flat;
            btnEditPlc001.Font = new Font("Segoe UI", 8F);
            btnEditPlc001.Location = new Point(110, 45);
            btnEditPlc001.Name = "btnEditPlc001";
            btnEditPlc001.Size = new Size(25, 25);
            btnEditPlc001.TabIndex = 1;
            btnEditPlc001.Text = "✎";
            // 
            // lblPlc001
            // 
            lblPlc001.BackColor = Color.Black;
            lblPlc001.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPlc001.ForeColor = Color.White;
            lblPlc001.Location = new Point(35, 45);
            lblPlc001.Name = "lblPlc001";
            lblPlc001.Padding = new Padding(5, 2, 5, 2);
            lblPlc001.Size = new Size(70, 25);
            lblPlc001.TabIndex = 2;
            lblPlc001.Text = "PLC-001";
            lblPlc001.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPlcTitle
            // 
            lblPlcTitle.AutoSize = true;
            lblPlcTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblPlcTitle.Location = new Point(10, 10);
            lblPlcTitle.Name = "lblPlcTitle";
            lblPlcTitle.Size = new Size(46, 25);
            lblPlcTitle.TabIndex = 3;
            lblPlcTitle.Text = "PLC";
            // 
            // txtPlc001Ip
            // 
            txtPlc001Ip.Location = new Point(90, 82);
            txtPlc001Ip.Name = "txtPlc001Ip";
            txtPlc001Ip.Size = new Size(200, 23);
            txtPlc001Ip.TabIndex = 4;
            // 
            // lblPlcIp
            // 
            lblPlcIp.AutoSize = true;
            lblPlcIp.Location = new Point(15, 85);
            lblPlcIp.Name = "lblPlcIp";
            lblPlcIp.Size = new Size(68, 15);
            lblPlcIp.TabIndex = 5;
            lblPlcIp.Text = "IP Address :";
            // 
            // pnlUvPrinters
            // 
            pnlUvPrinters.BackColor = Color.White;
            pnlUvPrinters.BorderStyle = BorderStyle.FixedSingle;
            pnlUvPrinters.Controls.Add(lblUv002Status);
            pnlUvPrinters.Controls.Add(lblUv001Status);
            pnlUvPrinters.Controls.Add(btnEditUv002);
            pnlUvPrinters.Controls.Add(btnEditUv001);
            pnlUvPrinters.Controls.Add(txtUv002Ip);
            pnlUvPrinters.Controls.Add(lblUv002Ip);
            pnlUvPrinters.Controls.Add(lblUv002);
            pnlUvPrinters.Controls.Add(txtUv001Ip);
            pnlUvPrinters.Controls.Add(lblUv001Ip);
            pnlUvPrinters.Controls.Add(lblUv001);
            pnlUvPrinters.Controls.Add(lblUvTitle);
            pnlUvPrinters.Location = new Point(23, 329);
            pnlUvPrinters.Name = "pnlUvPrinters";
            pnlUvPrinters.Size = new Size(320, 180);
            pnlUvPrinters.TabIndex = 2;
            // 
            // lblUv002Status
            // 
            lblUv002Status.BackColor = Color.FromArgb(100, 200, 100);
            lblUv002Status.BorderStyle = BorderStyle.FixedSingle;
            lblUv002Status.Location = new Point(15, 120);
            lblUv002Status.Name = "lblUv002Status";
            lblUv002Status.Size = new Size(15, 15);
            lblUv002Status.TabIndex = 0;
            // 
            // lblUv001Status
            // 
            lblUv001Status.BackColor = Color.FromArgb(100, 200, 100);
            lblUv001Status.BorderStyle = BorderStyle.FixedSingle;
            lblUv001Status.Location = new Point(15, 50);
            lblUv001Status.Name = "lblUv001Status";
            lblUv001Status.Size = new Size(15, 15);
            lblUv001Status.TabIndex = 1;
            // 
            // btnEditUv002
            // 
            btnEditUv002.FlatAppearance.BorderSize = 0;
            btnEditUv002.FlatStyle = FlatStyle.Flat;
            btnEditUv002.Font = new Font("Segoe UI", 8F);
            btnEditUv002.Location = new Point(131, 115);
            btnEditUv002.Name = "btnEditUv002";
            btnEditUv002.Size = new Size(25, 25);
            btnEditUv002.TabIndex = 2;
            btnEditUv002.Text = "✎";
            // 
            // btnEditUv001
            // 
            btnEditUv001.FlatAppearance.BorderSize = 0;
            btnEditUv001.FlatStyle = FlatStyle.Flat;
            btnEditUv001.Font = new Font("Segoe UI", 8F);
            btnEditUv001.Location = new Point(131, 46);
            btnEditUv001.Name = "btnEditUv001";
            btnEditUv001.Size = new Size(25, 25);
            btnEditUv001.TabIndex = 3;
            btnEditUv001.Text = "✎";
            // 
            // txtUv002Ip
            // 
            txtUv002Ip.Location = new Point(90, 152);
            txtUv002Ip.Name = "txtUv002Ip";
            txtUv002Ip.Size = new Size(200, 23);
            txtUv002Ip.TabIndex = 4;
            // 
            // lblUv002Ip
            // 
            lblUv002Ip.AutoSize = true;
            lblUv002Ip.Location = new Point(15, 155);
            lblUv002Ip.Name = "lblUv002Ip";
            lblUv002Ip.Size = new Size(68, 15);
            lblUv002Ip.TabIndex = 5;
            lblUv002Ip.Text = "IP Address :";
            // 
            // lblUv002
            // 
            lblUv002.BackColor = Color.Black;
            lblUv002.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUv002.ForeColor = Color.White;
            lblUv002.Location = new Point(35, 115);
            lblUv002.Name = "lblUv002";
            lblUv002.Padding = new Padding(5, 2, 5, 2);
            lblUv002.Size = new Size(91, 25);
            lblUv002.TabIndex = 6;
            lblUv002.Text = "UV-002";
            lblUv002.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtUv001Ip
            // 
            txtUv001Ip.Location = new Point(90, 82);
            txtUv001Ip.Name = "txtUv001Ip";
            txtUv001Ip.Size = new Size(200, 23);
            txtUv001Ip.TabIndex = 7;
            // 
            // lblUv001Ip
            // 
            lblUv001Ip.AutoSize = true;
            lblUv001Ip.Location = new Point(15, 85);
            lblUv001Ip.Name = "lblUv001Ip";
            lblUv001Ip.Size = new Size(68, 15);
            lblUv001Ip.TabIndex = 8;
            lblUv001Ip.Text = "IP Address :";
            // 
            // lblUv001
            // 
            lblUv001.BackColor = Color.Black;
            lblUv001.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUv001.ForeColor = Color.White;
            lblUv001.Location = new Point(35, 45);
            lblUv001.Name = "lblUv001";
            lblUv001.Padding = new Padding(5, 2, 5, 2);
            lblUv001.Size = new Size(91, 25);
            lblUv001.TabIndex = 9;
            lblUv001.Text = "UV-001";
            lblUv001.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUvTitle
            // 
            lblUvTitle.AutoSize = true;
            lblUvTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblUvTitle.Location = new Point(10, 10);
            lblUvTitle.Name = "lblUvTitle";
            lblUvTitle.Size = new Size(106, 25);
            lblUvTitle.TabIndex = 10;
            lblUvTitle.Text = "UV Printer";
            // 
            // pnlMkPrinters
            // 
            pnlMkPrinters.BackColor = Color.White;
            pnlMkPrinters.BorderStyle = BorderStyle.FixedSingle;
            pnlMkPrinters.Controls.Add(lblMk061Status);
            pnlMkPrinters.Controls.Add(lblMk060Status);
            pnlMkPrinters.Controls.Add(lblMk059Status);
            pnlMkPrinters.Controls.Add(lblMk058Status);
            pnlMkPrinters.Controls.Add(btnEditMk061);
            pnlMkPrinters.Controls.Add(btnEditMk060);
            pnlMkPrinters.Controls.Add(btnEditMk059);
            pnlMkPrinters.Controls.Add(btnEditMk058);
            pnlMkPrinters.Controls.Add(txtMk061Baud);
            pnlMkPrinters.Controls.Add(lblMk061Baud);
            pnlMkPrinters.Controls.Add(txtMk061Com);
            pnlMkPrinters.Controls.Add(lblMk061Com);
            pnlMkPrinters.Controls.Add(lblMk061);
            pnlMkPrinters.Controls.Add(txtMk060Baud);
            pnlMkPrinters.Controls.Add(lblMk060Baud);
            pnlMkPrinters.Controls.Add(txtMk060Com);
            pnlMkPrinters.Controls.Add(lblMk060Com);
            pnlMkPrinters.Controls.Add(lblMk060);
            pnlMkPrinters.Controls.Add(txtMk059Baud);
            pnlMkPrinters.Controls.Add(lblMk059Baud);
            pnlMkPrinters.Controls.Add(txtMk059Com);
            pnlMkPrinters.Controls.Add(lblMk059Com);
            pnlMkPrinters.Controls.Add(lblMk059);
            pnlMkPrinters.Controls.Add(txtMk058Baud);
            pnlMkPrinters.Controls.Add(lblMk058Baud);
            pnlMkPrinters.Controls.Add(txtMk058Com);
            pnlMkPrinters.Controls.Add(lblMk058Com);
            pnlMkPrinters.Controls.Add(lblMk058);
            pnlMkPrinters.Controls.Add(lblMkTitle);
            pnlMkPrinters.Location = new Point(23, 23);
            pnlMkPrinters.Name = "pnlMkPrinters";
            pnlMkPrinters.Size = new Size(644, 194);
            pnlMkPrinters.TabIndex = 3;
            // 
            // lblMk061Status
            // 
            lblMk061Status.BackColor = Color.FromArgb(100, 200, 100);
            lblMk061Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk061Status.Location = new Point(15, 155);
            lblMk061Status.Name = "lblMk061Status";
            lblMk061Status.Size = new Size(15, 15);
            lblMk061Status.TabIndex = 0;
            // 
            // lblMk060Status
            // 
            lblMk060Status.BackColor = Color.FromArgb(220, 80, 50);
            lblMk060Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk060Status.Location = new Point(15, 120);
            lblMk060Status.Name = "lblMk060Status";
            lblMk060Status.Size = new Size(15, 15);
            lblMk060Status.TabIndex = 1;
            // 
            // lblMk059Status
            // 
            lblMk059Status.BackColor = Color.FromArgb(100, 200, 100);
            lblMk059Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk059Status.Location = new Point(15, 85);
            lblMk059Status.Name = "lblMk059Status";
            lblMk059Status.Size = new Size(15, 15);
            lblMk059Status.TabIndex = 2;
            // 
            // lblMk058Status
            // 
            lblMk058Status.BackColor = Color.FromArgb(100, 200, 100);
            lblMk058Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk058Status.Location = new Point(15, 50);
            lblMk058Status.Name = "lblMk058Status";
            lblMk058Status.Size = new Size(15, 15);
            lblMk058Status.TabIndex = 3;
            // 
            // btnEditMk061
            // 
            btnEditMk061.FlatAppearance.BorderSize = 0;
            btnEditMk061.FlatStyle = FlatStyle.Flat;
            btnEditMk061.Font = new Font("Segoe UI", 8F);
            btnEditMk061.Location = new Point(137, 150);
            btnEditMk061.Name = "btnEditMk061";
            btnEditMk061.Size = new Size(25, 25);
            btnEditMk061.TabIndex = 4;
            btnEditMk061.Text = "✎";
            // 
            // btnEditMk060
            // 
            btnEditMk060.FlatAppearance.BorderSize = 0;
            btnEditMk060.FlatStyle = FlatStyle.Flat;
            btnEditMk060.Font = new Font("Segoe UI", 8F);
            btnEditMk060.Location = new Point(137, 115);
            btnEditMk060.Name = "btnEditMk060";
            btnEditMk060.Size = new Size(25, 25);
            btnEditMk060.TabIndex = 5;
            btnEditMk060.Text = "✎";
            // 
            // btnEditMk059
            // 
            btnEditMk059.FlatAppearance.BorderSize = 0;
            btnEditMk059.FlatStyle = FlatStyle.Flat;
            btnEditMk059.Font = new Font("Segoe UI", 8F);
            btnEditMk059.Location = new Point(137, 80);
            btnEditMk059.Name = "btnEditMk059";
            btnEditMk059.Size = new Size(25, 25);
            btnEditMk059.TabIndex = 6;
            btnEditMk059.Text = "✎";
            // 
            // btnEditMk058
            // 
            btnEditMk058.FlatAppearance.BorderSize = 0;
            btnEditMk058.FlatStyle = FlatStyle.Flat;
            btnEditMk058.Font = new Font("Segoe UI", 8F);
            btnEditMk058.Location = new Point(137, 45);
            btnEditMk058.Name = "btnEditMk058";
            btnEditMk058.Size = new Size(25, 25);
            btnEditMk058.TabIndex = 7;
            btnEditMk058.Text = "✎";
            // 
            // txtMk061Baud
            // 
            txtMk061Baud.Location = new Point(448, 153);
            txtMk061Baud.Name = "txtMk061Baud";
            txtMk061Baud.Size = new Size(100, 23);
            txtMk061Baud.TabIndex = 8;
            // 
            // lblMk061Baud
            // 
            lblMk061Baud.AutoSize = true;
            lblMk061Baud.Location = new Point(378, 156);
            lblMk061Baud.Name = "lblMk061Baud";
            lblMk061Baud.Size = new Size(66, 15);
            lblMk061Baud.TabIndex = 9;
            lblMk061Baud.Text = "Baud Rate :";
            // 
            // txtMk061Com
            // 
            txtMk061Com.Location = new Point(238, 153);
            txtMk061Com.Name = "txtMk061Com";
            txtMk061Com.Size = new Size(120, 23);
            txtMk061Com.TabIndex = 10;
            // 
            // lblMk061Com
            // 
            lblMk061Com.AutoSize = true;
            lblMk061Com.Location = new Point(168, 156);
            lblMk061Com.Name = "lblMk061Com";
            lblMk061Com.Size = new Size(66, 15);
            lblMk061Com.TabIndex = 11;
            lblMk061Com.Text = "COM Port :";
            // 
            // lblMk061
            // 
            lblMk061.BackColor = Color.Black;
            lblMk061.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMk061.ForeColor = Color.White;
            lblMk061.Location = new Point(35, 150);
            lblMk061.Name = "lblMk061";
            lblMk061.Padding = new Padding(5, 2, 5, 2);
            lblMk061.Size = new Size(101, 25);
            lblMk061.TabIndex = 12;
            lblMk061.Text = "MK-061";
            lblMk061.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk060Baud
            // 
            txtMk060Baud.Location = new Point(448, 118);
            txtMk060Baud.Name = "txtMk060Baud";
            txtMk060Baud.Size = new Size(100, 23);
            txtMk060Baud.TabIndex = 13;
            // 
            // lblMk060Baud
            // 
            lblMk060Baud.AutoSize = true;
            lblMk060Baud.Location = new Point(378, 121);
            lblMk060Baud.Name = "lblMk060Baud";
            lblMk060Baud.Size = new Size(66, 15);
            lblMk060Baud.TabIndex = 14;
            lblMk060Baud.Text = "Baud Rate :";
            // 
            // txtMk060Com
            // 
            txtMk060Com.Location = new Point(238, 118);
            txtMk060Com.Name = "txtMk060Com";
            txtMk060Com.Size = new Size(120, 23);
            txtMk060Com.TabIndex = 15;
            // 
            // lblMk060Com
            // 
            lblMk060Com.AutoSize = true;
            lblMk060Com.Location = new Point(168, 121);
            lblMk060Com.Name = "lblMk060Com";
            lblMk060Com.Size = new Size(66, 15);
            lblMk060Com.TabIndex = 16;
            lblMk060Com.Text = "COM Port :";
            // 
            // lblMk060
            // 
            lblMk060.BackColor = Color.Black;
            lblMk060.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMk060.ForeColor = Color.White;
            lblMk060.Location = new Point(35, 115);
            lblMk060.Name = "lblMk060";
            lblMk060.Padding = new Padding(5, 2, 5, 2);
            lblMk060.Size = new Size(101, 25);
            lblMk060.TabIndex = 17;
            lblMk060.Text = "MK-060";
            lblMk060.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk059Baud
            // 
            txtMk059Baud.Location = new Point(448, 82);
            txtMk059Baud.Name = "txtMk059Baud";
            txtMk059Baud.Size = new Size(100, 23);
            txtMk059Baud.TabIndex = 18;
            // 
            // lblMk059Baud
            // 
            lblMk059Baud.AutoSize = true;
            lblMk059Baud.Location = new Point(378, 86);
            lblMk059Baud.Name = "lblMk059Baud";
            lblMk059Baud.Size = new Size(66, 15);
            lblMk059Baud.TabIndex = 19;
            lblMk059Baud.Text = "Baud Rate :";
            // 
            // txtMk059Com
            // 
            txtMk059Com.Location = new Point(238, 82);
            txtMk059Com.Name = "txtMk059Com";
            txtMk059Com.Size = new Size(120, 23);
            txtMk059Com.TabIndex = 20;
            // 
            // lblMk059Com
            // 
            lblMk059Com.AutoSize = true;
            lblMk059Com.Location = new Point(168, 86);
            lblMk059Com.Name = "lblMk059Com";
            lblMk059Com.Size = new Size(66, 15);
            lblMk059Com.TabIndex = 21;
            lblMk059Com.Text = "COM Port :";
            // 
            // lblMk059
            // 
            lblMk059.BackColor = Color.Black;
            lblMk059.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMk059.ForeColor = Color.White;
            lblMk059.Location = new Point(35, 80);
            lblMk059.Name = "lblMk059";
            lblMk059.Padding = new Padding(5, 2, 5, 2);
            lblMk059.Size = new Size(101, 25);
            lblMk059.TabIndex = 22;
            lblMk059.Text = "MK-059";
            lblMk059.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk058Baud
            // 
            txtMk058Baud.Location = new Point(448, 48);
            txtMk058Baud.Name = "txtMk058Baud";
            txtMk058Baud.Size = new Size(100, 23);
            txtMk058Baud.TabIndex = 23;
            // 
            // lblMk058Baud
            // 
            lblMk058Baud.AutoSize = true;
            lblMk058Baud.Location = new Point(378, 51);
            lblMk058Baud.Name = "lblMk058Baud";
            lblMk058Baud.Size = new Size(66, 15);
            lblMk058Baud.TabIndex = 24;
            lblMk058Baud.Text = "Baud Rate :";
            // 
            // txtMk058Com
            // 
            txtMk058Com.Location = new Point(238, 48);
            txtMk058Com.Name = "txtMk058Com";
            txtMk058Com.Size = new Size(120, 23);
            txtMk058Com.TabIndex = 25;
            // 
            // lblMk058Com
            // 
            lblMk058Com.AutoSize = true;
            lblMk058Com.Location = new Point(168, 51);
            lblMk058Com.Name = "lblMk058Com";
            lblMk058Com.Size = new Size(66, 15);
            lblMk058Com.TabIndex = 26;
            lblMk058Com.Text = "COM Port :";
            // 
            // lblMk058
            // 
            lblMk058.BackColor = Color.Black;
            lblMk058.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMk058.ForeColor = Color.White;
            lblMk058.Location = new Point(35, 45);
            lblMk058.Name = "lblMk058";
            lblMk058.Padding = new Padding(5, 2, 5, 2);
            lblMk058.Size = new Size(101, 25);
            lblMk058.TabIndex = 27;
            lblMk058.Text = "MK-058";
            lblMk058.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMkTitle
            // 
            lblMkTitle.AutoSize = true;
            lblMkTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblMkTitle.Location = new Point(10, 10);
            lblMkTitle.Name = "lblMkTitle";
            lblMkTitle.Size = new Size(165, 25);
            lblMkTitle.TabIndex = 28;
            lblMkTitle.Text = "MK Inkjet Printer";
            // 
            // ucSetting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMainContent);
            Name = "ucSetting";
            Size = new Size(900, 600);
            panelPcStation1.ResumeLayout(false);
            panelPcStation1.PerformLayout();
            pnlMainContent.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            pnlPlc.ResumeLayout(false);
            pnlPlc.PerformLayout();
            pnlUvPrinters.ResumeLayout(false);
            pnlUvPrinters.PerformLayout();
            pnlMkPrinters.ResumeLayout(false);
            pnlMkPrinters.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Main Content
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Panel pnlMkPrinters;
        private System.Windows.Forms.Panel pnlUvPrinters;
        private System.Windows.Forms.Panel pnlPlc;
        private System.Windows.Forms.Panel pnlButtons;

        // MK Printers
        private System.Windows.Forms.Label lblMkTitle;
        private System.Windows.Forms.Label lblMk058Status;
        private System.Windows.Forms.Label lblMk058;
        private System.Windows.Forms.Button btnEditMk058;
        private System.Windows.Forms.Label lblMk058Com;
        private System.Windows.Forms.TextBox txtMk058Com;
        private System.Windows.Forms.Label lblMk058Baud;
        private System.Windows.Forms.TextBox txtMk058Baud;
        private System.Windows.Forms.Label lblMk059Status;
        private System.Windows.Forms.Label lblMk059;
        private System.Windows.Forms.Button btnEditMk059;
        private System.Windows.Forms.Label lblMk059Com;
        private System.Windows.Forms.TextBox txtMk059Com;
        private System.Windows.Forms.Label lblMk059Baud;
        private System.Windows.Forms.TextBox txtMk059Baud;
        private System.Windows.Forms.Label lblMk060Status;
        private System.Windows.Forms.Label lblMk060;
        private System.Windows.Forms.Button btnEditMk060;
        private System.Windows.Forms.Label lblMk060Com;
        private System.Windows.Forms.TextBox txtMk060Com;
        private System.Windows.Forms.Label lblMk060Baud;
        private System.Windows.Forms.TextBox txtMk060Baud;
        private System.Windows.Forms.Label lblMk061Status;
        private System.Windows.Forms.Label lblMk061;
        private System.Windows.Forms.Button btnEditMk061;
        private System.Windows.Forms.Label lblMk061Com;
        private System.Windows.Forms.TextBox txtMk061Com;
        private System.Windows.Forms.Label lblMk061Baud;
        private System.Windows.Forms.TextBox txtMk061Baud;

        // UV Printers
        private System.Windows.Forms.Label lblUvTitle;
        private System.Windows.Forms.Label lblUv001Status;
        private System.Windows.Forms.Label lblUv001;
        private System.Windows.Forms.Button btnEditUv001;
        private System.Windows.Forms.Label lblUv001Ip;
        private System.Windows.Forms.TextBox txtUv001Ip;
        private System.Windows.Forms.Label lblUv002Status;
        private System.Windows.Forms.Label lblUv002;
        private System.Windows.Forms.Button btnEditUv002;
        private System.Windows.Forms.Label lblUv002Ip;
        private System.Windows.Forms.TextBox txtUv002Ip;

        // PLC
        private System.Windows.Forms.Label lblPlcTitle;
        private System.Windows.Forms.Label lblPlc001Status;
        private System.Windows.Forms.Label lblPlc001;
        private System.Windows.Forms.Button btnEditPlc001;
        private System.Windows.Forms.Label lblPlcIp;
        private System.Windows.Forms.TextBox txtPlc001Ip;

        // Buttons
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private Panel panelPcStation1;
        private Label label1;
        private Button button1;
        private Label label2;
        private Label label3;
        private TextBox txtPcip;
        private Label label4;
    }
}