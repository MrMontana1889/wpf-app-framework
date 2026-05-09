// IFeatureMode.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services.Mode;

/// <summary>
/// Abstraction for a feature mode that may occupy the main content panel.
/// At most one feature mode may be active at a time; the shell enforces this constraint.
/// </summary>
/// <remarks>
/// Feature modes own the main content panel while active and may expose a scoped
/// toolbar. Nested feature modes are not supported; no API assumptions that would
/// prevent nesting are encoded here.
/// </remarks>
public interface IFeatureMode
{
    /// <summary>
    /// Stable, unique identifier for this feature mode.
    /// </summary>
    string ModeId { get; }

    /// <summary>
    /// The primary mode-owned toolbar, representing the main action surface
    /// for this feature mode (e.g., Save / Apply / Cancel equivalents in future phases).
    /// When non-null, the shell must register this toolbar as non-hideable
    /// (<c>canHide: false</c>). Mode state drives its visibility:
    /// visible while this mode is active, hidden otherwise.
    /// </summary>
    ToolbarModel? PrimaryToolbar { get; }

    /// <summary>
    /// Invoked by <see cref="IModeService"/> immediately after the feature mode
    /// becomes the active mode. Called before <see cref="IModeService.FeatureModeChanged"/>
    /// is raised.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Invoked by <see cref="IModeService"/> immediately after the feature mode
    /// is deactivated. Called before <see cref="IModeService.FeatureModeChanged"/>
    /// is raised.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Attempts to commit all staged (transient) state.
    /// </summary>
    /// <returns>
    /// A task that resolves to <c>true</c> if the commit succeeded and the mode should exit;
    /// <c>false</c> if the commit failed and the mode must remain active.
    /// </returns>
    /// <remarks>
    /// The feature mode is the sole owner of its transient state. On failure the
    /// shell must not exit the mode, restore baseline UI, or re-enable shutdown.
    /// Baseline application state must not be modified when <c>false</c> is returned.
    /// </remarks>
    Task<bool> TryApplyAsync();

    /// <summary>
    /// Discards all staged (transient) state unconditionally.
    /// This call never fails; the mode will always exit after it returns.
    /// No changes may be committed as a result of cancellation.
    /// </summary>
    void Cancel();
}
