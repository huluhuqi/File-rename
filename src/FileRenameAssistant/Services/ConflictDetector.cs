using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class ConflictDetector
{
    private static readonly char[] InvalidNameChars = Path.GetInvalidFileNameChars();

    public void Validate(IReadOnlyList<PreviewItem> items)
    {
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Source.IsDirectory ? item.NewName : Path.GetFileNameWithoutExtension(item.NewName)))
            {
                MarkError(item, "新文件名不能为空");
                continue;
            }

            if (item.NewName.IndexOfAny(InvalidNameChars) >= 0)
            {
                MarkError(item, "新文件名包含 Windows 不允许的字符");
                continue;
            }

            if (!File.Exists(item.OriginalPath) && !Directory.Exists(item.OriginalPath))
            {
                MarkError(item, "原文件不存在，可能已被移动或删除");
                continue;
            }
        }

        var duplicateGroups = items
            .Where(i => i.Status != PreviewStatus.Error)
            .GroupBy(i => Path.GetFullPath(i.NewPath), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            foreach (var item in group)
            {
                MarkError(item, "多个文件生成了相同目标路径");
            }
        }

        var sourcePaths = items
            .Select(i => Path.GetFullPath(i.OriginalPath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items.Where(i => i.Status != PreviewStatus.Error))
        {
            var samePath = string.Equals(
                Path.GetFullPath(item.OriginalPath),
                Path.GetFullPath(item.NewPath),
                StringComparison.OrdinalIgnoreCase);

            var targetIsAnotherSourceInThisBatch = sourcePaths.Contains(Path.GetFullPath(item.NewPath));

            if (!samePath && (File.Exists(item.NewPath) || Directory.Exists(item.NewPath)) && !targetIsAnotherSourceInThisBatch)
            {
                MarkError(item, "目标路径已存在");
            }
        }
    }

    private static void MarkError(PreviewItem item, string message)
    {
        item.Status = PreviewStatus.Error;
        item.Message = message;
    }
}
