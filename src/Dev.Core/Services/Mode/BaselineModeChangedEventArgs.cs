// BaselineModeChangedEventArgs.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services.Mode;

/// <summary>
/// Provides data for the <see cref="IModeService.BaselineModeChanged"/> event.
/// </summary>
public sealed class BaselineModeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The baseline mode that was active before the switch.
    /// </summary>
    public BaselineMode Previous { get; }

    /// <summary>
    /// The baseline mode that is now active.
    /// </summary>
    public BaselineMode Current { get; }

    public BaselineModeChangedEventArgs(BaselineMode previous, BaselineMode current)
    {
        Previous = previous;
        Current = current;
    }
}
