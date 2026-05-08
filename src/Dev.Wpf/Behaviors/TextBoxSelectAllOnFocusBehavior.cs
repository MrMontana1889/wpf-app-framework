// TextBoxSelectAllOnFocusBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Behaviors;

[ExcludeFromCodeCoverage]
public static class TextBoxSelectAllOnFocusBehavior
{
    #region Public Methods

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);

    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    #endregion Public Methods

    #region Public Fields

    public static readonly DependencyProperty EnableProperty =
                        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(TextBoxSelectAllOnFocusBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    #endregion Public Fields

    #region Private Methods

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.GotFocus += TextBox_GotFocus;
            }
            else
            {
                textBox.GotFocus -= TextBox_GotFocus;
            }
        }
    }

    private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Dispatcher.BeginInvoke(new Action(() =>
            {
                tb.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    #endregion Private Methods
}
