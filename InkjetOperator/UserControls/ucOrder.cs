using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using InkjetOperator.Adapters;
using InkjetOperator.Managers;
using InkjetOperator.Models;
using InkjetOperator.Services;
using static System.Collections.Specialized.BitVector32;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl, ILocalizable
    {
        private readonly ApiClient _api = ApiProvider.Instance;
        private readonly SqliteDataService _sqliteService = new SqliteDataService();
        private ResolvedJobResponse _currentResolved;

        private MkCompactAdapter _inkjetAdapter;
        private TcpManager _tcpManager;

        /// <summary>ใช้จำว่า first load เพื่อ auto-select row แรกครั้งเดียว</summary>
        private bool _isFirstLoad = true;

        /// <summary>UV detail (UV1/UV2) ของงานที่เลือกล่าสุด — ใช้ตอนกดส่ง/เทส M2</summary>
        private List<UvJobData> _uvPreview = new();

        private readonly UvTcpService _uvTcp = new();

        public ucOrder()
        {
            InitializeComponent();

            UiStyle.ApplyGrid(dgvList, dgvHistory, dataGridView2, dgvConfigs, dgvTextBlocks);
            UiStyle.ApplyGrid(dgvUv1Fields, dgvUv1Blocks, dgvUv2Fields, dgvUv2Blocks, dgvUvSummary);

            // 1. สร้าง Manager
            _tcpManager = new TcpManager();

            // 2. ส่ง Manager เข้าไปใน Adapter
            _inkjetAdapter = new MkCompactAdapter(_tcpManager);

            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            lblJobsTitle.Text       = Lang.Get("order.pending_jobs");
            lblDetailTitle.Text     = Lang.Get("order.job_detail");
            lblBarcode.Text         = Lang.Get("order.barcode");
            lblLot.Text             = Lang.Get("order.lot");
            lblStatus.Text          = Lang.Get("order.status");
            lblPattern.Text         = Lang.Get("order.pattern");
            grpInkjetConfigs.Text   = Lang.Get("order.inkjet_configs");
            grpTextBlocks.Text      = Lang.Get("order.text_blocks");
            groupBox1.Text          = Lang.Get("order.inkjet_uv");
            btnRefresh.Text         = Lang.Get("btn.refresh");
            btnSendMk1Mk2.Text     = Lang.Get("order.send_mk12");

            btnSendUV1.Text         = Lang.Get("order.send_uv1");
            btnSendUV2.Text         = Lang.Get("order.send_uv2");
            tabList.Text            = Lang.Get("order.tab_list");
            tabHistory.Text         = Lang.Get("order.tab_history");
        }

        // ══════════════════════════════════════════════
        //  DATA LOADING (poll-safe — รักษา row เดิม)
        // ══════════════════════════════════════════════

        /// <summary>โหลด Pending Jobs — รักษา row ที่เลือกอยู่, first load เลือกแถวแรก</summary>
        /// <summary>โหลด Pending Jobs — รักษา row ที่เลือกอยู่, first load เลือกแถวแรก</summary>
        public async Task get_job()
        {
            try
            {
                int? selectedJobId = (bindingSource1.Current as PrintJob)?.Id;

                var allJobs = await _api.GetPendingJobsAsync();

                var filteredJobs = allJobs.Where(j =>
                    !string.IsNullOrEmpty(j.Status) &&
                    (j.Status.Equals("Waiting", StringComparison.OrdinalIgnoreCase) ||
                     j.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                bindingSource1.DataSource = filteredJobs;

                if (_isFirstLoad)
                {
                    _isFirstLoad = false;
                    if (bindingSource1.Count > 0)
                    {
                        bindingSource1.Position = 0;
                        SelectGridRow(dgvList, 0);
                        await LoadJobDetailAsync(bindingSource1);
                    }
                    return;
                }

                if (selectedJobId.HasValue)
                {
                    int idx = filteredJobs.FindIndex(j => j.Id == selectedJobId.Value);
                    if (idx >= 0)
                    {
                        bindingSource1.Position = idx;
                        return;
                    }
                }

                if (bindingSource1.Count > 0)
                    bindingSource1.Position = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in get_job: " + ex.Message);
            }
        }

        public async Task get_job_completed()
        {
            try
            {
                int? selectedJobId = (bindingSourceJobCompleted.Current as PrintJob)?.Id;

                var allJobs = await _api.GetPendingJobsAsync();

                var filteredJobs = allJobs.Where(j =>
                    !string.IsNullOrEmpty(j.Status) &&
                    j.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                bindingSourceJobCompleted.DataSource = filteredJobs;

                if (selectedJobId.HasValue)
                {
                    int idx = filteredJobs.FindIndex(j => j.Id == selectedJobId.Value);
                    if (idx >= 0)
                    {
                        bindingSourceJobCompleted.Position = idx;
                        return;
                    }
                }

                if (bindingSourceJobCompleted.Count > 0)
                    bindingSourceJobCompleted.Position = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in get_job_completed: " + ex.Message);
            }
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
        /// <summary>โหลด detail ของ Job ที่เลือกอยู่ → bind Config → เลือก Config แรก → bind TextBlock</summary>
        private async Task LoadJobDetailAsync(BindingSource? source = null)
        {
            // ถ้าระบุ source มา ใช้ตัวนั้นเลย
            // ถ้าไม่ระบุ ไล่เช็คตามลำดับ (fallback สำหรับกรณีเรียกจาก timer/first load)
            PrintJob? selectedJob;

            if (source != null)
            {
                selectedJob = source.Current as PrintJob;
            }
            else
            {
                selectedJob = (bindingSource1.Current as PrintJob)
                           ?? (bindingSourceJobSt3.Current as PrintJob)
                           ?? (bindingSourceJobCompleted.Current as PrintJob);
            }

            if (selectedJob == null)
            {
                ClearDetailUI();
                return;
            }

            // 2. แสดงข้อมูลหลัก (Header) ของ Job บน UI
            txtBarcode.Text = selectedJob.BarcodeRaw;
            txtLot.Text = selectedJob.LotNumber;
            txtStatus.Text = selectedJob.Status;
            txtPattern.Text = selectedJob.PatternNoErp;

            // preview UV1/UV2 — poll uv_job_data ของงานนี้จาก backend
            await FillUvPreviewAsync(selectedJob.Id);

            try
            {
                // 3. ดึงข้อมูลรายละเอียดเชิงลึก (Resolved Job) จาก API
                _currentResolved = await _api.GetResolvedJobAsync(selectedJob.Id);

                // 4. ผูกข้อมูล InkjetConfigs เข้ากับ Grid รายละเอียด
                if (_currentResolved?.Pattern?.InkjetConfigs != null)
                {
                    bindSourceInkjetConfigDto.DataSource = _currentResolved.Pattern.InkjetConfigs;

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
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadJobDetail Error: {ex.Message}");
            }
        }

        // ฟังก์ชันสำหรับล้างค่าหน้าจอเมื่อไม่มี Job ถูกเลือก
        private void ClearDetailUI()
        {
            txtBarcode.Clear();
            txtLot.Clear();
            txtStatus.Clear();
            txtPattern.Clear();
            bindSourceInkjetConfigDto.DataSource = null;
            bindingSourceTextBlockDto.DataSource = null;
            ClearUvPreview();
        }

        // ══════════════════════════════════════════════
        //  UV PREVIEW — poll uv_job_data → เติมแท็บ UV1/UV2 (read-only ก่อนส่ง)
        // ══════════════════════════════════════════════

        /// <summary>poll UV detail ของงานที่เลือก → เติม Fields + Blocks + Summary</summary>
        private async Task FillUvPreviewAsync(int jobId)
        {
            try
            {
                var uvRows = await _api.GetUvJobDataByJobAsync(jobId);
                _uvPreview = uvRows;

                var uv1 = uvRows.FirstOrDefault(u => u.Machine == "UV1");
                var uv2 = uvRows.FirstOrDefault(u => u.Machine == "UV2");

                FillUvTab(dgvUv1Fields, dgvUv1Blocks, uv1, "MK063");
                FillUvTab(dgvUv2Fields, dgvUv2Blocks, uv2, "MK067");

                dgvUvSummary.DataSource = uvRows;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FillUvPreview error: {ex.Message}");
            }
        }

        /// <summary>เติม 1 แท็บ: Fields (ProgramName/Table) + Blocks (text1..5 + Status)</summary>
        private static void FillUvTab(DataGridView fields, DataGridView blocks, UvJobData? uv, string table)
        {
            fields.Rows.Clear();
            fields.Rows.Add("ProgramName", uv?.ProgramName ?? "");
            fields.Rows.Add("Table", table);

            blocks.Rows.Clear();
            var texts = new[] { uv?.Text1, uv?.Text2, uv?.Text3, uv?.Text4, uv?.Text5 };
            for (int i = 0; i < 5; i++)
            {
                string t = texts[i] ?? "";
                blocks.Rows.Add((i + 1).ToString(), t, string.IsNullOrEmpty(t) ? "—" : "Ready");
            }
        }

        private void ClearUvPreview()
        {
            _uvPreview = new();
            dgvUv1Fields.Rows.Clear();
            dgvUv1Blocks.Rows.Clear();
            dgvUv2Fields.Rows.Clear();
            dgvUv2Blocks.Rows.Clear();
            dgvUvSummary.DataSource = null;
        }

        // ══════════════════════════════════════════════
        //  M2 (TEST) — เขียน UV detail ลง CPI.db3 (MK063/MK067)
        // ══════════════════════════════════════════════

        private async void btnTestM2_Click(object sender, EventArgs e)
        {
            if (_uvPreview == null || _uvPreview.Count == 0)
            {
                MessageBox.Show("ยังไม่มีข้อมูล UV ของงานที่เลือก\n(เลือกงานที่มีข้อมูล UV ก่อน)",
                    "Test M2", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var uv1 = _uvPreview.FirstOrDefault(u => u.Machine == "UV1");
            var uv2 = _uvPreview.FirstOrDefault(u => u.Machine == "UV2");

            string? path1 = ucSettingUV.GetCpiPath(1);
            string? path2 = ucSettingUV.GetCpiPath(2);

            if (uv1 != null && path1 == null)
            {
                MessageBox.Show("ยังไม่ได้ตั้งค่าโฟลเดอร์ UV1 ในหน้า UV Setting\nหรือไม่พบ CPI.db3",
                    "Test M2", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (uv2 != null && path2 == null)
            {
                MessageBox.Show("ยังไม่ได้ตั้งค่าโฟลเดอร์ UV2 ในหน้า UV Setting\nหรือไม่พบ CPI.db3",
                    "Test M2", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTestM2.Enabled = false;
            try
            {
                var results = new List<string>();

                if (uv1 != null && path1 != null)
                {
                    var (_, msg) = await _sqliteService.WriteUvToCpiAsync(path1, "MK063", uv1);
                    results.Add("UV1 → " + msg);
                }
                if (uv2 != null && path2 != null)
                {
                    var (_, msg) = await _sqliteService.WriteUvToCpiAsync(path2, "MK067", uv2);
                    results.Add("UV2 → " + msg);
                }

                MessageBox.Show(string.Join("\n\n", results),
                    "Test M2 — เขียน CPI.db3", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Test M2",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestM2.Enabled = true;
            }
        }

        // ══════════════════════════════════════════════
        //  M3 (TEST) — ส่ง TCP KEY :10086 (โหลด .uvdx + start)
        // ══════════════════════════════════════════════

        private async void btnTestTcp_Click(object sender, EventArgs e)
        {
            if (_uvPreview == null || _uvPreview.Count == 0)
            {
                MessageBox.Show("ยังไม่มีข้อมูล UV ของงานที่เลือก",
                    "Test M3 (TCP)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var uv1 = _uvPreview.FirstOrDefault(u => u.Machine == "UV1");
            var uv2 = _uvPreview.FirstOrDefault(u => u.Machine == "UV2");

            string? prog = PromptText(
                "Test M3 — โหลดโปรแกรม",
                "ชื่อ .uvdx ที่จะโหลด เช่น compact / Lot Name / DEX a :",
                uv1?.ProgramName ?? "compact");
            if (prog == null) return;

            btnTestTcp.Enabled = false;
            try
            {
                var results = new List<string>();

                if (uv1 != null)
                {
                    string? docFolder = ucSettingUV.GetDocumentFolder(1);
                    if (docFolder == null)
                    {
                        results.Add("[UV1] ยังไม่ได้ตั้งค่าโฟลเดอร์ UV1 ในหน้า UV Setting");
                    }
                    else
                    {
                        string? resolved = ResolveUvProgram(prog, docFolder);
                        if (resolved != null)
                        {
                            string ip = CustomSettingsManager.GetValue("UV001_IP") ?? "";
                            int port = ParsePort(CustomSettingsManager.GetValue("UV001_PORT"));
                            var (_, log) = await _uvTcp.LoadAndStartAsync(ip, port, resolved);
                            results.Add("[UV1]\n" + log);
                        }
                        else
                        {
                            results.Add("[UV1] ยกเลิก");
                        }
                    }
                }
                if (uv2 != null)
                {
                    string? docFolder = ucSettingUV.GetDocumentFolder(2);
                    if (docFolder == null)
                    {
                        results.Add("[UV2] ยังไม่ได้ตั้งค่าโฟลเดอร์ UV2 ในหน้า UV Setting");
                    }
                    else
                    {
                        string? resolved = ResolveUvProgram(prog, docFolder);
                        if (resolved != null)
                        {
                            string ip = CustomSettingsManager.GetValue("UV002_IP") ?? "";
                            int port = ParsePort(CustomSettingsManager.GetValue("UV002_PORT"));
                            var (_, log) = await _uvTcp.LoadAndStartAsync(ip, port, resolved);
                            results.Add("[UV2]\n" + log);
                        }
                        else
                        {
                            results.Add("[UV2] ยกเลิก");
                        }
                    }
                }

                MessageBox.Show(string.Join("\n\n", results),
                    "Test M3 — TCP KEY :10086", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "Test M3 (TCP)",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestTcp.Enabled = true;
            }
        }

        private string? ResolveUvProgram(string program, string docFolder)
        {
            string baseName = program.EndsWith(".uvdx", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileNameWithoutExtension(program)
                : program;

            var allUvdx = Directory.GetFiles(docFolder, "*.uvdx")
                .Select(f => Path.GetFileNameWithoutExtension(f)!)
                .Where(name => name.StartsWith(baseName, StringComparison.Ordinal))
                .OrderBy(n => n)
                .ToList();

            if (allUvdx.Count == 1)
                return allUvdx[0];

            if (allUvdx.Count > 1)
                return PromptUvVariant(baseName, allUvdx);

            if (File.Exists(Path.Combine(docFolder, "default.uvdx")))
            {
                MessageBox.Show(
                    $"ไม่พบโปรแกรม \"{baseName}\" ในโฟลเดอร์ document\nจะใช้ default.uvdx ไปก่อน\n\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                    "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "default";
            }

            MessageBox.Show(
                $"ไม่พบโปรแกรม \"{baseName}\" และไม่มี default.uvdx\nกรุณาไปเพิ่มไฟล์โปรแกรมก่อน",
                "ไม่พบโปรแกรม", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        private string? PromptUvVariant(string baseName, List<string> variants)
        {
            using var frm = new Form
            {
                Text = "เลือกโปรแกรมย่อย",
                Size = new Size(360, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label
            {
                Text = $"โปรแกรม \"{baseName}\" มีหลายตัวเลือก :",
                Location = new Point(12, 12),
                AutoSize = true
            };

            var lst = new ListBox
            {
                Location = new Point(12, 40),
                Size = new Size(320, 120)
            };
            foreach (var v in variants)
                lst.Items.Add(v + ".uvdx");
            lst.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "ตกลง",
                DialogResult = DialogResult.OK,
                Location = new Point(170, 175),
                Size = new Size(80, 32)
            };
            var btnCn = new Button
            {
                Text = "ยกเลิก",
                DialogResult = DialogResult.Cancel,
                Location = new Point(256, 175),
                Size = new Size(80, 32)
            };

            frm.Controls.AddRange(new Control[] { lbl, lst, btnOk, btnCn });
            frm.AcceptButton = btnOk;
            frm.CancelButton = btnCn;

            if (frm.ShowDialog(this.ParentForm) != DialogResult.OK || lst.SelectedIndex < 0)
                return null;

            return variants[lst.SelectedIndex];
        }

        /// <summary>parse port จาก setting — ว่าง/ผิด → default 10086</summary>
        private static int ParsePort(string? s) =>
            int.TryParse(s, out int p) && p > 0 ? p : 10086;

        /// <summary>กล่องพิมพ์ข้อความง่าย ๆ — คืน null ถ้ากด Cancel</summary>
        private static string? PromptText(string title, string label, string defaultValue)
        {
            using var dlg = new Form
            {
                Text = title,
                Size = new System.Drawing.Size(440, 170),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label { Text = label, Location = new System.Drawing.Point(12, 12), AutoSize = true, MaximumSize = new System.Drawing.Size(410, 0) };
            var txt = new TextBox { Text = defaultValue, Location = new System.Drawing.Point(12, 55), Width = 405 };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(255, 90), Width = 78 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(339, 90), Width = 78 };
            dlg.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;
            return dlg.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
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
            await LoadJobDetailAsync(bindingSource1);
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
            // 1. ดึง Job จาก BindingSource ที่เลือกอยู่ (ไล่เช็คทีละตัว)
            var selectedJob = (bindingSource1.Current as PrintJob)
                           ?? (bindingSourceJobSt3.Current as PrintJob)
                           ?? (bindingSourceJobCompleted.Current as PrintJob);

            if (selectedJob == null)
            {
                // ไม่ต้องโชว์ MessageBox ก็ได้ถ้าเป็นการเรียกแบบ Auto-refresh
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

        // ══════════════════════════════════════════════
        //  TIMER — Polling (รักษา row เดิม)
        // ══════════════════════════════════════════════

        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            timerPoll.Stop();

            try
            {
                // await ทุกตัวให้ data โหลดเสร็จก่อน
                await get_job();
                await get_job_form_st3();
                await get_job_completed();
                get_uv();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Polling Error: " + ex.Message);
            }
            finally
            {
                if (!this.IsDisposed)
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
        //  SHARED HELPERS — Adapter Mapping
        // ══════════════════════════════════════════════

        /// <summary>Map Ordinal → Adapter ที่เชื่อมต่อจาก ucSetting (AdapterRegistry)</summary>
        private static IInkjetAdapter? GetAdapterByOrdinal(int ordinal)
        {
            return ordinal switch
            {
                1 => AdapterRegistry.MK058,
                2 => AdapterRegistry.MK059,
                3 => AdapterRegistry.MK060,
                4 => AdapterRegistry.MK061,
                _ => null
            };
        }

        /// <summary>Map Ordinal → ชื่อเครื่อง (สำหรับแสดง error)</summary>
        private static string GetAdapterName(int ordinal)
        {
            return ordinal switch
            {
                1 => CustomSettingsManager.GetValue("MK058_NAME") ?? "MK-058",
                2 => CustomSettingsManager.GetValue("MK059_NAME") ?? "MK-059",
                3 => CustomSettingsManager.GetValue("MK060_NAME") ?? "MK-060",
                4 => CustomSettingsManager.GetValue("MK061_NAME") ?? "MK-061",
                _ => $"Unknown (Ordinal {ordinal})"
            };
        }

        /// <summary>ส่งข้อมูล InkjetConfig ไปยังเครื่องพิมพ์ตาม Ordinal</summary>
        private async Task<bool> SendConfigToAdapterAsync(InkjetConfigDto config)
        {
            int ordinal = config.Ordinal;
            var adapter = GetAdapterByOrdinal(ordinal);
            string name = GetAdapterName(ordinal);

            // 1. เช็คว่ามี adapter อยู่ใน Registry ไหม
            if (adapter == null)
            {
                MessageBox.Show(
                    $"❌ ไม่พบการเชื่อมต่อสำหรับ {name}\n\n" +
                    $"กรุณาไปที่หน้า Setting ตั้งค่า IP และเชื่อมต่อก่อน",
                    Lang.Get("msg.connection_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 2. เช็คว่าเชื่อมต่ออยู่จริงไหม
            if (!adapter.IsConnected())
            {
                MessageBox.Show(
                    $"❌ {name} ไม่ได้เชื่อมต่ออยู่\n\n" +
                    $"กรุณาไปที่หน้า Setting ตรวจสอบสถานะการเชื่อมต่อ",
                    Lang.Get("msg.connection_error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 3. ส่งข้อมูลจริง
            ApplyPatternRules(config.TextBlocks);

            await adapter.ChangeProgramAsync(config.ProgramNumber ?? 1);
            await adapter.SendConfigAsync(config);
            await SendTextBlocksToAdapterAsync(adapter, config.TextBlocks);

            return true;
        }

        /// <summary>ส่ง TextBlocks ทั้งหมดไปยัง adapter ที่ระบุ</summary>
        private async Task SendTextBlocksToAdapterAsync(IInkjetAdapter adapter, List<TextBlockDto>? textBlocks)
        {
            if (textBlocks == null) return;

            foreach (var block in textBlocks)
            {
                string textToSend = !string.IsNullOrEmpty(block.RuleResult)
                                    ? block.RuleResult
                                    : block.Text ?? "";

                var tempBlock = new TextBlockDto
                {
                    BlockNumber = block.BlockNumber,
                    Text = textToSend,
                    X = block.X,
                    Y = block.Y,
                    Size = block.Size,
                    Scale = block.Scale
                };

                await adapter.SendTextBlockAsync(tempBlock, tempBlock.BlockNumber);
                await Task.Delay(100);
            }
        }

        // ══════════════════════════════════════════════
        //  SEND — MK1 / MK2
        // ══════════════════════════════════════════════

        private async void btnSendMk1Mk2_Click(object sender, EventArgs e)
        {
            // 1. ดึง Job ที่เลือกอยู่
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show(Lang.Get("msg.select_job"));
                return;
            }

            // 2. ดึง Configs (Ordinal 1, 2)
            var configs = _currentResolved?.Pattern?.InkjetConfigs;
            var mk12Configs = configs?.Where(c => c.Ordinal == 1 || c.Ordinal == 2).ToList();

            if (mk12Configs == null || mk12Configs.Count == 0)
            {
                MessageBox.Show("ไม่พบ Config สำหรับ MK1/MK2 (Ordinal 1, 2)");
                return;
            }

            // 3. ยืนยันการส่ง
            var confirm = MessageBox.Show(
                $"ต้องการส่งข้อมูลไปยัง MK1/MK2\nJob ID: {selectedJob.Id}\nBarcode: {selectedJob.BarcodeRaw}",
                "ยืนยันการส่ง", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnSendMk1Mk2.Enabled = false;

            try
            {
                // === STEP 1: [PRE-CHECK] ตรวจสอบความพร้อมและเก็บผลลัพธ์เพื่อแสดงผลแบบรวม ===
                var checkResults = new List<string>();
                var readyToProcess = new List<(IInkjetAdapter Adapter, InkjetConfigDto Config, string Name)>();
                bool isAnyDisconnected = false;

                foreach (var config in mk12Configs)
                {
                    string name = GetAdapterName(config.Ordinal);
                    var adapter = GetAdapterByOrdinal(config.Ordinal);
                    bool isConnected = adapter != null && adapter.IsConnected();

                    if (isConnected)
                    {
                        checkResults.Add($"{name}: ✅ เชื่อมต่อสำเร็จ");
                        readyToProcess.Add((adapter!, config, name));
                    }
                    else
                    {
                        checkResults.Add($"{name}: ❌ เชื่อมต่อไม่สำเร็จ");
                        isAnyDisconnected = true;
                    }
                }

                // --- หากมีเครื่องใดเครื่องหนึ่งไม่พร้อม ให้แสดงผลสรุปแบบ Error และหยุดทันที ---
                if (isAnyDisconnected)
                {
                    string checkSummary = string.Join("\n", checkResults);
                    MessageBox.Show(
                        $"ผลการตรวจสอบ Job ID: {selectedJob.Id}\n\n" +
                        $"{checkSummary}\n\n" +
                        "⚠️ สถานะ Job ยังไม่ถูกอัปเดต เนื่องจากมีเครื่องเชื่อมต่อไม่สำเร็จ",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return; // หยุดการทำงานทันที
                }

                // === STEP 2: [SENDING] เริ่มส่งข้อมูล (เฉพาะเมื่อทุกเครื่องพร้อมแล้ว) ===
                var sendResults = new List<string>();
                bool allSendSuccess = true;

                foreach (var item in readyToProcess)
                {
                    ApplyPatternRules(item.Config.TextBlocks);
                    bool ok = await SendConfigToAdapterAsync(item.Config);

                    sendResults.Add($"{item.Name}: {(ok ? "✅ ส่งสำเร็จ" : "❌ ส่งล้มเหลว")}");

                    if (!ok)
                    {
                        allSendSuccess = false;
                        break; // หยุดส่งเครื่องถัดไปหากเครื่องนี้พลาด
                    }
                }

                // === STEP 3: [FINALIZE] อัปเดตสถานะและแจ้งผลการส่ง ===
                if (allSendSuccess)
                {
                    var updateData = new { status = "Processing", st_status = "1" };
                    bool isUpdated = await _api.UpdateJobAsync(selectedJob.Id, updateData);

                    if (isUpdated)
                    {
                        selectedJob.Status = "Processing";
                        txtStatus.Text = "Processing";
                        bindingSource1.ResetCurrentItem();
                    }

                    MessageBox.Show(
                        $"ผลการส่ง Job ID: {selectedJob.Id}\n\n{string.Join("\n", sendResults)}",
                        Lang.Get("msg.success"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"ผลการส่ง Job ID: {selectedJob.Id}\n\n{string.Join("\n", sendResults)}\n\n" +
                        "⚠️ สถานะ Job ยังไม่ถูกอัปเดต เนื่องจากมีบางเครื่องส่งไม่สำเร็จ",
                        Lang.Get("msg.warning"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.Format("msg.send_error", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSendMk1Mk2.Enabled = true;
            }
        }

        // ══════════════════════════════════════════════
        // ══════════════════════════════════════════════
        //  SEND — UV Printer (UV1 / UV2 ใช้ร่วมกัน)
        // ══════════════════════════════════════════════

        private async void btnSendUV1_Click(object sender, EventArgs e)
        {
            await SendUvPrintAsync("UV Printer 1", "2", 1, "UV1", "MK063", btnSendUV1);
        }

        private async void btnSendUV2_Click_1(object sender, EventArgs e)
        {
            await SendUvPrintAsync("UV Printer 2", "5", 2, "UV2", "MK067", btnSendUV2);
        }

        private async Task SendUvPrintAsync(string inkjetName, string station, int uvNumber,
            string machineName, string table, Button senderButton)
        {
            // 1. ดึง Job
            if (bindingSource1.Current is not PrintJob selectedJob)
            {
                MessageBox.Show(Lang.Get("msg.select_job"));
                return;
            }

            if (selectedJob.Station == "2" || selectedJob.Station == "5")
            {
                MessageBox.Show("รายการนี้ส่งข้อมูลเรียบร้อยแล้ว ไม่สามารถส่งซ้ำได้", "แจ้งเตือน");
                return;
            }

            // 2. ดึง UV data จาก _uvPreview
            var uvData = _uvPreview?.FirstOrDefault(u => u.Machine == machineName);
            if (uvData == null)
            {
                MessageBox.Show($"ยังไม่มีข้อมูล {machineName} ของงานที่เลือก\n(เลือกงานที่มีข้อมูล UV ก่อน)",
                    inkjetName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Validate UV Setting
            string? cpiPath = ucSettingUV.GetCpiPath(uvNumber);
            if (cpiPath == null)
            {
                MessageBox.Show($"ยังไม่ได้ตั้งค่าโฟลเดอร์ UV{uvNumber} ในหน้า UV Setting\nหรือไม่พบ CPI.db3",
                    inkjetName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? docFolder = ucSettingUV.GetDocumentFolder(uvNumber);
            if (docFolder == null)
            {
                MessageBox.Show($"ไม่พบโฟลเดอร์ document สำหรับ UV{uvNumber}\nกรุณาตรวจสอบ UV Setting",
                    inkjetName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4. Resolve program
            string programName = uvData.ProgramName ?? "";
            string? resolved = string.IsNullOrEmpty(programName)
                ? "default"
                : ResolveUvProgram(programName, docFolder);
            if (resolved == null) return;

            // 5. ยืนยันก่อนส่ง
            var confirm = MessageBox.Show(
                $"ต้องการส่งข้อมูลไปยัง {inkjetName}\n" +
                $"Job ID: {selectedJob.Id}\n" +
                $"Barcode: {selectedJob.BarcodeRaw}\n" +
                $"Program: {resolved}.uvdx",
                "ยืนยันการส่ง", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            senderButton.Enabled = false;
            try
            {
                var results = new List<string>();

                // Step A: เขียน CPI.db3
                var (writeOk, writeMsg) = await _sqliteService.WriteUvToCpiAsync(cpiPath, table, uvData);
                results.Add($"เขียน CPI.db3: {writeMsg}");
                if (!writeOk)
                {
                    MessageBox.Show(string.Join("\n", results), inkjetName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Step B: TCP KEY:85 โหลดโปรแกรม + start
                string ip = CustomSettingsManager.GetValue(uvNumber == 1 ? "UV001_IP" : "UV002_IP") ?? "";
                int port = ParsePort(CustomSettingsManager.GetValue(uvNumber == 1 ? "UV001_PORT" : "UV002_PORT"));
                var (tcpOk, tcpLog) = await _uvTcp.LoadAndStartAsync(ip, port, resolved);
                results.Add($"TCP: {tcpLog}");
                if (!tcpOk)
                {
                    MessageBox.Show(string.Join("\n\n", results), inkjetName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Step C: บันทึก API
                var uvRequest = new UVinkjet
                {
                    PrintJobsId = selectedJob.Id,
                    InkjetName = inkjetName,
                    Lot = uvData.Lot ?? "",
                    Name = uvData.Name ?? "",
                    ProgramName = resolved,
                    Status = "printing",
                    Station = station
                };

                bool isSaved = await _api.CreateUvInkjetAsync(uvRequest);
                if (isSaved)
                {
                    int stStatus = int.Parse(station);
                    await _api.UpdateJobAsync(selectedJob.Id, new { st_status = stStatus });
                    results.Add("บันทึก API สำเร็จ");

                    MessageBox.Show(string.Join("\n\n", results), inkjetName,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    results.Add("ไม่สามารถบันทึกสถานะการพิมพ์ได้ (Server Error)");
                    MessageBox.Show(string.Join("\n\n", results), inkjetName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", inkjetName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public async Task get_job_form_st3()
        {
            try
            {
                int? selectedJobId = (bindingSourceJobSt3.Current as PrintJob)?.Id;

                var allJobs = await _api.GetPendingJobsAsync();

                var filteredJobs = allJobs.Where(j =>
                    !string.IsNullOrEmpty(j.st1_confirmation) &&
                    (j.st1_confirmation.Equals("Request", StringComparison.OrdinalIgnoreCase) ||
                     j.st1_confirmation.Equals("Receive", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                bindingSourceJobSt3.DataSource = filteredJobs;

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

        private async void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            await LoadJobDetailAsync(bindingSourceJobSt3);
        }

        private async void dgvHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            await LoadJobDetailAsync(bindingSourceJobCompleted);
        }
    }
}