using FileRenameAssistant.Models;

namespace FileRenameAssistant.ViewModels;

public sealed class SortConditionViewModel : ObservableObject
{
    private SortField _field;
    private bool _descending;

    public SortConditionViewModel(SortField field, bool descending = false)
    {
        _field = field;
        _descending = descending;
    }

    public SortField Field
    {
        get => _field;
        set
        {
            if (SetProperty(ref _field, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public bool Descending
    {
        get => _descending;
        set
        {
            if (SetProperty(ref _descending, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string DisplayName => $"{GetFieldName(Field)}（{(Descending ? "降序" : "升序")}）";

    private static string GetFieldName(SortField field) => field switch
    {
        SortField.Extension => "文件类型/扩展名",
        SortField.CreatedAt => "创建日期",
        SortField.ModifiedAt => "修改日期",
        SortField.SizeBytes => "文件大小",
        _ => "自然文件名"
    };
}
