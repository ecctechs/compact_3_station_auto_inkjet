using System;
using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucSettingDB3 : UserControl
    {
        public ucSettingDB3()
        {
            InitializeComponent();
            LoadData();
        }

        // ================= LOAD =================
        private void LoadData()
        {
            txtDbPath.Text = CustomSettingsManager.GetValue("DB_PATH") ?? "";
        }

        // ================= BROWSE =================
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                dlg.Title = "Select Database File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtDbPath.Text = dlg.FileName;
                }
            }
        }

        // ================= SAVE =================
        private void btnSave_Click(object sender, EventArgs e)
        {
            CustomSettingsManager.SetValue("DB_PATH", txtDbPath.Text);

            MessageBox.Show("บันทึกเรียบร้อย", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ================= CANCEL =================
        private void btnCancel_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {

        }
    }
}