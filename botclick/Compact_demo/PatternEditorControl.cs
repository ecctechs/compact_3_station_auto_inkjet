using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BotClickApp
{
    public partial class PatternEditorControl : UserControl
    {
        private bool _loading = false;

        // mapping ระหว่าง enum กับชื่อแสดงใน ComboBox
        private static readonly Dictionary<TransformRuleType, string> RuleDisplayNames = new Dictionary<TransformRuleType, string>
        {
            { TransformRuleType.DELETE, "DELETE" },
            { TransformRuleType.FIX_TEXT, "FIX_TEXT" },
            { TransformRuleType.COPY, "COPY" },
            { TransformRuleType.PAD_LEFT, "Keep + Pad Left" },
            { TransformRuleType.PAD_RIGHT, "Keep + Pad Right" },
            { TransformRuleType.AZ_LOWER, "Swap A-Z Lower" },
            { TransformRuleType.AZ_UPPER, "Swap A-Z Upper" },
            { TransformRuleType.YEAR_AZ, "YEAR_AZ" },
            { TransformRuleType.TAKE_RIGHT, "TAKE_RIGHT" },
            { TransformRuleType.TAKE_LEFT, "TAKE_LEFT" },
        };

        private static readonly Dictionary<string, TransformRuleType> DisplayNameToRule;

        static PatternEditorControl()
        {
            DisplayNameToRule = new Dictionary<string, TransformRuleType>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in RuleDisplayNames)
            {
                DisplayNameToRule[kv.Value] = kv.Key;
                // enum name ก็ใช้ได้ (รองรับ XML เก่า)
                DisplayNameToRule[kv.Key.ToString()] = kv.Key;
            }
        }

        private static string ToDisplayName(TransformRuleType rule)
        {
            string name;
            return RuleDisplayNames.TryGetValue(rule, out name) ? name : rule.ToString();
        }

        private static TransformRuleType FromDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return TransformRuleType.FIX_TEXT;
            TransformRuleType t;
            if (DisplayNameToRule.TryGetValue(displayName, out t)) return t;
            if (Enum.TryParse(displayName, out t)) return t;
            return TransformRuleType.FIX_TEXT;
        }

        public PatternEditorControl()
        {
            InitializeComponent();

            InitializeDataGrid();

            // no txtInput anymore; use txtBarcodeTest only
            txtPatternName.TextChanged += (s, e) => HandleBarcodeChange();
            txtDescription.TextChanged += (s, e) => { }; // reserved

            dgvRules.CellValueChanged += DgvRules_CellValueChanged;
            dgvRules.CurrentCellDirtyStateChanged += DgvRules_CurrentCellDirtyStateChanged;
            dgvRules.CellContentClick += DgvRules_CellContentClick; // handle delete button clicks

            btnAddRule.Click += BtnAddRule_Click;
            btnDeleteRule.Click += BtnDeleteRule_Click;
            btnSavePattern.Click += BtnSavePattern_Click;

            lstPatterns.SelectedIndexChanged += LstPatterns_SelectedIndexChanged;
            btnNewPattern.Click += BtnNewPattern_Click;
            btnDeletePattern.Click += BtnDeletePattern_Click;

            txtBarcodeTest.TextChanged += (s, e) => HandleBarcodeChange();
            txtBarcodeTest.KeyPress += TxtBarcodeTest_KeyPress;
            block_text.TextChanged += (s, e) => HandleBarcodeChange();

            // load existing patterns. Create defaults only on first run (no file yet).
            var patternsFile = Path.Combine(Application.StartupPath, "patterns.xml");
            bool firstRun = !File.Exists(patternsFile);
            PatternStore.LoadFromFile(patternsFile);

            if (firstRun)
            {
                EnsureDefaultPatterns();
            }

            RefreshPatternList();
        }

        private void EnsureDefaultPatterns()
        {
            // add default pattern EEEE if missing
            var existsE = PatternStore.Patterns.Exists(p => string.Equals(p.Name, "EEEE", StringComparison.OrdinalIgnoreCase));
            if (!existsE)
            {
                var p = new Pattern() { Name = "EEEE", Description = "Default EEEE pattern" };
                p.Rules.Add(new Rule() { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.DELETE });
                p.Rules.Add(new Rule() { SourceStart = 2, SourceEnd = 3, TransformRule = TransformRuleType.FIX_TEXT, Parameter = "JH" });
                p.Rules.Add(new Rule() { SourceStart = 4, SourceEnd = 5, TransformRule = TransformRuleType.COPY });
                p.Rules.Add(new Rule() { SourceStart = 7, SourceEnd = 9, TransformRule = TransformRuleType.COPY });
                p.Rules.Add(new Rule() { SourceStart = 10, SourceEnd = 11, TransformRule = TransformRuleType.PAD_LEFT, Parameter = "2" });
                PatternStore.Patterns.Add(p);
            }

            // create or update pattern 'test' with the requested rules
            var testPattern = PatternStore.Patterns.Find(p => string.Equals(p.Name, "test", StringComparison.OrdinalIgnoreCase));
            if (testPattern == null)
            {
                testPattern = new Pattern() { Name = "test", Description = "Custom test pattern per user steps" };
                PatternStore.Patterns.Add(testPattern);
            }
            // replace rules to match requested mapping
            testPattern.Rules.Clear();
            testPattern.Rules.Add(new Rule() { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.DELETE });
            testPattern.Rules.Add(new Rule() { SourceStart = 2, SourceEnd = 5, TransformRule = TransformRuleType.YEAR_AZ, Parameter = "2567" });
            testPattern.Rules.Add(new Rule() { SourceStart = 3, SourceEnd = 7, TransformRule = TransformRuleType.AZ_LOWER });
            testPattern.Rules.Add(new Rule() { SourceStart = 8, SourceEnd = 9, TransformRule = TransformRuleType.FIX_TEXT, Parameter = "A" });
            testPattern.Rules.Add(new Rule() { SourceStart = 10, SourceEnd = 10, TransformRule = TransformRuleType.PAD_LEFT, Parameter = "2" });

            // ensure DDDD exists
            var existsDDDD = PatternStore.Patterns.Exists(p => string.Equals(p.Name, "DDDD", StringComparison.OrdinalIgnoreCase));
            if (!existsDDDD)
            {
                var d = new Pattern() { Name = "DDDD", Description = "DDDD: C200521-001 → FE21-01" };
                d.Rules.Add(new Rule() { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.DELETE });
                d.Rules.Add(new Rule() { SourceStart = 2, SourceEnd = 3, TransformRule = TransformRuleType.YEAR_AZ, Parameter = "15" });
                d.Rules.Add(new Rule() { SourceStart = 4, SourceEnd = 5, TransformRule = TransformRuleType.AZ_UPPER });
                d.Rules.Add(new Rule() { SourceStart = 6, SourceEnd = 7, TransformRule = TransformRuleType.COPY });
                d.Rules.Add(new Rule() { SourceStart = 8, SourceEnd = 8, TransformRule = TransformRuleType.COPY });
                d.Rules.Add(new Rule() { SourceStart = 9, SourceEnd = 11, TransformRule = TransformRuleType.PAD_LEFT, Parameter = "2" });
                PatternStore.Patterns.Add(d);
            }

            // ensure test2 exists as before
            var existsT2 = PatternStore.Patterns.Exists(p => string.Equals(p.Name, "test2", StringComparison.OrdinalIgnoreCase));
            if (!existsT2)
            {
                var p3 = new Pattern() { Name = "test2", Description = "test2: produce JHXX-<suffix> from barcode prefix" };
                p3.Rules.Add(new Rule() { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.DELETE });
                p3.Rules.Add(new Rule() { SourceStart = 2, SourceEnd = 3, TransformRule = TransformRuleType.FIX_TEXT, Parameter = "JH" });
                p3.Rules.Add(new Rule() { SourceStart = 4, SourceEnd = 5, TransformRule = TransformRuleType.COPY });
                p3.Rules.Add(new Rule() { SourceStart = 6, SourceEnd = 9, TransformRule = TransformRuleType.COPY });
                PatternStore.Patterns.Add(p3);
            }

            // ensure CCCC default pattern exists
            var existsCCCC = PatternStore.Patterns.Exists(p => string.Equals(p.Name, "CCCC", StringComparison.OrdinalIgnoreCase));
            if (!existsCCCC)
            {
                var c = new Pattern() { Name = "CCCC", Description = "Default CCCC placeholder rule" };
                // basic rule: copy the whole prefix (implementation will handle block_text replacement)
                c.Rules.Add(new Rule() { SourceStart = 1, SourceEnd = 999, TransformRule = TransformRuleType.COPY });
                PatternStore.Patterns.Add(c);
            }

            PatternStore.SaveToFile(Path.Combine(Application.StartupPath, "patterns.xml"));
        }

        private void DgvRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var col = dgvRules.Columns[e.ColumnIndex];
                if (col is DataGridViewButtonColumn)
                {
                    if (MessageBox.Show("Delete this rule?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        dgvRules.Rows.RemoveAt(e.RowIndex);
                        HandleBarcodeChange();
                    }
                }
            }
        }

        private void RefreshPatternList()
        {
            lstPatterns.Items.Clear();
            for (int i = 0; i < PatternStore.Patterns.Count; i++)
            {
                var p = PatternStore.Patterns[i];
                lstPatterns.Items.Add(string.IsNullOrEmpty(p.Name) ? $"(unnamed) {i+1}" : p.Name);
            }
        }

        private void LstPatterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstPatterns.SelectedIndex;
            _lastSelectedIndex = idx;

            if (idx >= 0 && idx < PatternStore.Patterns.Count)
            {
                LoadPatternToUI(PatternStore.Patterns[idx]);
            }
            HandleBarcodeChange();
        }

        private void BtnNewPattern_Click(object sender, EventArgs e)
        {
            lstPatterns.ClearSelected();
            txtPatternName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            dgvRules.Rows.Clear();
            HandleBarcodeChange();
        }

        private void BtnDeletePattern_Click(object sender, EventArgs e)
        {
            int idx = lstPatterns.SelectedIndex;
            if (idx < 0 || idx >= PatternStore.Patterns.Count)
            {
                MessageBox.Show("Select a pattern to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var name = PatternStore.Patterns[idx].Name;
            if (MessageBox.Show($"Delete pattern '{name}'?","Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PatternStore.Patterns.RemoveAt(idx);
                PatternStore.SaveToFile(Path.Combine(Application.StartupPath, "patterns.xml"));
                RefreshPatternList();
                txtPatternName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                dgvRules.Rows.Clear();
                HandleBarcodeChange();
            }
        }

        private void LoadPatternToUI(Pattern p)
        {
            _loading = true;
            try
            {
                txtPatternName.Text = p.Name;
                txtDescription.Text = p.Description;
                dgvRules.Rows.Clear();
                foreach (var r in p.Rules)
                {
                    dgvRules.Rows.Add(r.SourceStart, r.SourceEnd, ToDisplayName(r.TransformRule), r.Parameter ?? string.Empty);
                }
                txtBarcodeTest.Text = p.TestBarcode ?? string.Empty;
                block_text.Text = p.TestBlockText ?? string.Empty;
                lblPreview.Text = "Preview: " + (p.TestPreview ?? string.Empty);
            }
            finally
            {
                _loading = false;
            }
            HandleBarcodeChange();
        }

        private Pattern ReadUIToPattern()
        {
            var p = new Pattern()
            {
                Name = txtPatternName.Text,
                Description = txtDescription.Text,
                TestBarcode = txtBarcodeTest.Text ?? string.Empty,
                TestBlockText = block_text.Text ?? string.Empty,
                TestPreview = lblPreview.Text?.Replace("Preview: ", "") ?? string.Empty
            };

            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                try
                {
                    var rule = new Rule();
                    rule.SourceStart = ToInt(row.Cells[0].Value);
                    rule.SourceEnd = ToInt(row.Cells[1].Value);
                    rule.TransformRule = FromDisplayName(Convert.ToString(row.Cells[2].Value));
                    rule.Parameter = Convert.ToString(row.Cells[3].Value) ?? string.Empty;
                    p.Rules.Add(rule);
                }
                catch { }
            }

            return p;
        }

        private void BtnSavePattern_Click(object sender, EventArgs e)
        {
            var p = ReadUIToPattern();

            if (string.IsNullOrWhiteSpace(p.Name))
            {
                MessageBox.Show("Please enter a Rule name.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPatternName.Focus();
                return;
            }

            int sel = lstPatterns.SelectedIndex;

            // ตรวจชื่อซ้ำ — ยกเว้นตัวที่กำลังแก้อยู่
            for (int i = 0; i < PatternStore.Patterns.Count; i++)
            {
                if (i == sel) continue;
                if (string.Equals(PatternStore.Patterns[i].Name, p.Name, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Rule name '{p.Name}' already exists.\nPlease use a different name.",
                        "Duplicate name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPatternName.Focus();
                    return;
                }
            }

            if (sel >= 0 && sel < PatternStore.Patterns.Count)
            {
                PatternStore.Patterns[sel] = p;
            }
            else
            {
                PatternStore.Patterns.Add(p);
            }

            PatternStore.Patterns.Sort((a,b)=>string.Compare(a.Name,b.Name,StringComparison.OrdinalIgnoreCase));
            PatternStore.SaveToFile(Path.Combine(Application.StartupPath, "patterns.xml"));
            RefreshPatternList();
            MessageBox.Show("Pattern saved.");
        }

        private void InitializeDataGrid()
        {
            dgvRules.Columns.Clear();

            var colStart = new DataGridViewTextBoxColumn() { Name = "SourceStart", HeaderText = "From" };
            var colEnd = new DataGridViewTextBoxColumn() { Name = "SourceEnd", HeaderText = "To" };
            var colRule = new DataGridViewComboBoxColumn() { Name = "TransformRule", HeaderText = "Transform Rule" };
            colRule.Items.AddRange(RuleDisplayNames.Values.ToArray());
            var colParam = new DataGridViewTextBoxColumn() { Name = "Parameter", HeaderText = "Value" };
            var colDelete = new DataGridViewButtonColumn() { Name = "DeleteRow", HeaderText = "Action", Text = "Delete", UseColumnTextForButtonValue = true };

            dgvRules.Columns.AddRange(new DataGridViewColumn[] { colStart, colEnd, colRule, colParam, colDelete });

            dgvRules.AllowUserToAddRows = false;
            dgvRules.AllowUserToDeleteRows = true;
            dgvRules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvRules.UserDeletedRow += (s2, e2) => HandleBarcodeChange();
        }

        private void DgvRules_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvRules.IsCurrentCellDirty)
            {
                dgvRules.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvRules_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            HandleBarcodeChange();
        }

        private void BtnAddRule_Click(object sender, EventArgs e)
        {
            dgvRules.Rows.Add(1, 1, "FIX_TEXT", string.Empty, true);
            HandleBarcodeChange();
        }

        private void BtnDeleteRule_Click(object sender, EventArgs e)
        {
            if (dgvRules.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in dgvRules.SelectedRows)
                {
                    dgvRules.Rows.Remove(r);
                }
                HandleBarcodeChange();
            }
            else if (dgvRules.CurrentRow != null)
            {
                dgvRules.Rows.Remove(dgvRules.CurrentRow);
                HandleBarcodeChange();
            }
        }

        private int ToInt(object v)
        {
            if (v == null) return 0;
            int x; return int.TryParse(v.ToString(), out x) ? x : 0;
        }

        private void TxtBarcodeTest_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allow control chars (backspace, etc.) and dash always
            if (char.IsControl(e.KeyChar) || e.KeyChar == '-')
                return;

            // count current non-dash characters
            var current = txtBarcodeTest.Text ?? string.Empty;
            int selectedLen = txtBarcodeTest.SelectionLength;
            int nonDashCount = 0;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != '-') nonDashCount++;
            }
            // subtract selected non-dash chars (they will be replaced)
            if (selectedLen > 0)
            {
                int selStart = txtBarcodeTest.SelectionStart;
                for (int i = selStart; i < selStart + selectedLen && i < current.Length; i++)
                {
                    if (current[i] != '-') nonDashCount--;
                }
            }

            if (nonDashCount >= 10)
            {
                e.Handled = true; // block input
            }
        }

        private Pattern ReadUIToPatternLive()
        {
            var p = new Pattern();
            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                try
                {
                    var rule = new Rule();
                    rule.SourceStart = ToInt(row.Cells[0].Value);
                    rule.SourceEnd = ToInt(row.Cells[1].Value);
                    rule.TransformRule = FromDisplayName(Convert.ToString(row.Cells[2].Value));
                    rule.Parameter = Convert.ToString(row.Cells[3].Value) ?? string.Empty;
                    p.Rules.Add(rule);
                }
                catch { }
            }
            return p;
        }

        /// <summary>
        /// barcode_test เข้า rule ทั้งก้อน (นับขีดเป็นตำแหน่งด้วย)
        /// block_text ไม่เข้า rule — ลบชื่อ pattern ออก แล้วต่อท้ายผลลัพธ์
        /// พิมพ์ barcode / block_text → preview ทันทีเสมอ
        /// </summary>
        private void HandleBarcodeChange()
        {
            var barcode = txtBarcodeTest.Text?.Trim() ?? string.Empty;
            var blockRaw = block_text.Text ?? string.Empty;

            if (string.IsNullOrEmpty(barcode))
            {
                lblPreview.Text = "Preview: ";
                return;
            }

            // สร้าง pattern จาก UI grid (live)
            var livePattern = ReadUIToPatternLive();

            // 1) barcode เข้า rule ทั้งก้อน — ถ้าไม่มี rule ก็แสดง barcode ดิบ
            string transformed = livePattern.Rules.Count > 0
                ? livePattern.Apply(barcode)
                : barcode;

            // 2) ถ้า block_text มี Rule name → replace ตรงตำแหน่งด้วยผล rule
            //    ไม่เจอ Rule name → แสดง block_text ตรงๆ
            //    ไม่มี block_text → แสดงผล rule อย่างเดียว
            if (!string.IsNullOrEmpty(blockRaw))
            {
                var patternName = txtPatternName.Text?.Trim() ?? string.Empty;

                // ลอง Rule name ปัจจุบันก่อน
                if (!string.IsNullOrEmpty(patternName) && blockRaw.Contains(patternName))
                {
                    lblPreview.Text = "Preview: " + blockRaw.Replace(patternName, transformed);
                    return;
                }

                // ลองหาจาก PatternStore
                foreach (var p in PatternStore.Patterns)
                {
                    if (!string.IsNullOrEmpty(p.Name) && blockRaw.Contains(p.Name))
                    {
                        lblPreview.Text = "Preview: " + blockRaw.Replace(p.Name, transformed);
                        return;
                    }
                }

                // ไม่เจอ Rule name → แสดง block_text ตรงๆ
                lblPreview.Text = "Preview: " + blockRaw;
                return;
            }

            // ไม่มี block_text → แสดงผล rule อย่างเดียว
            lblPreview.Text = "Preview: " + transformed;
        }

        /// <summary>
        /// Save preview text ลง pattern ปัจจุบัน
        /// </summary>
        private void SavePreviewToPattern()
        {
            int idx = lstPatterns.SelectedIndex;
            if (idx >= 0 && idx < PatternStore.Patterns.Count)
            {
                PatternStore.Patterns[idx].TestPreview = lblPreview.Text?.Replace("Preview: ", "") ?? string.Empty;
            }
        }

        private void PatternEditorControl_Load(object sender, EventArgs e)
        {

        }

        private int _lastSelectedIndex = -1;
    }
}
