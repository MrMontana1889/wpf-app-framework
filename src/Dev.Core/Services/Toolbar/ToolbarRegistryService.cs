// ToolbarRegistryService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Persists and restores toolbar row-level (show / hide) visibility state
/// to a JSON file on disk, keyed by <see cref="ToolbarModel.Name"/>.
/// </summary>
public class ToolbarRegistryService : IToolbarRegistryService
{
    private readonly string _settingsFilePath;
    private readonly List<ToolbarModel> _toolbars = new();
    private Dictionary<string, bool> _visibilityData = new();

    public IReadOnlyList<ToolbarModel> Toolbars => _toolbars.AsReadOnly();

    public ToolbarRegistryService(string appDataPath)
    {
        _settingsFilePath = Path.Combine(appDataPath, "toolbar-registry.json");
        LoadFromDisk();
    }

    public void Register(ToolbarModel toolbar, bool canHide = true)
    {
        toolbar.CanHide = canHide;
        _toolbars.Add(toolbar);

        // Restore persisted visibility for this toolbar
        if (_visibilityData.TryGetValue(toolbar.Name, out var savedVisibility))
            toolbar.IsToolbarVisible = savedVisibility;

        // Auto-save whenever the toolbar's row visibility changes
        toolbar.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolbarModel.IsToolbarVisible))
                SaveVisibility();
        };
    }

    public void SaveVisibility()
    {
        foreach (var toolbar in _toolbars)
            _visibilityData[toolbar.Name] = toolbar.IsToolbarVisible;

        var json = JsonSerializer.Serialize(_visibilityData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_settingsFilePath))
            return;

        var json = File.ReadAllText(_settingsFilePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
        if (data is not null)
            _visibilityData = data;
    }
}
