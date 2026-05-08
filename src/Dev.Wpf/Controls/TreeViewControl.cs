// TreeViewControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

/// <summary>
/// High-performance, MVVM-first tree view built on <see cref="ItemsControl"/>
/// with recycling virtualization, multi-select, lazy loading, tri-state
/// checkboxes, drag-drop, per-node context menus, and a typed routed event
/// surface.
/// <para>
/// The control derives from <see cref="ItemsControl"/> (not the built-in WPF
/// <c>TreeView</c>) and uses a recursive <see cref="ItemsControl"/> pattern for
/// child levels. This architecture enables <see cref="VirtualizingStackPanel"/>
/// in recycling mode, full template control, and avoidance of
/// <c>TreeViewItem</c>'s hardcoded visual states and selection model.
/// </para>
/// <para>
/// <strong>All interaction logic is implemented in attached behaviors in
/// <c>Dev.Wpf.Behaviors</c>. No business logic runs in this class beyond
/// dependency-property infrastructure and internal attachment-point methods
/// called by those behaviors.</strong>
/// </para>
/// </summary>
public class TreeViewControl : ItemsControl
{
    private static readonly Lazy<IIconProvider> DefaultIconProvider =
        new(() => new PackUriIconProvider("BentleyBuildApp", "Resources/Icons"));

    // -----------------------------------------------------------------------
    // Static constructor — default style key and virtualization defaults
    // -----------------------------------------------------------------------

