// IToolbarSettingsService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services;

/// <summary>
/// Persists and restores per-toolbar item visibility settings.
/// </summary>
public interface IToolbarSettingsService
{
    /// <summary>
    /// Persists the current visibility state of all items in the named toolbar.
    /// </summary>
    void Save(string toolbarName, IEnumerable<ToolbarItemModel> items);

    /// <summary>
    /// Applies previously persisted visibility state to the items in the named toolbar.
    /// Items whose label does not appear in the saved data are left unchanged.
    /// </summary>
    void Load(string toolbarName, IEnumerable<ToolbarItemModel> items);
}
