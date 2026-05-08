// MenuItemModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Dev.Core.Tree;

/// <summary>
/// Immutable data model for a single entry in a tree node context menu.
/// Produced by <see cref="ITreeContextMenuProvider"/> implementations.
/// <para>
/// Deliberately references only <see cref="ICommand"/> and no WPF types so
/// this record can be constructed and unit-tested without a WPF runtime.
/// Dev.Wpf's <c>ContextMenuBehavior</c> converts the returned list to WPF
/// <c>MenuItem</c> elements at display time.
/// </para>
/// </summary>
/// <param name="Label">Display text for the menu entry.</param>
/// <param name="Command">Command executed when the entry is invoked.</param>
/// <param name="CommandParameter">Optional parameter passed to <paramref name="Command"/>.</param>
/// <param name="IconKey">
/// Optional icon token from <c>Dev.Core.Tree.IconKeys</c> or a custom string,
/// resolved to an <c>ImageSource</c> by Dev.Wpf at display time.
/// </param>
/// <param name="IsEnabled">Whether the menu entry is interactable.</param>
/// <param name="Children">
/// Optional sub-menu entries. When non-empty the entry renders as a submenu
/// flyout rather than a direct command item.
/// </param>
[ExcludeFromCodeCoverage]
public sealed record MenuItemModel(
    string Label,
    ICommand Command,
    object? CommandParameter = null,
    string? IconKey = null,
    bool IsEnabled = true,
    IReadOnlyList<MenuItemModel>? Children = null);
