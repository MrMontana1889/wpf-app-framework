// IToolbarRegistryService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;

namespace Dev.Core.Services;

/// <summary>
/// Maintains the application's ordered list of semantic toolbar definitions and
/// persists their row-level visibility (show / hide) state across sessions.
/// This is the sole authoritative registry for toolbar and menu visibility.
/// </summary>
public interface IToolbarRegistryService
{
    /// <summary>
    /// Raised when a toolbar's visibility changes after registration.
    /// </summary>
    event EventHandler<ToolbarVisibilityChangedEventArgs>? VisibilityChanged;

    /// <summary>
    /// Raised when a registered toolbar item's visibility changes.
    /// </summary>
    event EventHandler<ToolbarItemVisibilityChangedEventArgs>? ItemVisibilityChanged;

    /// <summary>
    /// The ordered list of semantic toolbar definitions registered with the service.
    /// This is the single authoritative collection; all toolbars are represented here.
    /// </summary>
    IReadOnlyList<ToolbarDefinition> ToolbarDefinitions { get; }

    /// <summary>
    /// Registers a semantic toolbar definition. Duplicate ids are rejected.
    /// Visibility is initialized from persisted state when available; otherwise
    /// <see cref="ToolbarDefinition.DefaultVisible"/> is used.
    /// </summary>
    void RegisterDefinition(ToolbarDefinition definition);

    /// <summary>
    /// Gets the current visibility state for a registered toolbar id.
    /// </summary>
    bool IsVisible(ToolbarId toolbarId);

    /// <summary>
    /// Gets the current visibility state for a registered toolbar item id.
    /// </summary>
    bool IsItemVisible(ToolbarId toolbarId, ToolbarItemId itemId);

    /// <summary>
    /// Sets visibility for a registered toolbar id. Toolbars with
    /// <see cref="ToolbarDefinition.CanHide"/> set to <c>false</c> always remain visible.
    /// Raises <see cref="VisibilityChanged"/> and persists immediately.
    /// </summary>
    void SetVisibility(ToolbarId toolbarId, bool isVisible);

    /// <summary>
    /// Sets visibility for a registered toolbar item id. Raises
    /// <see cref="ItemVisibilityChanged"/> and persists immediately.
    /// </summary>
    void SetItemVisibility(ToolbarId toolbarId, ToolbarItemId itemId, bool isVisible);

    /// <summary>
    /// Explicitly saves the current row visibility of all registered toolbars.
    /// Normally called automatically on each <see cref="SetVisibility"/> call,
    /// but available for forced saves if needed.
    /// </summary>
    void SaveVisibility();
}
