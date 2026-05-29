// IThemeService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Service for managing application theme.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the currently active resolved theme.
    /// The value is always either "Light" or "Dark".
    /// </summary>
    string CurrentTheme { get; }

    /// <summary>
    /// Fires after a theme has been applied.
    /// The emitted theme is always the final resolved theme value.
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">Theme name: "System", "Light", or "Dark".</param>
    void ApplyTheme(string theme);
}
