// MultiSelectBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Handles all node selection gestures for <see cref="TreeViewControl"/>.
/// <list type="bullet">
///   <item>Regular left-click selects the clicked node and clears any prior selection.</item>
///   <item>Ctrl+Click toggles the clicked node without disturbing other selections
///         (<see cref="TreeSelectionMode.Multiple"/> only).</item>
///   <item>Shift+Click selects a contiguous range from the last anchor node to the
///         clicked node using <see cref="TreeNodeModel.SelectionIndex"/> ordering
///         (<see cref="TreeSelectionMode.Multiple"/> only).
///         Full range logic is delegated to the Dev.Core selection rule engine
///         and stubbed here until Phase D.</item>
///   <item>Right-click joins the selection non-destructively when the target node
///         is not already selected.</item>
/// </list>
/// Clicks on <c>PART_ExpandCollapseGlyph</c> and <c>PART_CheckBox</c> are
/// silently ignored so those behaviors handle them independently.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MultiSelectBehavior : TreeViewBehaviorBase
{
    private TreeNodeModel? _anchorNode;
    private TreeNodeModel? _pendingSingleSelectNode;

    private readonly MouseButtonEventHandler _onMouseLeftDown;
    private readonly MouseButtonEventHandler _onMouseLeftUp;
    private readonly MouseButtonEventHandler _onMouseRightDown;
    private readonly MouseEventHandler       _onMouseMove;

    internal MultiSelectBehavior()
    {
        _onMouseLeftDown  = OnMouseLeftButtonDown;
        _onMouseLeftUp    = OnMouseLeftButtonUp;
        _onMouseRightDown = OnMouseRightButtonDown;
        _onMouseMove      = OnMouseMove;
    }

    protected override void OnAttached()
    {
        Control!.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent,  _onMouseLeftDown);
        Control!.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent,    _onMouseLeftUp);
        Control!.AddHandler(UIElement.PreviewMouseRightButtonDownEvent, _onMouseRightDown);
        Control!.AddHandler(UIElement.MouseMoveEvent,                   _onMouseMove);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent,  _onMouseLeftDown);
        Control!.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent,    _onMouseLeftUp);
        Control!.RemoveHandler(UIElement.PreviewMouseRightButtonDownEvent, _onMouseRightDown);
        Control!.RemoveHandler(UIElement.MouseMoveEvent,                   _onMouseMove);
    }

    // -----------------------------------------------------------------------
    // Left-click — single / Ctrl / Shift
    // -----------------------------------------------------------------------

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var container = FindContainer(e.OriginalSource as DependencyObject);
        if (container?.DataContext is not TreeNodeModel node) return;
        if (!node.IsSelectable) return;
        if (IsOnNonSelectPart(e.OriginalSource as DependencyObject, container)) return;

        var mode = Control!.SelectionMode;
        if (mode == TreeSelectionMode.None) return;

        if (mode == TreeSelectionMode.Multiple && IsCtrlDown())
        {
            ToggleNode(node);
        }
        else if (mode == TreeSelectionMode.Multiple && IsShiftDown() && _anchorNode is not null)
        {
            CommitRangeSelection(node);
        }
        else
        {
            // Regular click: Check if clicking on an already-selected node
            // with multiple nodes selected. If so, defer selection change
            // to allow drag-and-drop of multiple nodes.
            var current = Control.SelectedNodes;
            if (node.IsSelected && current.Count > 1)
            {
                // Defer selection change until mouse-up
                _pendingSingleSelectNode = node;
            }
            else
            {
                // Immediate selection change
                CommitSingleSelection(node);
            }
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // If we have a pending single-select (clicked on already-selected node
        // with multi-selection active), and the mouse was released without
        // starting a drag, apply the single-selection now.
        if (_pendingSingleSelectNode is not null)
        {
            CommitSingleSelection(_pendingSingleSelectNode);
            _pendingSingleSelectNode = null;
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        // If mouse moves while we have a pending selection change, cancel it
        // (user is likely starting a drag)
        if (_pendingSingleSelectNode is not null && e.LeftButton == MouseButtonState.Pressed)
        {
            _pendingSingleSelectNode = null;
        }
    }

    // -----------------------------------------------------------------------
    // Right-click — non-destructive join
    // -----------------------------------------------------------------------

    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var container = FindContainer(e.OriginalSource as DependencyObject);
        if (container?.DataContext is not TreeNodeModel node) return;
        if (!node.IsSelectable) return;

        // Only adjust selection when the right-clicked node is not already selected.
        if (!node.IsSelected)
            CommitSingleSelection(node);
    }

    // -----------------------------------------------------------------------
    // Selection helpers
    // -----------------------------------------------------------------------

    private void CommitSingleSelection(TreeNodeModel node)
    {
        // Guard: node is already the sole selection — no work needed.
        var current = Control!.SelectedNodes;
        if (current.Count == 1 && current[0] == node) return;

        var old = current.FirstOrDefault();
        foreach (var n in current.ToArray())
            n.IsSelected = false;

        node.IsSelected = true;
        _anchorNode = node;
        Control.SetSelectedNodes([node]);
        Control.RaiseSelectionChanged(old, node);

        // Set keyboard focus to enable keyboard navigation
        SetFocusOnNode(node);
    }

    private void ToggleNode(TreeNodeModel node)
    {
        node.IsSelected = !node.IsSelected;

        var current = Control!.SelectedNodes;
        TreeNodeModel[] updated = node.IsSelected
            ? [.. current, node]
            : [.. current.Where(n => n != node)];

        if (node.IsSelected)
            _anchorNode = node;

        Control.SetSelectedNodes(updated);
        Control.RaiseMultiSelectionChanged(
            node.IsSelected ? [node] : [],
            node.IsSelected ? [] : [node]);

        // Set keyboard focus when toggling selection on
        if (node.IsSelected)
            SetFocusOnNode(node);
    }

    private void CommitRangeSelection(TreeNodeModel node)
    {
        // TODO: Implement via Dev.Core selection rule engine using
        // TreeNodeModel.SelectionIndex to determine the contiguous range
        // between _anchorNode and node. Falling back to single selection
        // until Phase D.
        CommitSingleSelection(node);
    }

    /// <summary>
    /// Finds and focuses the TreeNodeContainer for the given node.
    /// This enables keyboard navigation after mouse-based selection.
    /// </summary>
    private void SetFocusOnNode(TreeNodeModel node)
    {
        // Find the container in the visual tree
        var container = FindContainerForNode(Control, node);
        container?.Focus();
    }

    /// <summary>
    /// Recursively searches the visual tree for a TreeNodeContainer whose
    /// DataContext matches the target node.
    /// </summary>
    private static TreeNodeContainer? FindContainerForNode(DependencyObject? root, TreeNodeModel targetNode)
    {
        if (root is null)
            return null;

        // Check if this element is a matching container
        if (root is TreeNodeContainer container && container.DataContext == targetNode)
            return container;

        // Recursively search children
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var result = FindContainerForNode(child, targetNode);
            if (result is not null)
                return result;
        }

        return null;
    }

    // -----------------------------------------------------------------------
    // Keyboard modifier helpers
    // -----------------------------------------------------------------------

    private static bool IsCtrlDown() =>
        Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

    private static bool IsShiftDown() =>
        Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
}
