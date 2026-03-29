using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class frmSt3 : Form
    {
        private ApiClient _api;
        public frmSt3()
        {
            InitializeComponent();
            _api = new ApiClient("http://localhost:3000");
        }

        private async void frmSt3_Load(object sender, EventArgs e)
        {
            try
            {
                // Await the async API call, map PrintJob -> JobRow and bind to the BindingSource
                var jobs = await _api.GetPendingJobsAsync();

                var jobRows = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(
                    jobs.Select(j => new InkjetOperator.Models.JobRow
                    {
                        Id = j.Id,
                        BarcodeRaw = j.BarcodeRaw,
                        LotNumber = j.LotNumber ?? string.Empty,
                        Status = j.Status,
                        Attempt = j.Attempt
                    }).ToList()
                );

                printJobBindingSource.DataSource = jobRows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load pending jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // ดึง Row ที่เลือกจาก BindingSource
            if (printJobBindingSource.Current is InkjetOperator.Models.JobRow selectedRow)
            {
                var payload = new
                {
                    barcode_raw = selectedRow.BarcodeRaw,
                    pattern_id = 41, // ปรับตาม logic ของคุณ
                    lot_number = selectedRow.LotNumber,
                    status = selectedRow.Status,
                    created_by = "operator1",
                    attempt = selectedRow.Attempt,
                    order_no = "ORD-001",
                    customer_name = "Customer A",
                    type = "print",
                    qty = 100,
                    sent_time = DateTime.UtcNow, // ส่งเป็น DateTime ไปเลย PostAsJsonAsync จะจัดการ format ให้
                    st_status = "sent"
                };

                bool success = await _api.CreateLastSentJobAsync(payload);

                if (success)
                {
                    MessageBox.Show("ส่งข้อมูลไปที่ LastSent สำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ส่งข้อมูลไม่สำเร็จ กรุณาตรวจสอบ Log", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadLastSentData()
        {
            try
            {
                // เรียกใช้ API (ระบุ status เป็น sent, หน้า 1, จำนวน 10 รายการ)
                var lastSentJobs = await _api.GetLastSentJobsAsync("sent", 1, 10);

                // นำข้อมูลไปผูกกับ BindingSource หรือ DataGridView
                var jobRows = lastSentJobs.Select(j => new InkjetOperator.Models.JobRow
                {
                    Id = j.Id,
                    BarcodeRaw = j.BarcodeRaw,
                    LotNumber = j.LotNumber ?? string.Empty,
                    Status = j.Status,
                    Attempt = j.Attempt
                    // เพิ่ม field อื่นๆ ที่ต้องการแสดงในตาราง history
                }).ToList();

                printJobBindingSource.DataSource = new System.ComponentModel.BindingList<InkjetOperator.Models.JobRow>(jobRows);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"โหลดข้อมูลประวัติไม่สำเร็จ: {ex.Message}", "Error");
            }
        }

    
    }
}
