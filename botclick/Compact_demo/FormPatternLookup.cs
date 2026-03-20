using System;
using System.IO;
using System.Windows.Forms;
using System.Data.SQLite;

namespace BotClickApp
{
    public partial class FormPatternLookup : Form
    {
        private const string DatabasePath = @"D:\DB\uv_data.db3";

        public FormPatternLookup()
        {
            InitializeComponent();
            btnEnter.Click += BtnEnter_Click;
            btnEditPattern.Click += BtnEditPattern_Click;
            btnClose.Click += (s, e) => Close();

            // prepare 5 block rows
            dgvBlocks.Rows.Clear();
            for (int i = 1; i <= 5; i++)
            {
                // Block, Text, X, Y, Size, Scale, Result
                dgvBlocks.Rows.Add(i, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            string patternNo = txtPattern.Text?.Trim();
            if (string.IsNullOrEmpty(patternNo))
            {
                MessageBox.Show("Please enter Pattern No.", "Input required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPattern.Focus();
                return;
            }

            if (!File.Exists(DatabasePath))
            {
                MessageBox.Show($"Database file not found: {DatabasePath}", "DB missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ตัด suffix หลัง "-" ตัวแรก → ใช้เป็น barcode
            string barcode = string.Empty;
            int dashIdx = patternNo.IndexOf('-');
            if (dashIdx >= 0 && dashIdx < patternNo.Length - 1)
            {
                barcode = patternNo.Substring(dashIdx + 1);
            }

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM config_data WHERE pattern_no_erp = @p LIMIT 1";
                        cmd.Parameters.AddWithValue("@p", patternNo);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                            {
                                MessageBox.Show("Pattern not found.", "No result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearFields();
                                return;
                            }

                            txtProgramName.Text = GetStringSafe(rdr, "program_name");
                            txtMk1ProgramNo.Text = GetStringSafe(rdr, "mk1_program_no");

                            // ดึง block_text ทั้ง 5 block
                            var blockTexts = new string[5];
                            for (int i = 1; i <= 5; i++)
                            {
                                string stext = $"mk1_block{i}_text";
                                string sx = $"mk1_block{i}_x";
                                string sy = $"mk1_block{i}_y";
                                string ssize = $"mk1_block{i}_size";
                                string sscale = $"mk1_block{i}_scale_side";

                                blockTexts[i - 1] = GetStringSafe(rdr, stext);

                                SetBlockRow(i - 1,
                                    blockTexts[i - 1],
                                    GetStringSafe(rdr, sx),
                                    GetStringSafe(rdr, sy),
                                    GetStringSafe(rdr, ssize),
                                    GetStringSafe(rdr, sscale)
                                    );
                            }

                            // apply pattern rules ถ้ามี barcode
                            if (!string.IsNullOrEmpty(barcode))
                            {
                                // โหลด patterns ถ้ายังไม่ได้โหลด
                                if (PatternStore.Patterns.Count == 0)
                                {
                                    var patternsFile = System.IO.Path.Combine(Application.StartupPath, "patterns.xml");
                                    PatternStore.LoadFromFile(patternsFile);
                                }

                                var results = PatternEngine.ProcessBlocks(barcode, blockTexts);
                                for (int i = 0; i < results.Length; i++)
                                {
                                    if (i < dgvBlocks.Rows.Count && dgvBlocks.Rows[i].Cells.Count > 6)
                                    {
                                        dgvBlocks.Rows[i].Cells[6].Value = results[i];
                                    }
                                }
                            }

                            txtHeight.Text = GetStringSafe(rdr, "mk1_height");
                            txtWidth.Text = GetStringSafe(rdr, "mk1_width");
                            txtTriggerDelay.Text = GetStringSafe(rdr, "mk1_trigger_delay");
                            txtPostAct.Text = GetStringSafe(rdr, "mk1_pos_act");
                            txtDelay.Text = GetStringSafe(rdr, "mk1_delay");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading database:\r\n" + ex.Message, "DB error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetStringSafe(SQLiteDataReader rdr, string column)
        {
            try
            {
                int idx = rdr.GetOrdinal(column);
                if (rdr.IsDBNull(idx)) return string.Empty;
                return Convert.ToString(rdr.GetValue(idx));
            }
            catch { return string.Empty; }
        }

        private void SetBlockRow(int rowIndex, string x, string y, string size, string scale, string text)
        {
            if (rowIndex < 0 || rowIndex >= dgvBlocks.Rows.Count) return;
            var row = dgvBlocks.Rows[rowIndex];
            row.Cells[1].Value = x;
            row.Cells[2].Value = y;
            row.Cells[3].Value = size;
            row.Cells[4].Value = scale;
            // new text column
            if (row.Cells.Count > 5)
                row.Cells[5].Value = text;
        }

        private void ClearFields()
        {
            txtProgramName.Clear();
            txtMk1ProgramNo.Clear();
            for (int i = 0; i < dgvBlocks.Rows.Count; i++)
            {
                SetBlockRow(i, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                if (dgvBlocks.Rows[i].Cells.Count > 6)
                    dgvBlocks.Rows[i].Cells[6].Value = string.Empty;
            }
            txtHeight.Clear();
            txtWidth.Clear();
            txtTriggerDelay.Clear();
            txtPostAct.Clear();
            txtDelay.Clear();
        }

        private void BtnEditPattern_Click(object sender, EventArgs e)
        {
            using (var dlg = new FormEditPattern())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);
            }
        }
    }
}
