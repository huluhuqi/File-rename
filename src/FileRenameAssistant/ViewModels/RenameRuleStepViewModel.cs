namespace FileRenameAssistant.ViewModels;

public enum RenameRuleKind
{
    Replace,
    Prefix,
    Suffix,
    RemoveText,
    Numbering,
    UpperAll,
    LowerAll,
    FirstLetterUpper,
    EachWordUpper
}

public sealed class RenameRuleStepViewModel : ObservableObject
{
    private RenameRuleKind _kind;
    private bool _enabled = true;

    // Replace 参数
    private string _findText = "";
    private string _replaceText = "";
    private bool _useRegex;

    // Prefix 参数
    private string _prefixText = "";

    // Suffix 参数
    private string _suffixText = "";

    // RemoveText 参数
    private string _removeText = "";

    // Numbering 参数
    private int _numberStart = 1;
    private int _numberStep = 1;
    private int _numberDigits = 3;

    public RenameRuleStepViewModel(RenameRuleKind kind)
    {
        _kind = kind;
    }

    public RenameRuleKind Kind
    {
        get => _kind;
        set
        {
            if (SetProperty(ref _kind, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(HasParams));
            }
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    // Replace 参数
    public string FindText { get => _findText; set => SetProperty(ref _findText, value); }
    public string ReplaceText { get => _replaceText; set => SetProperty(ref _replaceText, value); }
    public bool UseRegex { get => _useRegex; set => SetProperty(ref _useRegex, value); }

    // Prefix 参数
    public string PrefixText { get => _prefixText; set => SetProperty(ref _prefixText, value); }

    // Suffix 参数
    public string SuffixText { get => _suffixText; set => SetProperty(ref _suffixText, value); }

    // RemoveText 参数
    public string RemoveText { get => _removeText; set => SetProperty(ref _removeText, value); }

    // Numbering 参数
    public int NumberStart { get => _numberStart; set => SetProperty(ref _numberStart, value); }
    public int NumberStep { get => _numberStep; set => SetProperty(ref _numberStep, value); }
    public int NumberDigits { get => _numberDigits; set => SetProperty(ref _numberDigits, value); }

    public bool HasParams => Kind is RenameRuleKind.Replace or RenameRuleKind.Prefix or RenameRuleKind.Suffix or RenameRuleKind.RemoveText or RenameRuleKind.Numbering;

    public string DisplayName => Kind switch
    {
        RenameRuleKind.Prefix => string.IsNullOrEmpty(PrefixText) ? "添加前缀" : $"添加前缀「{PrefixText}」",
        RenameRuleKind.Suffix => string.IsNullOrEmpty(SuffixText) ? "添加后缀" : $"添加后缀「{SuffixText}」",
        RenameRuleKind.RemoveText => string.IsNullOrEmpty(RemoveText) ? "删除文字" : $"删除「{RemoveText}」",
        RenameRuleKind.Numbering => $"自动编号（从{NumberStart}，步长{NumberStep}）",
        RenameRuleKind.UpperAll => "文件名全大写",
        RenameRuleKind.LowerAll => "文件名全小写",
        RenameRuleKind.FirstLetterUpper => "文件名首字母大写",
        RenameRuleKind.EachWordUpper => "文件名单词首字母大写",
        _ => string.IsNullOrEmpty(FindText) ? "查找替换" : $"查找「{FindText}」→「{ReplaceText}」"
    };
}
