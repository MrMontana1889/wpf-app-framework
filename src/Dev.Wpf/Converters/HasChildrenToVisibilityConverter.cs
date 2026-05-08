// HasChildrenToVisibilityConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
public class HasChildrenToVisibilityConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasChildren = value is bool b && b;
        return hasChildren ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    #endregion Public Methods
}
