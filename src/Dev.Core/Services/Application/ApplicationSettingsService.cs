// ApplicationSettingsService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Models;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Implementation of application settings service with JSON file persistence.
/// </summary>
public class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly string _settingsFilePath;
    private ApplicationSettings _currentSettings;

    public ApplicationSettings CurrentSettings => _currentSettings;

    public ApplicationSettingsService(string appDataPath)
    {
        _settingsFilePath = Path.Combine(appDataPath, "appsettings.json");
        _currentSettings = new ApplicationSettings();
        LoadSettings();
    }

    public void SaveSettings()
    {
        var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_settingsFilePath, json);
    }

    public void LoadSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
            if (settings != null)
            {
                _currentSettings = settings;
                return;
            }
        }

        // Use defaults if file doesn't exist or deserialization returned null
        _currentSettings = new ApplicationSettings();
    }

    public void AddRecentFile(string filePath)
    {
        var recent = _currentSettings.RecentFiles;
        recent.Remove(filePath);
        recent.Insert(0, filePath);
        while (recent.Count > _currentSettings.RecentFilesMaxCount)
            recent.RemoveAt(recent.Count - 1);
        SaveSettings();
    }
}
