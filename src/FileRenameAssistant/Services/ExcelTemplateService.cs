using ClosedXML.Excel;
using FileRenameAssistant.Models;

namespace FileRenameAssistant.Services;

public sealed class ExcelTemplateService
{
    public void Export(string path, IReadOnlyList<FileItem> sortedFiles)
    {
        using var workbook = new XLWorkbook();

        var nameSheet = workbook.Worksheets.Add("文件名");
        nameSheet.Cell(1, 1).Value = "原文件名";
        nameSheet.Cell(1, 2).Value = "修改后文件名";

        var extensionSheet = workbook.Worksheets.Add("扩展名");
        extensionSheet.Cell(1, 1).Value = "原扩展名";
        extensionSheet.Cell(1, 2).Value = "修改后扩展名";

        for (var i = 0; i < sortedFiles.Count; i++)
        {
            var row = i + 2;
            var file = sortedFiles[i];

            nameSheet.Cell(row, 1).Value = file.FileName;
            nameSheet.Cell(row, 2).Value = file.FileName;

            extensionSheet.Cell(row, 1).Value = file.Extension.TrimStart('.');
            extensionSheet.Cell(row, 2).Value = file.Extension.TrimStart('.');
        }

        nameSheet.Columns().AdjustToContents();
        extensionSheet.Columns().AdjustToContents();
        workbook.SaveAs(path);
    }

    public ExcelRenameMap Import(string path)
    {
        using var workbook = new XLWorkbook(path);
        var result = new ExcelRenameMap();

        if (workbook.TryGetWorksheet("文件名", out var nameSheet))
        {
            foreach (var row in nameSheet.RowsUsed().Skip(1))
            {
                var original = row.Cell(1).GetString().Trim();
                var target = row.Cell(2).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(original) && !string.IsNullOrWhiteSpace(target))
                {
                    result.FileNameMap[original] = target;
                }
            }
        }

        if (workbook.TryGetWorksheet("扩展名", out var extensionSheet))
        {
            foreach (var row in extensionSheet.RowsUsed().Skip(1))
            {
                var original = NormalizeExtension(row.Cell(1).GetString());
                var target = NormalizeExtension(row.Cell(2).GetString());
                if (!string.IsNullOrWhiteSpace(original) && !string.IsNullOrWhiteSpace(target))
                {
                    result.ExtensionMap[original] = target;
                }
            }
        }

        return result;
    }

    private static string NormalizeExtension(string value)
    {
        var text = value.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        return text.StartsWith('.') ? text : "." + text;
    }
}

public sealed class ExcelRenameMap
{
    public Dictionary<string, string> FileNameMap { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> ExtensionMap { get; } = new(StringComparer.OrdinalIgnoreCase);
}
