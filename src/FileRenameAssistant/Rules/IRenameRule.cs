using FileRenameAssistant.Models;

namespace FileRenameAssistant.Rules;

public interface IRenameRule
{
    string Name { get; }
    string Apply(string fileNameWithoutExtension, RenameContext context);
}

public sealed class RenameContext
{
    public required int Index { get; init; }
    public required FileItem File { get; init; }
}
