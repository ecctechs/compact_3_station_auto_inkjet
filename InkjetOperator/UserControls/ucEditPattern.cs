using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Xsl;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator
{
    public partial class ucEditPattern : UserControl
    {
        private BindingList<Pattern>? _patterns;
        private BindingList<Rule>? _currentRules;
        private Pattern? _selectedPattern;

        public ucEditPattern()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadData();
        }

        private void SetupDataGridView()
        {
            dgvRules.AutoGenerateColumns = false;
            colFrom.DataPropertyName = "SourceStart";
            colTo.DataPropertyName = "SourceEnd";
            colTransform.DataPropertyName = "TransformRule";
            colParameter.DataPropertyName = "Parameter";

            // ตั้งค่า Enum ให้กับ ComboBox ในตาราง
            // แก้ไขส่วนนี้: เพื่อให้ ComboBox แสดงชื่อตาม Description
            colTransform.DataSource = Enum.GetValues(typeof(TransformRuleType))
                .Cast<TransformRuleType>()
                .Select(e => new
                {
                    Value = e,
                    Name = GetEnumDescription(e)
                }).ToList();
            colTransform.ValueMember = "Value";
            colTransform.DisplayMember = "Name";
        }

        // เพิ่มฟังก์ชันช่วยอ่านค่า Description ไว้ข้างล่าง SetupDataGridView
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                  .FirstOrDefault() as DescriptionAttribute;
            return attribute != null ? attribute.Description : value.ToString();
        }

        private void LoadData()
        {
            _patterns = new BindingList<Pattern>(PatternStore.Patterns);
            lstPatterns.DataSource = _patterns;
            lstPatterns.DisplayMember = "Name";
        }

        private void lstPatterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPattern = lstPatterns.SelectedItem as Pattern;
            if (_selectedPattern == null) return;

            // ดึงค่าจาก Object มาแสดง (รวมถึงค่าที่โหลดจาก XML)
            txtPatternName.Text = _selectedPattern.Name;
            txtDescription.Text = _selectedPattern.Description;
            txtBarcodeTest.Text = _selectedPattern.TestBarcode;
            txtBlockText.Text = _selectedPattern.TestBlockText;

            _currentRules = new BindingList<Rule>(_selectedPattern.Rules);
            dgvRules.DataSource = _currentRules;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_selectedPattern == null) return;

            // จำลองการใช้กฎกับ Barcode ที่กรอกในช่อง Lot test
            string result = _selectedPattern.Apply(txtBarcodeTest.Text);

            // ถ้าใน Block text มีชื่อ Pattern อยู่ ให้แทนที่ด้วยผลลัพธ์
            if (!string.IsNullOrEmpty(txtPatternName.Text) && txtBlockText.Text.Contains(txtPatternName.Text))
                lblPreview.Text = txtBlockText.Text.Replace(txtPatternName.Text, result);
            else
                lblPreview.Text = result;
        }

        private void dgvRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // ตรวจสอบว่าคลิกที่ปุ่มในคอลัมน์ colDelete หรือไม่
            if (e.RowIndex >= 0 && dgvRules.Columns[e.ColumnIndex].Name == "colDelete")
            {
                _currentRules?.RemoveAt(e.RowIndex);
                UpdatePreview();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedPattern != null)
            {
                _selectedPattern.Name = txtPatternName.Text;
                _selectedPattern.Description = txtDescription.Text;
                _selectedPattern.TestBarcode = txtBarcodeTest.Text; // บันทึกค่าทดสอบลง XML
                _selectedPattern.TestBlockText = txtBlockText.Text;   // บันทึกค่าทดสอบลง XML
            }

            string xmlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "patterns.xml");
            PatternStore.Save(xmlPath);
            MessageBox.Show("บันทึกข้อมูลรูปแบบและค่าทดสอบลงไฟล์เรียบร้อยแล้ว", "บันทึกสำเร็จ");
        }

        private void btnAddRule_Click(object sender, EventArgs e)
        {
            _currentRules?.Add(new Rule());
        }

        private void btnAddPattern_Click(object sender, EventArgs e)
        {
            var newP = new Pattern { Name = "NEW_PATTERN" };
            _patterns?.Add(newP);
            lstPatterns.SelectedItem = newP;
        }

        private void btnDeletePattern_Click(object sender, EventArgs e)
        {
            if (_selectedPattern != null && MessageBox.Show("ยืนยันการลบ Pattern นี้?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _patterns?.Remove(_selectedPattern);
        }

        private void InputChanged(object sender, EventArgs e) => UpdatePreview();
        private void dgvRules_CellValueChanged(object sender, DataGridViewCellEventArgs e) => UpdatePreview();
    }
}