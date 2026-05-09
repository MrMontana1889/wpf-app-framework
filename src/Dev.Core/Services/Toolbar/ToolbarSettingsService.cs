// ToolbarSettingsService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.ViewModels.Controls;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Persists toolbar item visibility settings to a JSON file on disk.
/// </summary>
public class ToolbarSettingsService : IToolbarSettingsService
{
    private readonly string _settingsFilePath;
    private Dictionary<string, List<ToolbarItemSettings>> _data = new();

    public ToolbarSettingsService(string appDataPath)
    {
        _settingsFilePath = Path.Combine(appDataPath, "toolbar-settings.json");
        LoadFromDisk();
    }

    public void Save(string toolbarName, IEnumerable<ToolbarItemModel> items)
    {
        _data[toolbarName] = items
            .Select(i => new ToolbarItemSettings { Name = i.Name, IsVisible = i.IsVisible })
            .ToList();
        SaveToDisk();
    }

    public void Load(string toolbarName, IEnumerable<ToolbarItemModel> items)
    {
        if (!_data.TryGetValue(toolbarName, out var saved))
            return;

        foreach (var item in items)
        {
            var entry = saved.FirstOrDefault(s => s.Name == item.Name);
            if (entry is not null)
                item.IsVisible = entry.IsVisible;
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_settingsFilePath))
            return;

        var json = File.ReadAllText(_settingsFilePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<ToolbarItemSettings>>>(json);
        if (data is not null)
            _data = data;
    }

    private void SaveToDisk()
    {
        var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }
}
