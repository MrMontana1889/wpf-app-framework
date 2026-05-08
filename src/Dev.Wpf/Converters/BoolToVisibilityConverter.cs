// BoolToVisibilityConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{

    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        bool flag = value is bool b && b;

        if (invert)
            flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is Visibility visibility && visibility == Visibility.Visible);
    }

    #endregion Public Methods

    #region Public Properties

    public bool CollapseWhenFalse { get; set; } = true;

    #endregion Public Properties

}