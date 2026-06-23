using System.Text.RegularExpressions;

namespace FileRenameAssistant.Rules;

public sealed class RegexReplaceRule : IRenameRule
{
    private readonly string _pattern;
    private readonly string _replacement;

    public RegexReplaceRule(string pattern, string replacement)
    {
        _pattern = pattern;
        _replacement = replacement;
    }

    public string Name => "正则查找替换";

    public string Apply(string fileNameWithoutExtension, RenameContext context)
    {
        if (string.IsNullOrWhiteSpace(_pattern))
        {
            return fileNameWithoutExtension;
        }

        try
        {
            return Regex.Replace(
                fileNameWithoutExtension,
                _pattern,
                _replacement,
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(2));
        }
        catch (RegexMatchTimeoutException)
        {
            return fileNameWithoutExtension;
        }
        catch (ArgumentException)
        {
            return fileNameWithoutExtension;
        }
    }
}
