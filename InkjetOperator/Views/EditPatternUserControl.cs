using System.ComponentModel;
using System.Reflection;
using InkjetOperator.Models;
using InkjetOperator.Services;

namespace InkjetOperator.Views;

/// <summary>
/// Edit Pattern page. Left: pattern list. Right: pattern detail form + transform
/// rules (AntdUI.Table) + live preview + actions. Controls and layout live in the
/// Designer file; this file holds the page interaction/data-binding logic
/// (patterns come from <see cref="PatternStore"/>, preview from Pattern.Apply).
/// </summary>
public partial class EditPatternUserControl : UserControl
{
    // Enum <-> on-screen label maps (label = [Description] on TransformRuleType).
    private static readonly Dictionary<TransformRuleType, string> _labelByType = new();
    private static readonly Dictionary<string, TransformRuleType> _typeByLabel = new();

    static EditPatternUserControl()
    {
        foreach (TransformRuleType t in Enum.GetValues<TransformRuleType>())
        {
            string label = DescriptionOf(t);
            _labelByType[t] = label;
            _typeByLabel[label] = t;
        }
    }

    private readonly string _patternsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patterns.xml");

    private BindingList<Pattern>? _patterns;
    private Pattern? _selectedPattern;
    private List<RuleRow> _rows = new();
    private bool _loading;

    public EditPatternUserControl()
    {
        InitializeComponent();
        ConfigureRuleColumns();
        WireEvents();
        LoadData();
    }

    // ── Setup ──────────────────────────────────────────────

