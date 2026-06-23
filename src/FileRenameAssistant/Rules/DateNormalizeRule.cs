using System.Text.RegularExpressions;

namespace FileRenameAssistant.Rules;

public sealed class DateNormalizeRule : IRenameRule
{
    public string Name => "日期规范化";

    private static readonly Regex DateRegex = new(
        @"(?<year>20\d{2})[.\-_/]?(?<month>\d{1,2})[.\-_/]?(?<day>\d{1,2})",
        RegexOptions.Compiled);

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        return DateRegex.Replace(fileNameWithoutExtension, m =>
        {
            var year = m.Groups["year"].Value;
            var month = int.Parse(m.Groups["month"].Value);
            var day = int.Parse(m.Groups["day"].Value);
            return $"{year}-{month:00}-{day:00}";
        });
    }
}
