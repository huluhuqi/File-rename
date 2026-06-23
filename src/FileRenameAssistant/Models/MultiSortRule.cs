namespace FileRenameAssistant.Models;

public sealed class MultiSortRule
{
    public SortField Field { get; set; }
    public bool Descending { get; set; }
    public int Priority { get; set; }
}
