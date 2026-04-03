using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;
using System.Data.SQLite;

namespace InkjetOperator.UserControls
{
    public partial class ucBot : UserControl
    {
        private AppConfig _config;
        private readonly ApiClient _api = ApiProvider.Instance;
        private readonly SqliteDataService _sqliteService = new SqliteDataService();
        public ucBot()
        {
            InitializeComponent();
            _config = AppConfig.Load();
            InitGrid();
            LoadConfig();

           
        }

        // ================= INIT GRID =================
        private void InitGrid()
        {
            dgvXY.Rows.Clear();

            dgvXY.Rows.Add("Document", "0", "0");
            dgvXY.Rows.Add("Open", "0", "0");
            dgvXY.Rows.Add("SelectFile", "0", "0");
            dgvXY.Rows.Add("OpenBtn", "0", "0");
        }

        // ================= LOAD =================
        private void LoadConfig()
        {
            if (_config.MenuMode == 2)
            {
                txtMain.Text = UvSettingsManager.GetValue("MAIN_PATH") ?? "";
                txtBackup.Text = UvSettingsManager.GetValue("BACKUP_PATH") ?? "";
                txtDBUV1.Text = UvSettingsManager.GetValue("UV1DB3_PATH") ?? "";

                dgvXY.Rows[0].Cells[1].Value = UvSettingsManager.GetValue("DOC_X") ?? "0";
                dgvXY.Rows[0].Cells[2].Value = UvSettingsManager.GetValue("DOC_Y") ?? "0";

                dgvXY.Rows[1].Cells[1].Value = UvSettingsManager.GetValue("OPEN_X") ?? "0";
                dgvXY.Rows[1].Cells[2].Value = UvSettingsManager.GetValue("OPEN_Y") ?? "0";

                dgvXY.Rows[2].Cells[1].Value = UvSettingsManager.GetValue("SELECT_X") ?? "0";
                dgvXY.Rows[2].Cells[2].Value = UvSettingsManager.GetValue("SELECT_Y") ?? "0";

                dgvXY.Rows[3].Cells[1].Value = UvSettingsManager.GetValue("OPENBTN_X") ?? "0";
                dgvXY.Rows[3].Cells[2].Value = UvSettingsManager.GetValue("OPENBTN_Y") ?? "0";
            }
            else
            {
                txtMain.Text = UvSettingsManager.GetValue("MAIN_PATH_2") ?? "";
                txtBackup.Text = UvSettingsManager.GetValue("BACKUP_PATH_2") ?? "";
                txtDBUV1.Text = UvSettingsManager.GetValue("UV1DB3_PATH_2") ?? "";

                dgvXY.Rows[0].Cells[1].Value = UvSettingsManager.GetValue("DOC_X_2") ?? "0";
                dgvXY.Rows[0].Cells[2].Value = UvSettingsManager.GetValue("DOC_Y_2") ?? "0";

                dgvXY.Rows[1].Cells[1].Value = UvSettingsManager.GetValue("OPEN_X_2") ?? "0";
                dgvXY.Rows[1].Cells[2].Value = UvSettingsManager.GetValue("OPEN_Y_2") ?? "0";

                dgvXY.Rows[2].Cells[1].Value = UvSettingsManager.GetValue("SELECT_X_2") ?? "0";
                dgvXY.Rows[2].Cells[2].Value = UvSettingsManager.GetValue("SELECT_Y_2") ?? "0";

                dgvXY.Rows[3].Cells[1].Value = UvSettingsManager.GetValue("OPENBTN_X_2") ?? "0";
                dgvXY.Rows[3].Cells[2].Value = UvSettingsManager.GetValue("OPENBTN_Y_2") ?? "0";
            }
        }

