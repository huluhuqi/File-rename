using System.Collections.Generic;
using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public sealed class RuleChain
{
    public List<IRenameRule> Rules { get; } = new();

    public string Apply(string fileName, RenameContext context)
    {
        var current = fileName;
        foreach (var rule in Rules)
        {
            current = rule.Apply(current, context);
        }
        return current;
    }
}
