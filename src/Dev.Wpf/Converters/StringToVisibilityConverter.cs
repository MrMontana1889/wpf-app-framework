// StringToVisibilityConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
public class StringToVisibilityConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion Public Methods
}