        // ================= SAVE =================
        private void SaveConfig()
        {
            if (_config.MenuMode == 2)
            {
                UvSettingsManager.SetValue("MAIN_PATH", txtMain.Text);
                UvSettingsManager.SetValue("BACKUP_PATH", txtBackup.Text);

                UvSettingsManager.SetValue("DOC_X", dgvXY.Rows[0].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("DOC_Y", dgvXY.Rows[0].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("OPEN_X", dgvXY.Rows[1].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("OPEN_Y", dgvXY.Rows[1].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("SELECT_X", dgvXY.Rows[2].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("SELECT_Y", dgvXY.Rows[2].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("OPENBTN_X", dgvXY.Rows[3].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("OPENBTN_Y", dgvXY.Rows[3].Cells[2].Value?.ToString() ?? "0");
            }
            else
            {
                UvSettingsManager.SetValue("MAIN_PATH_2", txtMain.Text);
                UvSettingsManager.SetValue("BACKUP_PATH_2", txtBackup.Text);

                UvSettingsManager.SetValue("DOC_X_2", dgvXY.Rows[0].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("DOC_Y_2", dgvXY.Rows[0].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("OPEN_X_2", dgvXY.Rows[1].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("OPEN_Y_2", dgvXY.Rows[1].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("SELECT_X_2", dgvXY.Rows[2].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("SELECT_Y_2", dgvXY.Rows[2].Cells[2].Value?.ToString() ?? "0");

                UvSettingsManager.SetValue("OPENBTN_X_2", dgvXY.Rows[3].Cells[1].Value?.ToString() ?? "0");
                UvSettingsManager.SetValue("OPENBTN_Y_2", dgvXY.Rows[3].Cells[2].Value?.ToString() ?? "0");
            }

                //MessageBox.Show("Save เรียบร้อย");
            }


        // ================= VALIDATE =================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMain.Text))
            {
                MessageBox.Show("กรุณาใส่ Main Path");
                return false;
            }

            // validate X/Y ทุก row
            for (int i = 0; i < dgvXY.Rows.Count; i++)
            {
                var x = dgvXY.Rows[i].Cells[1].Value?.ToString();
                var y = dgvXY.Rows[i].Cells[2].Value?.ToString();

                if (!int.TryParse(x, out _) || !int.TryParse(y, out _))
                {
                    MessageBox.Show($"Step {dgvXY.Rows[i].Cells[0].Value} ค่า X/Y ไม่ถูกต้อง");
                    return false;
                }
            }

            return true;
        }

        // ================= GET STEP =================
        private BotStep GetStepFromGrid(int index)
        {
            var row = dgvXY.Rows[index];

            return new BotStep
            {
                Name = row.Cells[0].Value?.ToString() ?? "",
                X = int.Parse(row.Cells[1].Value?.ToString() ?? "0"),
                Y = int.Parse(row.Cells[2].Value?.ToString() ?? "0"),
                VerifyArea = GetVerifyArea(index)
            };
        }

        private Rectangle GetVerifyArea(int index)
        {
            return index switch
            {
                0 => new Rectangle(2100, 0, 400, 300),
                1 => new Rectangle(2100, 100, 400, 400),
                2 => new Rectangle(800, 400, 800, 600),
                3 => new Rectangle(1500, 800, 500, 300),
                _ => Rectangle.Empty
            };
        }

        private void btnBrowseMain_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "เลือกโฟลเดอร์ Main Program";

                if (!string.IsNullOrEmpty(txtMain.Text) && Directory.Exists(txtMain.Text))
                {
                    dialog.SelectedPath = txtMain.Text;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtMain.Text = dialog.SelectedPath;

                    // ✅ Save ลง config
                    if(_config.MenuMode == 2)
                    {
                        UvSettingsManager.SetValue("MAIN_PATH", txtMain.Text);
                    }
                    else
                    {
                         UvSettingsManager.SetValue("MAIN_PATH_2", txtMain.Text);
                    }
                }
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "เลือกโฟลเดอร์ Backup Program";

                if (!string.IsNullOrEmpty(txtBackup.Text) && Directory.Exists(txtBackup.Text))
                {
                    dialog.SelectedPath = txtBackup.Text;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtBackup.Text = dialog.SelectedPath;

                    if(_config.MenuMode == 2)
                    {
                        UvSettingsManager.SetValue("BACKUP_PATH", txtBackup.Text);
                    }
                    else
                    {
                         UvSettingsManager.SetValue("BACKUP_PATH_2", txtBackup.Text);
                    }
                }
            }
        }

        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            string logPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "capture_log"
            );

            // ถ้า folder ยังไม่มี → สร้างให้เลย
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // เปิด folder ด้วย Windows Explorer
            System.Diagnostics.Process.Start("explorer.exe", logPath);
        }


        private void btnRunBot_Click_1(object sender, EventArgs e)
        {
            // 1. Save & Validate ข้อมูลเบื้องต้น
            SaveConfig();
            LoadConfig();
            if (!ValidateInput()) return;

            // 2. เช็คความพร้อมของข้อมูลใน bindingSource1 (Data Pre-check)
            if (bindingSource1.Count == 0)
            {
                MessageBox.Show("ไม่พบข้อมูลในตาราง กรุณาเลือก Job ก่อนเริ่มระบบ", "ข้อมูลไม่ครบ",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. เตรียมตัวแปรสำหรับใช้งาน (ประกาศไว้นอก Scope ของ if เพื่อให้ Lambda เรียกใช้ได้)
            string fileName = "default.uvdx";
            UVinkjet programData = null; // ประกาศไว้รองรับ Lambda 

            if (bindingSource1[0] is UVinkjet data) // ใช้ชื่อ 'data' แทน 'ProgramName'
            {
                programData = data;
                fileName = data.ProgramName ?? "default.uvdx";
                Debug.WriteLine("Ready to use Program: " + fileName);
            }

            // ป้องกันกรณี Cast ข้อมูลไม่สำเร็จ
            if (programData == null)
            {
                MessageBox.Show("โครงสร้างข้อมูลในตารางไม่ถูกต้อง", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. เมื่อข้อมูลพร้อม แสดงการแจ้งเตือนยืนยัน (User Confirmation)
            string alertMsg = "⚠️ คำเตือนก่อนเริ่มทำงาน:\n\n" +
                              "1. กรุณาเปิดโปรแกรม UV และจัดหน้าจอให้อยู่ที่หน้าจอหลัก (Primary Monitor)\n" +
                              "2. ห้ามขยับเมาส์หรือคีย์บอร์ดในระหว่างที่ระบบกำลังทำงาน\n" +
                              "3. หากภาพใน Step 1 ไม่ตรง ระบบจะหยุดรอให้คุณกดยืนยัน\n\n" +
                              "คุณพร้อมที่จะเริ่มทำงานหรือไม่?";

            DialogResult confirm = MessageBox.Show(alertMsg, "ยืนยันการเริ่มระบบ",
                                                 MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (confirm != DialogResult.Yes) return;

            // 5. เริ่มกระบวนการทำงานของ Bot (Execution Phase)
            try
            {
                // เตรียม Step จาก Grid
                var steps = new[]
                {
            GetStepFromGrid(0),
            GetStepFromGrid(1),
            GetStepFromGrid(2),
            GetStepFromGrid(3)
        };

                // ก๊อปปี้ไฟล์โปรแกรมเตรียมไว้
                BotClickHelper.CopyFile(fileName);

                // รัน Bot แบบ Async
                BotClickHelper.RunAsync("uvinkjet", steps, async result =>
                {
                    // --- ทำงานใน Background Thread ---

                    // 1. ล้างไฟล์ในโฟลเดอร์ Document
                    BotClickHelper.ClearDocumentFolder();

                    if (result.Success)
                    {
                        // 2. อัปเดตสถานะใน Database เป็น completed (เรียกผ่าน API)
                        // ทำงานใน Background ได้เลย ไม่ต้อง Invoke
                        await UpdateUvDatabaseAsync();
                        await _api.UpdateUvInkjetAsync(programData.Id, new { status = "completed" });


                        object updatePayload = null;

                        // 2. เช็คเงื่อนไขตาม MenuMode
                        if (_config.MenuMode == 2)
                        {
                            // สำหรับ Station 2 (UV1) อัปเดต st_status เป็น 3
                            updatePayload = new { st_status = "3" };
                        }
                        else if (_config.MenuMode == 4)
                        {
                            // สำหรับ Station 4 (UV2) อัปเดต status เป็น Completed
                            updatePayload = new { status = "Completed" };
                        }

                        bool isJobUpdated = await _api.UpdateJobAsync(programData.PrintJobsId, updatePayload);

                        // 3. แจ้งเตือนหน้าจอ (ต้องกลับไป UI Thread)
                        if (this.IsHandleCreated)
                        {
                            this.Invoke((MethodInvoker)(() =>
                            {
                                MessageBox.Show("ทำงานสำเร็จ!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // (Option) ถ้าต้องการ Refresh ตารางหลังงานเสร็จ
                                // await LoadUvInkjetHistory(); 
                            }));
                        }
                    }
                    else
                    {
                        // กรณี Bot ทำงานไม่สำเร็จหรือถูกยกเลิก
                        if (this.IsHandleCreated)
                        {
                            this.Invoke((MethodInvoker)(() =>
                            {
                                MessageBox.Show("Bot หยุดทำงาน: " + result.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดก่อนเริ่ม Bot: {ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDBUV1_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                // ตั้งค่าหัวข้อและฟิลเตอร์ให้เลือกเฉพาะไฟล์ Database (.db3 หรือ .db)
                dialog.Title = "เลือกไฟล์ Database UV (.db3)";
                dialog.Filter = "SQLite Database (*.db3)|*.db3|All files (*.*)|*.*";

                // ถ้าในช่อง Text มี Path เดิมอยู่แล้ว ให้เปิดไปที่โฟลเดอร์นั้น
                if (!string.IsNullOrEmpty(txtDBUV1.Text))
                {
                    string currentDir = Path.GetDirectoryName(txtDBUV1.Text);
                    if (Directory.Exists(currentDir))
                    {
                        dialog.InitialDirectory = currentDir;
                    }
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // นำ Path ของไฟล์ที่เลือกมาใส่ใน TextBox
                    txtDBUV1.Text = dialog.FileName;

                    // ✅ Save ลง config (บันทึก Path ไฟล์เต็มๆ)
                    if (_config.MenuMode == 2)
                    {
                        UvSettingsManager.SetValue("UV1DB3_PATH", txtDBUV1.Text);
                    }
                    else
                    {
                        UvSettingsManager.SetValue("UV1DB3_PATH_2", txtDBUV1.Text);
                    }

                        MessageBox.Show("บันทึกเส้นทาง Database เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void get_uv1_data()
        {
            // เรียก API ดึงข้อมูลทั้งหมด
            List<UVinkjet> uvList = await _api.GetAllUvInkjetAsync();

            if (uvList != null && uvList.Count > 0)
            {
                // 1. ประกาศ List ไว้ด้านนอกเพื่อให้ Scope ครอบคลุมทั้งฟังก์ชัน
                List<UVinkjet> printingItems = new List<UVinkjet>();
                List<UVinkjet> completedItems = new List<UVinkjet>();

                // 2. ตรวจสอบเงื่อนไขตาม MenuMode
                if (_config.MenuMode == 2)
                {
                    // กรองเฉพาะ Station "2" และ Status ตามที่ต้องการ
                    // หมายเหตุ: เช็คใน Model ว่า Station เป็น string ("2") หรือ int (2)
                    printingItems = uvList.Where(x => x.Status == "printing" && x.Station == "2").ToList();
                    completedItems = uvList.Where(x => x.Status == "completed" && x.Station == "2").ToList();
                }
                else
                {
                    // กรณี Mode อื่นๆ อาจจะกรองแบบปกติ หรือไม่กรอง Station
                    printingItems = uvList.Where(x => x.Status == "printing" && x.Station == "5").ToList();
                    completedItems = uvList.Where(x => x.Status == "completed" && x.Station == "5").ToList();
                }

                // 3. อัปเดต UI ผ่าน BindingSource
                // รายการที่กำลังพิมพ์ (Printing)
                bindingSource1.DataSource = printingItems;
                bindingSource1.ResetBindings(false);

                // รายการที่เสร็จแล้ว (Completed)
                bindingSource2.DataSource = completedItems;
                bindingSource2.ResetBindings(false);
            }
            else
            {
                // เคลียร์ข้อมูลเดิมในตารางหากไม่พบข้อมูลจาก API
                bindingSource1.DataSource = new List<UVinkjet>();
                bindingSource2.DataSource = new List<UVinkjet>();
                Debug.WriteLine("No UV Inkjet data found.");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            get_uv1_data();
        }

        private async Task UpdateUvDatabaseAsync()
        {
            // ดึงข้อมูลจาก BindingSource แถวปัจจุบัน
            if (bindingSource1.Current is UVinkjet selectedJob)
            {
                Debug.WriteLine($"Updating local DB for Job: Lot={selectedJob.Lot}, Name={selectedJob.Name}");
                // เรียกใช้ Service ที่เราเพิ่งเขียนเพิ่ม
                bool success = await _sqliteService.UpdateUvLocalDatabaseAsync(
                    selectedJob.Lot,
                    selectedJob.Name
                );

                if (success)
                {
                    // (Optional) แจ้งเตือนหรือ Log
                    Debug.WriteLine("Local SQLite Updated!");
                }
                else
                {
                    MessageBox.Show("ไม่สามารถอัปเดตฐานข้อมูล Local ได้");
                }
            }
        }
    }
}