using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class FileClassificationService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tif", ".tiff", ".raw", ".heic"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf", ".txt", ".md", ".csv"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".aac", ".flac", ".m4a", ".ogg"
    };

    public IReadOnlyList<ClassificationPlan> BuildPlans(IEnumerable<FileItem> files)
    {
        return files.Select(file =>
        {
            var category = GetCategory(file.Extension);
            var targetDirectory = Path.Combine(file.DirectoryPath, category);
            return new ClassificationPlan
            {
                File = file,
                CategoryName = category,
                TargetDirectory = targetDirectory,
                TargetPath = Path.Combine(targetDirectory, file.FileName)
            };
        }).ToList();
    }

    private static string GetCategory(string extension)
    {
        if (ImageExtensions.Contains(extension)) return "图片";
        if (VideoExtensions.Contains(extension)) return "视频";
        if (DocumentExtensions.Contains(extension)) return "文档";
        if (ArchiveExtensions.Contains(extension)) return "压缩包";
        if (AudioExtensions.Contains(extension)) return "音频";
        return "其它";
    }
}
