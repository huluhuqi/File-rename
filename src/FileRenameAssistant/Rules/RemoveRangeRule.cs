using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public sealed class RemoveRangeRule : IRenameRule
{
    public string Name => "按位置删除字符";
    public int StartIndex { get; set; }
    public int Length { get; set; }

    public string Apply(string fileName, RenameContext context)
    {
        var ext = System.IO.Path.GetExtension(fileName);
        var stem = System.IO.Path.GetFileNameWithoutExtension(fileName);

        if (StartIndex < 0 || StartIndex >= stem.Length || Length <= 0)
        {
            return fileName;
        }

        var actualLength = System.Math.Min(Length, stem.Length - StartIndex);
        var newStem = stem.Remove(StartIndex, actualLength);
        return newStem + ext;
    }
}
