using System;
using System.Globalization;
using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public sealed class TemplateRule : IRenameRule
{
    public string Name => "命名模板";
    public string Template { get; set; } = "{name}{ext}";

    public string Apply(string fileName, RenameContext context)
    {
        if (string.IsNullOrWhiteSpace(Template))
        {
            return fileName;
        }

        var ext = System.IO.Path.GetExtension(fileName);
        var stem = System.IO.Path.GetFileNameWithoutExtension(fileName);

        var now = DateTime.Now;
        var modified = context.File.ModifiedAt;

        var result = Template
            .Replace("{name}", stem, StringComparison.OrdinalIgnoreCase)
            .Replace("{ext}", ext, StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{time}", now.ToString("HHmmss", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{mdate}", modified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{index}", (context.Index + 1).ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

        if (!result.Contains('.'))
        {
            result += ext;
        }

        return result;
    }
}
