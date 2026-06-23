using FileRenameAssistant.Models;
using FileRenameAssistant.Rules;

namespace FileRenameAssistant.Services;

public sealed class PreviewEngine
{
    private readonly SortService _sortService = new();
    private readonly ConflictDetector _conflictDetector = new();

    public IReadOnlyList<PreviewItem> BuildPreview(
        IReadOnlyList<FileItem> files,
        IReadOnlyList<IRenameRule> rules,
        IReadOnlyList<MultiSortRule> sortRules,
        RenameTargetMode targetMode,
        ExtensionCaseMode extensionCaseMode,
        IReadOnlyDictionary<string, string>? fileNameOverrides = null,
        IReadOnlyDictionary<string, string>? extensionOverrides = null)
    {
        var sortedFiles = _sortService.Sort(files, sortRules);
        var previewItems = new List<PreviewItem>();

        for (var i = 0; i < sortedFiles.Count; i++)
        {
            var file = sortedFiles[i];
            var newNameWithoutExtension = file.NameWithoutExtension;
            var newExtension = ApplyExtensionMode(file.Extension, extensionCaseMode);
            var context = new RenameContext
            {
                Index = i,
                File = file
            };

            try
            {
                if (targetMode == RenameTargetMode.FileName)
                {
                    foreach (var rule in rules)
                    {
                        newNameWithoutExtension = rule.Apply(newNameWithoutExtension, context);
                    }
                }
                else
                {
                    var extensionBody = newExtension.TrimStart('.');
                    foreach (var rule in rules)
                    {
                        extensionBody = rule.Apply(extensionBody, context).TrimStart('.');
                    }
                    newExtension = string.IsNullOrWhiteSpace(extensionBody) ? "" : "." + extensionBody;
                }

                var newName = newNameWithoutExtension + newExtension;
                if (fileNameOverrides is not null && fileNameOverrides.TryGetValue(file.FileName, out var overrideName))
                {
                    newName = overrideName;
                }
                else if (extensionOverrides is not null && extensionOverrides.TryGetValue(file.Extension, out var overrideExtension))
                {
                    newName = newNameWithoutExtension + overrideExtension;
                }
                var newPath = Path.Combine(file.DirectoryPath, newName);

                previewItems.Add(new PreviewItem
                {
                    Source = file,
                    OriginalPath = file.FullPath,
                    NewPath = newPath,
                    OriginalName = file.FileName,
                    NewName = newName,
                    Status = PathEquals(file.FullPath, newPath) ? PreviewStatus.Unchanged : PreviewStatus.Ready,
                    Message = PathEquals(file.FullPath, newPath) ? "文件名未变化" : ""
                });
            }
            catch (Exception ex)
            {
                previewItems.Add(new PreviewItem
                {
                    Source = file,
                    OriginalPath = file.FullPath,
                    NewPath = file.FullPath,
                    OriginalName = file.FileName,
                    NewName = file.FileName,
                    Status = PreviewStatus.Error,
                    Message = $"规则执行失败：{ex.Message}"
                });
            }
        }

        _conflictDetector.Validate(previewItems);
        return previewItems;
    }

    private static string ApplyExtensionMode(string extension, ExtensionCaseMode mode)
    {
        return mode switch
        {
            ExtensionCaseMode.Lower => extension.ToLowerInvariant(),
            ExtensionCaseMode.Upper => extension.ToUpperInvariant(),
            _ => extension
        };
    }

    private static bool PathEquals(string left, string right)
    {
        return string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            StringComparison.OrdinalIgnoreCase);
    }
}
