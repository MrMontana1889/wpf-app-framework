// IncrementalBuildFallbackMode.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Models;

/// <summary>
/// Controls when incremental build orchestration is abandoned in favor of a TMR build.
/// </summary>
public enum IncrementalBuildFallbackMode
{
    /// <summary>
    /// The tool decides automatically using the affected-parts ratio against
    /// <see cref="ApplicationSettings.IncrementalBuildFallbackThreshold"/>.
    /// </summary>
    Auto,

    /// <summary>
    /// Never fall back to TMR. Incremental orchestration is always used regardless
    /// of the number of affected parts.
    /// </summary>
    AlwaysIncremental,

    /// <summary>
    /// Always perform a TMR build. Incremental orchestration is never attempted.
    /// </summary>
    AlwaysTMR,
}
