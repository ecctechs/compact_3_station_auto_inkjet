namespace InkjetOperator
{
    partial class ucOrder
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControl;
        private TabPage tabList;
        private TabPage tabHistory;
        private DataGridView dgvList;
        private DataGridView dgvHistory;
        private Button btnStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabList = new TabPage();
            tabHistory = new TabPage();
            dgvList = new DataGridView();
            dgvHistory = new DataGridView();
            btnStart = new Button();

            tabControl.SuspendLayout();
            tabList.SuspendLayout();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(dgvList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(dgvHistory)).BeginInit();
            SuspendLayout();

            // tabControl
            tabControl.Controls.Add(tabList);
            tabControl.Controls.Add(tabHistory);
            tabControl.Location = new Point(20, 20);
            tabControl.Size = new Size(800, 400);

            // tabList
            tabList.Text = "List";
            tabList.Controls.Add(dgvList);

            // tabHistory
            tabHistory.Text = "History";
            tabHistory.Controls.Add(dgvHistory);

            // dgvList
            dgvList.Dock = DockStyle.Fill;

            // dgvHistory
            dgvHistory.Dock = DockStyle.Fill;

            // btnStart
            btnStart.Text = "Start";
            btnStart.Size = new Size(120, 50);
            btnStart.Location = new Point(350, 440);
            btnStart.BackColor = Color.FromArgb(160, 200, 140);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Click += btnStart_Click;

            // ucOrder
            Controls.Add(tabControl);
            Controls.Add(btnStart);
            Size = new Size(850, 520);

            tabControl.ResumeLayout(false);
            tabList.ResumeLayout(false);
            tabHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(dgvList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(dgvHistory)).EndInit();
            ResumeLayout(false);
        }
    }
}