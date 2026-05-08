// TextBoxPlaceholder.cs
// Copyright (c) 2025 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Behaviors;

[ExcludeFromCodeCoverage]
public static class TextBoxPlaceholder
{
    #region Public Methods

    public static string GetPlaceholder(UIElement element) =>
        (string)element.GetValue(PlaceholderProperty);

    public static void SetPlaceholder(UIElement element, string value) =>
        element.SetValue(PlaceholderProperty, value);

    #endregion Public Methods

    #region Public Fields

    public static readonly DependencyProperty PlaceholderProperty =
                        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(TextBoxPlaceholder),
            new FrameworkPropertyMetadata(string.Empty));

    #endregion Public Fields
}
