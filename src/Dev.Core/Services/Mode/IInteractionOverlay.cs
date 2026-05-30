// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInteractionOverlay.cs" company="MrMontana1889">
//   Copyright (c) 2026 MrMontana1889.  See LICENSE
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Dev.Core.Services.Mode;

/// <summary>
/// Abstraction for a transient interaction overlay.
/// </summary>
/// <remarks>
/// Overlays are intentionally framework-agnostic and expose only lifecycle hooks.
/// </remarks>
public interface IInteractionOverlay
{
    /// <summary>
    /// Invoked when the overlay becomes active.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Invoked when the overlay is dismissed.
    /// </summary>
    void OnExit();
}