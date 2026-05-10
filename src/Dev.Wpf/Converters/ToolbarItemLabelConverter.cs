// ToolbarItemLabelConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using System.Globalization;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

/// <summary>
/// Projects a toolbar item to its primary semantic label.
/// </summary>
[ValueConversion(typeof(ToolbarItem), typeof(string))]
public sealed class ToolbarItemLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ToolbarItem item)
            return string.Empty;

        return item.SemanticMetadata.Text.Label;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
