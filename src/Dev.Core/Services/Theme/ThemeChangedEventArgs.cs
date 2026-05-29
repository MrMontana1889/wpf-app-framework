// ThemeChangedEventArgs.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Provides data for the <see cref="IThemeService.ThemeChanged"/> event.
/// </summary>
public sealed class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The final resolved theme value.
    /// This value is always either "Light" or "Dark".
    /// </summary>
    public string Theme { get; }

    public ThemeChangedEventArgs(string theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        Theme = theme;
    }
}
