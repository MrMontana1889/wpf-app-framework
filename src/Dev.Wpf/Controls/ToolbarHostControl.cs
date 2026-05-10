// ToolbarHostControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dev.Wpf.Controls;

/// <summary>
/// WPF projection host for Core <see cref="ToolbarItem"/> data.
/// Renders items inside a native <see cref="ToolBarTray"/> and <see cref="ToolBar"/>.
/// </summary>
public sealed class ToolbarHostControl : Control
{
    static ToolbarHostControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(typeof(ToolbarHostControl)));
    }

    public ToolbarHostControl()
    {
        SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty IconProviderProperty =
        DependencyProperty.Register(
            nameof(IconProvider),
            typeof(IIconProvider),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(null, OnIconProviderChanged));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is null)
            return;

        var view = CollectionViewSource.GetDefaultView(e.NewValue);
        if (view is null)
            return;

        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(ToolbarItem.Order), ListSortDirection.Ascending));
        }
    }

    private static void OnIconProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;
        if (e.NewValue is not null)
            return;

        control.SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());
    }

    /// <summary>
    /// Core toolbar items to project into native WPF controls.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Resolves icon keys into WPF <see cref="System.Windows.Media.ImageSource"/> values.
    /// </summary>
    public IIconProvider? IconProvider
    {
        get => (IIconProvider?)GetValue(IconProviderProperty);
        set => SetValue(IconProviderProperty, value);
    }
}
