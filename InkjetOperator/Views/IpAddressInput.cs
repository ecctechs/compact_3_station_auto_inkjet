using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace InkjetOperator.Views;

/// <summary>
/// ช่องกรอก IP Address แบบ 4 ช่องคั่นด้วยจุด
///
/// <para>
/// ใช้แทน <c>AntdUI.Input</c> ช่องเดียวได้เลย เพราะเปิด <see cref="Text"/> ให้อ่าน/เขียน
/// เป็นสตริงเต็ม "192.168.1.10" เหมือนเดิม โค้ดหน้าเดิมที่อ่าน <c>.Text</c> จึงไม่ต้องแก้
/// </para>
///
/// <para>
/// กรอกครบ 3 หลักหรือกดจุดแล้วเด้งไปช่องถัดไปให้เอง และกด Backspace ตอนช่องว่าง
/// จะถอยกลับช่องก่อนหน้า เพื่อให้พิมพ์รวดเดียวได้โดยไม่ต้องใช้เมาส์
/// </para>
/// </summary>
public partial class IpAddressInput : UserControl
{
    private AntdUI.Input[] _octets = [];

    /// <summary>กันไม่ให้ตอนเซ็ตค่าจากโค้ดไปกระตุ้น TextChanged ซ้ำทีละช่อง</summary>
    private bool _loading;

    public IpAddressInput()
    {
        InitializeComponent();

        _octets = [txtOctet1, txtOctet2, txtOctet3, txtOctet4];

        for (int i = 0; i < _octets.Length; i++)
        {
            var box = _octets[i];
            int index = i;

            box.TextChanged += (_, _) => OctetChanged(index);
            box.KeyPress += (s, e) => OctetKeyPress(index, e);
            box.KeyDown += (s, e) => OctetKeyDown(index, e);
            box.GotFocus += (_, _) => box.SelectAll();
        }
    }

    /// <summary>ค่า IP เต็ม เช่น "192.168.1.10" — ว่างทุกช่องจะได้สตริงว่าง</summary>
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [AllowNull]
    public override string Text
    {
        get
        {
            var parts = _octets.Select(o => o.Text.Trim()).ToArray();
            return parts.All(string.IsNullOrEmpty) ? "" : string.Join(".", parts);
        }
        set
        {
            var parts = (value ?? "").Split('.');

            _loading = true;
            try
            {
                for (int i = 0; i < _octets.Length; i++)
                    _octets[i].Text = i < parts.Length ? Clean(parts[i]) : "";
            }
            finally { _loading = false; }

            OnTextChanged(EventArgs.Empty);
        }
    }

    /// <summary>สีพื้นของทุกช่อง — หน้าตั้งค่าใช้ทำเครื่องหมายว่ามีการแก้ยังไม่บันทึก</summary>
    public override Color BackColor
    {
        // AntdUI.Input เก็บสีพื้นเป็น Color? — ไม่ได้ตั้งไว้ก็ใช้สีของตัว control เอง
        get => _octets.Length > 0 ? _octets[0].BackColor ?? base.BackColor : base.BackColor;
        set
        {
            foreach (var box in _octets) box.BackColor = value;
        }
    }

    /// <summary>true = กรอกครบ 4 ช่องและทุกช่องอยู่ในช่วง 0–255</summary>
    public bool IsValid =>
        _octets.All(o => byte.TryParse(o.Text.Trim(), out _));

    // ── พฤติกรรมการพิมพ์ ───────────────────────────────────

    private void OctetChanged(int index)
    {
        if (_loading) return;

        var box = _octets[index];
        var cleaned = Clean(box.Text);

        if (cleaned != box.Text)
        {
            box.Text = cleaned;      // เข้ามาซ้ำอีกรอบแล้วจบที่รอบนั้น
            return;
        }

        OnTextChanged(EventArgs.Empty);

        // ครบ 3 หลักแล้วไปช่องถัดไปเลย ไม่ต้องกด Tab
        if (cleaned.Length == 3 && index < _octets.Length - 1)
            FocusOctet(index + 1);
    }

    private void OctetKeyPress(int index, KeyPressEventArgs e)
    {
        // จุดหรือลูกน้ำ = ข้ามไปช่องถัดไป ไม่ต้องพิมพ์ลงในช่อง
        if (e.KeyChar is '.' or ',')
        {
            e.Handled = true;
            if (index < _octets.Length - 1) FocusOctet(index + 1);
            return;
        }

        // รับเฉพาะตัวเลขกับปุ่มควบคุม
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
    }

    private void OctetKeyDown(int index, KeyEventArgs e)
    {
        if (index == 0) return;

        // ลบจนช่องว่างแล้วยังกด Backspace ต่อ = ถอยไปช่องก่อนหน้า
        if (e.KeyCode == Keys.Back && _octets[index].Text.Length == 0)
        {
            FocusOctet(index - 1);
            e.SuppressKeyPress = true;
        }
    }

    private void FocusOctet(int index)
    {
        var box = _octets[index];
        box.Focus();
        box.SelectAll();
    }

    /// <summary>เหลือเฉพาะตัวเลข ไม่เกิน 3 หลัก และไม่เกิน 255</summary>
    private static string Clean(string? raw)
    {
        var digits = new string((raw ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return "";
        if (digits.Length > 3) digits = digits.Substring(0, 3);

        return int.TryParse(digits, out int value) && value > 255 ? "255" : digits;
    }
}
