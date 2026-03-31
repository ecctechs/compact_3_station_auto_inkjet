using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InkjetOperator.UserControls
{
    public partial class ucSettingMenu : UserControl
    {
        public ucSettingMenu()
        {
            InitializeComponent();
            ShowSetting();
        }

        private void btnIpAddressSetting_Click(object sender, EventArgs e)
        {
            ShowSetting();
        }

        private void ShowSetting()
        {
            // ลบ control เดิมใน panelSetting (ถ้ามี)
            panelSettingShow.Controls.Clear();

            // สร้าง instance ของ ucSettingIP
            var uc = new ucSetting();
            uc.Dock = DockStyle.Fill;

            // เพิ่มเข้า panelSetting
            panelSettingShow.Controls.Add(uc);
            SetActiveMenuButton(btnIpAddressSetting);
        }

        private void btnDatabaseSetting_Click(object sender, EventArgs e)
        {
            // ลบ control เดิมใน panelSetting (ถ้ามี)
            panelSettingShow.Controls.Clear();

            // สร้าง instance ของ ucSettingIP
            var uc = new ucSettingDB3();
            uc.Dock = DockStyle.Fill;

            // เพิ่มเข้า panelSetting
            panelSettingShow.Controls.Add(uc);
            SetActiveMenuButton(btnDatabaseSetting); 
        }

        private void SetActiveMenuButton(Button activeBtn)
        {
            foreach (Control c in pnlLeftMenu.Controls)
            {
                if (c is Button btn)
                {
                    if (btn == activeBtn)
                    {
                        // Active style
                        btn.BackColor = Color.FromArgb(50, 100, 180);
                        btn.ForeColor = Color.White;
                        btn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                        btn.UseVisualStyleBackColor = false;
                    }
                    else
                    {
                        // Inactive style
                        btn.BackColor = Color.White;
                        btn.ForeColor = Color.FromArgb(60, 60, 60);
                        btn.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
                        btn.UseVisualStyleBackColor = true;
                    }
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
            }
        }
    }
}
