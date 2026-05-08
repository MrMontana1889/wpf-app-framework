// ApplicationSettings.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Models;

/// <summary>
/// Application-level settings that persist across sessions.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Gets or sets the theme override. Valid values: "Light", "Dark", "System".
    /// </summary>
    public string ThemeOverride { get; set; } = "System";

    /// <summary>
    /// Gets or sets the maximum number of recently opened files to remember.
    /// </summary>
    public int RecentFilesMaxCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the list of recently opened file paths, most recent first.
    /// </summary>
    public List<string> RecentFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets the execution backend used to invoke BentleyBuild.
    /// Defaults to <see cref="BuildExecutionBackend.Auto"/>, which behaves identically
    /// to <see cref="BuildExecutionBackend.DirectUv"/> in Phase 2.6a.
    /// </summary>
    public BuildExecutionBackend BuildExecutionBackend { get; set; } = BuildExecutionBackend.Auto;

    /// <summary>
    /// Gets or sets the fallback mode that controls when incremental orchestration is
    /// abandoned in favor of a TMR build.
    /// </summary>
    public IncrementalBuildFallbackMode IncrementalBuildFallbackMode { get; set; } = IncrementalBuildFallbackMode.Auto;

    /// <summary>
    /// Gets or sets the ratio threshold at which the automatic fallback triggers.
    /// When <c>affectedParts / totalProjects &gt;= threshold</c>, a TMR build is used.
    /// Valid range: 0.0 – 1.0. Default: 0.6 (60 %).
    /// </summary>
    public double IncrementalBuildFallbackThreshold { get; set; } = 0.6;

    /// <summary>
    /// Gets or sets the orchestration mode that controls whether native BentleyBuild switch
    /// or Phase 2.6 execution is used when applying strategy changes.
    /// Defaults to <see cref="BuildOrchestrationMode.Auto"/>.
    /// </summary>
    public BuildOrchestrationMode BuildOrchestrationMode { get; set; } = BuildOrchestrationMode.Auto;
}
