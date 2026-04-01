using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl
    {
        private readonly ApiClient _api = ApiProvider.Instance;
        private readonly SqliteDataService _sqliteService = new SqliteDataService();
        private ResolvedJobResponse _currentResolved;

        public ucOrder()
        {
            InitializeComponent();
        }

        public async void get_job()
        {
            try
            {
                var jobs = await _api.GetPendingJobsAsync();
                bindingSource1.DataSource = jobs;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        private async void dgvList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (bindingSource1.Current is PrintJob selectedJob)
            {
                txtBarcode.Text = selectedJob.BarcodeRaw;
                txtLot.Text = selectedJob.LotNumber;
                txtStatus.Text = selectedJob.Status;
                txtPattern.Text = selectedJob.PatternNoErp;
                await query_db3_async();
            }

            if (_currentResolved?.Pattern?.InkjetConfigs != null)
            {
                // Bind รายการหัวพิมพ์ (MK1, MK2)
                bindSourceInkjetConfigDto.DataSource = _currentResolved.Pattern.InkjetConfigs;
            }
        }

        // --- เริ่มส่วนที่ปรับปรุงใหม่ ---
        private async Task query_db3_async()
        {
            string patternNo = txtPattern.Text.Trim();

            // เรียกใช้ Service แทนการเขียนเอง
            var pattern = await _sqliteService.GetPatternDetailAsync(patternNo);

            if (pattern != null)
            {
                _currentResolved = new ResolvedJobResponse
                {
                    Job = bindingSource1.Current as InkjetOperator.Models.PrintJob,
                    Pattern = pattern
                };

                bindSourceInkjetConfigDto.DataSource = pattern.InkjetConfigs;
            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูลในระบบ SQLite");
            }
        }

        private void dgvConfigs_CellClick(object sender, DataGridViewCellEventArgs e) =>
                bindingSourceTextBlockDto.DataSource = (bindSourceInkjetConfigDto.Current as InkjetConfigDto)?.TextBlocks;

        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            // หยุด Timer ไว้ก่อนเพื่อกันการทำงานซ้อนกัน (ถ้าเน็ตช้า)
            timerPoll.Stop();

            try
            {
                  get_job();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Polling Error: " + ex.Message);
            }
            finally
            {
                // รันเสร็จแล้วค่อยเริ่มนับ 5 วิใหม่
                timerPoll.Start();
            }
        }
    }
}