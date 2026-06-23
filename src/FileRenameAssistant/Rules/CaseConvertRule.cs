using System.Globalization;
using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public sealed class CaseConvertRule : IRenameRule
{
    public string Name => "大小写转换";
    public CaseConvertMode Mode { get; set; } = CaseConvertMode.None;

    public string Apply(string fileName, RenameContext context)
    {
        if (Mode == CaseConvertMode.None || string.IsNullOrEmpty(fileName))
        {
            return fileName;
        }

        var ext = System.IO.Path.GetExtension(fileName);
        var stem = System.IO.Path.GetFileNameWithoutExtension(fileName);

        var converted = Mode switch
        {
            CaseConvertMode.UpperAll => stem.ToUpperInvariant(),
            CaseConvertMode.LowerAll => stem.ToLowerInvariant(),
            CaseConvertMode.FirstLetterUpper => FirstUpper(stem),
            CaseConvertMode.EachWordUpper => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(stem.ToLowerInvariant()),
            _ => stem
        };

        return converted + ext;
    }

    private static string FirstUpper(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return char.ToUpperInvariant(text[0]) + text[1..].ToLowerInvariant();
    }
}
