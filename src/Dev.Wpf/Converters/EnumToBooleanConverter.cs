// EnumToBooleanConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
public class EnumToBooleanConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        string enumValue = value.ToString()!;
        string targetValue = parameter.ToString()!;

        return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
            return Binding.DoNothing;

        return Enum.Parse(targetType, parameter.ToString()!);
    }

    #endregion Public Methods
}
