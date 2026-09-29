namespace InkjetOperator.Views;

internal static class NumericInput
{
    public static void DigitsOnly(params AntdUI.Input[] boxes)
    {
        foreach (var box in boxes)
            box.VerifyChar += (_, e) => e.Result = char.IsDigit(e.Char);
    }

    public static void DecimalOnly(params AntdUI.Input[] boxes)
    {
        foreach (var box in boxes)
        {
            box.VerifyChar += (sender, e) =>
            {
                if (char.IsDigit(e.Char)) return;

                e.Result = e.Char == '.'
                    && sender is AntdUI.Input input
                    && !input.Text.Contains('.');
            };
        }
    }

    public static void DigitsOnlyColumns(AntdUI.Table table, params string[] columnKeys)
    {
        table.CellBeginEditInputStyle += (_, e) =>
        {
            if (!columnKeys.Contains(e.Column.Key, StringComparer.Ordinal)) return;
            e.Input.VerifyChar += (_, key) => key.Result = char.IsDigit(key.Char);
        };
    }
}
