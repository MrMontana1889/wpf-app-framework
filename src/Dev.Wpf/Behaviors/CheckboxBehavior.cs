// CheckboxBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Handles tri-state checkbox interactions for <see cref="TreeViewControl"/>.
/// <para>
/// The <c>PART_CheckBox</c> element inside each <see cref="TreeNodeContainer"/>
/// is bound two-way to <see cref="TreeNodeModel.IsChecked"/>. To supply an
/// accurate <c>oldValue</c> to the
/// <see cref="TreeViewControl.NodeCheckedChanged"/> event, the behavior captures
/// the model's current value during <see cref="UIElement.PreviewMouseLeftButtonDownEvent"/>
/// — before the binding update propagates — then delivers it in the subsequent
/// <see cref="CheckBox.CheckedEvent"/>, <see cref="CheckBox.UncheckedEvent"/>,
/// or <see cref="CheckBox.IndeterminateEvent"/> handler.
/// </para>
/// <para>
/// Hierarchical tri-state propagation (parent-implies-children and
/// partial-child-implies-indeterminate rules) is a Dev.Core responsibility
/// and is stubbed here until Phase D.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CheckboxBehavior : TreeViewBehaviorBase
{
    private bool? _pendingOldChecked;

    private readonly MouseButtonEventHandler _onCapture;
    private readonly RoutedEventHandler      _onChecked;
    private readonly RoutedEventHandler      _onUnchecked;
    private readonly RoutedEventHandler      _onIndeterminate;

    internal CheckboxBehavior()
    {
        _onCapture       = OnCaptureOldValue;
        _onChecked       = OnChecked;
        _onUnchecked     = OnUnchecked;
        _onIndeterminate = OnIndeterminate;
    }

    protected override void OnAttached()
    {
        Control!.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, _onCapture);
        Control!.AddHandler(CheckBox.CheckedEvent,                     _onChecked);
        Control!.AddHandler(CheckBox.UncheckedEvent,                   _onUnchecked);
        Control!.AddHandler(CheckBox.IndeterminateEvent,               _onIndeterminate);
    }

    protected override void OnDetaching()
    {
        Control!.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, _onCapture);
        Control!.RemoveHandler(CheckBox.CheckedEvent,                     _onChecked);
        Control!.RemoveHandler(CheckBox.UncheckedEvent,                   _onUnchecked);
        Control!.RemoveHandler(CheckBox.IndeterminateEvent,               _onIndeterminate);
    }

    // -----------------------------------------------------------------------
    // Capture old value before the binding updates the model
    // -----------------------------------------------------------------------

    private void OnCaptureOldValue(object sender, MouseButtonEventArgs e)
    {
        if (!IsOnCheckBoxPart(e.OriginalSource as DependencyObject)) return;

        var container = FindContainer(e.OriginalSource as DependencyObject);
        if (container?.DataContext is TreeNodeModel node)
        {
            _pendingOldChecked = node.IsChecked;

            // Manually toggle the checkbox to prevent WPF's default three-state cycle.
            // Users should only toggle between checked/unchecked; indeterminate is set
            // programmatically via propagation.
            node.IsChecked = node.IsChecked switch
            {
                true => false,   // checked → unchecked
                false => true,   // unchecked → checked
                null => true,    // indeterminate → checked (user clicking mixed state)
            };

            // Mark event as handled to prevent WPF's default toggle behavior
            e.Handled = true;
        }
    }

    // -----------------------------------------------------------------------
    // Raise NodeCheckedChanged after the binding has updated the model
    // -----------------------------------------------------------------------

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not CheckBox { Name: TreeNodeContainer.PartCheckBox }) return;
        if (FindContainer(e.OriginalSource as DependencyObject)?.DataContext is not TreeNodeModel node) return;

        // Apply hierarchical tri-state propagation
        ApplyTriStatePropagation(node);

        Control!.RaiseNodeCheckedChanged(node, _pendingOldChecked, true);
        _pendingOldChecked = null;
    }

    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not CheckBox { Name: TreeNodeContainer.PartCheckBox }) return;
        if (FindContainer(e.OriginalSource as DependencyObject)?.DataContext is not TreeNodeModel node) return;

        // Apply hierarchical tri-state propagation
        ApplyTriStatePropagation(node);

        Control!.RaiseNodeCheckedChanged(node, _pendingOldChecked, false);
        _pendingOldChecked = null;
    }

    private void OnIndeterminate(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not CheckBox { Name: TreeNodeContainer.PartCheckBox }) return;
        if (FindContainer(e.OriginalSource as DependencyObject)?.DataContext is not TreeNodeModel node) return;

        // Indeterminate state is set programmatically via propagation, not by user clicks.
        // This event fires when propagation sets a parent to indeterminate.
        Control!.RaiseNodeCheckedChanged(node, _pendingOldChecked, null);
        _pendingOldChecked = null;
    }

    // -----------------------------------------------------------------------
    // Tri-state propagation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Applies hierarchical tri-state propagation logic when a checkbox is toggled.
    /// </summary>
    private void ApplyTriStatePropagation(TreeNodeModel node)
    {
        if (Control?.ItemsSource is not IEnumerable<TreeNodeModel> rootNodes)
            return;

        TreeCheckStatePropagator.PropagateCheckState(node, rootNodes);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static bool IsOnCheckBoxPart(DependencyObject? source)
    {
        var d = source;
        while (d is not null)
        {
            if (d is CheckBox { Name: TreeNodeContainer.PartCheckBox }) return true;
            d = VisualTreeHelper.GetParent(d);
        }
        return false;
    }
}
