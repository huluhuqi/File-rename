namespace FileRenameAssistant.Services;

public sealed class MetadataReaderService
{
    public FileMetadataSnapshot ReadBasic(string path)
    {
        var info = new FileInfo(path);
        return new FileMetadataSnapshot
        {
            FullPath = info.FullName,
            CreatedAt = info.Exists ? info.CreationTime : DateTime.MinValue,
            ModifiedAt = info.Exists ? info.LastWriteTime : DateTime.MinValue,
            SizeBytes = info.Exists ? info.Length : 0
        };
    }
}

public sealed class FileMetadataSnapshot
{
    public string FullPath { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public long SizeBytes { get; init; }
}
