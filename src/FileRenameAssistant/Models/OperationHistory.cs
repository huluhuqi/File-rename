namespace FileRenameAssistant.Models;

public sealed class OperationHistory
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string OperationType { get; init; } = "批量重命名";
    public string Summary { get; set; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public string Status { get; set; } = "Success";
    public List<OperationItem> Items { get; init; } = new();
}

public sealed class OperationItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string OriginalPath { get; init; }
    public required string NewPath { get; init; }
    public string OriginalName => Path.GetFileName(OriginalPath);
    public string NewName => Path.GetFileName(NewPath);
    public string Status { get; init; } = "Success";
    public string ErrorMessage { get; init; } = "";
}
