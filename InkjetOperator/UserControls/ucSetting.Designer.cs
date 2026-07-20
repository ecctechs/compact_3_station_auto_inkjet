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
            components = new System.ComponentModel.Container();
            panelPcStation1 = new Panel();
            lblPcStatus = new Label();
            btnPC2 = new Button();
            lblPC2 = new Label();
            label3 = new Label();
            txtPcip = new TextBox();
            label4 = new Label();
            pnlMainContent = new Panel();
            btnCancel = new Button();
            btnSave = new Button();
            pnlButtons = new Panel();
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
            txtMk061Com = new TextBox();
            lblMk061Com = new Label();
            lblMk061 = new Label();
            txtMk060Com = new TextBox();
            lblMk060Com = new Label();
            lblMk060 = new Label();
            txtMk059Com = new TextBox();
            lblMk059Com = new Label();
            lblMk059 = new Label();
            txtMk058Com = new TextBox();
            lblMk058Com = new Label();
            lblMk058 = new Label();
            lblMkTitle = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            panelPcStation1.SuspendLayout();
            pnlMainContent.SuspendLayout();
            pnlUvPrinters.SuspendLayout();
            pnlMkPrinters.SuspendLayout();
            SuspendLayout();
            // 
            // panelPcStation1
            // 
            panelPcStation1.BackColor = Color.White;
            panelPcStation1.BorderStyle = BorderStyle.FixedSingle;
            panelPcStation1.Controls.Add(lblPcStatus);
            panelPcStation1.Controls.Add(btnPC2);
            panelPcStation1.Controls.Add(lblPC2);
            panelPcStation1.Controls.Add(label3);
            panelPcStation1.Controls.Add(txtPcip);
            panelPcStation1.Controls.Add(label4);
            panelPcStation1.Location = new Point(85, 39);
            panelPcStation1.Margin = new Padding(4, 5, 4, 5);
            panelPcStation1.Name = "panelPcStation1";
            panelPcStation1.Size = new Size(556, 211);
            panelPcStation1.TabIndex = 4;
            // 
            // lblPcStatus
            // 
            lblPcStatus.BackColor = Color.Gray;
            lblPcStatus.BorderStyle = BorderStyle.FixedSingle;
            lblPcStatus.Location = new Point(21, 84);
            lblPcStatus.Margin = new Padding(4, 0, 4, 0);
            lblPcStatus.Name = "lblPcStatus";
            lblPcStatus.Size = new Size(21, 23);
            lblPcStatus.TabIndex = 0;
            // 
            // btnPC2
            // 
            btnPC2.FlatAppearance.BorderSize = 0;
            btnPC2.FlatStyle = FlatStyle.Flat;
            btnPC2.Font = new Font("Segoe UI", 9F);
            btnPC2.Location = new Point(196, 75);
            btnPC2.Margin = new Padding(4, 5, 4, 5);
            btnPC2.Name = "btnPC2";
            btnPC2.Size = new Size(36, 41);
            btnPC2.TabIndex = 1;
            btnPC2.Text = "✎";
            // 
            // lblPC2
            // 
            lblPC2.BackColor = Color.Black;
            lblPC2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPC2.ForeColor = Color.White;
            lblPC2.Location = new Point(50, 75);
            lblPC2.Margin = new Padding(4, 0, 4, 0);
            lblPC2.Name = "lblPC2";
            lblPC2.Padding = new Padding(8, 4, 8, 4);
            lblPC2.Size = new Size(144, 41);
            lblPC2.TabIndex = 2;
            lblPC2.Text = "IP Station 1";
            lblPC2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label3.Location = new Point(14, 16);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(75, 38);
            label3.TabIndex = 3;
            label3.Text = "PC 2";
            // 
            // txtPcip
            // 
            txtPcip.Font = new Font("Segoe UI", 10F);
            txtPcip.Location = new Point(129, 136);
            txtPcip.Margin = new Padding(4, 5, 4, 5);
            txtPcip.Name = "txtPcip";
            txtPcip.Size = new Size(284, 31);
            txtPcip.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10F);
            label4.Location = new Point(21, 140);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(106, 25);
            label4.TabIndex = 5;
            label4.Text = "IP Address :";
            // 
            // pnlMainContent
            // 
            pnlMainContent.BackColor = Color.White;
            pnlMainContent.Controls.Add(btnCancel);
            pnlMainContent.Controls.Add(panelPcStation1);
            pnlMainContent.Controls.Add(btnSave);
            pnlMainContent.Controls.Add(pnlButtons);
            pnlMainContent.Controls.Add(pnlUvPrinters);
            pnlMainContent.Controls.Add(pnlMkPrinters);
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(0, 0);
            pnlMainContent.Margin = new Padding(4, 5, 4, 5);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Padding = new Padding(29, 34, 29, 34);
            pnlMainContent.Size = new Size(1152, 939);
            pnlMainContent.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(150, 150, 150);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(955, 873);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(114, 53);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(70, 130, 180);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(814, 873);
            btnSave.Margin = new Padding(4, 5, 4, 5);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(114, 53);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = Color.Transparent;
            pnlButtons.Location = new Point(706, 1019);
            pnlButtons.Margin = new Padding(4, 5, 4, 5);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(458, 84);
            pnlButtons.TabIndex = 0;
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
            pnlUvPrinters.Location = new Point(85, 425);
            pnlUvPrinters.Margin = new Padding(4, 5, 4, 5);
            pnlUvPrinters.Name = "pnlUvPrinters";
            pnlUvPrinters.Size = new Size(456, 337);
            pnlUvPrinters.TabIndex = 2;
            // 
            // lblUv002Status
            // 
            lblUv002Status.BackColor = Color.Gray;
            lblUv002Status.BorderStyle = BorderStyle.FixedSingle;
            lblUv002Status.Location = new Point(21, 200);
            lblUv002Status.Margin = new Padding(4, 0, 4, 0);
            lblUv002Status.Name = "lblUv002Status";
            lblUv002Status.Size = new Size(21, 23);
            lblUv002Status.TabIndex = 0;
            // 
            // lblUv001Status
            // 
            lblUv001Status.BackColor = Color.Gray;
            lblUv001Status.BorderStyle = BorderStyle.FixedSingle;
            lblUv001Status.Location = new Point(21, 84);
            lblUv001Status.Margin = new Padding(4, 0, 4, 0);
            lblUv001Status.Name = "lblUv001Status";
            lblUv001Status.Size = new Size(21, 23);
            lblUv001Status.TabIndex = 1;
            // 
            // btnEditUv002
            // 
            btnEditUv002.FlatAppearance.BorderSize = 0;
            btnEditUv002.FlatStyle = FlatStyle.Flat;
            btnEditUv002.Font = new Font("Segoe UI", 9F);
            btnEditUv002.Location = new Point(196, 191);
            btnEditUv002.Margin = new Padding(4, 5, 4, 5);
            btnEditUv002.Name = "btnEditUv002";
            btnEditUv002.Size = new Size(36, 41);
            btnEditUv002.TabIndex = 2;
            btnEditUv002.Text = "✎";
            // 
            // btnEditUv001
            // 
            btnEditUv001.FlatAppearance.BorderSize = 0;
            btnEditUv001.FlatStyle = FlatStyle.Flat;
            btnEditUv001.Font = new Font("Segoe UI", 9F);
            btnEditUv001.Location = new Point(196, 75);
            btnEditUv001.Margin = new Padding(4, 5, 4, 5);
            btnEditUv001.Name = "btnEditUv001";
            btnEditUv001.Size = new Size(36, 41);
            btnEditUv001.TabIndex = 3;
            btnEditUv001.Text = "✎";
            // 
            // txtUv002Ip
            // 
            txtUv002Ip.Font = new Font("Segoe UI", 10F);
            txtUv002Ip.Location = new Point(129, 254);
            txtUv002Ip.Margin = new Padding(4, 5, 4, 5);
            txtUv002Ip.Name = "txtUv002Ip";
            txtUv002Ip.Size = new Size(284, 31);
            txtUv002Ip.TabIndex = 4;
            // 
            // lblUv002Ip
            // 
            lblUv002Ip.AutoSize = true;
            lblUv002Ip.Font = new Font("Segoe UI", 10F);
            lblUv002Ip.Location = new Point(21, 258);
            lblUv002Ip.Margin = new Padding(4, 0, 4, 0);
            lblUv002Ip.Name = "lblUv002Ip";
            lblUv002Ip.Size = new Size(106, 25);
            lblUv002Ip.TabIndex = 5;
            lblUv002Ip.Text = "IP Address :";
            // 
            // lblUv002
            // 
            lblUv002.BackColor = Color.Black;
            lblUv002.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblUv002.ForeColor = Color.White;
            lblUv002.Location = new Point(50, 191);
            lblUv002.Margin = new Padding(4, 0, 4, 0);
            lblUv002.Name = "lblUv002";
            lblUv002.Padding = new Padding(8, 4, 8, 4);
            lblUv002.Size = new Size(144, 41);
            lblUv002.TabIndex = 6;
            lblUv002.Text = "UV-002";
            lblUv002.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtUv001Ip
            // 
            txtUv001Ip.Font = new Font("Segoe UI", 10F);
            txtUv001Ip.Location = new Point(129, 136);
            txtUv001Ip.Margin = new Padding(4, 5, 4, 5);
            txtUv001Ip.Name = "txtUv001Ip";
            txtUv001Ip.Size = new Size(284, 31);
            txtUv001Ip.TabIndex = 7;
            // 
            // lblUv001Ip
            // 
            lblUv001Ip.AutoSize = true;
            lblUv001Ip.Font = new Font("Segoe UI", 10F);
            lblUv001Ip.Location = new Point(21, 140);
            lblUv001Ip.Margin = new Padding(4, 0, 4, 0);
            lblUv001Ip.Name = "lblUv001Ip";
            lblUv001Ip.Size = new Size(106, 25);
            lblUv001Ip.TabIndex = 8;
            lblUv001Ip.Text = "IP Address :";
            // 
            // lblUv001
            // 
            lblUv001.BackColor = Color.Black;
            lblUv001.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblUv001.ForeColor = Color.White;
            lblUv001.Location = new Point(50, 75);
            lblUv001.Margin = new Padding(4, 0, 4, 0);
            lblUv001.Name = "lblUv001";
            lblUv001.Padding = new Padding(8, 4, 8, 4);
            lblUv001.Size = new Size(144, 41);
            lblUv001.TabIndex = 9;
            lblUv001.Text = "UV-001";
            lblUv001.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUvTitle
            // 
            lblUvTitle.AutoSize = true;
            lblUvTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblUvTitle.Location = new Point(14, 16);
            lblUvTitle.Margin = new Padding(4, 0, 4, 0);
            lblUvTitle.Name = "lblUvTitle";
            lblUvTitle.Size = new Size(154, 38);
            lblUvTitle.TabIndex = 10;
            lblUvTitle.Text = "UV Printers";
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
            pnlMkPrinters.Controls.Add(txtMk061Com);
            pnlMkPrinters.Controls.Add(lblMk061Com);
            pnlMkPrinters.Controls.Add(lblMk061);
            pnlMkPrinters.Controls.Add(txtMk060Com);
            pnlMkPrinters.Controls.Add(lblMk060Com);
            pnlMkPrinters.Controls.Add(lblMk060);
            pnlMkPrinters.Controls.Add(txtMk059Com);
            pnlMkPrinters.Controls.Add(lblMk059Com);
            pnlMkPrinters.Controls.Add(lblMk059);
            pnlMkPrinters.Controls.Add(txtMk058Com);
            pnlMkPrinters.Controls.Add(lblMk058Com);
            pnlMkPrinters.Controls.Add(lblMk058);
            pnlMkPrinters.Controls.Add(lblMkTitle);
            pnlMkPrinters.Location = new Point(85, 39);
            pnlMkPrinters.Margin = new Padding(4, 5, 4, 5);
            pnlMkPrinters.Name = "pnlMkPrinters";
            pnlMkPrinters.Size = new Size(570, 344);
            pnlMkPrinters.TabIndex = 3;
            // 
            // lblMk061Status
            // 
            lblMk061Status.BackColor = Color.Gray;
            lblMk061Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk061Status.Location = new Point(21, 261);
            lblMk061Status.Margin = new Padding(4, 0, 4, 0);
            lblMk061Status.Name = "lblMk061Status";
            lblMk061Status.Size = new Size(21, 23);
            lblMk061Status.TabIndex = 0;
            // 
            // lblMk060Status
            // 
            lblMk060Status.BackColor = Color.Gray;
            lblMk060Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk060Status.Location = new Point(21, 202);
            lblMk060Status.Margin = new Padding(4, 0, 4, 0);
            lblMk060Status.Name = "lblMk060Status";
            lblMk060Status.Size = new Size(21, 23);
            lblMk060Status.TabIndex = 1;
            // 
            // lblMk059Status
            // 
            lblMk059Status.BackColor = Color.Gray;
            lblMk059Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk059Status.Location = new Point(21, 143);
            lblMk059Status.Margin = new Padding(4, 0, 4, 0);
            lblMk059Status.Name = "lblMk059Status";
            lblMk059Status.Size = new Size(21, 23);
            lblMk059Status.TabIndex = 2;
            // 
            // lblMk058Status
            // 
            lblMk058Status.BackColor = Color.Gray;
            lblMk058Status.BorderStyle = BorderStyle.FixedSingle;
            lblMk058Status.Location = new Point(21, 84);
            lblMk058Status.Margin = new Padding(4, 0, 4, 0);
            lblMk058Status.Name = "lblMk058Status";
            lblMk058Status.Size = new Size(21, 23);
            lblMk058Status.TabIndex = 3;
            // 
            // btnEditMk061
            // 
            btnEditMk061.FlatAppearance.BorderSize = 0;
            btnEditMk061.FlatStyle = FlatStyle.Flat;
            btnEditMk061.Font = new Font("Segoe UI", 9F);
            btnEditMk061.Location = new Point(196, 252);
            btnEditMk061.Margin = new Padding(4, 5, 4, 5);
            btnEditMk061.Name = "btnEditMk061";
            btnEditMk061.Size = new Size(36, 41);
            btnEditMk061.TabIndex = 4;
            btnEditMk061.Text = "✎";
            // 
            // btnEditMk060
            // 
            btnEditMk060.FlatAppearance.BorderSize = 0;
            btnEditMk060.FlatStyle = FlatStyle.Flat;
            btnEditMk060.Font = new Font("Segoe UI", 9F);
            btnEditMk060.Location = new Point(196, 193);
            btnEditMk060.Margin = new Padding(4, 5, 4, 5);
            btnEditMk060.Name = "btnEditMk060";
            btnEditMk060.Size = new Size(36, 41);
            btnEditMk060.TabIndex = 5;
            btnEditMk060.Text = "✎";
            // 
            // btnEditMk059
            // 
            btnEditMk059.FlatAppearance.BorderSize = 0;
            btnEditMk059.FlatStyle = FlatStyle.Flat;
            btnEditMk059.Font = new Font("Segoe UI", 9F);
            btnEditMk059.Location = new Point(196, 134);
            btnEditMk059.Margin = new Padding(4, 5, 4, 5);
            btnEditMk059.Name = "btnEditMk059";
            btnEditMk059.Size = new Size(36, 41);
            btnEditMk059.TabIndex = 6;
            btnEditMk059.Text = "✎";
            // 
            // btnEditMk058
            // 
            btnEditMk058.FlatAppearance.BorderSize = 0;
            btnEditMk058.FlatStyle = FlatStyle.Flat;
            btnEditMk058.Font = new Font("Segoe UI", 9F);
            btnEditMk058.Location = new Point(196, 75);
            btnEditMk058.Margin = new Padding(4, 5, 4, 5);
            btnEditMk058.Name = "btnEditMk058";
            btnEditMk058.Size = new Size(36, 41);
            btnEditMk058.TabIndex = 7;
            btnEditMk058.Text = "✎";
            // 
            // txtMk061Com
            // 
            txtMk061Com.Font = new Font("Segoe UI", 10F);
            txtMk061Com.Location = new Point(340, 257);
            txtMk061Com.Margin = new Padding(4, 5, 4, 5);
            txtMk061Com.Name = "txtMk061Com";
            txtMk061Com.Size = new Size(170, 31);
            txtMk061Com.TabIndex = 10;
            // 
            // lblMk061Com
            // 
            lblMk061Com.AutoSize = true;
            lblMk061Com.Font = new Font("Segoe UI", 10F);
            lblMk061Com.Location = new Point(232, 261);
            lblMk061Com.Margin = new Padding(4, 0, 4, 0);
            lblMk061Com.Name = "lblMk061Com";
            lblMk061Com.Size = new Size(106, 25);
            lblMk061Com.TabIndex = 11;
            lblMk061Com.Text = "IP Address :";
            // 
            // lblMk061
            // 
            lblMk061.BackColor = Color.Black;
            lblMk061.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMk061.ForeColor = Color.White;
            lblMk061.Location = new Point(50, 252);
            lblMk061.Margin = new Padding(4, 0, 4, 0);
            lblMk061.Name = "lblMk061";
            lblMk061.Padding = new Padding(8, 4, 8, 4);
            lblMk061.Size = new Size(144, 41);
            lblMk061.TabIndex = 12;
            lblMk061.Text = "MK-061";
            lblMk061.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk060Com
            // 
            txtMk060Com.Font = new Font("Segoe UI", 10F);
            txtMk060Com.Location = new Point(340, 198);
            txtMk060Com.Margin = new Padding(4, 5, 4, 5);
            txtMk060Com.Name = "txtMk060Com";
            txtMk060Com.Size = new Size(170, 31);
            txtMk060Com.TabIndex = 15;
            // 
            // lblMk060Com
            // 
            lblMk060Com.AutoSize = true;
            lblMk060Com.Font = new Font("Segoe UI", 10F);
            lblMk060Com.Location = new Point(232, 202);
            lblMk060Com.Margin = new Padding(4, 0, 4, 0);
            lblMk060Com.Name = "lblMk060Com";
            lblMk060Com.Size = new Size(106, 25);
            lblMk060Com.TabIndex = 16;
            lblMk060Com.Text = "IP Address :";
            // 
            // lblMk060
            // 
            lblMk060.BackColor = Color.Black;
            lblMk060.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMk060.ForeColor = Color.White;
            lblMk060.Location = new Point(50, 193);
            lblMk060.Margin = new Padding(4, 0, 4, 0);
            lblMk060.Name = "lblMk060";
            lblMk060.Padding = new Padding(8, 4, 8, 4);
            lblMk060.Size = new Size(144, 41);
            lblMk060.TabIndex = 17;
            lblMk060.Text = "MK-060";
            lblMk060.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk059Com
            // 
            txtMk059Com.Font = new Font("Segoe UI", 10F);
            txtMk059Com.Location = new Point(340, 139);
            txtMk059Com.Margin = new Padding(4, 5, 4, 5);
            txtMk059Com.Name = "txtMk059Com";
            txtMk059Com.Size = new Size(170, 31);
            txtMk059Com.TabIndex = 20;
            // 
            // lblMk059Com
            // 
            lblMk059Com.AutoSize = true;
            lblMk059Com.Font = new Font("Segoe UI", 10F);
            lblMk059Com.Location = new Point(232, 143);
            lblMk059Com.Margin = new Padding(4, 0, 4, 0);
            lblMk059Com.Name = "lblMk059Com";
            lblMk059Com.Size = new Size(106, 25);
            lblMk059Com.TabIndex = 21;
            lblMk059Com.Text = "IP Address :";
            // 
            // lblMk059
            // 
            lblMk059.BackColor = Color.Black;
            lblMk059.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMk059.ForeColor = Color.White;
            lblMk059.Location = new Point(50, 134);
            lblMk059.Margin = new Padding(4, 0, 4, 0);
            lblMk059.Name = "lblMk059";
            lblMk059.Padding = new Padding(8, 4, 8, 4);
            lblMk059.Size = new Size(144, 41);
            lblMk059.TabIndex = 22;
            lblMk059.Text = "MK-059";
            lblMk059.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMk058Com
            // 
            txtMk058Com.Font = new Font("Segoe UI", 10F);
            txtMk058Com.Location = new Point(340, 80);
            txtMk058Com.Margin = new Padding(4, 5, 4, 5);
            txtMk058Com.Name = "txtMk058Com";
            txtMk058Com.Size = new Size(170, 31);
            txtMk058Com.TabIndex = 25;
            // 
            // lblMk058Com
            // 
            lblMk058Com.AutoSize = true;
            lblMk058Com.Font = new Font("Segoe UI", 10F);
            lblMk058Com.Location = new Point(232, 84);
            lblMk058Com.Margin = new Padding(4, 0, 4, 0);
            lblMk058Com.Name = "lblMk058Com";
            lblMk058Com.Size = new Size(106, 25);
            lblMk058Com.TabIndex = 26;
            lblMk058Com.Text = "IP Address :";
            // 
            // lblMk058
            // 
            lblMk058.BackColor = Color.Black;
            lblMk058.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMk058.ForeColor = Color.White;
            lblMk058.Location = new Point(50, 75);
            lblMk058.Margin = new Padding(4, 0, 4, 0);
            lblMk058.Name = "lblMk058";
            lblMk058.Padding = new Padding(8, 4, 8, 4);
            lblMk058.Size = new Size(144, 41);
            lblMk058.TabIndex = 27;
            lblMk058.Text = "MK-058";
            lblMk058.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMkTitle
            // 
            lblMkTitle.AutoSize = true;
            lblMkTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblMkTitle.Location = new Point(14, 16);
            lblMkTitle.Margin = new Padding(4, 0, 4, 0);
            lblMkTitle.Name = "lblMkTitle";
            lblMkTitle.Size = new Size(244, 38);
            lblMkTitle.TabIndex = 28;
            lblMkTitle.Text = "MK Inkjet Printers";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // ucSetting
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMainContent);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ucSetting";
            Size = new Size(1152, 939);
            panelPcStation1.ResumeLayout(false);
            panelPcStation1.PerformLayout();
            pnlMainContent.ResumeLayout(false);
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
        private System.Windows.Forms.Panel pnlButtons;

        // MK Printers
        private System.Windows.Forms.Label lblMkTitle;
        private System.Windows.Forms.Label lblMk058Status;
        private System.Windows.Forms.Label lblMk058;
        private System.Windows.Forms.Button btnEditMk058;
        private System.Windows.Forms.Label lblMk058Com;
        private System.Windows.Forms.TextBox txtMk058Com;
        private System.Windows.Forms.Label lblMk059Status;
        private System.Windows.Forms.Label lblMk059;
        private System.Windows.Forms.Button btnEditMk059;
        private System.Windows.Forms.Label lblMk059Com;
        private System.Windows.Forms.TextBox txtMk059Com;
        private System.Windows.Forms.Label lblMk060Status;
        private System.Windows.Forms.Label lblMk060;
        private System.Windows.Forms.Button btnEditMk060;
        private System.Windows.Forms.Label lblMk060Com;
        private System.Windows.Forms.TextBox txtMk060Com;
        private System.Windows.Forms.Label lblMk061Status;
        private System.Windows.Forms.Label lblMk061;
        private System.Windows.Forms.Button btnEditMk061;
        private System.Windows.Forms.Label lblMk061Com;
        private System.Windows.Forms.TextBox txtMk061Com;

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

        // Buttons
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private Panel panelPcStation1;
        private Label lblPcStatus;
        private Button btnPC2;
        private Label lblPC2;
        private Label label3;
        private TextBox txtPcip;
        private Label label4;
        private System.Windows.Forms.Timer timer1;
    }
}