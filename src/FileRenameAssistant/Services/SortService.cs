using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class SortService
{
    private static readonly NaturalStringComparer NaturalComparer = new();

    public IReadOnlyList<FileItem> Sort(IEnumerable<FileItem> files, SortField field, bool descending)
    {
        IOrderedEnumerable<FileItem> ordered = field switch
        {
            SortField.Extension => files.OrderBy(f => f.Extension, StringComparer.CurrentCultureIgnoreCase),
            SortField.SizeBytes => files.OrderBy(f => f.SizeBytes),
            SortField.CreatedAt => files.OrderBy(f => f.CreatedAt),
            SortField.ModifiedAt => files.OrderBy(f => f.ModifiedAt),
            _ => files.OrderBy(f => f.NameWithoutExtension, NaturalComparer)
        };

        return (descending ? ordered.Reverse() : ordered).ToList();
    }

    public IReadOnlyList<FileItem> Sort(IEnumerable<FileItem> files, IReadOnlyList<MultiSortRule> rules)
    {
        var orderedRules = rules.OrderBy(r => r.Priority).ToList();
        if (orderedRules.Count == 0)
        {
            return Sort(files, SortField.FileName, false);
        }

        IOrderedEnumerable<FileItem>? ordered = null;
        foreach (var rule in orderedRules)
        {
            ordered = ApplySort(ordered ?? files, rule, ordered is not null);
        }

        return ordered?.ToList() ?? files.ToList();
    }

    private static IOrderedEnumerable<FileItem> ApplySort(IEnumerable<FileItem> files, MultiSortRule rule, bool thenBy)
    {
        if (thenBy)
        {
            return ApplyThenBy((IOrderedEnumerable<FileItem>)files, rule);
        }

        return (rule.Field, rule.Descending) switch
        {
            (SortField.Extension, false) => files.OrderBy(f => f.Extension, StringComparer.CurrentCultureIgnoreCase),
            (SortField.Extension, true) => files.OrderByDescending(f => f.Extension, StringComparer.CurrentCultureIgnoreCase),
            (SortField.SizeBytes, false) => files.OrderBy(f => f.SizeBytes),
            (SortField.SizeBytes, true) => files.OrderByDescending(f => f.SizeBytes),
            (SortField.CreatedAt, false) => files.OrderBy(f => f.CreatedAt),
            (SortField.CreatedAt, true) => files.OrderByDescending(f => f.CreatedAt),
            (SortField.ModifiedAt, false) => files.OrderBy(f => f.ModifiedAt),
            (SortField.ModifiedAt, true) => files.OrderByDescending(f => f.ModifiedAt),
            (SortField.FileName, true) => files.OrderByDescending(f => f.NameWithoutExtension, NaturalComparer),
            _ => files.OrderBy(f => f.NameWithoutExtension, NaturalComparer)
        };
    }

    private static IOrderedEnumerable<FileItem> ApplyThenBy(IOrderedEnumerable<FileItem> files, MultiSortRule rule)
    {
        return (rule.Field, rule.Descending) switch
        {
            (SortField.Extension, false) => files.ThenBy(f => f.Extension, StringComparer.CurrentCultureIgnoreCase),
            (SortField.Extension, true) => files.ThenByDescending(f => f.Extension, StringComparer.CurrentCultureIgnoreCase),
            (SortField.SizeBytes, false) => files.ThenBy(f => f.SizeBytes),
            (SortField.SizeBytes, true) => files.ThenByDescending(f => f.SizeBytes),
            (SortField.CreatedAt, false) => files.ThenBy(f => f.CreatedAt),
            (SortField.CreatedAt, true) => files.ThenByDescending(f => f.CreatedAt),
            (SortField.ModifiedAt, false) => files.ThenBy(f => f.ModifiedAt),
            (SortField.ModifiedAt, true) => files.ThenByDescending(f => f.ModifiedAt),
            (SortField.FileName, true) => files.ThenByDescending(f => f.NameWithoutExtension, NaturalComparer),
            _ => files.ThenBy(f => f.NameWithoutExtension, NaturalComparer)
        };
    }
}
