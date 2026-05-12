// IToolbarSettingsService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;

namespace Dev.Core.Services;

/// <summary>
/// Persists and restores per-toolbar item-level visibility settings keyed by
/// <see cref="ToolbarItemId"/>. No ViewModel references; purely semantic state.
/// </summary>
public interface IToolbarSettingsService
{
    /// <summary>
    /// Persists the current item-level visibility state for the specified toolbar,
    /// keyed by each item's <see cref="ToolbarItem.Id"/>.
    /// </summary>
    void Save(ToolbarId toolbarId, IEnumerable<ToolbarItem> items);

    /// <summary>
    /// Applies previously persisted item-level visibility state to the supplied items.
    /// Items whose <see cref="ToolbarItem.Id"/> does not appear in the saved data
    /// are left unchanged.
    /// </summary>
    void Load(ToolbarId toolbarId, IEnumerable<ToolbarItem> items);
}
