// IIconProvider.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Windows.Media;

namespace Dev.Wpf.Controls;

/// <summary>
/// Resolves an opaque <c>IconKey</c> string token (from
/// <see cref="Dev.Core.Tree.TreeNodeModel.IconKey"/>) to a WPF
/// <see cref="ImageSource"/> at binding time.
/// <para>
/// Assign an implementation to <c>TreeViewControl.IconProvider</c> to supply
/// theme-aware, vector-based, or application-specific icon sets without
/// modifying the control.
/// </para>
/// </summary>
/// <remarks>
/// Named implementations planned for Phase C/D:
/// <list type="bullet">
///   <item><c>DefaultIconProvider</c> — resolves pack URIs to embedded PNG resources.</item>
///   <item><c>VectorIconProvider</c> — loads XAML-defined vector geometry.</item>
///   <item><c>ThemedIconProvider</c> — delegates to one of two providers based on the active theme.</item>
///   <item><c>ChainedIconProvider</c> — fallback chain across multiple providers.</item>
/// </list>
/// Implementations must call <see cref="Freezable.Freeze"/> on all resolved
/// <see cref="ImageSource"/> instances to enable cross-thread access and minimise
/// memory pressure through caching.
/// </remarks>
public interface IIconProvider
{
    /// <summary>
    /// Returns the <see cref="ImageSource"/> for the given <paramref name="iconKey"/>.
    /// Returns <c>null</c> when the key is unrecognised so the template can render
    /// a fallback or collapse the icon slot.
    /// </summary>
    ImageSource? GetIcon(string iconKey);
}
