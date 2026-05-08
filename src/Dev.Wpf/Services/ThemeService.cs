// ThemeService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Wpf.Themes;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Services;

[ExcludeFromCodeCoverage]
public class ThemeService : IThemeService
{
    public void ApplyTheme(string theme)
    {
#pragma warning disable WPF0001
        Application.Current.ThemeMode = theme switch
        {
            "Dark" => System.Windows.ThemeMode.Dark,
            "Light" => System.Windows.ThemeMode.Light,
            _ => System.Windows.ThemeMode.System
        };
#pragma warning restore WPF0001

        if (theme == "System")
            ThemeManager.ApplySystemTheme();
        else
            ThemeManager.ApplyTheme(theme);
    }
}