    static TreeViewControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(typeof(TreeViewControl)));

        // Opt into VirtualizingStackPanel recycling for high-performance
        // rendering of large trees. The Items panel template in the control
        // style must use VirtualizingStackPanel to honour these settings.
        VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(true));

        VirtualizingPanel.VirtualizationModeProperty.OverrideMetadata(
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(VirtualizationMode.Recycling));

        // Pixel-based scrolling (CanContentScroll=False in template) is used so
        // nested tree structures scroll correctly. The OverrideMetadata here keeps
        // the attached-property default consistent with the template value.
        ScrollViewer.CanContentScrollProperty.OverrideMetadata(
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(false));
    }

    // -----------------------------------------------------------------------
    // ComponentResourceKeys — referenced by the control template and behaviors
    // -----------------------------------------------------------------------

    /// <summary>Style key for the node container element (future <c>TreeNodeContainer</c>).</summary>
    public static readonly ComponentResourceKey NodeContainerStyleKey =
        new(typeof(TreeViewControl), "NodeContainerStyle");

    /// <summary>Style key for the expand/collapse chevron glyph path.</summary>
    public static readonly ComponentResourceKey ExpandGlyphStyleKey =
        new(typeof(TreeViewControl), "ExpandGlyphStyle");

    /// <summary>Style key for the tri-state node checkbox.</summary>
    public static readonly ComponentResourceKey CheckboxStyleKey =
        new(typeof(TreeViewControl), "CheckboxStyle");

    /// <summary>Style key for the drag-and-drop insertion-point indicator.</summary>
    public static readonly ComponentResourceKey InsertionMarkerStyleKey =
        new(typeof(TreeViewControl), "InsertionMarkerStyle");

    /// <summary>Style key for the lazy-load spinner / placeholder.</summary>
    public static readonly ComponentResourceKey LazyLoadPlaceholderStyleKey =
        new(typeof(TreeViewControl), "LazyLoadPlaceholderStyle");

    // -----------------------------------------------------------------------
    // Instance constructor — behaviors
    // -----------------------------------------------------------------------

    private readonly TreeViewBehaviorBase[] _behaviors;

    /// <summary>
    /// Initializes a new <see cref="TreeViewControl"/> and wires all
    /// interaction behaviors. Behaviors are attached on
    /// <see cref="FrameworkElement.Loaded"/> and detached on
    /// <see cref="FrameworkElement.Unloaded"/> so they hold no references
    /// that prevent GC after the control is removed from the visual tree.
    /// </summary>
    public TreeViewControl()
    {
        SetCurrentValue(IconProviderProperty, DefaultIconProvider.Value);

        _behaviors =
        [
            new ExpandCollapseBehavior(),
            new MultiSelectBehavior(),
            new KeyboardNavigationBehavior(),
            new CheckboxBehavior(),
            new DragDropBehavior(),
            new ContextMenuBehavior(),
        ];
        Loaded   += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var behavior in _behaviors)
            behavior.Attach(this);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var behavior in _behaviors)
            behavior.Detach();
    }

    // -----------------------------------------------------------------------
    // SelectionMode DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(
            nameof(SelectionMode),
            typeof(TreeSelectionMode),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(TreeSelectionMode.Single, OnSelectionModeChanged));

    /// <summary>
    /// Controls whether the tree allows no selection, single-node selection,
    /// or multi-node selection. Defaults to <see cref="TreeSelectionMode.Single"/>.
    /// </summary>
    public TreeSelectionMode SelectionMode
    {
        get => (TreeSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // MultiSelectBehavior observes this change and re-evaluates the
        // active selection against the new mode's rules.
    }

    // -----------------------------------------------------------------------
    // SelectedNodes DP (read-only — written only by internal attachment points)
    // -----------------------------------------------------------------------

    private static readonly DependencyPropertyKey SelectedNodesPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SelectedNodes),
            typeof(IReadOnlyList<TreeNodeModel>),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(Array.Empty<TreeNodeModel>()));

    public static readonly DependencyProperty SelectedNodesProperty =
        SelectedNodesPropertyKey.DependencyProperty;

    /// <summary>
    /// The current selection snapshot. Updated by <c>MultiSelectBehavior</c>
    /// after each selection gesture completes. Bind to this in read-only mode
    /// to react to selection changes declaratively.
    /// </summary>
    public IReadOnlyList<TreeNodeModel> SelectedNodes =>
        (IReadOnlyList<TreeNodeModel>)GetValue(SelectedNodesProperty);

    // -----------------------------------------------------------------------
    // ContextMenuProvider DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty ContextMenuProviderProperty =
        DependencyProperty.Register(
            nameof(ContextMenuProvider),
            typeof(ITreeContextMenuProvider),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Supplies the context menu item list for the current selection.
    /// <c>ContextMenuBehavior</c> calls <see cref="ITreeContextMenuProvider.BuildMenu"/>
    /// immediately before the menu is shown. When <c>null</c> the context menu
    /// is suppressed.
    /// </summary>
    public ITreeContextMenuProvider? ContextMenuProvider
    {
        get => (ITreeContextMenuProvider?)GetValue(ContextMenuProviderProperty);
        set => SetValue(ContextMenuProviderProperty, value);
    }

    // -----------------------------------------------------------------------
    // IconProvider DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty IconProviderProperty =
        DependencyProperty.Register(
            nameof(IconProvider),
            typeof(IIconProvider),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(null, OnIconProviderChanged));

    private static void OnIconProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var treeView = (TreeViewControl)d;
        if (e.NewValue is not null)
            return;

        treeView.SetCurrentValue(IconProviderProperty, DefaultIconProvider.Value);
    }

    /// <summary>
    /// Resolves <see cref="TreeNodeModel.IconKey"/> tokens to WPF
    /// <c>ImageSource</c> instances. When <c>null</c> icon slots are collapsed
    /// in the node template. Assign a <c>ThemedIconProvider</c> to participate
    /// in the BBApp.Next dark/light theme system.
    /// </summary>
    public IIconProvider? IconProvider
    {
        get => (IIconProvider?)GetValue(IconProviderProperty);
        set => SetValue(IconProviderProperty, value);
    }

    // -----------------------------------------------------------------------
    // AutoExpandSearchMatches DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty AutoExpandSearchMatchesProperty =
        DependencyProperty.Register(
            nameof(AutoExpandSearchMatches),
            typeof(bool),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// When <c>true</c> (default) the control automatically expands ancestor
    /// nodes of search-matched results after <see cref="Search"/> is called,
    /// ensuring all matches are visible without manual tree navigation.
    /// </summary>
    public bool AutoExpandSearchMatches
    {
        get => (bool)GetValue(AutoExpandSearchMatchesProperty);
        set => SetValue(AutoExpandSearchMatchesProperty, value);
    }

    // -----------------------------------------------------------------------
    // IsSearchActive DP (read-only)
    // -----------------------------------------------------------------------

    private static readonly DependencyPropertyKey IsSearchActivePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsSearchActive),
            typeof(bool),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsSearchActiveProperty =
        IsSearchActivePropertyKey.DependencyProperty;

    /// <summary>
    /// <c>true</c> while a <see cref="TreeQuery"/> filter is active.
    /// Set to <c>true</c> by <see cref="Search"/> and to <c>false</c> by
    /// <see cref="ClearSearch"/>. Bind to this to show/hide a search-active
    /// indicator in the host UI.
    /// </summary>
    public bool IsSearchActive => (bool)GetValue(IsSearchActiveProperty);

    // -----------------------------------------------------------------------
    // CanDragDrop DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty CanDragDropProperty =
        DependencyProperty.Register(
            nameof(CanDragDrop),
            typeof(bool),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Enables drag-and-drop reordering. When <c>true</c>, <c>DragDropBehavior</c>
    /// activates and calls the Dev.Core rule engine to validate each drop target
    /// before allowing the move.
    /// </summary>
    public bool CanDragDrop
    {
        get => (bool)GetValue(CanDragDropProperty);
        set => SetValue(CanDragDropProperty, value);
    }

    // -----------------------------------------------------------------------
    // ShowCheckboxes DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty ShowCheckboxesProperty =
        DependencyProperty.Register(
            nameof(ShowCheckboxes),
            typeof(bool),
            typeof(TreeViewControl),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// When <c>true</c>, each node renders a tri-state checkbox bound to
    /// <see cref="TreeNodeModel.IsChecked"/>. Hierarchical tri-state propagation
    /// rules are evaluated in Dev.Core by <c>CheckboxBehavior</c>.
    /// </summary>
    public bool ShowCheckboxes
    {
        get => (bool)GetValue(ShowCheckboxesProperty);
        set => SetValue(ShowCheckboxesProperty, value);
    }

    // -----------------------------------------------------------------------
    // Routed events
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires when the primary selection changes (single-node focus change).
    /// For multi-select gesture results see <see cref="MultiSelectionChanged"/>.
    /// </summary>
    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<TreeSelectionChangedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<TreeSelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <summary>
    /// Fires after any multi-select gesture (Ctrl+Click, Shift+Click, Ctrl+A,
    /// right-click join) completes and the selection set has changed.
    /// </summary>
    public static readonly RoutedEvent MultiSelectionChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(MultiSelectionChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<TreeMultiSelectionChangedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<TreeMultiSelectionChangedEventArgs> MultiSelectionChanged
    {
        add => AddHandler(MultiSelectionChangedEvent, value);
        remove => RemoveHandler(MultiSelectionChangedEvent, value);
    }

    /// <summary>
    /// Fires when a node is activated by double-clicking it or pressing Enter
    /// while it is the keyboard-focused node.
    /// </summary>
    public static readonly RoutedEvent NodeActivatedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(NodeActivated),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NodeActivatedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<NodeActivatedEventArgs> NodeActivated
    {
        add => AddHandler(NodeActivatedEvent, value);
        remove => RemoveHandler(NodeActivatedEvent, value);
    }

    /// <summary>
    /// Fires after a node transitions from collapsed to expanded.
    /// </summary>
    public static readonly RoutedEvent NodeExpandedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(NodeExpanded),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NodeExpandedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<NodeExpandedEventArgs> NodeExpanded
    {
        add => AddHandler(NodeExpandedEvent, value);
        remove => RemoveHandler(NodeExpandedEvent, value);
    }

    /// <summary>
    /// Fires after a node transitions from expanded to collapsed.
    /// </summary>
    public static readonly RoutedEvent NodeCollapsedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(NodeCollapsed),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NodeCollapsedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<NodeCollapsedEventArgs> NodeCollapsed
    {
        add => AddHandler(NodeCollapsedEvent, value);
        remove => RemoveHandler(NodeCollapsedEvent, value);
    }

    /// <summary>
    /// Fires after a drag-and-drop operation completes and the Dev.Core rule
    /// engine has approved the move. The handler is responsible for updating
    /// the view model tree structure to reflect the new positions.
    /// </summary>
    public static readonly RoutedEvent NodesDroppedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(NodesDropped),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NodesDroppedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<NodesDroppedEventArgs> NodesDropped
    {
        add => AddHandler(NodesDroppedEvent, value);
        remove => RemoveHandler(NodesDroppedEvent, value);
    }

    /// <summary>
    /// Fires after a node's checkbox is toggled and the Dev.Core tri-state
    /// propagation rules have been applied to all affected ancestors and
    /// descendants.
    /// </summary>
    public static readonly RoutedEvent NodeCheckedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(NodeCheckedChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NodeCheckedChangedEventArgs>),
            typeof(TreeViewControl));

    public event EventHandler<NodeCheckedChangedEventArgs> NodeCheckedChanged
    {
        add => AddHandler(NodeCheckedChangedEvent, value);
        remove => RemoveHandler(NodeCheckedChangedEvent, value);
    }

    /// <summary>
    /// Fires before the context menu is built and displayed. Set
    /// <see cref="ContextMenuOpeningEventArgs.Cancel"/> to <c>true</c> to
    /// suppress the menu for a specific invocation.
    /// <para>
    /// This routed event is registered on <see cref="TreeViewControl"/> and
    /// carries tree-specific arguments. It intentionally shadows the inherited
    /// <c>UIElement.ContextMenuOpening</c> event which carries only generic
    /// <c>ContextMenuEventArgs</c>.
    /// </para>
    /// </summary>
    public static readonly RoutedEvent TreeContextMenuOpeningEvent =
        EventManager.RegisterRoutedEvent(
            "ContextMenuOpening",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ContextMenuOpeningEventArgs>),
            typeof(TreeViewControl));

    public new event EventHandler<ContextMenuOpeningEventArgs> ContextMenuOpening
    {
        add => AddHandler(TreeContextMenuOpeningEvent, value);
        remove => RemoveHandler(TreeContextMenuOpeningEvent, value);
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Recursively sets <see cref="TreeNodeModel.IsExpanded"/> to <c>true</c>
    /// on every node in the tree. For lazy nodes this triggers their
    /// <see cref="TreeNodeModel.LazyLoadCallback"/> as each level expands.
    /// Full implementation is delivered in Phase C of the subsystem roadmap.
    /// </summary>
    public void ExpandAll() { }

    /// <summary>
    /// Recursively sets <see cref="TreeNodeModel.IsExpanded"/> to <c>false</c>
    /// on every node in the tree.
    /// Full implementation is delivered in Phase C of the subsystem roadmap.
    /// </summary>
    public void CollapseAll() { }

    /// <summary>
    /// Applies a <see cref="TreeQuery"/> filter. The search engine sets
    /// <see cref="TreeNodeModel.IsMatch"/> and
    /// <see cref="TreeNodeModel.IsAncestorOfMatch"/> on every node, then
    /// <see cref="IsSearchActive"/> is set to <c>true</c>. When
    /// <see cref="AutoExpandSearchMatches"/> is <c>true</c>, ancestor nodes of
    /// all matches are automatically expanded so results are visible.
    /// Full implementation is delivered in Phase B/C of the subsystem roadmap.
    /// </summary>
    public void Search(TreeQuery query)
    {
        SetValue(IsSearchActivePropertyKey, true);
    }

    /// <summary>
    /// Clears the active search: resets <see cref="TreeNodeModel.IsMatch"/> and
    /// <see cref="TreeNodeModel.IsAncestorOfMatch"/> on all nodes, then sets
    /// <see cref="IsSearchActive"/> to <c>false</c>.
    /// Full implementation is delivered in Phase B/C of the subsystem roadmap.
    /// </summary>
    public void ClearSearch()
    {
        SetValue(IsSearchActivePropertyKey, false);
    }

    /// <summary>
    /// Re-triggers lazy loading on every currently-expanded lazy node whose
    /// data may be stale (e.g. after a background model refresh).
    /// Full implementation is delivered in Phase C of the subsystem roadmap.
    /// </summary>
    public void RefreshLazyLoadedNodes() { }

    // -----------------------------------------------------------------------
    // ItemsControl overrides
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> only when <paramref name="item"/> is already a
    /// <see cref="TreeNodeContainer"/>, preventing double-wrapping when containers
    /// are pre-created (e.g. by <see cref="GetContainerForItemOverride"/>).
    /// </summary>
    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is TreeNodeContainer;

    /// <summary>
    /// Creates a <see cref="TreeNodeContainer"/> for each data item, back-linking
    /// it to this control so behaviors can navigate the container hierarchy.
    /// </summary>
    protected override DependencyObject GetContainerForItemOverride() =>
        new TreeNodeContainer { ParentTreeView = this };

    // -----------------------------------------------------------------------
    // Internal behavior attachment points
    // Called exclusively by behaviors in Dev.Wpf.Behaviors; not part of the
    // public API and subject to change as behavior implementations are added.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Commits a new selection snapshot to the read-only <see cref="SelectedNodes"/>
    /// dependency property. Called by <c>MultiSelectBehavior</c> after each
    /// selection gesture resolves.
    /// </summary>
    internal void SetSelectedNodes(IReadOnlyList<TreeNodeModel> nodes) =>
        SetValue(SelectedNodesPropertyKey, nodes);

    /// <summary>Raises <see cref="SelectionChanged"/>. Called by <c>MultiSelectBehavior</c>.</summary>
    internal void RaiseSelectionChanged(TreeNodeModel? oldNode, TreeNodeModel? newNode) =>
        RaiseEvent(new TreeSelectionChangedEventArgs(SelectionChangedEvent, oldNode, newNode));

    /// <summary>Raises <see cref="MultiSelectionChanged"/>. Called by <c>MultiSelectBehavior</c>.</summary>
    internal void RaiseMultiSelectionChanged(
        IReadOnlyList<TreeNodeModel> added,
        IReadOnlyList<TreeNodeModel> removed) =>
        RaiseEvent(new TreeMultiSelectionChangedEventArgs(MultiSelectionChangedEvent, added, removed));

    /// <summary>Raises <see cref="NodeActivated"/>. Called by <c>KeyboardNavigationBehavior</c> (Enter) and <c>ExpandCollapseBehavior</c> (double-click).</summary>
    internal void RaiseNodeActivated(TreeNodeModel node) =>
        RaiseEvent(new NodeActivatedEventArgs(NodeActivatedEvent, node));

    /// <summary>Raises <see cref="NodeExpanded"/>. Called by <c>ExpandCollapseBehavior</c>.</summary>
    internal void RaiseNodeExpanded(TreeNodeModel node) =>
        RaiseEvent(new NodeExpandedEventArgs(NodeExpandedEvent, node));

    /// <summary>Raises <see cref="NodeCollapsed"/>. Called by <c>ExpandCollapseBehavior</c>.</summary>
    internal void RaiseNodeCollapsed(TreeNodeModel node) =>
        RaiseEvent(new NodeCollapsedEventArgs(NodeCollapsedEvent, node));

    /// <summary>Raises <see cref="NodesDropped"/>. Called by <c>DragDropBehavior</c>.</summary>
    internal void RaiseNodesDropped(
        IReadOnlyList<TreeNodeModel> droppedNodes,
        TreeNodeModel? targetNode,
        int insertionIndex) =>
        RaiseEvent(new NodesDroppedEventArgs(NodesDroppedEvent, droppedNodes, targetNode, insertionIndex));

    /// <summary>Raises <see cref="NodeCheckedChanged"/>. Called by <c>CheckboxBehavior</c>.</summary>
    internal void RaiseNodeCheckedChanged(TreeNodeModel node, bool? oldValue, bool? newValue) =>
        RaiseEvent(new NodeCheckedChangedEventArgs(NodeCheckedChangedEvent, node, oldValue, newValue));

    /// <summary>
    /// Raises <see cref="ContextMenuOpening"/>. Called by <c>ContextMenuBehavior</c>
    /// before menu construction begins.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the menu should be shown; <c>false</c> if the consumer
    /// set <see cref="ContextMenuOpeningEventArgs.Cancel"/> to <c>true</c>.
    /// </returns>
    internal bool RaiseContextMenuOpening(IReadOnlyList<TreeNodeModel> selectedNodes)
    {
        var args = new ContextMenuOpeningEventArgs(TreeContextMenuOpeningEvent, selectedNodes);
        RaiseEvent(args);
        return !args.Cancel;
    }
}
