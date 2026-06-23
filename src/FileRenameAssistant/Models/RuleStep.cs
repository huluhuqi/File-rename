namespace FileRenameAssistant.Models;

public sealed class RuleStep
{
    public int Order { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
