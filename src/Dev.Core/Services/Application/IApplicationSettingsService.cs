// IApplicationSettingsService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Models;

namespace Dev.Core.Services;

/// <summary>
/// Service for loading and saving application settings.
/// </summary>
public interface IApplicationSettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    ApplicationSettings CurrentSettings { get; }

    /// <summary>
    /// Saves the current settings to persistent storage.
    /// </summary>
    void SaveSettings();

    /// <summary>
    /// Loads settings from persistent storage. If no saved settings exist, returns defaults.
    /// </summary>
    void LoadSettings();

    /// <summary>
    /// Adds a file path to the top of the recent files list, removing any duplicate entry
    /// and trimming the list to <see cref="ApplicationSettings.RecentFilesMaxCount"/>.
    /// Saves settings automatically.
    /// </summary>
    /// <param name="filePath">The file path to add.</param>
    void AddRecentFile(string filePath);
}
