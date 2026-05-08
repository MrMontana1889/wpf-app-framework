// KeyboardNavigationBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Handles keyboard navigation and node activation for <see cref="TreeViewControl"/>.
/// <list type="bullet">
///   <item>
///     <kbd>Enter</kbd> activates the keyboard-focused node by raising
///     <see cref="TreeViewControl.RaiseNodeActivated"/>.
///   </item>
///   <item>
///     <kbd>Up</kbd> / <kbd>Down</kbd> move selection to the previous or next
///     visible node in tree order. Expansion state is not modified.
///   </item>
///   <item>
///     <kbd>Right Arrow</kbd> expands the focused node when it is expandable
///     and currently collapsed. When already expanded, moves selection to the
///     first child.
///   </item>
///   <item>
///     <kbd>Left Arrow</kbd> collapses the focused node when it is currently
///     expanded. When already collapsed, moves selection to the parent node.
///   </item>
///   <item>
///     <kbd>Ctrl+A</kbd> selects all selectable nodes when
///     <see cref="TreeSelectionMode.Multiple"/> is active.
///     Full implementation is stubbed until Phase D.
///   </item>
/// </list>
/// The focused node is resolved by walking the visual tree from
/// <see cref="Keyboard.FocusedElement"/> to find the nearest
/// <see cref="TreeNodeContainer"/> ancestor.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class KeyboardNavigationBehavior : TreeViewBehaviorBase
{
    private readonly KeyEventHandler _onPreviewKeyDown;

    internal KeyboardNavigationBehavior()
    {
        _onPreviewKeyDown = OnPreviewKeyDown;
    }

    protected override void OnAttached()
    {
        Control!.AddHandler(UIElement.PreviewKeyDownEvent, _onPreviewKeyDown);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(UIElement.PreviewKeyDownEvent, _onPreviewKeyDown);
    }

    // -----------------------------------------------------------------------
    // Key dispatch
    // -----------------------------------------------------------------------

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var container = FindContainer(Keyboard.FocusedElement as DependencyObject);
        if (container?.DataContext is not TreeNodeModel node) return;

        switch (e.Key)
        {
            case Key.Enter:
                Control!.RaiseNodeActivated(node);
                e.Handled = true;
                break;

            case Key.Up:
                HandleUpArrow(node);
                e.Handled = true;
                break;

            case Key.Down:
                HandleDownArrow(node);
                e.Handled = true;
                break;

            case Key.Right:
                HandleRightArrow(node);
                e.Handled = true;
                break;

            case Key.Left:
                HandleLeftArrow(node);
                e.Handled = true;
                break;

            case Key.A when IsCtrlDown():
                // TODO (Phase D): select all nodes via Dev.Core selection engine.
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Arrow key handlers
    // -----------------------------------------------------------------------

    private void HandleUpArrow(TreeNodeModel currentNode)
    {
        var visibleNodes = GetVisibleNodesInTreeOrder();
        var currentIndex = visibleNodes.IndexOf(currentNode);

        if (currentIndex > 0)
        {
            var previousNode = visibleNodes[currentIndex - 1];
            SelectSingleNode(previousNode);
        }
    }

    private void HandleDownArrow(TreeNodeModel currentNode)
    {
        var visibleNodes = GetVisibleNodesInTreeOrder();
        var currentIndex = visibleNodes.IndexOf(currentNode);

        if (currentIndex >= 0 && currentIndex < visibleNodes.Count - 1)
        {
            var nextNode = visibleNodes[currentIndex + 1];
            SelectSingleNode(nextNode);
        }
    }

    private void HandleRightArrow(TreeNodeModel node)
    {
        // If collapsed and expandable, expand it
        if (!node.IsExpanded && node.IsExpandable)
        {
            node.IsExpanded = true;
            Control!.RaiseNodeExpanded(node);
        }
        // If already expanded and has children, select first child
        else if (node.IsExpanded && node.Children.Count > 0)
        {
            var firstChild = node.Children[0];
            SelectSingleNode(firstChild);
        }
    }

    private void HandleLeftArrow(TreeNodeModel node)
    {
        // If expanded, collapse it
        if (node.IsExpanded)
        {
            node.IsExpanded = false;
            Control!.RaiseNodeCollapsed(node);
        }
        // If collapsed, move to parent
        else
        {
            var parent = FindParentNode(node);
            if (parent is not null)
            {
                SelectSingleNode(parent);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Tree traversal helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns all visible nodes in tree order (depth-first pre-order traversal).
    /// Only includes nodes that are currently visible based on expansion state.
    /// </summary>
    private List<TreeNodeModel> GetVisibleNodesInTreeOrder()
    {
        var result = new List<TreeNodeModel>();

        if (Control?.ItemsSource is not IEnumerable<TreeNodeModel> rootNodes)
            return result;

        foreach (var root in rootNodes)
        {
            CollectVisibleNodes(root, result);
        }

        return result;
    }

    /// <summary>
    /// Recursively collects visible nodes in depth-first pre-order.
    /// </summary>
    private static void CollectVisibleNodes(TreeNodeModel node, List<TreeNodeModel> result)
    {
        result.Add(node);

        if (node.IsExpanded && node.Children.Count > 0)
        {
            foreach (var child in node.Children)
            {
                CollectVisibleNodes(child, result);
            }
        }
    }

    /// <summary>
    /// Finds the parent of the given node by searching the tree structure.
    /// Returns null if the node is a root node or parent cannot be found.
    /// </summary>
    private TreeNodeModel? FindParentNode(TreeNodeModel targetNode)
    {
        if (Control?.ItemsSource is not IEnumerable<TreeNodeModel> rootNodes)
            return null;

        foreach (var root in rootNodes)
        {
            var parent = FindParentNodeRecursive(root, targetNode);
            if (parent is not null)
                return parent;
        }

        return null;
    }

    /// <summary>
    /// Recursively searches for the parent of targetNode starting from currentNode.
    /// </summary>
    private static TreeNodeModel? FindParentNodeRecursive(TreeNodeModel currentNode, TreeNodeModel targetNode)
    {
        // Check if targetNode is a direct child of currentNode
        if (currentNode.Children.Contains(targetNode))
            return currentNode;

        // Recursively search in children
        foreach (var child in currentNode.Children)
        {
            var parent = FindParentNodeRecursive(child, targetNode);
            if (parent is not null)
                return parent;
        }

        return null;
    }

    // -----------------------------------------------------------------------
    // Selection helper
    // -----------------------------------------------------------------------

    /// <summary>
    /// Selects a single node, clearing any previous selection.
    /// This mimics the behavior of a regular click selection.
    /// </summary>
    private void SelectSingleNode(TreeNodeModel node)
    {
        if (!node.IsSelectable)
            return;

        // Clear all selections first
        var visibleNodes = GetVisibleNodesInTreeOrder();
        foreach (var n in visibleNodes)
        {
            if (n.IsSelected)
                n.IsSelected = false;
        }

        // Select the target node
        node.IsSelected = true;

        // Update the control's selected nodes collection
        Control!.SetSelectedNodes(new[] { node });
        Control!.RaiseSelectionChanged(null, node);

        // Set keyboard focus to the container for this node
        FocusContainer(node);
    }

    /// <summary>
    /// Attempts to find and focus the TreeNodeContainer for the given node.
    /// Uses the visual tree to locate the container since nodes can be nested
    /// at any depth in recursive TreeChildItemsControl instances.
    /// </summary>
    private void FocusContainer(TreeNodeModel node)
    {
        // Force layout update to ensure containers are generated
        Control?.UpdateLayout();

        // Search the visual tree for a container with matching DataContext
        var container = FindContainerForNode(Control, node);
        if (container is not null)
        {
            container.Focus();
        }
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
        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var result = FindContainerForNode(child, targetNode);
            if (result is not null)
                return result;
        }

        return null;
    }

    private static bool IsCtrlDown() =>
        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
}
