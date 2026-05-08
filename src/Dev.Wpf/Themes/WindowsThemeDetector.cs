// WindowsThemeDetector.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;

namespace Dev.Wpf.Themes;

public enum ThemeMode
{
    Dark,
    Light,
    System
}

public static class WindowsThemeDetector
{
    #region Public Methods

    [ExcludeFromCodeCoverage]
    public static ThemeMode GetWindowsAppTheme()
    {
        try
        {
            object? value = Registry.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme",
                null);

            if (value is int intValue)
            {
                return intValue == 0 ? ThemeMode.Dark : ThemeMode.Light;
            }
        }
        catch
        {
            // Ignore and fall through
        }

        return ThemeMode.System;
    }

    #endregion Public Methods
}