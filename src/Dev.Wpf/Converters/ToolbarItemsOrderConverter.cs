// ToolbarItemsOrderConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

/// <summary>
/// Produces a stable order-projected list of <see cref="ToolbarItem"/> values.
/// </summary>
[ValueConversion(typeof(IEnumerable), typeof(IReadOnlyList<ToolbarItem>))]
public sealed class ToolbarItemsOrderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable source)
            return Array.Empty<ToolbarItem>();

        var ordered = source
            .OfType<ToolbarItem>()
            .OrderBy(item => item.Order)
            .ToArray();

        return ordered;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
