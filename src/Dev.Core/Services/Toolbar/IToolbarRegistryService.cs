// IToolbarRegistryService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services;

/// <summary>
/// Maintains the application's ordered list of toolbars and persists their
/// row-level visibility (show / hide) state across sessions.
/// </summary>
public interface IToolbarRegistryService
{
    /// <summary>
    /// Raised when a toolbar's visibility changes after registration.
    /// </summary>
    event EventHandler<ToolbarVisibilityChangedEventArgs>? VisibilityChanged;

    /// <summary>
    /// The ordered list of toolbars that have been registered with the service.
    /// </summary>
    IReadOnlyList<ToolbarModel> Toolbars { get; }

    /// <summary>
    /// The ordered list of semantic toolbar definitions registered with the service.
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
    /// Sets visibility for a registered toolbar id. Toolbars with
    /// <see cref="ToolbarDefinition.CanHide"/> set to <c>false</c> always remain visible.
    /// </summary>
    void SetVisibility(ToolbarId toolbarId, bool isVisible);

    /// <summary>
    /// Registers a toolbar, restoring any previously persisted row visibility
    /// for it. Pass <paramref name="canHide"/> as <c>false</c> to mark the
    /// toolbar as always-visible (its context-menu entry will be disabled).
    /// Changes to <see cref="ToolbarModel.IsToolbarVisible"/> on the registered
    /// instance are automatically persisted.
    ///
    /// This legacy registration path is retained for backward compatibility and
    /// delegates internally to semantic registration keyed by <see cref="ToolbarId"/>.
    /// </summary>
    void Register(ToolbarModel toolbar, bool canHide = true);

    /// <summary>
    /// Explicitly saves the current row visibility of all registered toolbars.
    /// Normally called automatically, but available for forced saves if needed.
    /// </summary>
    void SaveVisibility();
}
