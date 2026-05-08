// ExpandCollapseBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Handles expand/collapse gestures and node activation for
/// <see cref="TreeViewControl"/>.
/// <list type="bullet">
///   <item>
///     Clicking the <c>PART_ExpandCollapseGlyph</c> <see cref="ToggleButton"/>
///     toggles <see cref="TreeNodeModel.IsExpanded"/> via its two-way binding;
///     the behavior listens for the resulting bubbled
///     <see cref="ToggleButton.CheckedEvent"/> / <see cref="ToggleButton.UncheckedEvent"/>
///     to raise <see cref="TreeViewControl.RaiseNodeExpanded"/> or
///     <see cref="TreeViewControl.RaiseNodeCollapsed"/>.
///   </item>
///   <item>
///     Double-clicking anywhere on the node row (outside the glyph and checkbox)
///     raises <see cref="TreeViewControl.RaiseNodeActivated"/> and, when exactly
///     one node is selected, toggles the node's <see cref="TreeNodeModel.IsExpanded"/>
///     state (if the node is expandable).
///   </item>
/// </list>
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class ExpandCollapseBehavior : TreeViewBehaviorBase
{
    private readonly RoutedEventHandler      _onGlyphChecked;
    private readonly RoutedEventHandler      _onGlyphUnchecked;
    private readonly MouseButtonEventHandler _onMouseLeftDown;

    internal ExpandCollapseBehavior()
    {
        _onGlyphChecked   = OnGlyphChecked;
        _onGlyphUnchecked = OnGlyphUnchecked;
        _onMouseLeftDown  = OnMouseLeftButtonDown;
    }

    protected override void OnAttached()
    {
        Control!.AddHandler(ToggleButton.CheckedEvent,               _onGlyphChecked);
        Control!.AddHandler(ToggleButton.UncheckedEvent,             _onGlyphUnchecked);
        Control!.AddHandler(UIElement.MouseLeftButtonDownEvent,      _onMouseLeftDown);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(ToggleButton.CheckedEvent,            _onGlyphChecked);
        Control!.RemoveHandler(ToggleButton.UncheckedEvent,          _onGlyphUnchecked);
        Control!.RemoveHandler(UIElement.MouseLeftButtonDownEvent,   _onMouseLeftDown);
    }

    // -----------------------------------------------------------------------
    // Expand / collapse
    // -----------------------------------------------------------------------

    private void OnGlyphChecked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is ToggleButton { Name: TreeNodeContainer.PartExpandCollapseGlyph } &&
            FindContainer(e.OriginalSource as DependencyObject) is { } container &&
            container.DataContext is TreeNodeModel node)
        {
            Control!.RaiseNodeExpanded(node);
        }
    }

    private void OnGlyphUnchecked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is ToggleButton { Name: TreeNodeContainer.PartExpandCollapseGlyph } &&
            FindContainer(e.OriginalSource as DependencyObject) is { } container &&
            container.DataContext is TreeNodeModel node)
        {
            Control!.RaiseNodeCollapsed(node);
        }
    }

    // -----------------------------------------------------------------------
    // Activation (double-click)
    // -----------------------------------------------------------------------

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount < 2) return;

        var container = FindContainer(e.OriginalSource as DependencyObject);
        if (container?.DataContext is not TreeNodeModel node) return;

        // Do not activate when the click is on the glyph or checkbox;
        // those are handled by their own gestures.
        if (IsOnNonSelectPart(e.OriginalSource as DependencyObject, container)) return;

        Control!.RaiseNodeActivated(node);

        // Toggle expand/collapse only when exactly one node is selected.
        // When multiple nodes are selected, double-click does nothing beyond
        // raising the activation event.
        var selectedNodes = Control!.SelectedNodes;
        if (selectedNodes.Count == 1 && node.IsExpandable)
        {
            node.IsExpanded = !node.IsExpanded;
        }

        e.Handled = true;
    }
}
