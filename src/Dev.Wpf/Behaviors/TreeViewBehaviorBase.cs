// TreeViewBehaviorBase.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Wpf.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Abstract base for all behaviors that attach to a <see cref="TreeViewControl"/>.
/// <para>
/// Each concrete behavior subscribes to WPF input events in <see cref="OnAttached"/>
/// and unsubscribes in <see cref="OnDetaching"/>. All business-rule evaluation is
/// delegated to Dev.Core; behaviors own only WPF input handling and event raising.
/// </para>
/// </summary>
[ExcludeFromCodeCoverage]
internal abstract class TreeViewBehaviorBase
{
    protected TreeViewControl? Control { get; private set; }

    internal void Attach(TreeViewControl control)
    {
        if (Control is not null)
            Detach();
        Control = control;
        OnAttached();
    }

    internal void Detach()
    {
        if (Control is null) return;
        OnDetaching();
        Control = null;
    }

    protected abstract void OnAttached();
    protected abstract void OnDetaching();

    /// <summary>
    /// Walks the visual tree from <paramref name="source"/> toward the root,
    /// returning the first <see cref="TreeNodeContainer"/> ancestor found,
    /// or <c>null</c> if none exists.
    /// </summary>
    protected static TreeNodeContainer? FindContainer(DependencyObject? source)
    {
        var d = source;
        while (d is not null)
        {
            if (d is TreeNodeContainer container) return container;
            d = VisualTreeHelper.GetParent(d);
        }
        return null;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="source"/> is inside the expand/collapse
    /// glyph or the checkbox template part, meaning click events on those elements should
    /// not be treated as node selection gestures.
    /// </summary>
    protected static bool IsOnNonSelectPart(DependencyObject? source, TreeNodeContainer boundary)
    {
        var d = source;
        while (d is not null && d != boundary)
        {
            if (d is FrameworkElement { Name: TreeNodeContainer.PartExpandCollapseGlyph
                                           or TreeNodeContainer.PartCheckBox })
                return true;
            d = VisualTreeHelper.GetParent(d);
        }
        return false;
    }
}
