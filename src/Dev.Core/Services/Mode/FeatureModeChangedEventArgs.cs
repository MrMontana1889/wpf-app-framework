// FeatureModeChangedEventArgs.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services.Mode;

/// <summary>
/// Provides data for the <see cref="IModeService.FeatureModeChanged"/> event,
/// fired whenever a feature mode is entered or exited.
/// </summary>
public sealed class FeatureModeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The feature mode that entered or exited.
    /// </summary>
    public IFeatureMode Mode { get; }

    /// <summary>
    /// <c>true</c> when the mode has just become active (entered);
    /// <c>false</c> when the mode has just become inactive (exited).
    /// </summary>
    public bool IsEntering { get; }

    public FeatureModeChangedEventArgs(IFeatureMode mode, bool isEntering)
    {
        Mode = mode;
        IsEntering = isEntering;
    }
}
