// TreeChildItemsControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

/// <summary>
/// Inner <see cref="ItemsControl"/> placed inside each
/// <see cref="TreeNodeContainer"/> to render the recursive child level of the
/// tree.
/// <para>
/// Overrides <see cref="GetContainerForItemOverride"/> so every child item is
/// wrapped in a <see cref="TreeNodeContainer"/>, propagating the
/// <see cref="ParentTreeView"/> reference downward so all containers in the
/// hierarchy share access to the root control's dependency properties and
/// internal attachment-point methods.
/// </para>
/// <para>
/// This control intentionally does not override
/// <c>DefaultStyleKeyProperty</c>, so it inherits the standard
/// <see cref="ItemsControl"/> theme style — a transparent, border-free wrapper
/// around an <see cref="System.Windows.Controls.ItemsPresenter"/>. This
/// keeps the visual output minimal and avoids introducing extra scroll viewers
/// or margins at child levels.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TreeChildItemsControl : ItemsControl
{
    /// <summary>
    /// The root <see cref="TreeViewControl"/> that owns this sub-tree.
    /// Set by <see cref="TreeNodeContainer.OnApplyTemplate"/> after the
    /// template is applied and propagated to every new container created
    /// by <see cref="GetContainerForItemOverride"/>.
    /// </summary>
    internal TreeViewControl? ParentTreeView { get; set; }

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is TreeNodeContainer;

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() =>
        new TreeNodeContainer { ParentTreeView = ParentTreeView };
}
