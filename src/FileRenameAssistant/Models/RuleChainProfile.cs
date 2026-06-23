using System.Collections.Generic;

namespace FileRenameAssistant.Models;

public sealed class RuleChainProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<RuleStep> Steps { get; set; } = new();
}
