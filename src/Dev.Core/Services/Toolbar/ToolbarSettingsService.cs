// ToolbarSettingsService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Persists toolbar item-level visibility settings to a JSON file on disk.
/// Keyed by <see cref="ToolbarId"/> (outer) and <see cref="ToolbarItemId"/> (inner).
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

    public void Save(ToolbarId toolbarId, IEnumerable<ToolbarItem> items)
    {
        _data[toolbarId.Value] = items
            .Select(i => new ToolbarItemSettings { Name = i.Id.Value, IsVisible = i.IsVisible })
            .ToList();
        SaveToDisk();
    }

    public void Load(ToolbarId toolbarId, IEnumerable<ToolbarItem> items)
    {
        if (!_data.TryGetValue(toolbarId.Value, out var saved))
            return;

        foreach (var item in items)
        {
            var entry = saved.FirstOrDefault(s => s.Name == item.Id.Value);
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
