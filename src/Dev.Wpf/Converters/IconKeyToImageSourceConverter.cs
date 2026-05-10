// IconKeyToImageSourceConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dev.Wpf.Converters;

/// <summary>
/// Converts an icon key plus <see cref="IIconProvider"/> into an <see cref="ImageSource"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class IconKeyToImageSourceConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return null;

        var iconKey = values[0] switch
        {
            string key when !string.IsNullOrWhiteSpace(key) => key,
            IconKey key => key.Value,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(iconKey))
            return null;

        if (values[1] is not IIconProvider iconProvider)
            return null;

        return iconProvider.GetIcon(iconKey);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
