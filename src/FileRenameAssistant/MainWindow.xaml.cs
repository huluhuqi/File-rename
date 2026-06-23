using FileRenameAssistant.Models;
using FileRenameAssistant.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinForms = System.Windows.Forms;

namespace FileRenameAssistant;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void PickFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = true,
            CheckFileExists = true,
            Title = "选择文件"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.LoadFiles(dialog.FileNames);
        }
    }

    private void PickFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new WinForms.FolderBrowserDialog
        {
            Description = "选择文件夹",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            _viewModel.LoadFolder(dialog.SelectedPath);
        }
    }

    private void RefreshPreview_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshPreview();
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        var selected = PreviewGrid.SelectedItems.OfType<PreviewItem>().ToList();
        await _viewModel.ExecuteAsync(selected);
    }

    private async void Undo_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.UndoAsync();
    }

    private async void SaveRule_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveCurrentRuleAsync();
    }

    private void RemoveSelectedFile_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveSelectedPreviewItem();
    }

    private void AddSortCondition_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AddSortCondition();
    }

    private void RemoveSortCondition_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveSortCondition();
    }

    private void MoveSortUp_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.MoveSortConditionUp();
    }

    private void MoveSortDown_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.MoveSortConditionDown();
    }

    private void AddRenameStep_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AddRenameStep();
    }

    private void RemoveRenameStep_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveRenameStep();
    }

    private void MoveRenameUp_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.MoveRenameStepUp();
    }

    private void MoveRenameDown_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.MoveRenameStepDown();
    }

    private void ExportExcelTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "导出文件名模板",
            Filter = "Excel 文件 (*.xlsx)|*.xlsx",
            FileName = $"文件名模板_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ExportExcelTemplate(dialog.FileName);
        }
    }

    private void ImportExcelTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "导入文件名模板",
            Filter = "Excel 文件 (*.xlsx)|*.xlsx",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ImportExcelTemplate(dialog.FileName);
        }
    }

    private void LoadRule_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadSelectedRule();
    }

    private async void DeleteRule_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.DeleteSelectedRuleAsync();
    }

    private void MoveSortItemUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: SortConditionViewModel item })
        {
            _viewModel.SelectedSortCondition = item;
            _viewModel.MoveSortConditionUp();
            AnimateMoveItem(sender as FrameworkElement, -1);
        }
    }

    private void MoveSortItemDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: SortConditionViewModel item })
        {
            _viewModel.SelectedSortCondition = item;
            _viewModel.MoveSortConditionDown();
            AnimateMoveItem(sender as FrameworkElement, 1);
        }
    }

    private void MoveRenameItemUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: RenameRuleStepViewModel item })
        {
            _viewModel.SelectedRenameStep = item;
            _viewModel.MoveRenameStepUp();
            AnimateMoveItem(sender as FrameworkElement, -1);
        }
    }

    private void MoveRenameItemDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: RenameRuleStepViewModel item })
        {
            _viewModel.SelectedRenameStep = item;
            _viewModel.MoveRenameStepDown();
            AnimateMoveItem(sender as FrameworkElement, 1);
        }
    }

    private void AnimateMoveItem(FrameworkElement? button, int direction)
    {
        if (button == null) return;
        var listBox = FindParent<System.Windows.Controls.ListBox>(button);
        if (listBox == null) return;

        var container = listBox.ItemContainerGenerator.ContainerFromItem(button.DataContext);
        if (container is not System.Windows.Controls.ListBoxItem listBoxItem) return;

        var offset = direction * listBoxItem.ActualHeight;
        var transform = new TranslateTransform(0, -offset);
        listBoxItem.RenderTransform = transform;
        listBoxItem.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

        var animation = new DoubleAnimation
        {
            From = -offset,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        transform.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T result) return result;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}
