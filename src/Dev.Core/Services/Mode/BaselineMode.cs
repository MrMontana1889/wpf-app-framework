// BaselineMode.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services.Mode;

/// <summary>
/// Represents the active baseline UI posture of the application.
/// Exactly one baseline mode is always active. Baseline modes are mutually
/// exclusive, non-transactional, and do not support Save / Apply / Cancel semantics.
/// Switching baseline mode changes the default UI posture only.
/// </summary>
public enum BaselineMode
{
    /// <summary>
    /// Simple baseline: the default posture, exposing only essential controls.
    /// </summary>
    Simple,

    /// <summary>
    /// Advanced baseline: reveals additional controls for experienced users.
    /// </summary>
    Advanced,
}
