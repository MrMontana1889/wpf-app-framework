// SelectionToBrushConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
public class SelectionToBrushConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isSelected = value is bool b && b;
        return isSelected ? SelectedBrush : DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    #endregion Public Methods

    #region Private Fields

    private static readonly Brush DefaultBrush = new SolidColorBrush(Color.FromRgb(34, 34, 34));
    private static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DarkSlateBlue);

    #endregion Private Fields

    // #222222
}
