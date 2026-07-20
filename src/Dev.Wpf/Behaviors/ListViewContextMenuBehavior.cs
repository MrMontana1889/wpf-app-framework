// ListViewContextMenuBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfMenuItem = System.Windows.Controls.MenuItem;
using SemanticMenuItem = Dev.Core.Menu.MenuItem;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Renders a <see cref="ContextMenu"/> for a <see cref="ListView"/> from
/// semantic <see cref="MenuItem"/> definitions.
/// <para>
/// The behavior listens to <see cref="UIElement.MouseRightButtonUpEvent"/> (bubble)
/// to display the context menu after selection has been adjusted.
/// </para>
/// <para>
/// The menu is suppressed when:
/// <list type="bullet">
///   <item><see cref="ContextMenuItems"/> is <c>null</c>.</item>
///   <item><see cref="ContextMenuItems"/> is empty.</item>
///   <item>All items are invisible (<see cref="MenuItem.IsVisible"/> = <c>false</c>).</item>
///   <item>No WPF menu elements are produced after filtering.</item>
/// </list>
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ListViewContextMenuBehavior : Behavior<ListView>
{
    private MouseButtonEventHandler? _onMouseRightButtonUp;

    // -----------------------------------------------------------------------
    // ContextMenuItems DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty ContextMenuItemsProperty =
        DependencyProperty.Register(
            nameof(ContextMenuItems),
            typeof(IReadOnlyList<SemanticMenuItem>),
            typeof(ListViewContextMenuBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// The semantic menu item definitions to render as a WPF context menu.
    /// </summary>
    public IReadOnlyList<SemanticMenuItem>? ContextMenuItems
    {
        get => (IReadOnlyList<SemanticMenuItem>?)GetValue(ContextMenuItemsProperty);
        set => SetValue(ContextMenuItemsProperty, value);
    }

    protected override void OnAttached()
    {
        _onMouseRightButtonUp = OnMouseRightButtonUp;
        AssociatedObject.AddHandler(UIElement.MouseRightButtonUpEvent, _onMouseRightButtonUp);
    }

    protected override void OnDetaching()
    {
        if (_onMouseRightButtonUp is not null)
        {
            AssociatedObject.RemoveHandler(UIElement.MouseRightButtonUpEvent, _onMouseRightButtonUp);
        }
    }

    // -----------------------------------------------------------------------
    // Context menu
    // -----------------------------------------------------------------------

    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var items = ContextMenuItems;
        if (items is null || items.Count == 0) return;

        // Convert semantic menu items to WPF elements and suppress menu if none produce visible items
        var wpfItems = BuildWpfMenuItems(items);
        if (wpfItems.Count == 0) return;

        var menu = new ContextMenu
        {
            PlacementTarget = AssociatedObject,
            Placement       = PlacementMode.MousePoint,
        };

        foreach (var item in wpfItems)
            menu.Items.Add(item);

        menu.IsOpen = true;
        e.Handled   = true;
    }

    // -----------------------------------------------------------------------
    // WPF MenuItem construction from semantic MenuItem tree
    // -----------------------------------------------------------------------

    private IReadOnlyList<Control> BuildWpfMenuItems(IReadOnlyList<SemanticMenuItem> items)
    {
        var result = new List<Control>(items.Count);

        foreach (var item in items)
        {
            if (!item.IsVisible)
                continue;

            switch (item.Kind)
            {
                case MenuItemKind.Separator:
                    result.Add(new Separator());
                    break;

                case MenuItemKind.Command:
                    var cmdItem = BuildCommandMenuItem(item);
                    if (cmdItem is not null)
                        result.Add(cmdItem);
                    break;

                case MenuItemKind.Checkable:
                    var checkItem = BuildCheckableMenuItem(item);
                    if (checkItem is not null)
                        result.Add(checkItem);
                    break;

                case MenuItemKind.Submenu:
                    var subItem = BuildSubmenuMenuItem(item);
                    if (subItem is not null)
                        result.Add(subItem);
                    break;
            }
        }

        return result;
    }

    private WpfMenuItem? BuildCommandMenuItem(SemanticMenuItem item)
    {
        if (item.Command is null)
            return null;

        var commandParameter =
            item.CommandParameterProvider?.Invoke()
            ?? item.CommandParameter;

        var wpfItem = new WpfMenuItem
        {
            Header           = item.SemanticMetadata.Text.Label,
            Command          = item.Command,
            CommandParameter = commandParameter,
            IsEnabled        = item.IsEnabled,
        };

        return wpfItem;
    }

    private WpfMenuItem? BuildCheckableMenuItem(SemanticMenuItem item)
    {
        var wpfItem = new WpfMenuItem
        {
            Header      = item.SemanticMetadata.Text.Label,
            IsCheckable = true,
            IsChecked   = item.IsChecked ?? false,
            IsEnabled   = item.IsEnabled,
        };

        return wpfItem;
    }

    private WpfMenuItem? BuildSubmenuMenuItem(SemanticMenuItem item)
    {
        if (item.Children.Count == 0)
            return null;

        var wpfItem = new WpfMenuItem
        {
            Header    = item.SemanticMetadata.Text.Label,
            IsEnabled = item.IsEnabled,
        };

        var childItems = BuildWpfMenuItems(item.Children);
        foreach (var child in childItems)
            wpfItem.Items.Add(child);

        // If all children were filtered out, don't add empty submenu
        if (wpfItem.Items.Count == 0)
            return null;

        return wpfItem;
    }
}
