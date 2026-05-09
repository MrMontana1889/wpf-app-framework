// ITreeContextMenuProvider.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Tree;

/// <summary>
/// Constructs the context menu item list for a set of selected tree nodes.
/// <para>
/// Assign an implementation to <c>TreeViewControl.ContextMenuProvider</c>
/// to supply per-application, multi-selection-aware context menus without
/// subclassing the control or placing logic in code-behind.
/// </para>
/// <para>
/// <c>ContextMenuBehavior</c> calls <see cref="BuildMenu"/> immediately before
/// the menu is shown, converts the result to WPF <c>MenuItem</c> elements
/// (resolving any <c>IconKey</c> values via <c>IIconProvider</c>), and
/// suppresses the menu entirely when the returned list is empty.
/// </para>
/// </summary>
public interface ITreeContextMenuProvider
{
    /// <summary>
    /// Builds the flat or hierarchical list of menu items to display for the
    /// given selection.
    /// </summary>
    /// <param name="selectedNodes">
    /// The nodes currently selected in the tree at the time of the right-click.
    /// Always contains at least one entry. Implementations should filter or
    /// disable items that are not meaningful for the specific selection.
    /// </param>
    /// <returns>
    /// The ordered list of <see cref="MenuItemModel"/> entries to render.
    /// Return an empty list to suppress the context menu entirely.
    /// </returns>
    IReadOnlyList<MenuItemModel> BuildMenu(IReadOnlyList<TreeNodeModel> selectedNodes);
}
