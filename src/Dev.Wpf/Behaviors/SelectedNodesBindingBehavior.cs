// SelectedNodesBindingBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Binds <see cref="TreeViewControl.SelectedNodes"/> (a read-only dependency property)
/// to a writable ViewModel property via <see cref="BoundSelectedNodes"/>.
/// <para>
/// This behavior eliminates the need for code-behind observation of the read-only
/// <c>SelectedNodes</c> property by internally using <see cref="DependencyPropertyDescriptor"/>
/// to detect changes and update the bound property.
/// </para>
/// </summary>
/// <example>
/// <code><![CDATA[
/// <TreeViewControl ItemsSource="{Binding Nodes}">
///     <i:Interaction.Behaviors>
///         <behaviors:SelectedNodesBindingBehavior
///             BoundSelectedNodes="{Binding SelectedNodes, Mode=OneWayToSource}" />
///     </i:Interaction.Behaviors>
/// </TreeViewControl>
/// ]]></code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class SelectedNodesBindingBehavior : Behavior<TreeViewControl>
{
    // -----------------------------------------------------------------------
    // BoundSelectedNodes DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty BoundSelectedNodesProperty =
        DependencyProperty.Register(
            nameof(BoundSelectedNodes),
            typeof(IReadOnlyList<TreeNodeModel>),
            typeof(SelectedNodesBindingBehavior),
            new FrameworkPropertyMetadata(
                Array.Empty<TreeNodeModel>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// The bound ViewModel property to update when <see cref="TreeViewControl.SelectedNodes"/> changes.
    /// Bind this with <c>Mode=OneWayToSource</c> to push changes from the control to the ViewModel.
    /// </summary>
    public IReadOnlyList<TreeNodeModel> BoundSelectedNodes
    {
        get => (IReadOnlyList<TreeNodeModel>)GetValue(BoundSelectedNodesProperty);
        set => SetValue(BoundSelectedNodesProperty, value);
    }

    // -----------------------------------------------------------------------
    // Observation logic
    // -----------------------------------------------------------------------

    private DependencyPropertyDescriptor? _descriptor;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
            return;

        // Observe TreeViewControl.SelectedNodes using DependencyPropertyDescriptor
        _descriptor = DependencyPropertyDescriptor.FromProperty(
            TreeViewControl.SelectedNodesProperty,
            typeof(TreeViewControl));

        _descriptor.AddValueChanged(AssociatedObject, OnSelectedNodesChanged);

        // Initialize with current value
        UpdateBoundProperty();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null && _descriptor is not null)
        {
            _descriptor.RemoveValueChanged(AssociatedObject, OnSelectedNodesChanged);
            _descriptor = null;
        }
    }

    private void OnSelectedNodesChanged(object? sender, EventArgs e)
    {
        UpdateBoundProperty();
    }

    private void UpdateBoundProperty()
    {
        if (AssociatedObject is null)
            return;

        // Push the current SelectedNodes value to the bound ViewModel property
        BoundSelectedNodes = AssociatedObject.SelectedNodes;
    }
}
