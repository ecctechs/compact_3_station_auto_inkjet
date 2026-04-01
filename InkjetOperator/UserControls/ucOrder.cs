using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl
    {
        private readonly ApiClient _api = ApiProvider.Instance;
        private readonly SqliteDataService _sqliteService = new SqliteDataService();
        private ResolvedJobResponse _currentResolved;

        private MkCompactAdapter _inkjetAdapter;
        private TcpManager _tcpManager;

        public ucOrder()
        {
            InitializeComponent();
            get_uv();

            // 1. สร้าง Manager
            _tcpManager = new TcpManager();

            // 2. ส่ง Manager เข้าไปใน Adapter
            _inkjetAdapter = new MkCompactAdapter(_tcpManager);
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

        public async void get_uv()
        {
            try
            {
                var uvLogs = await _sqliteService.GetUvPrintDataAsync();
                bindingSourceUVinkjet.DataSource = uvLogs;
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
                // Bind ข้อมูลปกติ
                bindSourceInkjetConfigDto.DataSource = pattern.InkjetConfigs;

                // ถ้าต้องการให้ Grid อัปเดตทันที
                bindingSourceUVinkjet.ResetBindings(false);
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

        private async void btnSendMk1Mk2_Click(object sender, EventArgs e)
        {
            try
            {
                // ใช้ IP/Port ที่คุณระบุไว้
                await _tcpManager.ConnectAsync("192.168.3.77", 9004);

                if (_inkjetAdapter.IsConnected())
                {                   
                    if (bindSourceInkjetConfigDto.Current is InkjetConfigDto config)
                    {
                        await _inkjetAdapter.ChangeProgramAsync(config.ProgramNumber ?? 1);

                        // 1. ส่งการตั้งค่าหลัก (FM Command)
                        await _inkjetAdapter.SendConfigAsync(config);

                        // 2. ส่งข้อความพิมพ์ (FS + F1 Commands)
                        if (config.TextBlocks != null)
                        {
                            foreach (var block in config.TextBlocks)
                            {
                                // deviceBlock คือลำดับบล็อกในเครื่องพิมพ์ (เช่น 1, 2, 3...)
                                await _inkjetAdapter.SendTextBlockAsync(block, block.BlockNumber);
                            }
                        }

                        MessageBox.Show("ส่งข้อมูลชุดคำสั่งไปยังเครื่องพิมพ์เรียบร้อยแล้ว");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เชื่อมต่อไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}