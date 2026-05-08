// IModeService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Services.Mode;

/// <summary>
/// Central service governing baseline and feature mode state for the application shell.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Baseline modes</strong> (<see cref="BaselineMode"/>) are mutually exclusive
/// and always-active. Switching baseline mode changes the default UI posture only;
/// no Save / Apply / Cancel semantics are involved.
/// </para>
/// <para>
/// <strong>Feature modes</strong> (<see cref="IFeatureMode"/>) are optional and single-
/// active. While a feature mode is active, baseline content is suspended. Exiting the
/// feature mode automatically restores baseline content. The shell enforces that at most
/// one feature mode may be active at a time.
/// </para>
/// <para>
/// While any feature mode is active, application shutdown is blocked and the output
/// panel must remain visible. Both constraints are enforced centrally by the shell and
/// are not configurable by individual feature modes.
/// </para>
/// </remarks>
public interface IModeService
{
    /// <summary>
    /// The currently active baseline mode. Always has a value; defaults to
    /// <see cref="BaselineMode.Simple"/>.
    /// </summary>
    BaselineMode ActiveBaselineMode { get; }

    /// <summary>
    /// The currently active feature mode, or <c>null</c> when no feature mode is active.
    /// </summary>
    IFeatureMode? ActiveFeatureMode { get; }

    /// <summary>
    /// <c>true</c> while exactly one feature mode is active.
    /// </summary>
    bool IsFeatureModeActive { get; }

    /// <summary>
    /// <c>true</c> while a feature mode is active, indicating that normal application
    /// shutdown paths (including window close) must be blocked by the shell.
    /// No confirmation dialog or auto-cancel is implied.
    /// </summary>
    bool IsShutdownBlocked { get; }

    /// <summary>
    /// Fires immediately after <see cref="ActiveBaselineMode"/> changes.
    /// Not fired when the new mode equals the current mode.
    /// </summary>
    event EventHandler<BaselineModeChangedEventArgs>? BaselineModeChanged;

    /// <summary>
    /// Fires after a feature mode is entered or exited.
    /// <see cref="FeatureModeChangedEventArgs.IsEntering"/> distinguishes the direction.
    /// </summary>
    event EventHandler<FeatureModeChangedEventArgs>? FeatureModeChanged;

    /// <summary>
    /// Switches the active baseline mode. No-op when <paramref name="mode"/> matches
    /// the current value.
    /// </summary>
    void SetBaselineMode(BaselineMode mode);

    /// <summary>
    /// Activates the supplied feature mode. <see cref="IFeatureMode.OnEnter"/> is
    /// called, then <see cref="FeatureModeChanged"/> is raised.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a feature mode is already active. The caller must exit the current
    /// feature mode before entering a new one.
    /// </exception>
    void EnterFeatureMode(IFeatureMode mode);

    /// <summary>
    /// Exits the currently active feature mode. <see cref="IFeatureMode.OnExit"/> is
    /// called, then <see cref="FeatureModeChanged"/> is raised.
    /// No-op when no feature mode is active.
    /// </summary>
    void ExitFeatureMode();

    /// <summary>
    /// Attempts to apply the active feature mode by invoking
    /// <see cref="IFeatureMode.TryApplyAsync"/>.
    /// </summary>
    /// <returns>
    /// A task that resolves to <c>true</c> when apply succeeded and the mode exited;
    /// <c>false</c> when apply failed and the mode remains active.
    /// </returns>
    /// <remarks>
    /// On failure: the mode stays active, baseline UI is not restored, and
    /// shutdown remains blocked. On success: <see cref="IFeatureMode.OnExit"/> is
    /// called, baseline UI is restored, and shutdown is re-enabled.
    /// No-op (returns <c>false</c>) when no feature mode is active.
    /// </remarks>
    Task<bool> TryApplyAndExitFeatureModeAsync();

    /// <summary>
    /// Cancels the active feature mode by invoking <see cref="IFeatureMode.Cancel"/>,
    /// then unconditionally exits the mode.
    /// </summary>
    /// <remarks>
    /// Cancel always discards staged state and always exits. Baseline UI is
    /// restored and shutdown is re-enabled after this call.
    /// No-op when no feature mode is active.
    /// </remarks>
    void CancelAndExitFeatureMode();
}
