// TextBoxAutoScrollBehavior.cs
// Copyright (c) 2025 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Behaviors;

[ExcludeFromCodeCoverage]
public static class TextBoxAutoScrollBehavior
{
    #region Public Methods

    public static bool GetEnableAutoScroll(DependencyObject obj) =>
        (bool)obj.GetValue(EnableAutoScrollProperty);

    public static void SetEnableAutoScroll(DependencyObject obj, bool value) =>
        obj.SetValue(EnableAutoScrollProperty, value);

    #endregion Public Methods

    #region Public Fields

    public static readonly DependencyProperty EnableAutoScrollProperty =
                        DependencyProperty.RegisterAttached(
            "EnableAutoScroll",
            typeof(bool),
            typeof(TextBoxAutoScrollBehavior),
            new PropertyMetadata(false, OnEnableAutoScrollChanged));

    #endregion Public Fields

    #region Private Methods

    private static void OnEnableAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.TextChanged += TextBox_TextChanged;
            }
            else
            {
                textBox.TextChanged -= TextBox_TextChanged;
            }
        }
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.ScrollToEnd();
        }
    }

    #endregion Private Methods
}
