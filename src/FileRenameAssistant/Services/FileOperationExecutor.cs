using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class FileOperationExecutor
{
    public Task<OperationHistory> ExecuteRenameAsync(IReadOnlyList<PreviewItem> previewItems)
    {
        var executableItems = previewItems
            .Where(i => i.Status == PreviewStatus.Ready)
            .ToList();

        if (previewItems.Any(i => i.Status == PreviewStatus.Error))
        {
            throw new InvalidOperationException("存在错误项，请修正后再执行。");
        }

        var history = new OperationHistory();
        var tempMoves = new List<(string OriginalPath, string TempPath, string FinalPath)>();

        foreach (var item in executableItems)
        {
            var tempPath = BuildTempPath(item.OriginalPath);
            MovePath(item.OriginalPath, tempPath);
            tempMoves.Add((item.OriginalPath, tempPath, item.NewPath));
        }

        foreach (var move in tempMoves)
        {
            MovePath(move.TempPath, move.FinalPath);
            history.Items.Add(new OperationItem
            {
                OriginalPath = move.OriginalPath,
                NewPath = move.FinalPath
            });
        }

        return Task.FromResult(history);
    }

    public Task UndoAsync(OperationHistory history)
    {
        foreach (var item in history.Items.AsEnumerable().Reverse())
        {
            if (!File.Exists(item.NewPath) && !Directory.Exists(item.NewPath))
            {
                throw new FileNotFoundException("撤销失败，新路径文件不存在。", item.NewPath);
            }

            if (File.Exists(item.OriginalPath) || Directory.Exists(item.OriginalPath))
            {
                throw new IOException($"撤销失败，原路径已被占用：{item.OriginalPath}");
            }

            MovePath(item.NewPath, item.OriginalPath);
        }

        return Task.CompletedTask;
    }

    private static string BuildTempPath(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath) ?? "";
        var fileName = Path.GetFileName(originalPath);
        var tempName = $".file-organizer-{Guid.NewGuid():N}-{fileName}.tmp";
        return Path.Combine(directory, tempName);
    }

    private static void MovePath(string source, string target)
    {
        if (Directory.Exists(source))
        {
            Directory.Move(source, target);
            return;
        }

        File.Move(source, target);
    }
}
