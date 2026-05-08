// NodeCheckedChangedEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.NodeCheckedChanged"/> routed
/// event, which fires after <c>CheckboxBehavior</c> has toggled a node's
/// <see cref="TreeNodeModel.IsChecked"/> value and the Dev.Core tri-state
/// propagation rules have been applied to the affected ancestors and descendants.
/// </summary>
public sealed class NodeCheckedChangedEventArgs : RoutedEventArgs
{
    /// <summary>The node whose checkbox state changed.</summary>
    public TreeNodeModel Node { get; }

    /// <summary>The checkbox state before the change.</summary>
    public bool? OldValue { get; }

    /// <summary>The checkbox state after the change and any tri-state propagation.</summary>
    public bool? NewValue { get; }

    public NodeCheckedChangedEventArgs(
        RoutedEvent routedEvent,
        TreeNodeModel node,
        bool? oldValue,
        bool? newValue) : base(routedEvent)
    {
        Node = node;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
