using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FileRenameAssistant.ViewModels;

namespace FileRenameAssistant.Converters;

public class IsCaseRuleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RenameRuleKind kind)
            return kind is RenameRuleKind.UpperAll or RenameRuleKind.LowerAll or RenameRuleKind.FirstLetterUpper or RenameRuleKind.EachWordUpper
                ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
