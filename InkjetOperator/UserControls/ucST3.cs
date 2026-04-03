using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InkjetOperator.Adapters;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.UserControls
{
    public partial class ucST3 : UserControl
    {
        /// <summary>ใช้จำว่า first load เพื่อ auto-select row แรกครั้งเดียว</summary>
        private bool _isFirstLoad = true;
        private readonly ApiClient _api = ApiProvider.Instance;
        public ucST3()
        {
            InitializeComponent();
        }

        public async void get_job()
        {
            try
            {
                // จำ Job Id ที่เลือกอยู่ก่อน bind ใหม่
                int? selectedJobId = (bindingSource1.Current as PrintJob)?.Id;

                // 1. ดึงข้อมูลทั้งหมดจาก API
                var allJobs = await _api.GetPendingJobsAsync();

                // 2. กรองข้อมูล (Station >= 3 AND Status == "Waiting")
                var jobs = allJobs.Where(j =>
                    // เงื่อนไข Station 3 ขึ้นไป
                    j.stations_required != null && j.stations_required.Any(s => s >= 3) &&
                    // เพิ่มเงื่อนไข Status ต้องเป็น Waiting เท่านั้น
                    !string.IsNullOrEmpty(j.Status) && j.Status.Equals("Waiting", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                // นำข้อมูลที่กรองแล้วใส่ BindingSource
                bindingSource1.DataSource = jobs;

                // --- ส่วนที่เหลือคงเดิม (Restore Position) ---
                if (_isFirstLoad)
                {
                    _isFirstLoad = false;
                    if (bindingSource1.Count > 0)
                    {
                        bindingSource1.Position = 0;
                        SelectGridRow(dgvList, 0);
                    }
                    return;
                }

                if (selectedJobId.HasValue)
                {
                    int idx = jobs.FindIndex(j => j.Id == selectedJobId.Value);
                    if (idx >= 0)
                    {
                        bindingSource1.Position = idx;
                        return;
                    }
                }

                if (bindingSource1.Count > 0)
                    bindingSource1.Position = 0;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        private static void SelectGridRow(DataGridView dgv, int index)
        {
            if (dgv.Rows.Count == 0 || index < 0 || index >= dgv.Rows.Count) return;
            dgv.ClearSelection();
            dgv.Rows[index].Selected = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            get_job();
        }

        private async void btnRunBot_Click(object sender, EventArgs e)
        {
            // 1. ดึง Job ที่เลือกอยู่
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. แสดง Popup ยืนยันก่อนเริ่มทำงาน
            string confirmMsg = $"คุณต้องการส่ง Job ID: {selectedJob.Id}\n" +
                                $"ไปที่ Station 1 ใช่หรือไม่?";

            DialogResult dialogResult = MessageBox.Show(confirmMsg, "ยืนยันการดำเนินการ",
                                                       MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Question);

            // ถ้า Operator กด "No" ให้หยุดการทำงานทันที
            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                // เริ่มทำงาน (ปิดปุ่มป้องกันกดซ้ำ)
                btnRunBot.Enabled = false;

                int jobId = selectedJob.Id;
                string newStatus = "Request";

                // 3. อัปเดตไปยัง API
                var updateData = new { st1_confirmation = newStatus , st1_send_time = DateTime.Now };
                bool isUpdated = await _api.UpdateJobAsync(jobId, updateData);

                if (isUpdated)
                {
                    // 4. อัปเดต UI
                    selectedJob.Status = newStatus;
                    bindingSource1.ResetCurrentItem();

                    MessageBox.Show($"ดำเนินการส่ง Job ID: {jobId} เรียบร้อยแล้ว",
                                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ไม่สามารถอัปเดตสถานะได้ (API Error)", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunBot.Enabled = true;
            }
        }
    }
}
