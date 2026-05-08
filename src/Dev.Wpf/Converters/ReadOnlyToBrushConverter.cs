// ReadOnlyToBrushConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dev.Wpf.Converters;

[ExcludeFromCodeCoverage]
public class ReadOnlyToBrushConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool isReadOnly && isReadOnly) ? ReadOnlyBrush : EditableBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion Public Methods

    #region Public Properties

    public Brush EditableBrush { get; set; } = Brushes.White;
    public Brush ReadOnlyBrush { get; set; } = Brushes.LightGoldenrodYellow;

    #endregion Public Properties
}
