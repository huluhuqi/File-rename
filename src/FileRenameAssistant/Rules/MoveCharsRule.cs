using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public sealed class MoveCharsRule : IRenameRule
{
    public string Name => "移动字符";
    public MoveCharsMode Mode { get; set; } = MoveCharsMode.MoveStartToEnd;
    public int Length { get; set; } = 3;

    public string Apply(string fileName, RenameContext context)
    {
        var ext = System.IO.Path.GetExtension(fileName);
        var stem = System.IO.Path.GetFileNameWithoutExtension(fileName);

        if (Length <= 0 || Length >= stem.Length)
        {
            return fileName;
        }

        var result = Mode switch
        {
            MoveCharsMode.MoveStartToEnd => stem[Length..] + stem[..Length],
            MoveCharsMode.MoveEndToStart => stem[^Length..] + stem[..^Length],
            _ => stem
        };

        return result + ext;
    }
}
