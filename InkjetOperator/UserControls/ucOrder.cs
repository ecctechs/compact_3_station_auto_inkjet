using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl
    {
        public ucOrder()
        {
            InitializeComponent();
            SetupGrid(dgvList);
            SetupGrid(dgvHistory);

            LoadMockData();
        }

        private void SetupGrid(DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.RowTemplate.Height = 35;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;

            dgv.Columns.Clear();
            dgv.Columns.Add("OrderNo", "Order No.");
            dgv.Columns.Add("Customer", "Customer");
            dgv.Columns.Add("Type", "Type");
            dgv.Columns.Add("Qty", "Qty");
            dgv.Columns.Add("Status", "Status");

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadMockData()
        {
            // LIST TAB
            dgvList.Rows.Add("XX88888", "ECC Solutions", "I", "500", "Processing");
            dgvList.Rows.Add("YY77777", "Compact Brake", "O", "1000", "Waiting");
            dgvList.Rows.Add("YY99999", "Compact Brake2", "O", "1500", "Waiting");

            // HISTORY TAB
            dgvHistory.Rows.Add("AA11111", "Old Customer", "I", "200", "Done");
            dgvHistory.Rows.Add("BB22222", "Old Customer2", "O", "300", "Done");
            dgvHistory.Rows.Add("CC33333", "Old Customer3", "I", "150", "Done");

            StyleRows(dgvList);
            StyleRows(dgvHistory);
        }

        private void StyleRows(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string status = row.Cells["Status"].Value?.ToString() ?? "";

                if (status == "Processing")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(180, 200, 160);
                }
                else if (status == "Waiting")
                {
                    row.Cells["Status"].Style.ForeColor = Color.Red;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Start clicked");
        }
    }
}