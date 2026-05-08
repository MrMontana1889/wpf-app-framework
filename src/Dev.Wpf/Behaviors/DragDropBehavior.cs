// DragDropBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Handles drag-and-drop reordering for <see cref="TreeViewControl"/>.
/// <para>
/// Dragging is active only when <see cref="TreeViewControl.CanDragDrop"/> is
/// <c>true</c>. A drag begins when the pointer travels beyond
/// <see cref="SystemParameters.MinimumHorizontalDragDistance"/> or
/// <see cref="SystemParameters.MinimumVerticalDragDistance"/> while the left
/// mouse button is held, using the current
/// <see cref="TreeViewControl.SelectedNodes"/> as the dragged payload.
/// </para>
/// <para>
/// <see cref="DragDrop.DoDragDrop"/> is a blocking WPF call; the
/// <c>_isDragging</c> flag prevents re-entry during the operation.
/// </para>
/// <para>
/// Drop-target validation and precise insertion-index calculation are Dev.Core
/// responsibilities, stubbed here until Phase D. The behavior raises
/// <see cref="TreeViewControl.RaiseNodesDropped"/> on a successful drop so
/// the host view model can update the tree structure.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class DragDropBehavior : TreeViewBehaviorBase
{
    private const string DragFormat = "Dev.Core.Tree.TreeNodeModels";

    private Point _dragStartPoint;
    private bool  _isDragging;
    private IReadOnlyList<TreeNodeModel> _draggedNodes = [];

    private readonly MouseButtonEventHandler _onMouseLeftDown;
    private readonly MouseEventHandler       _onMouseMove;
    private readonly DragEventHandler        _onDragOver;
    private readonly DragEventHandler        _onDrop;

    internal DragDropBehavior()
    {
        _onMouseLeftDown = OnMouseLeftButtonDown;
        _onMouseMove     = OnMouseMove;
        _onDragOver      = OnDragOver;
        _onDrop          = OnDrop;
    }

    protected override void OnAttached()
    {
        Control!.AllowDrop = true;
        Control!.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, _onMouseLeftDown);
        Control!.AddHandler(UIElement.MouseMoveEvent,                  _onMouseMove);
        Control!.AddHandler(UIElement.DragOverEvent,                   _onDragOver);
        Control!.AddHandler(UIElement.DropEvent,                       _onDrop);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, _onMouseLeftDown);
        Control!.RemoveHandler(UIElement.MouseMoveEvent,                  _onMouseMove);
        Control!.RemoveHandler(UIElement.DragOverEvent,                   _onDragOver);
        Control!.RemoveHandler(UIElement.DropEvent,                       _onDrop);
        Control!.AllowDrop = false;
        _isDragging   = false;
        _draggedNodes = [];
    }

    // -----------------------------------------------------------------------
    // Drag initiation
    // -----------------------------------------------------------------------

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!Control!.CanDragDrop) return;
        _dragStartPoint = e.GetPosition(Control);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!Control!.CanDragDrop) return;
        if (_isDragging) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var current = e.GetPosition(Control);
        if (!ExceedsMinimumDragDistance(current)) return;

        var nodes = Control.SelectedNodes;
        if (nodes.Count == 0) return;

        // Only allow dragging nodes that are selectable (not read-only)
        if (nodes.Any(n => !n.IsSelectable)) return;

        _isDragging   = true;
        _draggedNodes = nodes;

        var data = new DataObject(DragFormat, nodes);
        DragDrop.DoDragDrop(Control, data, DragDropEffects.Move);

        // DoDragDrop blocks until the drop completes or is cancelled.
        _isDragging = false;
    }

    // -----------------------------------------------------------------------
    // Drop target
    // -----------------------------------------------------------------------

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (!Control!.CanDragDrop || !e.Data.GetDataPresent(DragFormat))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var targetContainer = FindContainer(e.OriginalSource as DependencyObject);
        var targetNode = targetContainer?.DataContext as TreeNodeModel;

        // Extract dragged nodes from the data (use cached if available)
        var draggedNodes = _draggedNodes.Count > 0 
            ? _draggedNodes 
            : (e.Data.GetData(DragFormat) as IReadOnlyList<TreeNodeModel> ?? []);

        // Validate drop using Dev.Core rules
        var canDrop = TreeDropValidator.CanDrop(draggedNodes, targetNode);

        e.Effects = canDrop ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!Control!.CanDragDrop || !e.Data.GetDataPresent(DragFormat)) return;

        var targetContainer = FindContainer(e.OriginalSource as DependencyObject);
        var targetNode = targetContainer?.DataContext as TreeNodeModel;

        // Extract dragged nodes from the data
        var draggedNodes = e.Data.GetData(DragFormat) as IReadOnlyList<TreeNodeModel>;
        if (draggedNodes is null || draggedNodes.Count == 0) return;

        // Final validation before drop
        if (!TreeDropValidator.CanDrop(draggedNodes, targetNode))
        {
            e.Handled = true;
            return;
        }

        // Calculate insertion index: append to target's children
        var insertionIndex = targetNode?.Children.Count ?? 0;

        Control.RaiseNodesDropped(draggedNodes, targetNode, insertionIndex);
        e.Handled = true;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private bool ExceedsMinimumDragDistance(Point current) =>
        Math.Abs(current.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
        Math.Abs(current.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance;
}
