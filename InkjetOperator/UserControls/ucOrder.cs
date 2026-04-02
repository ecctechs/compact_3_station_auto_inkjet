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
                //await query_db3_async();
                await QueryBackendJobAsync();
            }

            if (_currentResolved?.Pattern?.InkjetConfigs != null)
            {
                // Bind รายการหัวพิมพ์ (MK1, MK2)
                bindSourceInkjetConfigDto.DataSource = _currentResolved.Pattern.InkjetConfigs;
            }
        }

        // --- เริ่มส่วนที่ปรับปรุงใหม่ ---
        //private async Task query_db3_async()
        //{
        //    string patternNo = txtPattern.Text.Trim();

        //    // เรียกใช้ Service แทนการเขียนเอง
        //    var pattern = await _sqliteService.GetPatternDetailAsync(patternNo);

        //    if (pattern != null)
        //    {
        //        // Bind ข้อมูลปกติ
        //        bindSourceInkjetConfigDto.DataSource = pattern.InkjetConfigs;

        //        // ถ้าต้องการให้ Grid อัปเดตทันที
        //        bindingSourceUVinkjet.ResetBindings(false);
        //    }
        //    else
        //    {
        //        MessageBox.Show("ไม่พบข้อมูลในระบบ SQLite");
        //    }
        //}

        private async Task QueryBackendJobAsync()
        {
            // 1. ดึง Job ID จาก BindingSource ที่เลือกอยู่ (Current Row)
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อน");
                return;
            }

            try
            {
                // เปลี่ยนจาก SQLite เป็นการเรียก Resolved Job จาก API
                // ข้อมูลที่ได้ (resolvedJob) จะมี Property .InkjetConfigs อยู่ข้างในตามโครงสร้าง DTO
                var resolvedJob = await _api.GetResolvedJobAsync(selectedJob.Id);

                if (resolvedJob != null)
                {
                    // 1. Bind ข้อมูล Configs เข้ากับ BindingSource หลัก
                    // Backend มักจะส่งมาเป็น List<InkjetConfigDto>
                    bindSourceInkjetConfigDto.DataSource = resolvedJob.Pattern.InkjetConfigs;

                    // 2. สั่งรีเฟรช Grid เพื่อแสดงค่าใหม่
                    bindingSourceUVinkjet.ResetBindings(false);

                    // 3. (Option) ถ้าต้องการแสดงข้อมูล Job อื่นๆ เช่น OrderNo, Customer
                    // txtOrderNo.Text = resolvedJob.OrderNo;
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูล Job หรือ Pattern นี้ในระบบ Backend", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    bindSourceInkjetConfigDto.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching resolved job: {ex.Message}");
                MessageBox.Show("เกิดข้อผิดพลาดในการเชื่อมต่อกับ Backend");
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

        private async void btnSendUV1_Click(object sender, EventArgs e)
        {
            // 1. ดึง Job จาก BindingSource หลัก (รายการ Job)
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อน");
                return;
            }
            MessageBox.Show($"กำลังส่งข้อมูลการพิมพ์ UV สำหรับ Job ID: {selectedJob.Id}"); // Debugging Message

            // 2. ดึงข้อมูลจาก bindingSourceUVinkjet แถวที่ 1 (Index 0)
            // ตรวจสอบก่อนว่าใน List มีข้อมูลอย่างน้อย 1 แถวหรือไม่
            if (bindingSourceUVinkjet.Count == 0)
            {
                MessageBox.Show("ไม่พบข้อมูล Config ของเครื่องพิมพ์");
                return;
            }
            MessageBox.Show($"กำลังส่งข้อมูลการพิมพ์ UV สำหรับ Job ID: {selectedJob.Id}"); // Debugging Message

            // ดึงข้อมูลแถวที่ 1 มาเก็บไว้ในตัวแปร (สมมติว่า Model คือ InkjetConfigDto)
            var firstConfig = bindingSourceUVinkjet[0] as UVinkjet;

            if (firstConfig == null) return;

            // เตรียมตัวแปร (อ้างอิงตาม Property ใน InkjetConfigDto และ PrintJob)
            string currentLot1 = firstConfig.Lot ?? "";
            string currentName1 = firstConfig.Name ?? "";
            string programName1 = firstConfig.ProgramName ?? "";

            MessageBox.Show($"ข้อมูลที่จะส่ง: Lot={currentLot1}, Name={currentName1}, Program={programName1}"); // Debugging Message

            // 3. เตรียมข้อมูลส่ง API
            var uvRequest = new UVinkjet
            {
                PrintJobsId = selectedJob.Id,
                InkjetName = "UV Printer 1",
                Lot = currentLot1,
                Name = currentName1,
                ProgramName = programName1,
                Status = "printing",
                Station = "2"
            };

            // 4. เรียก API บันทึกข้อมูล
            btnSendUV1.Enabled = false; // ป้องกันการกดซ้ำระหว่างรอ Network
            try
            {
                bool isSaved = await _api.CreateUvInkjetAsync(uvRequest);

                if (isSaved)
                {
                    var updatePayload = new { st_status = 2 };
                    bool isJobUpdated = await _api.UpdateJobAsync(selectedJob.Id, updatePayload);

                    Debug.WriteLine("บันทึกข้อมูลการพิมพ์ UV สำเร็จ");
                    // อาจจะเพิ่ม MessageBox แสดงความยินดีที่นี่
                }
                else
                {
                    MessageBox.Show("ไม่สามารถบันทึกสถานะการพิมพ์ได้ (Server Error)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}");
            }
            finally
            {
                btnSendUV1.Enabled = true;
            }
        }
    }
}