using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class FileScanner
{
    public IReadOnlyList<FileItem> LoadFiles(IEnumerable<string> paths)
    {
        return paths
            .Where(File.Exists)
            .Select(FileItem.FromPath)
            .ToList();
    }

    public IReadOnlyList<FileItem> LoadFolders(IEnumerable<string> folderPaths)
    {
        return folderPaths
            .Where(Directory.Exists)
            .Select(FileItem.FromDirectoryPath)
            .ToList();
    }

    public IReadOnlyList<FileItem> LoadFolderFiles(string folderPath, bool recursive)
    {
        if (!Directory.Exists(folderPath))
        {
            return Array.Empty<FileItem>();
        }

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var result = new List<FileItem>();

        try
        {
            foreach (var path in Directory.EnumerateFiles(folderPath, "*", option))
            {
                try
                {
                    result.Add(FileItem.FromPath(path));
                }
                catch
                {
                    // 单个文件读取失败时跳过，避免整个文件夹导入失败。
                }
            }
        }
        catch
        {
            return result;
        }

        return result;
    }
}
