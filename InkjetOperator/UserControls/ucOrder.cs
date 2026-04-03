using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
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

        /// <summary>ใช้จำว่า first load เพื่อ auto-select row แรกครั้งเดียว</summary>
        private bool _isFirstLoad = true;

        public ucOrder()
        {
            InitializeComponent();

            // 1. สร้าง Manager
            _tcpManager = new TcpManager();

            // 2. ส่ง Manager เข้าไปใน Adapter
            _inkjetAdapter = new MkCompactAdapter(_tcpManager);
        }

        // ══════════════════════════════════════════════
        //  DATA LOADING (poll-safe — รักษา row เดิม)
        // ══════════════════════════════════════════════

        /// <summary>โหลด Pending Jobs — รักษา row ที่เลือกอยู่, first load เลือกแถวแรก</summary>
        public async void get_job()
        {
            try
            {
                // จำ Job Id ที่เลือกอยู่ก่อน bind ใหม่
                int? selectedJobId = (bindingSource1.Current as PrintJob)?.Id;

                var jobs = await _api.GetPendingJobsAsync();
                bindingSource1.DataSource = jobs;

                // ครั้งแรก → เลือกแถวแรก + โหลด detail
                if (_isFirstLoad)
                {
                    _isFirstLoad = false;
                    if (bindingSource1.Count > 0)
                    {
                        bindingSource1.Position = 0;
                        SelectGridRow(dgvList, 0);
                        await LoadJobDetailAsync();
                    }
                    return;
                }

                // Poll ครั้งถัดไป → restore ตำแหน่งเดิมจาก Id
                if (selectedJobId.HasValue)
                {
                    int idx = jobs.FindIndex(j => j.Id == selectedJobId.Value);
                    if (idx >= 0)
                    {
                        bindingSource1.Position = idx;
                        return; // เจอ row เดิม → ไม่ต้องทำอะไรเพิ่ม
                    }
                }

                // row เดิมหายไป → fallback เลือกแถวแรก
                if (bindingSource1.Count > 0)
                    bindingSource1.Position = 0;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        /// <summary>โหลดข้อมูล UV inkjet — รักษา row ที่เลือกอยู่</summary>
        public async void get_uv()
        {
            try
            {
                int oldPos = bindingSourceUVinkjet.Position;

                var uvLogs = await _sqliteService.GetUvPrintDataAsync();
                bindingSourceUVinkjet.DataSource = uvLogs;

                // Restore ตำแหน่งเดิม (ถ้ายังอยู่ในช่วง)
                if (oldPos >= 0 && oldPos < bindingSourceUVinkjet.Count)
                    bindingSourceUVinkjet.Position = oldPos;
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        // ══════════════════════════════════════════════
        //  SELECTION HELPERS
        // ══════════════════════════════════════════════

        /// <summary>โหลด detail ของ Job ที่เลือกอยู่ → bind Config → เลือก Config แรก → bind TextBlock</summary>
        private async Task LoadJobDetailAsync()
        {
            if (bindingSource1.Current is not PrintJob selectedJob) return;

            // แสดงข้อมูลหลักของ Job
            txtBarcode.Text = selectedJob.BarcodeRaw;
            txtLot.Text = selectedJob.LotNumber;
            txtStatus.Text = selectedJob.Status;
            txtPattern.Text = selectedJob.PatternNoErp;

            // เรียก API ดึง Resolved Job
            await QueryBackendJobAsync();

            // Bind InkjetConfigs
            if (_currentResolved?.Pattern?.InkjetConfigs != null)
            {
                bindSourceInkjetConfigDto.DataSource = _currentResolved.Pattern.InkjetConfigs;

                // เลือก Config แถวแรก + โหลด TextBlocks
                if (bindSourceInkjetConfigDto.Count > 0)
                {
                    bindSourceInkjetConfigDto.Position = 0;
                    SelectGridRow(dgvConfigs, 0);
                    LoadTextBlocksForCurrentConfig();
                }
            }
            else
            {
                bindSourceInkjetConfigDto.DataSource = null;
                bindingSourceTextBlockDto.DataSource = null;
            }
        }

        /// <summary>โหลด TextBlocks จาก Config ที่เลือกอยู่ + ประมวลผล Pattern Rule</summary>
        private void LoadTextBlocksForCurrentConfig()
        {
            if (bindSourceInkjetConfigDto.Current is not InkjetConfigDto config)
            {
                bindingSourceTextBlockDto.DataSource = null;
                return;
            }

            bindingSourceTextBlockDto.DataSource = config.TextBlocks;

            // ประมวลผล Pattern สำหรับทุก Block ในชุดนี้
            ApplyPatternRules(config.TextBlocks);

            bindingSourceTextBlockDto.ResetBindings(false);
        }

        /// <summary>ประมวลผล PatternEngine สำหรับทุก TextBlock</summary>
        private void ApplyPatternRules(List<TextBlockDto>? textBlocks)
        {
            if (textBlocks == null) return;

            string barcode = txtBarcode.Text;
            foreach (var block in textBlocks)
            {
                // ส่ง barcode และ Text ของแต่ละ Block เข้า Engine
                // และเก็บผลลัพธ์ลงใน Property RuleResult
                block.RuleResult = PatternEngine.Process(barcode, block.Text);
            }
        }

        /// <summary>เลือก row ใน DataGridView อย่างปลอดภัย (กัน row count = 0)</summary>
        private static void SelectGridRow(DataGridView dgv, int index)
        {
            if (dgv.Rows.Count == 0 || index < 0 || index >= dgv.Rows.Count) return;
            dgv.ClearSelection();
            dgv.Rows[index].Selected = true;
        }

        // ══════════════════════════════════════════════
        //  EVENT HANDLERS — Grid Selection
        // ══════════════════════════════════════════════

        private async void dgvList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // เมื่อเลือก Job ใหม่ → refresh detail ทั้งหมด (Config + TextBlock)
            await LoadJobDetailAsync();
        }

        private void dgvConfigs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // เมื่อเลือก Config ใหม่ → refresh TextBlocks
            LoadTextBlocksForCurrentConfig();
        }

        // ══════════════════════════════════════════════
        //  QUERY BACKEND
        // ══════════════════════════════════════════════

        // --- เริ่มส่วนที่ปรับปรุงใหม่ ---
        //private async Task query_db3_async()
        //{
        //    string patternNo = txtPattern.Text.Trim();
        //
        //    // เรียกใช้ Service แทนการเขียนเอง
        //    var pattern = await _sqliteService.GetPatternDetailAsync(patternNo);
        //
        //    if (pattern != null)
        //    {
        //        // Bind ข้อมูลปกติ
        //        bindSourceInkjetConfigDto.DataSource = pattern.InkjetConfigs;
        //
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
                _currentResolved = await _api.GetResolvedJobAsync(selectedJob.Id);

                if (_currentResolved != null)
                {
                    // สั่งรีเฟรช Grid เพื่อแสดงค่าใหม่
                    bindingSourceUVinkjet.ResetBindings(false);
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
                _currentResolved = null;
            }
        }

        // ══════════════════════════════════════════════
        //  TIMER — Polling (รักษา row เดิม)
        // ══════════════════════════════════════════════

        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            // หยุด Timer ไว้ก่อนเพื่อกันการทำงานซ้อนกัน (ถ้าเน็ตช้า)
            timerPoll.Stop();

            try
            {
                get_job();
                get_uv();
                get_job_form_st3();
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

        // ══════════════════════════════════════════════
        //  SHARED HELPERS — Connection & TextBlock Send
        // ══════════════════════════════════════════════

        /// <summary>เชื่อมต่อ TCP กับเครื่องพิมพ์ — คืน true ถ้าสำเร็จ</summary>
        private async Task<bool> ConnectInkjetAsync(string ip = "192.168.3.77", int port = 9004)
        {
            await _tcpManager.ConnectAsync(ip, port);

            if (_inkjetAdapter.IsConnected()) return true;

            MessageBox.Show($"ไม่สามารถเชื่อมต่อเครื่องพิมพ์ ({ip}:{port})",
                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        /// <summary>ส่ง TextBlocks ทั้งหมด — ใช้ RuleResult ถ้ามี มิฉะนั้นใช้ Text เดิม</summary>
        private async Task SendTextBlocksWithRuleAsync(List<TextBlockDto>? textBlocks)
        {
            if (textBlocks == null) return;

            foreach (var block in textBlocks)
            {
                // เลือกใช้ RuleResult หากมีค่า ถ้าไม่มีให้ใช้ Text เดิม
                string textToSend = !string.IsNullOrEmpty(block.RuleResult)
                                    ? block.RuleResult
                                    : block.Text;

                // สร้าง Object ชั่วคราวเพื่อส่งข้อมูลที่ประมวลผลแล้ว
                var tempBlock = new TextBlockDto
                {
                    BlockNumber = block.BlockNumber,
                    Text = textToSend, // ส่งผลลัพธ์ที่ได้จาก Rule
                    X = block.X,
                    Y = block.Y,
                    Size = block.Size,
                    Scale = block.Scale
                };

                await _inkjetAdapter.SendTextBlockAsync(tempBlock, tempBlock.BlockNumber);
                await Task.Delay(100); // ป้องกัน Buffer เต็ม
            }
        }

        /// <summary>ส่ง TextBlocks ทั้งหมดแบบ raw (ไม่ใช้ RuleResult)</summary>
        private async Task SendTextBlocksRawAsync(List<TextBlockDto>? textBlocks)
        {
            if (textBlocks == null || !textBlocks.Any()) return;

            foreach (var block in textBlocks)
            {
                // ส่งทีละ Block (ระบุพิกัด X, Y และขนาดตัวอักษร)
                await _inkjetAdapter.SendTextBlockAsync(block, block.BlockNumber);

                // ป้องกัน Buffer เครื่องพิมพ์เต็ม
                await Task.Delay(150);
            }
        }

        // ══════════════════════════════════════════════
        //  SEND — MK1 / MK2
        // ══════════════════════════════════════════════

        private async void btnSendMk1Mk2_Click(object sender, EventArgs e)
        {
            // 1. ดึง Job ที่เลือกอยู่จาก Grid (สมมติใช้ bindingSource1 เก็บรายการจาก API)
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อนส่งข้อมูล");
                return;
            }

            // 2. ดึง Config สำหรับ Inkjet (จากอีก BindingSource หนึ่ง)
            if (bindSourceInkjetConfigDto.Current is not InkjetConfigDto config) return;

            try
            {
                // --- ส่วนการส่งข้อมูล Inkjet ---
                if (!await ConnectInkjetAsync()) return;

                ApplyPatternRules(config.TextBlocks);
                await _inkjetAdapter.ChangeProgramAsync(config.ProgramNumber ?? 1);
                await _inkjetAdapter.SendConfigAsync(config);
                await SendTextBlocksWithRuleAsync(config.TextBlocks);

                // --- ส่วนการอัปเดต Status กลับไปยัง API ---
                // ใช้ selectedJob.Id ที่เราดึงมาจากแถวที่เลือก
                int jobId = selectedJob.Id;

                var updateData = new { status = "Processing" };
                bool isUpdated = await _api.UpdateJobAsync(jobId, updateData);

                if (isUpdated)
                {
                    // อัปเดต UI ในแอปเราด้วยเพื่อให้ Operator เห็นว่าสถานะเปลี่ยนแล้ว
                    selectedJob.Status = "Processing";
                    txtStatus.Text = "Processing";
                    bindingSource1.ResetCurrentItem(); // สั่งให้ Grid บรรทัดนั้นรีเฟรชตัวอักษร

                    MessageBox.Show($"ส่งข้อมูลและเริ่มประมวลผล Job ID: {jobId} เรียบร้อย");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════
        //  SEND — MK3
        // ══════════════════════════════════════════════

        private async void btnSendMk3_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. ดึง Job ที่เลือกอยู่จาก Grid (สมมติใช้ bindingSource1 เก็บรายการจาก API)
                if (bindingSource1.Current is not PrintJob selectedJob)
                {
                    MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อนส่งข้อมูล");
                    return;
                }

                // 1. ดึงข้อมูล Barcode จากหน้าจอ (เช่นจาก TextBox หรือ Label)
                // ในที่นี้สมมติว่าดึงจากตัวแปร หรือคุณจะเปลี่ยนเป็น txtBarcode.Text ก็ได้
                string barcode = "CCRC0291-DEX0663MS";

                if (string.IsNullOrEmpty(barcode))
                {
                    MessageBox.Show("ไม่พบข้อมูล Barcode");
                    return;
                }

                // 2. ดึงข้อมูลรายละเอียดจากตาราง config_data_mk3
                var patternInfo = await _sqliteService.GetPatternDetailMk3Async(barcode);

                if (patternInfo == null)
                {
                    MessageBox.Show($"ไม่พบข้อมูล Config สำหรับ: {barcode} ในฐานข้อมูล");
                    return;
                }

                // 3. เลือก Config ของเครื่องที่ต้องการ (Ordinal 1 คือ MK1 หรือชุดแรกในไฟล์)
                var config = patternInfo.InkjetConfigs.FirstOrDefault(x => x.Ordinal == 1);
                if (config == null) return;

                // --- เริ่มกระบวนการส่งข้อมูลไปยังเครื่องพิมพ์ ---

                // A. เชื่อมต่อ TCP (IP และ Port ของเครื่องพิมพ์ MK3)
                if (!await ConnectInkjetAsync()) return;

                // B. เปลี่ยน Program Number (คำสั่งบังคับเครื่องเปลี่ยน Job)
                int targetProg = config.ProgramNumber ?? 158;
                await _inkjetAdapter.ChangeProgramAsync(targetProg);

                // หน่วงเวลาเล็กน้อยให้เครื่องพิมพ์เตรียมตัว (200-500ms)
                await Task.Delay(300);

                // C. ส่งการตั้งค่าพื้นฐาน (FM Command) เช่น Width, Height, Delay
                // ข้อมูลเหล่านี้ดึงมาจากคอลัมน์ mk1_width, mk1_height ฯลฯ
                await _inkjetAdapter.SendConfigAsync(config);
                await Task.Delay(100);

                // D. วนลูปส่งข้อความพิมพ์ (FS + F1 Commands) ตามจำนวน Block ที่มีข้อมูล
                // จากภาพ DB ของคุณ ข้อมูลจะถูกเก็บใน block1_text, block2_text...
                await SendTextBlocksRawAsync(config.TextBlocks);

                int jobId = selectedJob.Id;
                var updateData = new { st_status = "3" };
                await _api.UpdateJobAsync(jobId, updateData);

                MessageBox.Show($"[Success] ส่งข้อมูล {barcode}\nไปยัง Program: {targetProg} เรียบร้อยแล้ว",
                                "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการส่งข้อมูล: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // (Optional) หากต้องการตัดการเชื่อมต่อทันทีหลังส่งจบ ให้เรียกใช้บรรทัดล่างนี้
                // await _tcpManager.DisconnectAsync();
            }
        }

        // ══════════════════════════════════════════════
        //  SEND — UV Printer (UV1 / UV2 ใช้ร่วมกัน)
        // ══════════════════════════════════════════════

        private async void btnSendUV1_Click(object sender, EventArgs e)
        {
            await SendUvPrintAsync("UV Printer 1", "2", btnSendUV1);
        }

        private async void btnSendUV2_Click_1(object sender, EventArgs e)
        {
            await SendUvPrintAsync("UV Printer 2", "4", btnSendUV2);
        }

        /// <summary>ส่งข้อมูล UV Print (ใช้ร่วมกันระหว่าง UV1 และ UV2)</summary>
        private async Task SendUvPrintAsync(string inkjetName, string station, Button senderButton)
        {
            // 1. ดึง Job จาก BindingSource หลัก (รายการ Job)
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ในตารางก่อน");
                return;
            }
            //MessageBox.Show($"กำลังส่งข้อมูลการพิมพ์ UV สำหรับ Job ID: {selectedJob.Id}"); // Debugging Message

            // 2. ดึงข้อมูลจาก bindingSourceUVinkjet แถวที่ 1 (Index 0)
            // ตรวจสอบก่อนว่าใน List มีข้อมูลอย่างน้อย 1 แถวหรือไม่
            if (bindingSourceUVinkjet.Count == 0)
            {
                MessageBox.Show("ไม่พบข้อมูล Config ของเครื่องพิมพ์");
                return;
            }
            //MessageBox.Show($"กำลังส่งข้อมูลการพิมพ์ UV สำหรับ Job ID: {selectedJob.Id}"); // Debugging Message

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
                InkjetName = inkjetName,
                Lot = currentLot1,
                Name = currentName1,
                ProgramName = programName1,
                Status = "printing",
                Station = station
            };

            // 4. เรียก API บันทึกข้อมูล
            senderButton.Enabled = false; // ป้องกันการกดซ้ำระหว่างรอ Network
            try
            {
                bool isSaved = await _api.CreateUvInkjetAsync(uvRequest);

                if (isSaved)
                {
                    int stStatus = int.Parse(station);
                    var updatePayload = new { st_status = stStatus };
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
                senderButton.Enabled = true;
            }
        }

        private void dgvList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public async void get_job_form_st3()
        {
            try
            {
                // 1. จำ Job Id ที่เลือกอยู่ (ใช้จาก BindingSource ตัวที่จะโชว์)
                int? selectedJobId = (bindingSourceJobSt3.Current as PrintJob)?.Id;

                // 2. ดึงข้อมูลทั้งหมดจาก API
                var allJobs = await _api.GetPendingJobsAsync();

                // 3. กรองเฉพาะ st1_confirmation เป็น "Request" หรือ "Receive"
                // ใช้ StringComparison.OrdinalIgnoreCase เพื่อป้องกันปัญหาตัวพิมพ์เล็ก-ใหญ่
                var filteredJobs = allJobs.Where(j =>
                    !string.IsNullOrEmpty(j.st1_confirmation) &&
                    (j.st1_confirmation.Equals("Request", StringComparison.OrdinalIgnoreCase) ||
                     j.st1_confirmation.Equals("Receive", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                // 4. นำข้อมูลที่กรองแล้วใส่ BindingSource
                bindingSourceJobSt3.DataSource = filteredJobs;

                // --- ส่วนการจัดการตำแหน่ง (Restore Position) ---

                if (_isFirstLoad)
                {
                    _isFirstLoad = false;
                    if (bindingSourceJobSt3.Count > 0)
                    {
                        bindingSourceJobSt3.Position = 0;
                        // เปลี่ยน dgvList เป็นชื่อ DataGridView ของตาราง ST3
                        SelectGridRow(dataGridView2, 0);
                        await LoadJobDetailAsync();
                    }
                    return;
                }

                if (selectedJobId.HasValue)
                {
                    int idx = filteredJobs.FindIndex(j => j.Id == selectedJobId.Value);
                    if (idx >= 0)
                    {
                        bindingSourceJobSt3.Position = idx;
                        return;
                    }
                }

                if (bindingSourceJobSt3.Count > 0)
                    bindingSourceJobSt3.Position = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error get_job_form_st3: " + ex.Message);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // 1. ดึง Job ที่เลือกอยู่
            if (bindingSourceJobSt3.Current is not PrintJob selectedJob)
            {
                MessageBox.Show("กรุณาเลือกรายการ Job ST3 ในตารางก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. แสดง Popup ยืนยันก่อนเริ่มทำงาน
            string confirmMsg = $"คุณต้องการรับ Job ID: {selectedJob.Id}\n" +
                                $"จากที่ Station 3 ใช่หรือไม่?";

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

                int jobId = selectedJob.Id;
                string newStatus = "Receive";

                // 3. อัปเดตไปยัง API
                var updateData = new { st1_confirmation = newStatus, st1_send_time = DateTime.Now };
                bool isUpdated = await _api.UpdateJobAsync(jobId, updateData);

                if (isUpdated)
                {
                    // 4. อัปเดต UI
                    selectedJob.Status = newStatus;
                    bindingSourceJobSt3.ResetCurrentItem();

                    MessageBox.Show($"รับงานจาก ST3 Job: {jobId} เรียบร้อยแล้ว",
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
               
            }
        }
    }
}