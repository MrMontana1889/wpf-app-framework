// TreeDropValidator.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace Dev.Core.Tree;

/// <summary>
/// Validates drag-and-drop operations for tree nodes.
/// <para>
/// This validator implements basic structural rules to prevent invalid
/// tree mutations:
/// <list type="bullet">
///   <item>Cannot drop onto a non-selectable (read-only) node.</item>
///   <item>Cannot drop a node onto itself.</item>
///   <item>Cannot drop a node onto its own descendant (creates cycle).</item>
///   <item>Can drop onto any other selectable node or the root level.</item>
/// </list>
/// </para>
/// <para>
/// Application-specific drop rules (e.g., project inclusion semantics)
/// are implemented in higher layers, not in this generic tree validator.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public static class TreeDropValidator
{
    /// <summary>
    /// Determines whether the given nodes can be dropped onto the target node.
    /// </summary>
    /// <param name="droppedNodes">The nodes being dragged.</param>
    /// <param name="targetNode">
    /// The target node where the nodes would be dropped.
    /// <c>null</c> indicates dropping onto the root level.
    /// </param>
    /// <returns>
    /// <c>true</c> if the drop is structurally valid; <c>false</c> otherwise.
    /// </returns>
    public static bool CanDrop(IEnumerable<TreeNodeModel> droppedNodes, TreeNodeModel? targetNode)
    {
        ArgumentNullException.ThrowIfNull(droppedNodes);

        // Allow drop onto root (targetNode == null)
        if (targetNode is null)
            return true;

        // Cannot drop onto a non-selectable (read-only) node
        if (!targetNode.IsSelectable)
            return false;

        foreach (var node in droppedNodes)
        {
            // Cannot drop a node onto itself
            if (node == targetNode)
                return false;

            // Cannot drop a node onto its own descendant (creates cycle)
            if (IsDescendantOf(targetNode, node))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether <paramref name="potentialDescendant"/> is a
    /// descendant of <paramref name="ancestor"/>.
    /// </summary>
    private static bool IsDescendantOf(TreeNodeModel potentialDescendant, TreeNodeModel ancestor)
    {
        foreach (var child in ancestor.Children)
        {
            if (child == potentialDescendant)
                return true;

            if (IsDescendantOf(potentialDescendant, child))
                return true;
        }

        return false;
    }
}
