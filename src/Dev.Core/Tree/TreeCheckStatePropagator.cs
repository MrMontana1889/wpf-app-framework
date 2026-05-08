// TreeCheckStatePropagator.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace Dev.Core.Tree;

/// <summary>
/// Handles hierarchical tri-state checkbox propagation for tree nodes.
/// <para>
/// When a node's <see cref="TreeNodeModel.IsChecked"/> state changes,
/// this propagator recursively sets all descendants to match the toggled
/// node's state (parent → children cascade).
/// </para>
/// <para>
/// Upward re-derivation of ancestor folder state (children → parent bubble-up) is
/// handled by the visibility-aware layer (see
/// <c>SolutionStateTreeViewModel.RefreshFolderCheckedStates</c>), which only considers
/// visible children when computing tri-state, and is triggered automatically by each
/// individual child property-change notification.
/// </para>
/// <para>
/// Tri-state logic:
/// <list type="bullet">
///   <item><c>true</c> — All visible descendants are checked.</item>
///   <item><c>false</c> — All visible descendants are unchecked.</item>
///   <item><c>null</c> — Mixed state; some visible descendants checked, some unchecked.</item>
/// </list>
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public static class TreeCheckStatePropagator
{
    /// <summary>
    /// Propagates a checkbox state change from the given node downward
    /// to all descendants.
    /// </summary>
    /// <param name="node">The node whose checkbox was toggled.</param>
    /// <param name="rootNodes">
    /// Reserved parameter kept for API compatibility. Not used internally;
    /// upward ancestor re-derivation is handled by the visibility-aware layer.
    /// </param>
    public static void PropagateCheckState(TreeNodeModel node, IEnumerable<TreeNodeModel> rootNodes)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(rootNodes);

        // Propagate downward: set all descendants to match this node's state.
        // Upward re-derivation (parent tri-state from visible children) is handled
        // by SolutionStateTreeViewModel.RefreshFolderCheckedStates, which is visibility-aware.
        PropagateToDescendants(node);
    }

    // -----------------------------------------------------------------------
    // Downward propagation (parent → children)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Recursively sets all descendants of <paramref name="node"/> to match
    /// the node's <see cref="TreeNodeModel.IsChecked"/> value.
    /// </summary>
    private static void PropagateToDescendants(TreeNodeModel node)
    {
        var targetState = node.IsChecked;

        foreach (var child in node.Children)
        {
            child.IsChecked = targetState;
            PropagateToDescendants(child);
        }
    }
}
