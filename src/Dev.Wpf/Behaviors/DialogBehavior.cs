// DialogBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Attached behavior that closes a Window by binding its DialogResult
/// to a ViewModel property, keeping code-behind free of dialog lifecycle logic.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DialogBehavior
{
    public static readonly DependencyProperty DialogResultProperty =
        DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool?),
            typeof(DialogBehavior),
            new PropertyMetadata(null, OnDialogResultChanged));

    public static bool? GetDialogResult(DependencyObject obj) =>
        (bool?)obj.GetValue(DialogResultProperty);

    public static void SetDialogResult(DependencyObject obj, bool? value) =>
        obj.SetValue(DialogResultProperty, value);

    private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window && e.NewValue is bool result)
            window.DialogResult = result;
    }
}
