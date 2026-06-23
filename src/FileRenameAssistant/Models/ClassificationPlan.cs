namespace FileRenameAssistant.Models;

public sealed class ClassificationPlan
{
    public required FileItem File { get; init; }
    public required string CategoryName { get; init; }
    public required string TargetDirectory { get; init; }
    public required string TargetPath { get; init; }
}
