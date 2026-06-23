namespace FileRenameAssistant.Models;

public sealed class FileItem
{
    public required string FullPath { get; init; }
    public required string DirectoryPath { get; init; }
    public required string FileName { get; init; }
    public required string NameWithoutExtension { get; init; }
    public required string Extension { get; init; }
    public long SizeBytes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public bool IsDirectory { get; init; }

    public static FileItem FromPath(string path)
    {
        var info = new FileInfo(path);
        return new FileItem
        {
            FullPath = info.FullName,
            DirectoryPath = info.DirectoryName ?? string.Empty,
            FileName = info.Name,
            NameWithoutExtension = Path.GetFileNameWithoutExtension(info.Name),
            Extension = info.Extension,
            SizeBytes = info.Exists ? info.Length : 0,
            CreatedAt = info.Exists ? info.CreationTime : DateTime.MinValue,
            ModifiedAt = info.Exists ? info.LastWriteTime : DateTime.MinValue,
            IsDirectory = false
        };
    }

    public static FileItem FromDirectoryPath(string path)
    {
        var info = new DirectoryInfo(path);
        return new FileItem
        {
            FullPath = info.FullName,
            DirectoryPath = info.Parent?.FullName ?? string.Empty,
            FileName = info.Name,
            NameWithoutExtension = info.Name,
            Extension = string.Empty,
            SizeBytes = 0,
            CreatedAt = info.Exists ? info.CreationTime : DateTime.MinValue,
            ModifiedAt = info.Exists ? info.LastWriteTime : DateTime.MinValue,
            IsDirectory = true
        };
    }
}
