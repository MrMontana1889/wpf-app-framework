// MenuHostControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CoreMenuItem = Dev.Core.Menu.MenuItem;
using Dev.Core.Menu;
using Dev.Wpf.Converters;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dev.Wpf.Controls;

/// <summary>
/// WPF projection host for Core <see cref="CoreMenuItem"/> data.
/// Renders semantic menu items as native WPF menu primitives.
/// </summary>
public sealed class MenuHostControl : Control
{
    private readonly ObservableCollection<object> _projectedItems = [];
    private readonly BoolToVisibilityConverter _boolToVisibilityConverter = new();
    private readonly IconKeyToImageSourceConverter _iconConverter = new();
    private readonly MenuShortcutToTextConverter _shortcutConverter = new();

    static MenuHostControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuHostControl),
            new FrameworkPropertyMetadata(typeof(MenuHostControl)));
    }

    public MenuHostControl()
    {
        SetValue(ProjectedItemsPropertyKey, _projectedItems);
        SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MenuHostControl),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty IconProviderProperty =
        DependencyProperty.Register(
            nameof(IconProvider),
            typeof(IIconProvider),
            typeof(MenuHostControl),
            new FrameworkPropertyMetadata(null, OnIconProviderChanged));

    private static readonly DependencyPropertyKey ProjectedItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ProjectedItems),
            typeof(IEnumerable),
            typeof(MenuHostControl),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ProjectedItemsProperty = ProjectedItemsPropertyKey.DependencyProperty;

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MenuHostControl)d).RebuildProjectedItems();
    }

    private static void OnIconProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MenuHostControl)d;

        if (e.NewValue is null)
            control.SetCurrentValue(IconProviderProperty, new ApplicationResourceIconProvider());

        control.RebuildProjectedItems();
    }

    /// <summary>
    /// Semantic menu items to project into native WPF menu controls.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Resolves icon keys to WPF image sources.
    /// </summary>
    public IIconProvider? IconProvider
    {
        get => (IIconProvider?)GetValue(IconProviderProperty);
        set => SetValue(IconProviderProperty, value);
    }

    /// <summary>
    /// Internal projected WPF item collection rendered by the control template.
    /// </summary>
    public IEnumerable ProjectedItems => (IEnumerable)GetValue(ProjectedItemsProperty);

    private void RebuildProjectedItems()
    {
        _projectedItems.Clear();

        if (ItemsSource is null)
            return;

        foreach (var item in ItemsSource.OfType<CoreMenuItem>())
            _projectedItems.Add(CreateProjectedItem(item));
    }

    private object CreateProjectedItem(CoreMenuItem source)
    {
        if (source.Kind == MenuItemKind.Separator)
        {
            var separator = new Separator();
            separator.SetBinding(
                VisibilityProperty,
                new Binding(nameof(CoreMenuItem.IsVisible))
                {
                    Source = source,
                    Converter = _boolToVisibilityConverter,
                    Mode = BindingMode.OneWay,
                });
            return separator;
        }

        var item = new System.Windows.Controls.MenuItem();

        item.SetBinding(
            HeaderedItemsControl.HeaderProperty,
            new Binding("SemanticMetadata.Text.Label")
            {
                Source = source,
                Mode = BindingMode.OneWay,
            });

        item.SetBinding(
            UIElement.IsEnabledProperty,
            new Binding(nameof(CoreMenuItem.IsEnabled))
            {
                Source = source,
                Mode = BindingMode.OneWay,
            });

        item.SetBinding(
            VisibilityProperty,
            new Binding(nameof(CoreMenuItem.IsVisible))
            {
                Source = source,
                Converter = _boolToVisibilityConverter,
                Mode = BindingMode.OneWay,
            });

        item.SetBinding(
            System.Windows.Controls.MenuItem.InputGestureTextProperty,
            new Binding(nameof(CoreMenuItem.Shortcut))
            {
                Source = source,
                Converter = _shortcutConverter,
                Mode = BindingMode.OneWay,
            });

        if (source.Kind is MenuItemKind.Command or MenuItemKind.Checkable)
        {
            item.SetBinding(
                System.Windows.Controls.MenuItem.CommandProperty,
                new Binding(nameof(CoreMenuItem.Command))
                {
                    Source = source,
                    Mode = BindingMode.OneWay,
                });
        }

        if (source.Kind == MenuItemKind.Checkable)
        {
            item.IsCheckable = true;
            item.SetBinding(
                System.Windows.Controls.MenuItem.IsCheckedProperty,
                new Binding(nameof(CoreMenuItem.IsChecked))
                {
                    Source = source,
                    Mode = BindingMode.OneWay,
                });
        }

        var icon = new Image();
        var iconBinding = new MultiBinding
        {
            Converter = _iconConverter,
            Mode = BindingMode.OneWay,
        };
        iconBinding.Bindings.Add(
            new Binding("SemanticMetadata.IconKey")
            {
                Source = source,
                Mode = BindingMode.OneWay,
            });
        iconBinding.Bindings.Add(
            new Binding(nameof(IconProvider))
            {
                Source = this,
                Mode = BindingMode.OneWay,
            });
        BindingOperations.SetBinding(icon, Image.SourceProperty, iconBinding);
        item.Icon = icon;

        if (source.Kind == MenuItemKind.Submenu)
        {
            foreach (var child in source.Children)
                item.Items.Add(CreateProjectedItem(child));
        }

        return item;
    }
}
