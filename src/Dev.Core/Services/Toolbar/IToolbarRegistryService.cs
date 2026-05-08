// IToolbarRegistryService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services;

/// <summary>
/// Maintains the application's ordered list of toolbars and persists their
/// row-level visibility (show / hide) state across sessions.
/// </summary>
public interface IToolbarRegistryService
{
    /// <summary>
    /// The ordered list of toolbars that have been registered with the service.
    /// </summary>
    IReadOnlyList<ToolbarModel> Toolbars { get; }

    /// <summary>
    /// Registers a toolbar, restoring any previously persisted row visibility
    /// for it. Pass <paramref name="canHide"/> as <c>false</c> to mark the
    /// toolbar as always-visible (its context-menu entry will be disabled).
    /// Changes to <see cref="ToolbarModel.IsToolbarVisible"/> on the registered
    /// instance are automatically persisted.
    /// </summary>
    void Register(ToolbarModel toolbar, bool canHide = true);

    /// <summary>
    /// Explicitly saves the current row visibility of all registered toolbars.
    /// Normally called automatically, but available for forced saves if needed.
    /// </summary>
    void SaveVisibility();
}
