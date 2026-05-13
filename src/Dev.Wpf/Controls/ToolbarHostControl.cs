// ToolbarHostControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dev.Wpf.Controls;

/// <summary>
/// WPF projection host for Core <see cref="ToolbarItem"/> data.
/// Can be associated with a semantic ToolbarId and registry to automatically
/// respond to visibility changes.
/// </summary>
public sealed class ToolbarHostControl : Control
{
    private IToolbarRegistryService? _currentRegistry;
    private EventHandler<ToolbarVisibilityChangedEventArgs>? _visibilityChangedHandler;
    private EventHandler<ToolbarItemVisibilityChangedEventArgs>? _itemVisibilityChangedHandler;
    private INotifyCollectionChanged? _currentItemsSource;
    private NotifyCollectionChangedEventHandler? _collectionChangedHandler;
    private ItemsControl? _itemsHostElement;

    static ToolbarHostControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(typeof(ToolbarHostControl)));
    }

    public ToolbarHostControl()
    {
        SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());
        Loaded += (_, _) => AttachItemsSourceCollectionChangedHandler(ItemsSource);
        Unloaded += (_, _) => DetachItemsSourceCollectionChangedHandler();
        PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsHostElement = TemplateTestFindItemsHost();
        RefreshItemsProjection();
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

    public static readonly DependencyProperty EnableGroupedToolbarProjectionProperty =
        DependencyProperty.Register(
            nameof(EnableGroupedToolbarProjection),
            typeof(bool),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(false, OnEnableGroupedToolbarProjectionChanged));

    public static readonly DependencyProperty MenuBarIdProperty =
        DependencyProperty.Register(
            nameof(MenuBarId),
            typeof(ToolbarId),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(new ToolbarId("MenuBar")));

    private static readonly DependencyPropertyKey ProjectedItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ProjectedItems),
            typeof(IReadOnlyList<ToolbarItem>),
            typeof(ToolbarHostControl),
            new FrameworkPropertyMetadata(Array.Empty<ToolbarItem>()));

    public static readonly DependencyProperty ProjectedItemsProperty = ProjectedItemsPropertyKey.DependencyProperty;

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;

        control.DetachItemsSourceCollectionChangedHandler();
        control.AttachItemsSourceCollectionChangedHandler(e.NewValue as IEnumerable);
        control.ConfigureItemsProjection(e.NewValue as IEnumerable);
    }

    private static void OnIconProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;
        if (e.NewValue is not null)
            return;

        control.SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());
    }

    private void AttachItemsSourceCollectionChangedHandler(IEnumerable? source)
    {
        if (source is not INotifyCollectionChanged notifyCollectionChanged)
            return;

        _collectionChangedHandler ??= (_, _) =>
        {
            // ItemsSource content changed but reference stayed the same.
            // Reconfigure projection so registry-based visibility remains enforced.
            ConfigureItemsProjection(ItemsSource);
        };

        notifyCollectionChanged.CollectionChanged += _collectionChangedHandler;
        _currentItemsSource = notifyCollectionChanged;
    }

    private void DetachItemsSourceCollectionChangedHandler()
    {
        if (_currentItemsSource is not null && _collectionChangedHandler is not null)
        {
            _currentItemsSource.CollectionChanged -= _collectionChangedHandler;
            _currentItemsSource = null;
        }
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

    private static void OnEnableGroupedToolbarProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToolbarHostControl)d;
        control.RefreshItemsProjection();
    }

    private void UpdateVisibilityBinding()
    {
        // Unsubscribe from previous registry if needed
        if (_currentRegistry is not null && _visibilityChangedHandler is not null)
        {
            _currentRegistry.VisibilityChanged -= _visibilityChangedHandler;
            if (_itemVisibilityChangedHandler is not null)
                _currentRegistry.ItemVisibilityChanged -= _itemVisibilityChangedHandler;

            _currentRegistry = null;
            _visibilityChangedHandler = null;
            _itemVisibilityChangedHandler = null;
        }

        if (ToolbarRegistry is null || !ToolbarId.HasValue)
        {
            ClearValue(VisibilityProperty);
            ConfigureItemsProjection(ItemsSource);
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

        _itemVisibilityChangedHandler = (_, e) =>
        {
            if (e.ToolbarId == toolbarId)
            {
                RefreshItemsProjection();
            }
        };

        registry.VisibilityChanged += _visibilityChangedHandler;
        registry.ItemVisibilityChanged += _itemVisibilityChangedHandler;
        _currentRegistry = registry;

        // Set initial visibility
        var initialVisibility = registry.IsVisible(toolbarId) ? Visibility.Visible : Visibility.Collapsed;
        SetCurrentValue(VisibilityProperty, initialVisibility);

        // Apply items visibility filter
        ConfigureItemsProjection(ItemsSource);
    }

    private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (TryShowToolbarContextMenu(e.OriginalSource as DependencyObject, e.GetPosition(this)))
            e.Handled = true;
    }

    private bool TryShowToolbarContextMenu(DependencyObject? originalSource, Point positionInControl)
    {
        if (_itemsHostElement is null || ToolbarRegistry is null || originalSource is null)
            return false;

        if (!IsWithinToolbarVisualTree(originalSource))
            return false;

        var contextMenu = ContextMenu ?? new ContextMenu();
        contextMenu.Items.Clear();

        BuildToolbarVisibilityMenuEntries(contextMenu, ToolbarRegistry);
        BuildCustomizeSubMenuEntry(contextMenu, ToolbarRegistry);

        ContextMenu = contextMenu;

        if (contextMenu.Items.Count == 0)
            return false;

        contextMenu.PlacementTarget = this;
        contextMenu.Placement = PlacementMode.RelativePoint;
        contextMenu.HorizontalOffset = positionInControl.X;
        contextMenu.VerticalOffset = positionInControl.Y;
        contextMenu.IsOpen = true;

        return true;
    }

    private bool IsWithinToolbarVisualTree(DependencyObject source)
    {
        var current = source;
        while (current is not null)
        {
            if (ReferenceEquals(current, this))
                return true;

            if (ReferenceEquals(current, _itemsHostElement))
                return true;

            current = GetParent(current);
        }

        return false;
    }

    private static DependencyObject? GetParent(DependencyObject current)
    {
        return current switch
        {
            Visual or Visual3D => VisualTreeHelper.GetParent(current),
            FrameworkContentElement frameworkContentElement => frameworkContentElement.Parent,
            ContentElement contentElement => ContentOperations.GetParent(contentElement),
            _ => LogicalTreeHelper.GetParent(current)
        };
    }

    private void BuildToolbarVisibilityMenuEntries(ContextMenu contextMenu, IToolbarRegistryService registry)
    {
        var menuBarDefinition = registry.ToolbarDefinitions.FirstOrDefault(IsMenuBarDefinition);
        var toolbarDefinitions = registry.ToolbarDefinitions.Where(definition => !IsMenuBarDefinition(definition)).ToList();

        var addedAnyRows = false;

        if (menuBarDefinition is not null)
        {
            AddRowVisibilityEntry(contextMenu, registry, menuBarDefinition);
            addedAnyRows = true;
        }

        if (menuBarDefinition is not null && toolbarDefinitions.Count > 0)
        {
            contextMenu.Items.Add(new Separator());
        }

        foreach (var definition in toolbarDefinitions)
        {
            AddRowVisibilityEntry(contextMenu, registry, definition);
            addedAnyRows = true;
        }

        if (!addedAnyRows)
            return;
    }

    private bool IsMenuBarDefinition(ToolbarDefinition definition)
    {
        return definition.Id == MenuBarId;
    }

    private static void AddRowVisibilityEntry(ContextMenu contextMenu, IToolbarRegistryService registry, ToolbarDefinition definition)
    {
        var definitionId = definition.Id;

        var toggleItem = new MenuItem
        {
            Header = definition.DisplayName,
            IsCheckable = true,
            IsChecked = registry.IsVisible(definitionId),
            IsEnabled = definition.CanHide
        };

        toggleItem.Click += (_, _) =>
        {
            registry.SetVisibility(definitionId, toggleItem.IsChecked);
        };

        contextMenu.Items.Add(toggleItem);
    }

    private void BuildCustomizeSubMenuEntry(ContextMenu contextMenu, IToolbarRegistryService registry)
    {
        if (!ToolbarId.HasValue || ItemsSource is null)
            return;

        var toolbarId = ToolbarId.Value;
        var definition = registry.ToolbarDefinitions.FirstOrDefault(d => d.Id == toolbarId);
        if (definition is null)
            return;

        var customizeRoot = new MenuItem { Header = "Customize..." };

        // Iterate items from ItemsSource (application-provided)
        var itemsToProcess = ItemsSource is not null ? ItemsSource : Enumerable.Empty<object>();
        foreach (var item in itemsToProcess)
        {
            if (item is not ToolbarItem toolbarItem)
                continue;

            var itemId = toolbarItem.Id;
            var label = toolbarItem.SemanticMetadata.Text.Label;

            var itemEntry = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                IsChecked = registry.IsItemVisible(toolbarId, itemId)
            };

            itemEntry.Click += (_, _) =>
            {
                registry.SetItemVisibility(toolbarId, itemId, itemEntry.IsChecked);
                RefreshItemsProjection();
            };

            customizeRoot.Items.Add(itemEntry);
        }

        if (customizeRoot.Items.Count == 0)
        {
            customizeRoot.Items.Add(new MenuItem
            {
                Header = "No customizable items",
                IsEnabled = false
            });
        }

        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(customizeRoot);
    }

    private void RefreshItemsProjection()
    {
        ConfigureItemsProjection(ItemsSource);
    }

    private void ConfigureItemsProjection(IEnumerable? source)
    {
        var visibleItems = BuildVisibleOrderedItems(source);
        SetValue(ProjectedItemsPropertyKey, visibleItems);
    }

    private List<ToolbarItem> BuildVisibleOrderedItems(IEnumerable? source)
    {
        if (source is null)
            return [];

        IEnumerable<ToolbarItem> items = source.OfType<ToolbarItem>();

        if (ToolbarRegistry is not null && ToolbarId.HasValue)
        {
            var toolbarId = ToolbarId.Value;
            var registry = ToolbarRegistry;
            items = items.Where(item => registry.IsItemVisible(toolbarId, item.Id));
        }

        return items
            .OrderBy(item => item.Order)
            .ToList();
    }

    private ItemsControl? TemplateTestFindItemsHost()
    {
        if (GetTemplateChild("PART_ItemsHost") is ItemsControl named)
            return named;

        return FindVisualChild<ItemsControl>(this);
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is T directMatch)
            return directMatch;

        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            var match = FindVisualChild<T>(child);
            if (match is not null)
                return match;
        }

        return null;
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

    /// <summary>
    /// Gets or sets whether toolbar item projection is partitioned into multiple native
    /// visual groups by <see cref="ToolbarItem.LogicalGroup"/>.
    /// Default is <c>false</c>, which preserves single-group projection.
    /// </summary>
    public bool EnableGroupedToolbarProjection
    {
        get => (bool)GetValue(EnableGroupedToolbarProjectionProperty);
        set => SetValue(EnableGroupedToolbarProjectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the semantic identity of the menu bar row definition in the toolbar registry.
    /// </summary>
    public ToolbarId MenuBarId
    {
        get => (ToolbarId)GetValue(MenuBarIdProperty);
        set => SetValue(MenuBarIdProperty, value);
    }

    /// <summary>
    /// Gets the projected toolbar items produced from <see cref="ItemsSource"/>.
    /// This is consumed by the control template to render a flat toolbar list.
    /// </summary>
    public IReadOnlyList<ToolbarItem> ProjectedItems
    {
        get => (IReadOnlyList<ToolbarItem>)GetValue(ProjectedItemsProperty);
        private set => SetValue(ProjectedItemsPropertyKey, value);
    }
}