    /// <summary>Define the AntdUI.Table columns (data-binding config, required in code).</summary>
    private void ConfigureRuleColumns()
    {
        tblRules.Columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("From", "From", AntdUI.ColumnAlign.Center) { Editable = true, Width = "115" },
            new AntdUI.Column("To", "To", AntdUI.ColumnAlign.Center) { Editable = true, Width = "115" },
            new AntdUI.ColumnSelect("RuleLabel", "Rule", AntdUI.ColumnAlign.Center) { Editable = true, Width = "184", Items = RuleTypeItems() },
            new AntdUI.Column("Value", "Value", AntdUI.ColumnAlign.Center) { Editable = true, Width = "161" },
            new AntdUI.Column("Op", "Action", AntdUI.ColumnAlign.Center) { Width = "103" },
        };
    }

    private void WireEvents()
    {
        lstPatterns.SelectedIndexChanged += (_, _) => LoadSelectedPattern();
        btnAddRule.Click += btnAddRule_Click;
        btnNewPattern.Click += btnNewPattern_Click;
        btnDeletePattern.Click += btnDeletePattern_Click;
        btnSaveRule.Click += btnSaveRule_Click;
        btnCancel.Click += (_, _) => LoadSelectedPattern();
        txtLotTest.TextChanged += (_, _) => UpdatePreview();
        txtBlockText.TextChanged += (_, _) => UpdatePreview();
        tblRules.CellButtonClick += tblRules_CellButtonClick;
        tblRules.CellEndEdit += tblRules_CellEndEdit;
    }

    private void LoadData()
    {
        _patterns = new BindingList<Pattern>(PatternStore.Patterns);
        lstPatterns.DataSource = _patterns;
        lstPatterns.DisplayMember = "Name";
        if (_patterns.Count > 0) lstPatterns.SelectedIndex = 0;
        else LoadSelectedPattern();
    }

    // ── Pattern selection ──────────────────────────────────

    private void LoadSelectedPattern()
    {
        _loading = true;
        _selectedPattern = lstPatterns.SelectedItem as Pattern;

        if (_selectedPattern == null)
        {
            txtPatternName.Text = "";
            txtDescription.Text = "";
            txtLotTest.Text = "";
            txtBlockText.Text = "";
            _rows = new List<RuleRow>();
            RebindTable();
            lblPreview.Text = "";
            _loading = false;
            return;
        }

        txtPatternName.Text = _selectedPattern.Name;
        txtDescription.Text = _selectedPattern.Description;
        txtLotTest.Text = _selectedPattern.TestBarcode;
        txtBlockText.Text = _selectedPattern.TestBlockText;

        _rows = _selectedPattern.Rules.Select(FromRule).ToList();
        RebindTable();

        _loading = false;
        UpdatePreview();
    }

    // ── Rules table ────────────────────────────────────────

    private void RebindTable()
    {
        tblRules.DataSource = null;
        tblRules.DataSource = _rows;
    }

    /// <summary>Rebuild the selected pattern's Rules from the table rows.</summary>
    private void SyncRulesToPattern()
    {
        if (_selectedPattern == null) return;
        _selectedPattern.Rules = _rows.Select(ToRule).ToList();
    }

    private void btnAddRule_Click(object? sender, EventArgs e)
    {
        if (_selectedPattern == null) return;
        _rows.Add(new RuleRow
        {
            From = "1",
            To = "1",
            RuleLabel = _labelByType[TransformRuleType.COPY],
            Value = "",
            Op = NewDeleteButtons(),
        });
        RebindTable();
        SyncRulesToPattern();
        UpdatePreview();
    }

    private void tblRules_CellButtonClick(object? sender, AntdUI.TableButtonEventArgs e)
    {
        if (e.Btn?.Id == "del" && e.Record is RuleRow row)
        {
            _rows.Remove(row);
            RebindTable();
            SyncRulesToPattern();
            UpdatePreview();
        }
    }

    private bool tblRules_CellEndEdit(object? sender, AntdUI.TableEndEditEventArgs e)
    {
        if (e.Record is RuleRow row)
        {
            switch (e.Column?.Key)
            {
                case "From": row.From = e.Value ?? ""; break;
                case "To": row.To = e.Value ?? ""; break;
                case "RuleLabel": row.RuleLabel = e.Value ?? ""; break;
                case "Value": row.Value = e.Value ?? ""; break;
            }
        }
        SyncRulesToPattern();
        UpdatePreview();
        return true;
    }

    // ── Pattern add / delete / save ────────────────────────

    private void btnNewPattern_Click(object? sender, EventArgs e)
    {
        if (_patterns == null) return;
        var p = new Pattern { Name = "NEW_PATTERN" };
        _patterns.Add(p);
        lstPatterns.SelectedItem = p;
    }

    private void btnDeletePattern_Click(object? sender, EventArgs e)
    {
        if (_patterns == null || _selectedPattern == null) return;

        if (!Confirm.Ask(this, "Confirm delete",
                $"Delete pattern \"{_selectedPattern.Name}\"?"))
            return;

        _patterns.Remove(_selectedPattern);
        SaveQuiet();
    }

    private void btnSaveRule_Click(object? sender, EventArgs e)
    {
        if (_selectedPattern != null)
        {
            // Write text fields back to the object (they are not two-way bound).
            _selectedPattern.Name = txtPatternName.Text;
            _selectedPattern.Description = txtDescription.Text;
            _selectedPattern.TestBarcode = txtLotTest.Text;
            _selectedPattern.TestBlockText = txtBlockText.Text;
            SyncRulesToPattern();
            _selectedPattern.TestPreview = lblPreview.Text ?? "";
        }

        try
        {
            PatternStore.Save(_patternsPath);
            RefreshPatternListDisplay();
            Notify.Success(this, "Patterns saved.");
        }
        catch (Exception ex)
        {
            Notify.ErrorModal(this, "Error", "Save failed: " + ex.Message);
        }
    }

    /// <summary>Refresh the list text (e.g. after a rename) without losing the selection.</summary>
    private void RefreshPatternListDisplay()
    {
        var keep = _selectedPattern;
        _patterns?.ResetBindings();
        if (keep != null) lstPatterns.SelectedItem = keep;
    }

    private void SaveQuiet()
    {
        try { PatternStore.Save(_patternsPath); } catch { }
    }

    // ── Preview ────────────────────────────────────────────

    private void UpdatePreview()
    {
        if (_loading || _selectedPattern == null) return;

        string result = _selectedPattern.Apply(txtLotTest.Text);
        string block = txtBlockText.Text ?? "";

        lblPreview.Text = (!string.IsNullOrEmpty(_selectedPattern.Name) && block.Contains(_selectedPattern.Name))
            ? block.Replace(_selectedPattern.Name, result)
            : result;
    }

    // ── Helpers ────────────────────────────────────────────

    private static AntdUI.CellButton[] NewDeleteButtons() =>
        new[] { new AntdUI.CellButton("del", "Delete", AntdUI.TTypeMini.Error) { Radius = 6 } };

    private static RuleRow FromRule(Rule r) => new()
    {
        From = r.SourceStart.ToString(),
        To = r.SourceEnd.ToString(),
        RuleLabel = _labelByType.TryGetValue(r.TransformRule, out var l) ? l : r.TransformRule.ToString(),
        Value = r.Parameter ?? "",
        Op = NewDeleteButtons(),
    };

    private static Rule ToRule(RuleRow row)
    {
        int.TryParse(row.From, out int start);
        int.TryParse(row.To, out int end);
        _typeByLabel.TryGetValue(row.RuleLabel ?? "", out var type);
        return new Rule
        {
            SourceStart = start,
            SourceEnd = end,
            TransformRule = type,
            Parameter = row.Value ?? "",
            IsActive = true,
        };
    }

    private static List<AntdUI.SelectItem> RuleTypeItems() =>
        _labelByType.Values.Select(l => new AntdUI.SelectItem(l, l)).ToList();

    private static string DescriptionOf(TransformRuleType t)
    {
        MemberInfo mi = typeof(TransformRuleType).GetMember(t.ToString())[0];
        return mi.GetCustomAttribute<DescriptionAttribute>()?.Description ?? t.ToString();
    }
}

/// <summary>Row view-model bound to the AntdUI.Table (string cells + a delete button).</summary>
internal class RuleRow : AntdUI.NotifyProperty
{
    private string _from = "";
    private string _to = "";
    private string _ruleLabel = "";
    private string _value = "";
    private AntdUI.CellButton[] _op = Array.Empty<AntdUI.CellButton>();

    public string From { get => _from; set { _from = value; OnPropertyChanged(); } }
    public string To { get => _to; set { _to = value; OnPropertyChanged(); } }
    public string RuleLabel { get => _ruleLabel; set { _ruleLabel = value; OnPropertyChanged(); } }
    public string Value { get => _value; set { _value = value; OnPropertyChanged(); } }
    public AntdUI.CellButton[] Op { get => _op; set { _op = value; OnPropertyChanged(); } }
}
