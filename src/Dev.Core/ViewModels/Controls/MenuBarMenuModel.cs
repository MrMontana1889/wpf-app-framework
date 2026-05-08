// MenuBarMenuModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a top-level menu projected from a toolbar.
/// </summary>
public sealed class MenuBarMenuModel
{
    public MenuBarMenuModel(string label, IReadOnlyList<MenuBarEntryModel> items)
    {
        Label = label;
        Items = items;
    }

    public string Label { get; }

    public IReadOnlyList<MenuBarEntryModel> Items { get; }
}
