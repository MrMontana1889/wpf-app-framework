// NodeExpandedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.NodeExpanded"/> routed event,
/// which fires after a node transitions from collapsed to expanded.
/// Raised by <c>ExpandCollapseBehavior</c> after setting
/// <see cref="TreeNodeModel.IsExpanded"/> to <c>true</c> and, for lazy nodes,
/// after the <see cref="TreeNodeModel.LazyLoadCallback"/> has been invoked.
/// </summary>
public sealed class NodeExpandedEventArgs : RoutedEventArgs
{
    /// <summary>The node that was expanded.</summary>
    public TreeNodeModel Node { get; }

    public NodeExpandedEventArgs(RoutedEvent routedEvent, TreeNodeModel node) : base(routedEvent)
    {
        Node = node;
    }
}
