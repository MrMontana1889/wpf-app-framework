// ToolbarHostControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
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
/// Can be associated with a semantic ToolbarId and registry to automatically
/// respond to visibility changes.
/// </summary>
public sealed class ToolbarHostControl : Control
{
    private IToolbarRegistryService? _currentRegistry;
    private EventHandler<ToolbarVisibilityChangedEventArgs>? _visibilityChangedHandler;

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

    public static readonly DependencyProperty ToolbarIdProperty =
        DependencyProperty.Register(
            nameof(ToolbarId),
            typeof(Dev.Core.Toolbar.ToolbarId?),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(null, OnToolbarIdChanged));

    public static readonly DependencyProperty ToolbarRegistryProperty =
        DependencyProperty.Register(
            nameof(ToolbarRegistry),
            typeof(Dev.Core.Services.IToolbarRegistryService),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(null, OnToolbarRegistryChanged));

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

    private static void OnToolbarIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;
        control.UpdateVisibilityBinding();
    }

    private static void OnToolbarRegistryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;
        control.UpdateVisibilityBinding();
    }

    private void UpdateVisibilityBinding()
    {
        // Unsubscribe from previous registry if needed
        if (_currentRegistry is not null && _visibilityChangedHandler is not null)
        {
            _currentRegistry.VisibilityChanged -= _visibilityChangedHandler;
            _currentRegistry = null;
            _visibilityChangedHandler = null;
        }

        if (ToolbarRegistry is null || !ToolbarId.HasValue)
        {
            ClearValue(VisibilityProperty);
            return;
        }

        var toolbarId = ToolbarId.Value;
        var registry = ToolbarRegistry;

        // Subscribe to future visibility changes
        _visibilityChangedHandler = (sender, e) =>
        {
            if (e.ToolbarId == toolbarId)
            {
                var visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                SetCurrentValue(VisibilityProperty, visibility);
            }
        };

        registry.VisibilityChanged += _visibilityChangedHandler;
        _currentRegistry = registry;

        // Set initial visibility
        var initialVisibility = registry.IsVisible(toolbarId) ? Visibility.Visible : Visibility.Collapsed;
        SetCurrentValue(VisibilityProperty, initialVisibility);
    }

    /// <summary>
    /// Gets or sets the full list of registered toolbars used to build the
    /// context menu. Typically bound to the main ViewModel's Toolbars property.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the resolver for icon keys into WPF <see cref="System.Windows.Media.ImageSource"/> values.
    /// </summary>
    public IIconProvider? IconProvider
    {
        get => (IIconProvider?)GetValue(IconProviderProperty);
        set => SetValue(IconProviderProperty, value);
    }

    /// <summary>
    /// Gets or sets the semantic toolbar identity. When set along with
    /// <see cref="ToolbarRegistry"/>, the control's visibility automatically
    /// responds to registry visibility changes.
    /// </summary>
    public Dev.Core.Toolbar.ToolbarId? ToolbarId
    {
        get => (Dev.Core.Toolbar.ToolbarId?)GetValue(ToolbarIdProperty);
        set => SetValue(ToolbarIdProperty, value);
    }

    /// <summary>
    /// Gets or sets the toolbar registry service. When set along with
    /// <see cref="ToolbarId"/>, the control's visibility automatically
    /// responds to registry visibility changes.
    /// </summary>
    public Dev.Core.Services.IToolbarRegistryService? ToolbarRegistry
    {
        get => (Dev.Core.Services.IToolbarRegistryService?)GetValue(ToolbarRegistryProperty);
        set => SetValue(ToolbarRegistryProperty, value);
    }
}
