using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Diagnostics;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl
    {
        private ApiClient _api;

        public ucOrder()
        {
            InitializeComponent();
            _api = ApiProvider.Instance; // 🔥 ใช้จาก global

            get_job();

        }

        public async void get_job()
        {
            var jobs = await _api.GetPendingJobsAsync();

            bindingSource1.DataSource = jobs;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Start clicked");
        }

        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            timerPoll.Stop(); // 🔥 กันยิงซ้ำ

            try
            {
                var jobs = await _api.GetPendingJobsAsync();

                bindingSource1.DataSource = jobs;

                // ถ้ามี history
                // bindingSource2.DataSource = jobs.Where(x => x.Status == "Done").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                timerPoll.Start(); // 🔁 เริ่มใหม่
            }
        }

        private void dgvList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ตรวจสอบว่ามีข้อมูลถูกเลือกอยู่จริง (ป้องกัน Error กรณีคลิกโดน Header)
            if (bindingSource1.Current is PrintJob selectedJob)
            {
                txtBarcode.Text = selectedJob.BarcodeRaw;
                txtLot.Text = selectedJob.LotNumber;    // เปลี่ยนชื่อ Property ให้ตรงกับ Model ของคุณ
                txtStatus.Text = selectedJob.Status;
                txtPattern.Text = selectedJob.PatternNoErp;  // เปลี่ยนชื่อ Property ให้ตรงกับ Model ของคุณ
            }
        }

        private void query_db3()
        {

            string pattern = txtPattern.Text.Trim();
            // ระบุพาธไฟล์ database ของคุณ
            string dbPath = CustomSettingsManager.GetValue("DB_PATH") ?? "";
            if (!System.IO.File.Exists(dbPath))
            {
                Debug.WriteLine("ไม่พบไฟล์ฐานข้อมูลในตำแหน่ง: " + dbPath);
                return;
            }
            string connectionString = $"Data Source={dbPath};Version=3;";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // เขียน SQL Query
                    string sql = "SELECT * FROM config_data WHERE pattern_no_erp = @pattern";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        // ใช้ Parameter เพื่อป้องกัน SQL Injection
                        cmd.Parameters.AddWithValue("@pattern", pattern);

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Debug.WriteLine("--- New Record ---");
                                    // Loop อ่านข้อมูลทุก Column ใน Record นั้นๆ
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);

                                        Debug.WriteLine($"{columnName}: {columnValue}");
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine("No data found for pattern: " + pattern);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            query_db3();
        }
    }
}