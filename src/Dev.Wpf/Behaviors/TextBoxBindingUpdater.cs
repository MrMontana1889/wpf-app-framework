// TextBoxBindingUpdater.cs
// Copyright (c) 2025 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dev.Wpf.Behaviors;

[ExcludeFromCodeCoverage]
public static class TextBoxBindingUpdater
{
    #region Public Methods

    public static TextBox GetUpdateSourceOnClickTarget(Button button)
    {
        return (TextBox)button.GetValue(UpdateSourceOnClickTargetProperty);
    }

    public static void SetUpdateSourceOnClickTarget(Button button, TextBox value)
    {
        button.SetValue(UpdateSourceOnClickTargetProperty, value);
    }

    #endregion Public Methods

    #region Public Fields

    public static readonly DependencyProperty UpdateSourceOnClickTargetProperty =
                        DependencyProperty.RegisterAttached(
            "UpdateSourceOnClickTarget",
            typeof(TextBox),
            typeof(TextBoxBindingUpdater),
            new PropertyMetadata(null, OnUpdateSourceOnClickTargetChanged));

    #endregion Public Fields

    #region Private Methods

    private static void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var textBox = GetUpdateSourceOnClickTarget(button);
            if (textBox != null)
            {
                var binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                binding?.UpdateSource();
            }
        }
    }

    private static void OnUpdateSourceOnClickTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Button button)
        {
            button.Click -= OnButtonClick;
            button.Click += OnButtonClick;
        }
    }

    #endregion Private Methods
}
