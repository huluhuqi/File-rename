using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileRenameAssistant.Data;
using FileRenameAssistant.Models;
using FileRenameAssistant.Rules;
using FileRenameAssistant.Services;

namespace FileRenameAssistant.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly FileScanner _fileScanner = new();
    private readonly PreviewEngine _previewEngine = new();
    private readonly FileOperationExecutor _executor = new();
    private readonly ExcelTemplateService _excelTemplateService = new();
    private readonly SortService _sortService = new();
    private readonly AppDatabase _database = new();
    private readonly DebouncedPreviewScheduler _refreshScheduler = new();
    private readonly List<FileItem> _files = new();
    private readonly RuleProfileRepository _ruleRepository;
    private readonly HistoryRepository _historyRepository;
    private OperationHistory? _lastHistory;
    private bool _isApplyingProfile;

    private int _selectedSortIndex;
    private bool _sortDescending;
    private bool _recursiveFolder;
    private int _extensionModeIndex;
    private string _ruleName = "";
    private string _message = "请选择文件或文件夹开始。";
    private string _refreshStateText = "预览会在输入规则后自动刷新。";
    private PreviewItem? _selectedPreviewItem;
    private SortConditionViewModel? _selectedSortCondition;
    private RenameRuleStepViewModel? _selectedRenameStep;
    private int _newSortFieldIndex;
    private bool _newSortDescending;
    private int _newRenameRuleKindIndex;
    private int _renameTargetIndex;
    private RuleProfile? _selectedProfile;
    private ExcelRenameMap? _excelRenameMap;
    private bool _isBusy;

    public ObservableCollection<PreviewItem> PreviewItems { get; } = new();
    public ObservableCollection<RuleProfile> SavedProfiles { get; } = new();
    public ObservableCollection<string> RecentHistorySummaries { get; } = new();
    public ObservableCollection<SortConditionViewModel> SortConditions { get; } = new();
    public ObservableCollection<RenameRuleStepViewModel> RenameSteps { get; } = new();

    public MainViewModel()
    {
        _ruleRepository = new RuleProfileRepository(_database);
        _historyRepository = new HistoryRepository(_database);
        SortConditions.Add(new SortConditionViewModel(SortField.FileName));
        var initialStep = new RenameRuleStepViewModel(RenameRuleKind.Replace);
        initialStep.PropertyChanged += OnStepPropertyChanged;
        RenameSteps.Add(initialStep);
        _ = LoadPersistedDataAsync();
    }

    public int SelectedSortIndex { get => _selectedSortIndex; set => SetAndRefresh(ref _selectedSortIndex, value); }
    public bool SortDescending { get => _sortDescending; set => SetAndRefresh(ref _sortDescending, value); }
    public bool RecursiveFolder { get => _recursiveFolder; set => SetProperty(ref _recursiveFolder, value); }
    public int ExtensionModeIndex { get => _extensionModeIndex; set => SetAndRefresh(ref _extensionModeIndex, value); }
    public string RuleName { get => _ruleName; set => SetProperty(ref _ruleName, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }
    public string RefreshStateText { get => _refreshStateText; set => SetProperty(ref _refreshStateText, value); }
    public PreviewItem? SelectedPreviewItem { get => _selectedPreviewItem; set => SetProperty(ref _selectedPreviewItem, value); }
    public SortConditionViewModel? SelectedSortCondition { get => _selectedSortCondition; set => SetProperty(ref _selectedSortCondition, value); }
    public RenameRuleStepViewModel? SelectedRenameStep { get => _selectedRenameStep; set => SetProperty(ref _selectedRenameStep, value); }
    public int NewSortFieldIndex { get => _newSortFieldIndex; set => SetProperty(ref _newSortFieldIndex, value); }
    public bool NewSortDescending { get => _newSortDescending; set => SetProperty(ref _newSortDescending, value); }
    public int NewRenameRuleKindIndex { get => _newRenameRuleKindIndex; set => SetProperty(ref _newRenameRuleKindIndex, value); }
    public int RenameTargetIndex { get => _renameTargetIndex; set => SetAndRefresh(ref _renameTargetIndex, value); }
    public RuleProfile? SelectedProfile { get => _selectedProfile; set => SetProperty(ref _selectedProfile, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public string SummaryText
    {
        get
        {
            var total = PreviewItems.Count;
            var ready = PreviewItems.Count(i => i.Status == PreviewStatus.Ready);
            var error = PreviewItems.Count(i => i.Status == PreviewStatus.Error);
            var unchanged = PreviewItems.Count(i => i.Status == PreviewStatus.Unchanged);
            return $"已导入 {total} 个文件；可执行 {ready} 个；不变 {unchanged} 个；错误 {error} 个。";
        }
    }

    public void LoadFiles(IEnumerable<string> paths)
    {
        AddItems(_fileScanner.LoadFiles(paths));
        Message = $"已追加文件，当前列表 {_files.Count} 项。";
        RefreshPreview();
    }

    public void LoadFolder(string folderPath)
    {
        AddItems(_fileScanner.LoadFolders(new[] { folderPath }));
        Message = $"已追加文件夹，当前列表 {_files.Count} 项。";
        RefreshPreview();
    }

    private void AddItems(IEnumerable<FileItem> items)
    {
        var existing = _files.Select(f => f.FullPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (existing.Add(item.FullPath))
            {
                _files.Add(item);
            }
        }
    }

    public void RemoveSelectedPreviewItem()
    {
        if (SelectedPreviewItem is null)
        {
            Message = "请先在预览列表中选择要移除的文件。";
            return;
        }

        _files.RemoveAll(f => string.Equals(f.FullPath, SelectedPreviewItem.Source.FullPath, StringComparison.OrdinalIgnoreCase));
        Message = $"已从列表移除：{SelectedPreviewItem.OriginalName}。原文件未删除。";
        SelectedPreviewItem = null;
        RefreshPreview();
    }

    public void AddSortCondition()
    {
        SortConditions.Add(new SortConditionViewModel(MapSortField(NewSortFieldIndex), NewSortDescending));
        Message = "已添加排序条件。排序条件按列表从上到下作为优先级。";
        RefreshPreview();
    }

    public void RemoveSortCondition()
    {
        if (SelectedSortCondition is null || SortConditions.Count <= 1)
        {
            Message = "至少保留一个排序条件。";
            return;
        }

        SortConditions.Remove(SelectedSortCondition);
        RefreshPreview();
    }

    public void MoveSortConditionUp() => MoveItem(SortConditions, SelectedSortCondition, -1);

    public void MoveSortConditionDown() => MoveItem(SortConditions, SelectedSortCondition, 1);

    public void AddRenameStep()
    {
        var step = new RenameRuleStepViewModel(MapRenameRuleKind(NewRenameRuleKindIndex));
        step.PropertyChanged += OnStepPropertyChanged;
        RenameSteps.Add(step);
        Message = "已添加重命名方式。重命名方式按列表从上到下执行。";
        RefreshPreview();
    }

    public void RemoveRenameStep()
    {
        if (SelectedRenameStep is null || RenameSteps.Count <= 1)
        {
            Message = "至少保留一个重命名方式。";
            return;
        }

        SelectedRenameStep.PropertyChanged -= OnStepPropertyChanged;
        RenameSteps.Remove(SelectedRenameStep);
        RefreshPreview();
    }

    public void MoveRenameStepUp() => MoveItem(RenameSteps, SelectedRenameStep, -1);

    public void MoveRenameStepDown() => MoveItem(RenameSteps, SelectedRenameStep, 1);

    public void ExportExcelTemplate(string path)
    {
        if (_files.Count == 0)
        {
            Message = "请先选择文件或文件夹，再导出 Excel 模板。";
            return;
        }

        var sortedFiles = _sortService.Sort(_files, BuildSortRules());
        _excelTemplateService.Export(path, sortedFiles);
        Message = $"Excel 模板已导出：{path}";
    }

    public void ImportExcelTemplate(string path)
    {
        _excelRenameMap = _excelTemplateService.Import(path);
        Message = "Excel 模板已导入，预览将使用第二列作为目标名称。";
        RefreshPreview();
    }

    public void LoadSelectedRule()
    {
        if (SelectedProfile is null)
        {
            Message = "请先选择一个已保存规则。";
            return;
        }

        ApplyConfig(SelectedProfile.Config);
        RuleName = SelectedProfile.Name;
        Message = $"已加载规则：{SelectedProfile.Name}";
        RefreshPreview();
    }

    public void RefreshPreview()
    {
        var rules = BuildRules();
        var files = _files.ToList();
        var sortRules = BuildSortRules();
        var extensionMode = ExtensionModeIndex switch
        {
            1 => ExtensionCaseMode.Lower,
            2 => ExtensionCaseMode.Upper,
            _ => ExtensionCaseMode.Keep
        };

        RefreshStateText = "正在生成预览...";
        var targetMode = RenameTargetIndex == 1 ? RenameTargetMode.Extension : RenameTargetMode.FileName;
        var fileNameOverrides = _excelRenameMap?.FileNameMap;
        var extensionOverrides = _excelRenameMap?.ExtensionMap;
        Task.Run(() => _previewEngine.BuildPreview(files, rules, sortRules, targetMode, extensionMode, fileNameOverrides, extensionOverrides))
            .ContinueWith(task =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (task.IsFaulted)
                    {
                        RefreshStateText = "预览刷新失败：" + task.Exception?.GetBaseException().Message;
                        return;
                    }

                    PreviewItems.Clear();
                    foreach (var item in task.Result)
                    {
                        PreviewItems.Add(item);
                    }
                    RefreshStateText = $"预览已自动刷新：{DateTime.Now:HH:mm:ss}";
                    OnPropertyChanged(nameof(SummaryText));
                });
            });
    }

    public async Task ExecuteAsync(IReadOnlyList<PreviewItem>? selectedItems = null)
    {
        try
        {
            IsBusy = true;
            var targets = selectedItems is { Count: > 0 } ? selectedItems : PreviewItems.ToList();
            _lastHistory = await _executor.ExecuteRenameAsync(targets);
            _lastHistory.Summary = $"批量重命名，共修改 {_lastHistory.Items.Count} 个文件";
            await _historyRepository.SaveAsync(_lastHistory);
            await ReloadRecentHistoryAsync();
            Message = $"执行完成，共重命名 {_lastHistory.Items.Count} 个文件。";
            _files.RemoveAll(f => _lastHistory.Items.Any(i => string.Equals(i.OriginalPath, f.FullPath, StringComparison.OrdinalIgnoreCase)));
            AddItems(_lastHistory.Items.Select(i => Directory.Exists(i.NewPath) ? FileItem.FromDirectoryPath(i.NewPath) : FileItem.FromPath(i.NewPath)));
            RefreshPreview();
        }
        catch (Exception ex)
        {
            Message = "执行失败：" + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task UndoAsync()
    {
        try
        {
            IsBusy = true;
            var history = _lastHistory ?? await _historyRepository.LoadLastAsync();
            if (history is null || history.Items.Count == 0)
            {
                Message = "当前没有可撤销的操作。";
                return;
            }
            await _executor.UndoAsync(history);
            _files.RemoveAll(f => history.Items.Any(i => string.Equals(i.NewPath, f.FullPath, StringComparison.OrdinalIgnoreCase)));
            AddItems(history.Items.Select(i => Directory.Exists(i.OriginalPath) ? FileItem.FromDirectoryPath(i.OriginalPath) : FileItem.FromPath(i.OriginalPath)));
            _lastHistory = null;
            RefreshPreview();
            Message = $"撤销完成，共恢复 {history.Items.Count} 个文件。";
        }
        catch (Exception ex)
        {
            Message = "撤销失败：" + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveCurrentRuleAsync()
    {
        var name = string.IsNullOrWhiteSpace(RuleName) ? $"规则_{DateTime.Now:yyyyMMdd_HHmmss}" : RuleName.Trim();
        var profile = new RuleProfile
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Config = BuildConfig(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        await _ruleRepository.SaveAsync(profile);
        await ReloadProfilesAsync();
        Message = $"规则「{name}」已保存。";
    }

    public async Task DeleteSelectedRuleAsync()
    {
        if (SelectedProfile is null)
        {
            Message = "请先选择要删除的规则。";
            return;
        }

        var name = SelectedProfile.Name;
        await _ruleRepository.DeleteAsync(SelectedProfile.Id);
        SelectedProfile = null;
        await ReloadProfilesAsync();
        Message = $"规则「{name}」已删除。";
    }

    private IReadOnlyList<IRenameRule> BuildRules()
    {
        var rules = new List<IRenameRule>();
        foreach (var step in RenameSteps.Where(s => s.Enabled))
        {
            switch (step.Kind)
            {
                case RenameRuleKind.Replace:
                    if (!string.IsNullOrEmpty(step.FindText))
                        rules.Add(step.UseRegex ? new RegexReplaceRule(step.FindText, step.ReplaceText) : new ReplaceRule(step.FindText, step.ReplaceText));
                    break;
                case RenameRuleKind.Prefix:
                    if (!string.IsNullOrEmpty(step.PrefixText))
                        rules.Add(new AddPrefixRule(step.PrefixText));
                    break;
                case RenameRuleKind.Suffix:
                    if (!string.IsNullOrEmpty(step.SuffixText))
                        rules.Add(new AddSuffixRule(step.SuffixText));
                    break;
                case RenameRuleKind.RemoveText:
                    if (!string.IsNullOrEmpty(step.RemoveText))
                        rules.Add(new RemoveTextRule(step.RemoveText));
                    break;
                case RenameRuleKind.Numbering:
                    rules.Add(new NumberingRule(step.NumberStart, step.NumberStep, step.NumberDigits));
                    break;
                case RenameRuleKind.UpperAll:
                    rules.Add(new CaseConvertRule { Mode = CaseConvertMode.UpperAll });
                    break;
                case RenameRuleKind.LowerAll:
                    rules.Add(new CaseConvertRule { Mode = CaseConvertMode.LowerAll });
                    break;
                case RenameRuleKind.FirstLetterUpper:
                    rules.Add(new CaseConvertRule { Mode = CaseConvertMode.FirstLetterUpper });
                    break;
                case RenameRuleKind.EachWordUpper:
                    rules.Add(new CaseConvertRule { Mode = CaseConvertMode.EachWordUpper });
                    break;
            }
        }
        return rules;
    }

    private static SortField MapSortField(int index) => index switch
    {
        1 => SortField.Extension,
        2 => SortField.CreatedAt,
        3 => SortField.SizeBytes,
        4 => SortField.ModifiedAt,
        _ => SortField.FileName
    };

    private static RenameRuleKind MapRenameRuleKind(int index) => index switch
    {
        1 => RenameRuleKind.Prefix,
        2 => RenameRuleKind.Suffix,
        3 => RenameRuleKind.RemoveText,
        4 => RenameRuleKind.Numbering,
        5 => RenameRuleKind.UpperAll,
        6 => RenameRuleKind.LowerAll,
        7 => RenameRuleKind.FirstLetterUpper,
        8 => RenameRuleKind.EachWordUpper,
        _ => RenameRuleKind.Replace
    };

    private void MoveItem<T>(ObservableCollection<T> collection, T? item, int direction)
    {
        if (item is null)
        {
            return;
        }

        var oldIndex = collection.IndexOf(item);
        var newIndex = oldIndex + direction;
        if (oldIndex < 0 || newIndex < 0 || newIndex >= collection.Count)
        {
            return;
        }

        collection.Move(oldIndex, newIndex);
        RefreshPreview();
    }

    private RuleProfileConfig BuildConfig() => new()
    {
        SelectedSortIndex = SelectedSortIndex,
        SortDescending = SortDescending,
        ExtensionModeIndex = ExtensionModeIndex,
        RenameTargetIndex = RenameTargetIndex,
        Steps = RenameSteps.Select(s => new StepConfig
        {
            Kind = (int)s.Kind,
            Enabled = s.Enabled,
            FindText = s.FindText,
            ReplaceText = s.ReplaceText,
            UseRegex = s.UseRegex,
            PrefixText = s.PrefixText,
            SuffixText = s.SuffixText,
            RemoveText = s.RemoveText,
            NumberStart = s.NumberStart,
            NumberStep = s.NumberStep,
            NumberDigits = s.NumberDigits
        }).ToList()
    };

    private void ApplyConfig(RuleProfileConfig config)
    {
        _isApplyingProfile = true;
        SelectedSortIndex = config.SelectedSortIndex;
        SortDescending = config.SortDescending;
        ExtensionModeIndex = config.ExtensionModeIndex;
        RenameTargetIndex = config.RenameTargetIndex;

        RenameSteps.Clear();
        foreach (var stepConfig in config.Steps.DefaultIfEmpty(new StepConfig { Kind = (int)RenameRuleKind.Replace }))
        {
            var step = new RenameRuleStepViewModel((RenameRuleKind)stepConfig.Kind)
            {
                Enabled = stepConfig.Enabled,
                FindText = stepConfig.FindText,
                ReplaceText = stepConfig.ReplaceText,
                UseRegex = stepConfig.UseRegex,
                PrefixText = stepConfig.PrefixText,
                SuffixText = stepConfig.SuffixText,
                RemoveText = stepConfig.RemoveText,
                NumberStart = stepConfig.NumberStart,
                NumberStep = stepConfig.NumberStep,
                NumberDigits = stepConfig.NumberDigits
            };
            step.PropertyChanged += OnStepPropertyChanged;
            RenameSteps.Add(step);
        }

        _isApplyingProfile = false;
    }

    private List<MultiSortRule> BuildSortRules()
    {
        return SortConditions.Select((condition, index) => new MultiSortRule
        {
            Field = condition.Field,
            Descending = condition.Descending,
            Priority = index
        }).ToList();
    }

    private void OnStepPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RenameRuleStepViewModel.Kind))
        {
            if (!_isApplyingProfile && _files.Count > 0)
            {
                RefreshStateText = "规则已变化，正在等待输入完成...";
                _refreshScheduler.Schedule(_ =>
                {
                    RefreshPreview();
                    return Task.CompletedTask;
                });
            }
        }
        else if (e.PropertyName != nameof(RenameRuleStepViewModel.Enabled) &&
                 e.PropertyName != nameof(RenameRuleStepViewModel.DisplayName))
        {
            // 参数变更，触发预览刷新
            if (!_isApplyingProfile && _files.Count > 0)
            {
                RefreshStateText = "规则已变化，正在等待输入完成...";
                _refreshScheduler.Schedule(_ =>
                {
                    RefreshPreview();
                    return Task.CompletedTask;
                });
            }
        }
    }

    private async Task LoadPersistedDataAsync()
    {
        await ReloadProfilesAsync();
        await ReloadRecentHistoryAsync();
    }

    private async Task ReloadProfilesAsync()
    {
        SavedProfiles.Clear();
        foreach (var profile in await _ruleRepository.LoadAllAsync())
        {
            SavedProfiles.Add(profile);
        }
    }

    private async Task ReloadRecentHistoryAsync()
    {
        RecentHistorySummaries.Clear();
        foreach (var summary in await _historyRepository.LoadRecentSummariesAsync())
        {
            RecentHistorySummaries.Add(summary);
        }
    }

    private bool SetAndRefresh<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        var changed = SetProperty(ref field, value, propertyName);
        if (changed && !_isApplyingProfile && _files.Count > 0)
        {
            RefreshStateText = "规则已变化，正在等待输入完成...";
            _refreshScheduler.Schedule(_ =>
            {
                RefreshPreview();
                return Task.CompletedTask;
            });
        }
        return changed;
    }
}
