// TreeSelectionChangedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.SelectionChanged"/> routed event,
/// which fires when the primary (single-node) selection changes.
/// For multi-select gesture results see
/// <see cref="TreeMultiSelectionChangedEventArgs"/>.
/// </summary>
public sealed class TreeSelectionChangedEventArgs : RoutedEventArgs
{
    /// <summary>The node that was previously selected, or <c>null</c> if none.</summary>
    public TreeNodeModel? OldNode { get; }

    /// <summary>The node that is now selected, or <c>null</c> if the selection was cleared.</summary>
    public TreeNodeModel? NewNode { get; }

    public TreeSelectionChangedEventArgs(
        RoutedEvent routedEvent,
        TreeNodeModel? oldNode,
        TreeNodeModel? newNode) : base(routedEvent)
    {
        OldNode = oldNode;
        NewNode = newNode;
    }
}
