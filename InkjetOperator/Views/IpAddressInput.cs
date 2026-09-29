using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace InkjetOperator.Views;

public partial class IpAddressInput : UserControl
{
    private AntdUI.Input[] _octets = [];

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

    private string _placeholder = "";

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue("")]
    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value ?? "";

            var parts = _placeholder.Split('.');
            for (int i = 0; i < _octets.Length; i++)
                _octets[i].PlaceholderText = i < parts.Length ? parts[i].Trim() : "";
        }
    }

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

    public override Color BackColor
    {
        get => _octets.Length > 0 ? _octets[0].BackColor ?? base.BackColor : base.BackColor;
        set
        {
            foreach (var box in _octets) box.BackColor = value;
        }
    }

    public bool IsValid =>
        _octets.All(o => byte.TryParse(o.Text.Trim(), out _));

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

        if (cleaned.Length == 3 && index < _octets.Length - 1)
            FocusOctet(index + 1);
    }

    private void OctetKeyPress(int index, KeyPressEventArgs e)
    {
        if (e.KeyChar is '.' or ',')
        {
            e.Handled = true;
            if (index < _octets.Length - 1) FocusOctet(index + 1);
            return;
        }

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
    }

    private void OctetKeyDown(int index, KeyEventArgs e)
    {
        if (index == 0) return;

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

    private static string Clean(string? raw)
    {
        var digits = new string((raw ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return "";
        if (digits.Length > 3) digits = digits.Substring(0, 3);

        return int.TryParse(digits, out int value) && value > 255 ? "255" : digits;
    }
}
