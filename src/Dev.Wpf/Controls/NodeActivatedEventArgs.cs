// NodeActivatedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.NodeActivated"/> routed event,
/// which fires when a node is activated by double-clicking it or by pressing
/// Enter while it is the focused node.
/// </summary>
public sealed class NodeActivatedEventArgs : RoutedEventArgs
{
    /// <summary>The node that was activated.</summary>
    public TreeNodeModel Node { get; }

    public NodeActivatedEventArgs(RoutedEvent routedEvent, TreeNodeModel node) : base(routedEvent)
    {
        Node = node;
    }
}
