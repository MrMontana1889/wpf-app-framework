// ContextMenuBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Builds and displays the context menu for a <see cref="TreeViewControl"/>
/// selection.
/// <para>
/// The behavior subscribes to <see cref="UIElement.MouseRightButtonUpEvent"/>
/// (bubble), which fires after <c>MultiSelectBehavior</c> has already adjusted
/// the selection on <see cref="UIElement.PreviewMouseRightButtonDownEvent"/>
/// (tunnel), ensuring <see cref="TreeViewControl.SelectedNodes"/> is current
/// when the menu is built.
/// </para>
/// <para>
/// The menu is suppressed when:
/// <list type="bullet">
///   <item><see cref="TreeViewControl.ContextMenuProvider"/> is <c>null</c>.</item>
///   <item>All nodes in the selection have <see cref="TreeNodeModel.SupportsContextMenu"/> = <c>false</c>.</item>
///   <item>A <see cref="TreeViewControl.ContextMenuOpening"/> handler sets
///         <see cref="ContextMenuOpeningEventArgs.Cancel"/> to <c>true</c>.</item>
///   <item><see cref="ITreeContextMenuProvider.BuildMenu"/> returns an empty list.</item>
/// </list>
/// </para>
/// <para>
/// Icon resolution from <see cref="MenuItemModel.IconKey"/> via
/// <see cref="TreeViewControl.IconProvider"/> is stubbed until Phase D.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class ContextMenuBehavior : TreeViewBehaviorBase
{
    private readonly MouseButtonEventHandler _onMouseRightButtonUp;

    internal ContextMenuBehavior()
    {
        _onMouseRightButtonUp = OnMouseRightButtonUp;
    }

    protected override void OnAttached()
    {
        Control!.AddHandler(UIElement.MouseRightButtonUpEvent, _onMouseRightButtonUp);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(UIElement.MouseRightButtonUpEvent, _onMouseRightButtonUp);
    }

    // -----------------------------------------------------------------------
    // Context menu
    // -----------------------------------------------------------------------

    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var provider = Control!.ContextMenuProvider;
        if (provider is null) return;

        var selected = Control.SelectedNodes;
        if (selected.Count == 0) return;

        // Suppress if every selected node has opted out of context menus.
        if (!selected.Any(n => n.SupportsContextMenu)) return;

        // Allow subscribers to cancel the menu before it is built.
        if (!Control.RaiseContextMenuOpening(selected)) return;

        var items = provider.BuildMenu(selected);
        if (items.Count == 0) return;

        var menu = new ContextMenu
        {
            PlacementTarget = Control,
            Placement       = PlacementMode.MousePoint,
        };

        foreach (var item in BuildWpfMenuItems(items))
            menu.Items.Add(item);

        menu.IsOpen = true;
        e.Handled   = true;
    }

    // -----------------------------------------------------------------------
    // WPF MenuItem construction from MenuItemModel tree
    // -----------------------------------------------------------------------

    private IReadOnlyList<MenuItem> BuildWpfMenuItems(IReadOnlyList<MenuItemModel> models)
    {
        var result = new List<MenuItem>(models.Count);
        foreach (var model in models)
            result.Add(BuildWpfMenuItem(model));
        return result;
    }

    private MenuItem BuildWpfMenuItem(MenuItemModel model)
    {
        var item = new MenuItem
        {
            Header           = model.Label,
            Command          = model.Command,
            CommandParameter = model.CommandParameter,
            IsEnabled        = model.IsEnabled,
        };

        // TODO (Phase D): resolve model.IconKey via Control.IconProvider and assign item.Icon.

        if (model.Children is { Count: > 0 } children)
        {
            foreach (var child in children)
                item.Items.Add(BuildWpfMenuItem(child));
        }

        return item;
    }
}
