// IRequiresStableState.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services.Mode;

/// <summary>
/// Capability marker for feature modes that require a stable global state.
/// </summary>
public interface IRequiresStableState
{
    /// <summary>
    /// Gets a value indicating whether the active mode requires the global state
    /// to remain stable while the mode is active.
    /// </summary>
    bool RequiresStableState { get; }
}
