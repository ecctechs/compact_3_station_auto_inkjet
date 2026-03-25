using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class frmCreateJob : Form
    {
        private ApiClient _api;

        public frmCreateJob(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        private void frmCreateJob_Load(object sender, EventArgs e)
        {
            cmbType.Items.AddRange(new string[]
            {
                "กล่อง",
                "ถุง",
                "ขวด",
                "อื่นๆ"
            });

            cmbType.SelectedIndex = 0;
        }

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                var req = new CreateJobRequest
                {
                    BarcodeRaw = "BARCODE123", // หรือ generate
                    CreatedBy = "operator",     // อาจเอาจาก login

                    OrderNo = txtOrderNo.Text.Trim(),
                    CustomerName = txtCustomerName.Text.Trim(),
                    Type = cmbType.SelectedItem?.ToString(),
                    Qty = (int)numQty.Value
                };

                // validation
                if (string.IsNullOrWhiteSpace(req.OrderNo))
                {
                    MessageBox.Show("กรุณาใส่ Order No");
                    return;
                }

                var created = await _api.CreateJobAsync(req);

                if (created != null)
                {
                    MessageBox.Show($"สร้าง Job สำเร็จ ID: {created.Id}");
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Create job failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtOrderNo.Text = "";
            txtCustomerName.Text = "";
            cmbType.SelectedIndex = 0;
            numQty.Value = 1;
        }
    }
}