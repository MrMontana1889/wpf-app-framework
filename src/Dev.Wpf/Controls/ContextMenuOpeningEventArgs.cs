// ContextMenuOpeningEventArgs.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="TreeViewControl.ContextMenuOpening"/> routed
/// event, raised by <c>ContextMenuBehavior</c> before the context menu is built
/// and displayed.
/// <para>
/// Set <see cref="Cancel"/> to <c>true</c> to suppress the context menu entirely
/// for this invocation without affecting future right-clicks.
/// </para>
/// </summary>
public sealed class ContextMenuOpeningEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The nodes that are selected at the time of the right-click. Passed
    /// directly to <see cref="ITreeContextMenuProvider.BuildMenu"/> unless the
    /// event is cancelled.
    /// </summary>
    public IReadOnlyList<TreeNodeModel> SelectedNodes { get; }

    /// <summary>
    /// Set to <c>true</c> to suppress context menu display for this invocation.
    /// The <see cref="ITreeContextMenuProvider"/> will not be called and no
    /// menu will appear.
    /// </summary>
    public bool Cancel { get; set; }

    public ContextMenuOpeningEventArgs(
        RoutedEvent routedEvent,
        IReadOnlyList<TreeNodeModel> selectedNodes) : base(routedEvent)
    {
        SelectedNodes = selectedNodes;
    }
}
