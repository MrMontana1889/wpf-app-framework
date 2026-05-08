// NodesDroppedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.NodesDropped"/> routed event,
/// which fires after <c>DragDropBehavior</c> completes a successful drop operation
/// and the Dev.Core rule engine has approved the move.
/// </summary>
public sealed class NodesDroppedEventArgs : RoutedEventArgs
{
    /// <summary>The nodes that were dragged and dropped.</summary>
    public IReadOnlyList<TreeNodeModel> DroppedNodes { get; }

    /// <summary>
    /// The node onto which the dragged nodes were dropped, or <c>null</c>
    /// when dropped onto the tree root.
    /// </summary>
    public TreeNodeModel? TargetNode { get; }

    /// <summary>
    /// The zero-based insertion index within <see cref="TargetNode"/>'s
    /// <c>Children</c> collection (or the root) where the nodes should be placed.
    /// </summary>
    public int InsertionIndex { get; }

    public NodesDroppedEventArgs(
        RoutedEvent routedEvent,
        IReadOnlyList<TreeNodeModel> droppedNodes,
        TreeNodeModel? targetNode,
        int insertionIndex) : base(routedEvent)
    {
        DroppedNodes = droppedNodes;
        TargetNode = targetNode;
        InsertionIndex = insertionIndex;
    }
}
