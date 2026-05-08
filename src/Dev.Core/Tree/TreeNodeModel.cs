// TreeNodeModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dev.Core.Tree;

/// <summary>
/// Represents a single node in a tree. Carries display data, structural state,
/// selection metadata, search metadata, and lazy-load plumbing.
/// <para>
/// All mutable state properties implement <see cref="System.ComponentModel.INotifyPropertyChanged"/>
/// so WPF bindings react without any code-behind in the view layer. The immutable
/// structural properties (<see cref="Id"/>, <see cref="IsExpandable"/>,
/// <see cref="IsSelectable"/>, <see cref="SupportsContextMenu"/>) are fixed
/// at construction time and never raise change notifications.
/// </para>
/// <para>
/// Dev.Core never references any WPF type. The <see cref="IconKey"/> is an opaque
/// string token resolved to an <c>ImageSource</c> only inside Dev.Wpf at binding time.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public partial class TreeNodeModel : ObservableObject
{
    /// <param name="id">Stable identifier used for diffing, persistence, and equality.</param>
    /// <param name="label">Primary display text shown in the tree node.</param>
    /// <param name="isExpandable">
    /// Whether the node can be expanded. Set <c>true</c> for nodes with children
    /// or a <see cref="LazyLoadCallback"/>; <c>false</c> for leaf nodes.
    /// </param>
    /// <param name="iconKey">
    /// Optional icon token from <c>Dev.Core.Tree.IconKeys</c> or a custom string.
    /// Resolved to an <c>ImageSource</c> by <c>IIconProvider</c> in Dev.Wpf.
    /// </param>
    public TreeNodeModel(string id, string label, bool isExpandable = false, string? iconKey = null)
    {
        Id = id;
        this.label = label;
        IsExpandable = isExpandable;
        this.iconKey = iconKey;
    }

    // -----------------------------------------------------------------------
    // Structural properties (immutable after construction)
    // -----------------------------------------------------------------------

    /// <summary>Stable identifier used for diffing, persistence, and tree equality checks.</summary>
    public string Id { get; }

    /// <summary>
    /// Whether the node supports expansion. Nodes with a <see cref="LazyLoadCallback"/>
    /// must set this to <c>true</c> even before children are loaded.
    /// </summary>
    public bool IsExpandable { get; }

    /// <summary>
    /// When <c>false</c> the node cannot be selected regardless of the control's
    /// <c>SelectionMode</c>. Checked by the Dev.Core selection rule engine.
    /// </summary>
    public bool IsSelectable { get; init; } = true;

    /// <summary>
    /// When <c>false</c> the context menu is suppressed for this node.
    /// If all nodes in the active selection set this to <c>false</c>,
    /// <c>ContextMenuBehavior</c> will not open the menu at all.
    /// </summary>
    public bool SupportsContextMenu { get; init; } = true;

    /// <summary>
    /// When <c>false</c> the checkbox is disabled or hidden for this node.
    /// Typically set to <c>false</c> for root-level nodes in scenarios where
    /// only child nodes should be checkable.
    /// </summary>
    public bool IsCheckable { get; init; } = true;

    // -----------------------------------------------------------------------
    // Display (observable — bindings react without code-behind)
    // -----------------------------------------------------------------------

    /// <summary>Primary display text shown in the tree node label.</summary>
    [ObservableProperty]
    private string label;

    /// <summary>
    /// Opaque icon token. Dev.Wpf resolves this to an <c>ImageSource</c> via
    /// <c>IIconProvider</c> at binding time; Dev.Core never references a WPF type.
    /// Use constants from <c>Dev.Core.Tree.IconKeys</c> for well-known icon names.
    /// </summary>
    [ObservableProperty]
    private string? iconKey;

    // -----------------------------------------------------------------------
    // Expansion and lazy-load state (observable)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Whether the node is currently expanded. Setting this to <c>true</c> on a
    /// lazy node triggers <see cref="LazyLoadCallback"/> via <c>ExpandCollapseBehavior</c>.
    /// </summary>
    [ObservableProperty]
    private bool isExpanded;

    /// <summary>
    /// <c>true</c> once the <see cref="LazyLoadCallback"/> has been successfully
    /// invoked and the <see cref="Children"/> collection populated. Dev.Wpf sets
    /// this after resolving the callback; Dev.Core owns the state contract.
    /// </summary>
    [ObservableProperty]
    private bool isLazyLoaded;

    /// <summary>
    /// Async callback invoked on first expansion for lazy-loaded nodes.
    /// <c>null</c> for eager nodes whose <see cref="Children"/> are pre-populated
    /// at construction time. Once resolved the callback should be cleared and
    /// <see cref="IsLazyLoaded"/> set to <c>true</c>.
    /// </summary>
    public Func<Task<IReadOnlyList<TreeNodeModel>>>? LazyLoadCallback { get; set; }

    /// <summary>
    /// Observable collection of immediate child nodes. For lazy nodes this
    /// collection starts empty and is populated when <see cref="LazyLoadCallback"/>
    /// resolves. WPF binds to this directly for recursive child rendering.
    /// </summary>
    public ObservableCollection<TreeNodeModel> Children { get; } = new();

    // -----------------------------------------------------------------------
    // Selection state (observable)
    // -----------------------------------------------------------------------

    /// <summary>Whether this node is part of the current selection set.</summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Internal ordering index used by the multi-select range engine (Shift+Click)
    /// to determine contiguous selection boundaries. Assigned by Dev.Core selection
    /// rule evaluation during tree construction or node insertion. Not intended for
    /// direct assignment by consumers.
    /// </summary>
    [ObservableProperty]
    private int selectionIndex;

    // -----------------------------------------------------------------------
    // Checkbox state (observable)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Optional tri-state checkbox value.
    /// <list type="bullet">
    ///   <item><c>true</c> — checked.</item>
    ///   <item><c>false</c> — unchecked.</item>
    ///   <item><c>null</c> — indeterminate; produced by partial child selection
    ///         during hierarchical tri-state propagation in Dev.Core.</item>
    /// </list>
    /// Defaults to <c>false</c> (unchecked) when checkboxes are not in use.
    /// </summary>
    [ObservableProperty]
    private bool? isChecked = false;

    // -----------------------------------------------------------------------
    // Search metadata (observable — set by the search engine, not by consumers)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Set to <c>true</c> by the search engine when this node satisfies the
    /// active <see cref="TreeQuery"/>. Used by the node template to apply
    /// inline highlight styling.
    /// </summary>
    [ObservableProperty]
    private bool isMatch;

    /// <summary>
    /// Set to <c>true</c> by the search engine when a descendant of this node
    /// satisfies the active query. Used by the control to auto-expand ancestor
    /// paths when <c>AutoExpandSearchMatches</c> is enabled.
    /// </summary>
    [ObservableProperty]
    private bool isAncestorOfMatch;
}
