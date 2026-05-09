// IMenuBarToggleStateProvider.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Abstraction for a Menu Bar visibility state provider.
/// Allows menu entry models to observe the authoritative Menu Bar user preference
/// without taking a dependency on application-level ViewModels.
/// </summary>
public interface IMenuBarToggleStateProvider
{
    /// <summary>
    /// Gets the current Menu Bar user visibility preference.
    /// This is the authoritative user intent, independent of feature mode suppression.
    /// </summary>
    bool IsMenuBarUserVisible { get; }
}
