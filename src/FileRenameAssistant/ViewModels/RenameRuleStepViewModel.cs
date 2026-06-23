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
            }
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public string DisplayName => Kind switch
    {
        RenameRuleKind.Prefix => "添加前缀",
        RenameRuleKind.Suffix => "添加后缀",
        RenameRuleKind.RemoveText => "删除文字",
        RenameRuleKind.Numbering => "自动编号",
        RenameRuleKind.UpperAll => "文件名全大写",
        RenameRuleKind.LowerAll => "文件名全小写",
        RenameRuleKind.FirstLetterUpper => "文件名首字母大写",
        RenameRuleKind.EachWordUpper => "文件名单词首字母大写",
        _ => "查找替换"
    };
}
