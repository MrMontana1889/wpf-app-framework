// TreeMultiSelectionChangedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.MultiSelectionChanged"/> routed
/// event, which fires after any multi-select gesture (Ctrl+Click, Shift+Click,
/// Ctrl+A, or right-click join) completes and the selection set has changed.
/// </summary>
public sealed class TreeMultiSelectionChangedEventArgs : RoutedEventArgs
{
    /// <summary>Nodes that were added to the selection by this gesture.</summary>
    public IReadOnlyList<TreeNodeModel> AddedNodes { get; }

    /// <summary>Nodes that were removed from the selection by this gesture.</summary>
    public IReadOnlyList<TreeNodeModel> RemovedNodes { get; }

    public TreeMultiSelectionChangedEventArgs(
        RoutedEvent routedEvent,
        IReadOnlyList<TreeNodeModel> addedNodes,
        IReadOnlyList<TreeNodeModel> removedNodes) : base(routedEvent)
    {
        AddedNodes = addedNodes;
        RemovedNodes = removedNodes;
    }
}
