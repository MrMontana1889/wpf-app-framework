// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInteractionOverlay.cs" company="MrMontana1889">
//   Copyright (c) 2026 MrMontana1889.  See LICENSE
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Input;

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
    /// Gets the command that cancels the active overlay interaction.
    /// </summary>
    ICommand CancelCommand { get; }

    /// <summary>
    /// Invoked when the overlay becomes active.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Invoked when the overlay is dismissed.
    /// </summary>
    void OnExit();
}