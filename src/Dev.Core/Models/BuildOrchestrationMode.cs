// BuildOrchestrationMode.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Models;

/// <summary>
/// Controls which execution path is used when orchestrating a build strategy switch.
/// </summary>
public enum BuildOrchestrationMode
{
    /// <summary>
    /// Automatically select native BentleyBuild switch orchestration when supported,
    /// and fall back to Phase 2.6 execution when native support is unavailable.
    /// </summary>
    Auto,

    /// <summary>
    /// Always use native BentleyBuild switch orchestration.
    /// Execution fails if native support is not detected.
    /// </summary>
    NativeSwitch,

    /// <summary>
    /// Always use Phase 2.6 execution regardless of native support.
    /// </summary>
    Phase26Fallback,
}
