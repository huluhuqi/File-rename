using System.Text;
using FileRenameAssistant.Models;
using FileRenameAssistant.Services;

namespace FileRenameAssistant.Rules;

public sealed class ChineseConvertRule : IRenameRule
{
    public string Name => "简繁体转换";
    public ChineseConvertMode Mode { get; set; } = ChineseConvertMode.None;

    public string Apply(string fileName, RenameContext context)
    {
        if (Mode == ChineseConvertMode.None) return fileName;

        var ext = System.IO.Path.GetExtension(fileName);
        var stem = System.IO.Path.GetFileNameWithoutExtension(fileName);

        var converted = Mode == ChineseConvertMode.SimplifiedToTraditional
            ? ChineseConverter.S2T(stem)
            : ChineseConverter.T2S(stem);

        return converted + ext;
    }
}
