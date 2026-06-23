using FileRenameAssistant.ViewModels;

namespace FileRenameAssistant.Models;

public sealed class PreviewItem : ObservableObject
{
    private string _newPath = "";
    private string _newName = "";
    private PreviewStatus _status;
    private string _message = "";

    public required FileItem Source { get; init; }
    public required string OriginalPath { get; init; }
    public required string NewPath
    {
        get => _newPath;
        set => SetProperty(ref _newPath, value);
    }
    public required string OriginalName { get; init; }
    public required string NewName
    {
        get => _newName;
        set
        {
            if (SetProperty(ref _newName, value))
            {
                NewPath = Path.Combine(Path.GetDirectoryName(OriginalPath) ?? "", value);
                Status = string.Equals(OriginalName, value, StringComparison.OrdinalIgnoreCase)
                    ? PreviewStatus.Unchanged
                    : PreviewStatus.Ready;
                Message = Status == PreviewStatus.Ready ? "手动编辑" : "名称未变化";
            }
        }
    }

    public PreviewStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string StatusText => Status switch
    {
        PreviewStatus.Ready => "可执行",
        PreviewStatus.Unchanged => "不变",
        PreviewStatus.Warning => "提醒",
        PreviewStatus.Error => "错误",
        _ => "未知"
    };
}
