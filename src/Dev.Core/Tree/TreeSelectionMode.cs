// TreeSelectionMode.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Tree;

/// <summary>
/// Controls how a <c>TreeViewControl</c> handles node selection interactions.
/// The selection rule engine in Dev.Core uses this value to validate each
/// selection gesture before updating <see cref="TreeNodeModel.IsSelected"/>.
/// </summary>
public enum TreeSelectionMode
{
    /// <summary>No selection is permitted. Clicks do not alter node state.</summary>
    None,

    /// <summary>
    /// At most one node may be selected at a time. Clicking a node replaces
    /// the current selection; clicking the same node again deselects it.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple nodes may be selected simultaneously.
    /// <list type="bullet">
    ///   <item>Ctrl+Click toggles the clicked node without affecting others.</item>
    ///   <item>Shift+Click selects a contiguous range defined by
    ///         <see cref="TreeNodeModel.SelectionIndex"/> ordering.</item>
    ///   <item>Right-clicking a node that is not part of the active selection
    ///         joins it to the selection without clearing other selected nodes.</item>
    /// </list>
    /// </summary>
    Multiple,
}
