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
    }
}
