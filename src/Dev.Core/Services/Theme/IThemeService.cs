// IThemeService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Service for managing application theme.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Applies the specified theme.
    /// </summary>
    /// <param name="theme">Theme name: "System", "Light", or "Dark".</param>
    void ApplyTheme(string theme);
}
