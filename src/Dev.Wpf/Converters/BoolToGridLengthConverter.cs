// BoolToGridLengthConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

/// <summary>
/// Converts a <see cref="bool"/> to a <see cref="GridLength"/>.
/// <c>true</c> returns <c>1*</c> (star); <c>false</c> returns <c>0</c> (collapsed).
/// Intended for binding to <see cref="System.Windows.Controls.RowDefinition.Height"/>
/// or <see cref="System.Windows.Controls.ColumnDefinition.Width"/> to collapse
/// a grid track without requiring code-behind.
/// </summary>
[ExcludeFromCodeCoverage]
[ValueConversion(typeof(bool), typeof(GridLength))]
public sealed class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
