// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInteractionOverlayTResult.cs" company="MrMontana1889">
//   Copyright (c) 2026 MrMontana1889.  See LICENSE
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Dev.Core.Services.Mode;

/// <summary>
/// Abstraction for a transient interaction overlay that produces a typed result.
/// </summary>
/// <typeparam name="TResult">The result type produced by the overlay.</typeparam>
public interface IInteractionOverlay<TResult> : IInteractionOverlay
{
    /// <summary>
    /// Registers the callback that receives the overlay result.
    /// </summary>
    /// <param name="callback">The callback to invoke when the overlay completes.</param>
    void SetResultCallback(Action<TResult> callback);
}