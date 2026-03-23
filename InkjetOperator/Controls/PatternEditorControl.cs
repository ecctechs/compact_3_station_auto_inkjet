using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Controls
{
    public class PatternEditorControl : UserControl
    {
        // ─── Controls ───
        private ListBox lstPatterns;
        private Button btnNewPattern, btnDeletePattern;
        private TextBox txtPatternName, txtDescription, txtBarcodeTest, txtBlockText;
        private RichTextBox rtbPreview;
        private DataGridView dgvRules;
        private Button btnAddRule, btnDeleteRule, btnSave;

        private readonly string _xmlPath;
        private Pattern _current;

        public PatternEditorControl()
        {
            _xmlPath = Path.Combine(Application.StartupPath, "patterns.xml");
            InitControls();
            RefreshList();
        }

        // ─── Layout ───
        private void InitControls()
        {
            this.SuspendLayout();

            // ── Left panel: pattern list + buttons ──
            var panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 160,
                Padding = new Padding(4),
            };

            lstPatterns = new ListBox { Dock = DockStyle.Fill };
            lstPatterns.SelectedIndexChanged += LstPatterns_SelectedIndexChanged;

            var panelLeftButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
            };
            btnNewPattern = new Button { Text = "New", Width = 70, Height = 28 };
            btnDeletePattern = new Button { Text = "Delete", Width = 70, Height = 28 };
            btnNewPattern.Click += BtnNewPattern_Click;
            btnDeletePattern.Click += BtnDeletePattern_Click;
            panelLeftButtons.Controls.AddRange(new Control[] { btnNewPattern, btnDeletePattern });

            panelLeft.Controls.Add(lstPatterns);
            panelLeft.Controls.Add(panelLeftButtons);

            // ── Right panel ──
            var panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 4, 4, 4),
            };

            // ── Top fields ──
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 105,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = false,
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtPatternName = new TextBox { Dock = DockStyle.Fill };
            txtDescription = new TextBox { Dock = DockStyle.Fill };
            txtBarcodeTest = new TextBox { Dock = DockStyle.Fill };
            txtBlockText = new TextBox { Dock = DockStyle.Fill };

            txtPatternName.TextChanged += (s, e) => UpdatePreview();
            txtBarcodeTest.TextChanged += (s, e) => UpdatePreview();
            txtBlockText.TextChanged += (s, e) => UpdatePreview();

            tbl.Controls.Add(new Label { Text = "Rule name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            tbl.Controls.Add(txtPatternName, 1, 0);
            tbl.Controls.Add(new Label { Text = "Description:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            tbl.Controls.Add(txtDescription, 1, 1);
            tbl.Controls.Add(new Label { Text = "Lot test:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            tbl.Controls.Add(txtBarcodeTest, 1, 2);
            tbl.Controls.Add(new Label { Text = "Block text:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            tbl.Controls.Add(txtBlockText, 1, 3);

            // ── Preview panel (step-by-step breakdown) ──
            rtbPreview = new RichTextBox
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 255),
                Font = new Font("Consolas", 9.5f),
            };

            // ── Rules grid ──
            dgvRules = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
            };

            dgvRules.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "From", DataPropertyName = "SourceStart", Width = 55 });
            dgvRules.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "To", DataPropertyName = "SourceEnd", Width = 55 });
            dgvRules.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "Transform Rule",
                DataPropertyName = "TransformRule",
                DataSource = Enum.GetValues(typeof(TransformRuleType)),
                Width = 140,
            });
            dgvRules.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", DataPropertyName = "Parameter", Width = 80 });
            dgvRules.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = "Del",
                Text = "✕",
                UseColumnTextForButtonValue = true,
                Width = 40,
                Name = "colDelete",
            });

            dgvRules.CellValueChanged += (s, e) => UpdatePreview();
            dgvRules.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvRules.IsCurrentCellDirty) dgvRules.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvRules.CellContentClick += DgvRules_CellContentClick;

            // ── Bottom buttons ──
            var panelBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0),
            };
            btnAddRule = new Button { Text = "Add Rule", Width = 90, Height = 28 };
            btnDeleteRule = new Button { Text = "Delete Rule", Width = 90, Height = 28 };
            btnSave = new Button { Text = "Save Rule", Width = 100, Height = 28 };
            btnSave.Font = new Font(btnSave.Font, FontStyle.Bold);
            btnAddRule.Click += BtnAddRule_Click;
            btnDeleteRule.Click += BtnDeleteRule_Click;
            btnSave.Click += BtnSave_Click;

            panelBottom.Controls.Add(btnAddRule);
            panelBottom.Controls.Add(btnDeleteRule);
            panelBottom.Controls.Add(new Label { Width = 100 }); // spacer
            panelBottom.Controls.Add(btnSave);

            // Dock order matters: Bottom first, then Fill
            panelRight.Controls.Add(dgvRules);       // Fill
            panelRight.Controls.Add(rtbPreview);      // Bottom
            panelRight.Controls.Add(panelBottom);     // Bottom
            panelRight.Controls.Add(tbl);             // Top

            this.Controls.Add(panelRight);
            this.Controls.Add(panelLeft);

            this.ResumeLayout(false);

            SetFieldsEnabled(false);
        }

        // ─── List ───
        private void RefreshList()
        {
            lstPatterns.Items.Clear();
            foreach (var p in PatternStore.Patterns)
                lstPatterns.Items.Add(p.Name ?? "(no name)");

            if (lstPatterns.Items.Count > 0)
                lstPatterns.SelectedIndex = 0;
            else
                ClearFields();
        }

        private void LstPatterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstPatterns.SelectedIndex;
            if (idx < 0 || idx >= PatternStore.Patterns.Count)
            {
                ClearFields();
                return;
            }

            _current = PatternStore.Patterns[idx];
            LoadPattern(_current);
            SetFieldsEnabled(true);
        }

        // ─── Load / Save ───
        private void LoadPattern(Pattern p)
        {
            txtPatternName.Text = p.Name ?? "";
            txtDescription.Text = p.Description ?? "";
            txtBarcodeTest.Text = p.TestBarcode ?? "";
            txtBlockText.Text = p.TestBlockText ?? "";
            dgvRules.DataSource = null;
            dgvRules.DataSource = new BindingList<Rule>(p.Rules);
            UpdatePreview();
        }

        private void SavePatternToModel()
        {
            if (_current == null) return;
            _current.Name = txtPatternName.Text.Trim();
            _current.Description = txtDescription.Text.Trim();
            _current.TestBarcode = txtBarcodeTest.Text.Trim();
            _current.TestBlockText = txtBlockText.Text.Trim();
            _current.TestPreview = GetResultText();
        }

        // ─── Preview ───
        private void UpdatePreview()
        {
            if (_current == null) { rtbPreview.Clear(); return; }

            try
            {
                string barcode = txtBarcodeTest.Text;
                string blockText = txtBlockText.Text;

                if (string.IsNullOrEmpty(barcode))
                {
                    rtbPreview.Clear();
                    return;
                }

                rtbPreview.Clear();

                // Header — แสดง Lot + Block text
                AppendText("Lot:        ", Color.DimGray);
                AppendLine(barcode, Color.Black, true);
                if (!string.IsNullOrEmpty(blockText))
                {
                    AppendText("Block text: ", Color.DimGray);
                    AppendLine(blockText, Color.Black, true);
                }
                AppendLine(new string('─', 50), Color.Gray, false);

                // Step-by-step rule breakdown
                var resultParts = new List<string>();
                for (int i = 0; i < _current.Rules.Count; i++)
                {
                    var rule = _current.Rules[i];
                    string extracted = rule.ExtractSource(barcode);
                    string output = rule.Apply(barcode);
                    resultParts.Add(output);

                    string extractDisplay = string.IsNullOrEmpty(extracted) ? "(empty)" : $"\"{extracted}\"";
                    string outputDisplay = string.IsNullOrEmpty(output) ? "(ลบ)" : $"\"{output}\"";
                    string paramDisplay = string.IsNullOrEmpty(rule.Parameter) ? "" : $"({rule.Parameter})";
                    string ruleLabel = $"{rule.TransformRule}{paramDisplay}";

                    string line = $"  Rule {i + 1}: [{rule.SourceStart}-{rule.SourceEnd}] {extractDisplay,-12} → {ruleLabel,-18} → ";

                    AppendText(line, Color.DimGray);
                    AppendLine(outputDisplay, output == string.Empty ? Color.Red : Color.DarkGreen, true);
                }

                AppendLine(new string('─', 50), Color.Gray, false);

                // Final combined result
                string combined = string.Concat(resultParts);
                string patternName = txtPatternName.Text.Trim();

                // แสดงการแทนที่ pattern name ใน block text
                if (!string.IsNullOrEmpty(patternName) && !string.IsNullOrEmpty(blockText) && blockText.Contains(patternName))
                {
                    AppendText($"  {patternName}", Color.Red, true);
                    AppendText(" → ", Color.DimGray);
                    AppendLine(combined, Color.DarkGreen, true);

                    string finalResult = blockText.Replace(patternName, combined);
                    AppendText("  Result: ", Color.Black, true);
                    AppendLine(finalResult, Color.DarkBlue, true);
                }
                else if (!string.IsNullOrEmpty(blockText))
                {
                    AppendText("  Result: ", Color.Black, true);
                    AppendLine(combined, Color.DarkBlue, true);
                }
                else
                {
                    AppendText("  Result: ", Color.Black, true);
                    AppendLine(combined, Color.DarkBlue, true);
                }
            }
            catch
            {
                rtbPreview.Clear();
                AppendLine("(error)", Color.Red, false);
            }
        }

        private string GetResultText()
        {
            if (_current == null) return "";
            string barcode = txtBarcodeTest.Text;
            string blockText = txtBlockText.Text;
            string patternName = txtPatternName.Text.Trim();
            if (string.IsNullOrEmpty(barcode)) return "";

            string combined = _current.Apply(barcode);
            if (!string.IsNullOrEmpty(patternName) && !string.IsNullOrEmpty(blockText) && blockText.Contains(patternName))
                return blockText.Replace(patternName, combined);
            return combined;
        }

        private void AppendText(string text, Color color, bool bold = false)
        {
            int start = rtbPreview.TextLength;
            rtbPreview.AppendText(text);
            rtbPreview.Select(start, text.Length);
            rtbPreview.SelectionColor = color;
            rtbPreview.SelectionFont = new Font(rtbPreview.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbPreview.SelectionLength = 0;
        }

        private void AppendLine(string text, Color color, bool bold)
        {
            AppendText(text + Environment.NewLine, color, bold);
        }

        // ─── Pattern CRUD ───
        private void BtnNewPattern_Click(object sender, EventArgs e)
        {
            var p = new Pattern { Name = "NEW" };
            PatternStore.Patterns.Add(p);
            RefreshList();
            lstPatterns.SelectedIndex = lstPatterns.Items.Count - 1;
        }

        private void BtnDeletePattern_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            if (MessageBox.Show($"Delete pattern '{_current.Name}'?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            PatternStore.Patterns.Remove(_current);
            _current = null;
            RefreshList();
        }

        // ─── Rule CRUD ───
        private void BtnAddRule_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            _current.Rules.Add(new Rule { SourceStart = 1, SourceEnd = 1, TransformRule = TransformRuleType.COPY });
            dgvRules.DataSource = null;
            dgvRules.DataSource = new BindingList<Rule>(_current.Rules);
            UpdatePreview();
        }

        private void BtnDeleteRule_Click(object sender, EventArgs e)
        {
            if (_current == null || dgvRules.SelectedRows.Count == 0) return;
            int idx = dgvRules.SelectedRows[0].Index;
            DeleteRuleAt(idx);
        }

        private void DgvRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvRules.Columns[e.ColumnIndex].Name == "colDelete")
                DeleteRuleAt(e.RowIndex);
        }

        private void DeleteRuleAt(int idx)
        {
            if (_current == null || idx < 0 || idx >= _current.Rules.Count) return;
            _current.Rules.RemoveAt(idx);
            dgvRules.DataSource = null;
            dgvRules.DataSource = new BindingList<Rule>(_current.Rules);
            UpdatePreview();
        }

        // ─── Save ───
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_current == null) return;
            SavePatternToModel();

            try
            {
                PatternStore.Save(_xmlPath);
                int idx = lstPatterns.SelectedIndex;
                RefreshList();
                if (idx >= 0 && idx < lstPatterns.Items.Count)
                    lstPatterns.SelectedIndex = idx;
                MessageBox.Show("Saved!", "PatternEditor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Helpers ───
        private void ClearFields()
        {
            _current = null;
            txtPatternName.Text = "";
            txtDescription.Text = "";
            txtBarcodeTest.Text = "";
            txtBlockText.Text = "";
            rtbPreview.Clear();
            dgvRules.DataSource = null;
            SetFieldsEnabled(false);
        }

        private void InitializeComponent()
        {

        }

        private void SetFieldsEnabled(bool enabled)
        {
            txtPatternName.Enabled = enabled;
            txtDescription.Enabled = enabled;
            txtBarcodeTest.Enabled = enabled;
            txtBlockText.Enabled = enabled;
            dgvRules.Enabled = enabled;
            btnAddRule.Enabled = enabled;
            btnDeleteRule.Enabled = enabled;
            btnSave.Enabled = enabled;
        }
    }

    // BindingList for DataGridView editing support
    class BindingList<T> : System.ComponentModel.BindingList<T>
    {
        public BindingList(List<T> list) : base(list) { }
    }
}
