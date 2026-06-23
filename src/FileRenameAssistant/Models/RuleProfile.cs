namespace FileRenameAssistant.Models;

public sealed class RuleProfile
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required RuleProfileConfig Config { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class RuleProfileConfig
{
    public bool EnableReplace { get; set; }
    public bool UseRegex { get; set; }
    public string FindText { get; set; } = "";
    public string ReplaceText { get; set; } = "";
    public bool EnablePrefix { get; set; }
    public string PrefixText { get; set; } = "";
    public bool EnableSuffix { get; set; }
    public string SuffixText { get; set; } = "";
    public bool EnableRemoveText { get; set; }
    public string RemoveText { get; set; } = "";
    public bool EnableNumbering { get; set; }
    public int NumberStart { get; set; } = 1;
    public int NumberStep { get; set; } = 1;
    public int NumberDigits { get; set; } = 3;
    public int SelectedSortIndex { get; set; }
    public bool SortDescending { get; set; }
    public int ExtensionModeIndex { get; set; }
    public int RenameTargetIndex { get; set; }
    public List<int> RenameStepKinds { get; set; } = new();
}
