// ToolbarDisplayIntentToVisibilityConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

/// <summary>
/// Projects toolbar display intent to icon or text visibility.
/// </summary>
[ValueConversion(typeof(ToolbarItemDisplayIntent), typeof(Visibility))]
public sealed class ToolbarDisplayIntentToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ToolbarItemDisplayIntent intent)
            return Visibility.Collapsed;

        var target = parameter as string;
        return target switch
        {
            "Icon" => intent is ToolbarItemDisplayIntent.IconOnly or ToolbarItemDisplayIntent.IconAndText
                ? Visibility.Visible
                : Visibility.Collapsed,
            "Text" => intent is ToolbarItemDisplayIntent.TextOnly or ToolbarItemDisplayIntent.IconAndText
                ? Visibility.Visible
                : Visibility.Collapsed,
            _ => Visibility.Collapsed,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
