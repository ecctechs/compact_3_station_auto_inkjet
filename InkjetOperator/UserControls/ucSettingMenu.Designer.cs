namespace InkjetOperator.UserControls
{
    partial class ucSettingMenu
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            pnlLeftMenu = new Panel();
            btnUvSetting = new Button();
            btnPlcSetting = new Button();
            btnIpAddressSetting = new Button();
            btnDatabaseSetting = new Button();
            panelSettingShow = new Panel();
            pnlLeftMenu.SuspendLayout();
            SuspendLayout();
            //
            // pnlLeftMenu
            //
            pnlLeftMenu.BackColor = Color.FromArgb(230, 240, 250);
            pnlLeftMenu.Controls.Add(btnUvSetting);
            pnlLeftMenu.Controls.Add(btnPlcSetting);
            pnlLeftMenu.Controls.Add(btnIpAddressSetting);
            pnlLeftMenu.Controls.Add(btnDatabaseSetting);
            pnlLeftMenu.Dock = DockStyle.Left;
            pnlLeftMenu.Location = new Point(0, 0);
            pnlLeftMenu.Margin = new Padding(3, 4, 3, 4);
            pnlLeftMenu.Name = "pnlLeftMenu";
            pnlLeftMenu.Size = new Size(229, 939);
            pnlLeftMenu.TabIndex = 1;
            //
            // btnUvSetting
            //
            btnUvSetting.Dock = DockStyle.Top;
            btnUvSetting.FlatAppearance.BorderSize = 0;
            btnUvSetting.FlatStyle = FlatStyle.Flat;
            btnUvSetting.Font = new Font("Segoe UI", 13F);
            btnUvSetting.Location = new Point(0, 201);
            btnUvSetting.Margin = new Padding(3, 4, 3, 4);
            btnUvSetting.Name = "btnUvSetting";
            btnUvSetting.Size = new Size(229, 67);
            btnUvSetting.TabIndex = 3;
            btnUvSetting.Text = "UV Printer Setting";
            btnUvSetting.UseVisualStyleBackColor = true;
            btnUvSetting.Click += btnUvSetting_Click;
            //
            // btnPlcSetting
            //
            btnPlcSetting.Dock = DockStyle.Top;
            btnPlcSetting.FlatAppearance.BorderSize = 0;
            btnPlcSetting.FlatStyle = FlatStyle.Flat;
            btnPlcSetting.Font = new Font("Segoe UI", 13F);
            btnPlcSetting.Location = new Point(0, 134);
            btnPlcSetting.Margin = new Padding(3, 4, 3, 4);
            btnPlcSetting.Name = "btnPlcSetting";
            btnPlcSetting.Size = new Size(229, 67);
            btnPlcSetting.TabIndex = 2;
            btnPlcSetting.Text = "PLC Setting";
            btnPlcSetting.UseVisualStyleBackColor = true;
            btnPlcSetting.Click += btnPlcSetting_Click;
            //
            // btnIpAddressSetting
            //
            btnIpAddressSetting.BackColor = Color.FromArgb(50, 100, 180);
            btnIpAddressSetting.Dock = DockStyle.Top;
            btnIpAddressSetting.FlatAppearance.BorderSize = 0;
            btnIpAddressSetting.FlatStyle = FlatStyle.Flat;
            btnIpAddressSetting.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            btnIpAddressSetting.ForeColor = Color.White;
            btnIpAddressSetting.Location = new Point(0, 67);
            btnIpAddressSetting.Margin = new Padding(3, 4, 3, 4);
            btnIpAddressSetting.Name = "btnIpAddressSetting";
            btnIpAddressSetting.Size = new Size(229, 67);
            btnIpAddressSetting.TabIndex = 0;
            btnIpAddressSetting.Text = "IP Address Setting";
            btnIpAddressSetting.UseVisualStyleBackColor = false;
            btnIpAddressSetting.Click += btnIpAddressSetting_Click;
            // 
            // btnDatabaseSetting
            // 
            btnDatabaseSetting.Dock = DockStyle.Top;
            btnDatabaseSetting.FlatAppearance.BorderSize = 0;
            btnDatabaseSetting.FlatStyle = FlatStyle.Flat;
            btnDatabaseSetting.Font = new Font("Segoe UI", 13F);
            btnDatabaseSetting.Location = new Point(0, 0);
            btnDatabaseSetting.Margin = new Padding(3, 4, 3, 4);
            btnDatabaseSetting.Name = "btnDatabaseSetting";
            btnDatabaseSetting.Size = new Size(229, 67);
            btnDatabaseSetting.TabIndex = 1;
            btnDatabaseSetting.Text = "Database Setting";
            btnDatabaseSetting.UseVisualStyleBackColor = true;
            btnDatabaseSetting.Click += btnDatabaseSetting_Click;
            // 
            // panelSettingShow
            // 
            panelSettingShow.Location = new Point(228, 0);
            panelSettingShow.Name = "panelSettingShow";
            panelSettingShow.Size = new Size(924, 800);
            panelSettingShow.TabIndex = 2;
            // 
            // ucSettingMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelSettingShow);
            Controls.Add(pnlLeftMenu);
            Name = "ucSettingMenu";
            Size = new Size(1152, 939);
            pnlLeftMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlLeftMenu;
        private Button btnUvSetting;
        private Button btnPlcSetting;
        private Button btnIpAddressSetting;
        private Button btnDatabaseSetting;
        private Panel panelSettingShow;
    }
}
