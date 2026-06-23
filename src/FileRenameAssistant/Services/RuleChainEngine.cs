using System.Collections.Generic;
using FileRenameAssistant.Models;
using FileRenameAssistant.Rules;

namespace FileRenameAssistant.Services;

/// <summary>
/// 规则链执行器：按顺序应用一组规则，并在每一步保留中间结果用于调试。
/// </summary>
public sealed class RuleChainEngine
{
    public IReadOnlyList<string> ApplyAndTrace(IReadOnlyList<IRenameRule> rules, string fileName, RenameContext context)
    {
        var trace = new List<string>(rules.Count + 1) { fileName };
        var current = fileName;
        foreach (var rule in rules)
        {
            current = rule.Apply(current, context);
            trace.Add(current);
        }
        return trace;
    }
}
