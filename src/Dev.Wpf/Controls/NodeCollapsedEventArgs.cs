// NodeCollapsedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.NodeCollapsed"/> routed event,
/// which fires after a node transitions from expanded to collapsed.
/// Raised by <c>ExpandCollapseBehavior</c> after setting
/// <see cref="TreeNodeModel.IsExpanded"/> to <c>false</c>.
/// </summary>
public sealed class NodeCollapsedEventArgs : RoutedEventArgs
{
    /// <summary>The node that was collapsed.</summary>
    public TreeNodeModel Node { get; }

    public NodeCollapsedEventArgs(RoutedEvent routedEvent, TreeNodeModel node) : base(routedEvent)
    {
        Node = node;
    }
}
