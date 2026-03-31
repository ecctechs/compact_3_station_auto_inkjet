using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucOrder : UserControl
    {
        private ApiClient _api;

        public ucOrder()
        {
            InitializeComponent();
            _api = new ApiClient("http://localhost:3000");

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
    }
}