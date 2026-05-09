// WindowPersistenceManager.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Newtonsoft.Json;

namespace Dev.Core.Services;

public class WindowPersistenceService : IWindowPersistenceService
{
    #region Constructors

    public WindowPersistenceService(string baseFolderPath)
    {
        if (string.IsNullOrWhiteSpace(baseFolderPath))
            throw new ArgumentException("Base folder path must be provided.", nameof(baseFolderPath));

        Directory.CreateDirectory(baseFolderPath);
        _filePath = Path.Combine(baseFolderPath, "WindowState.json");

        Load();
    }

    #endregion

    #region Public Methods

    public WindowSettings? LoadWindowState(string key)
    {
        return _state.TryGetValue(key, out var windowSettings)
            ? windowSettings
            : null;
    }

    public void ResetWindowState(string key)
    {
        if (_state.ContainsKey(key))
        {
            _state.Remove(key);
            Save();
        }
    }

    public void SaveWindowState(string key, WindowSettings settings)
    {
        _state[key] = settings;
        Save();
    }

    #endregion

    #region Private Methods

    private void Load()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                _state = JsonConvert.DeserializeObject<Dictionary<string, WindowSettings>>(json)
                         ?? new Dictionary<string, WindowSettings>();
            }
            catch
            {
                _state = new Dictionary<string, WindowSettings>();
            }
        }
        else
        {
            _state = new Dictionary<string, WindowSettings>();
        }
    }

    private void Save()
    {
        var json = JsonConvert.SerializeObject(_state, Formatting.Indented);
        File.WriteAllText(_filePath, json);
    }

    #endregion

    #region Private Fields

    private readonly string _filePath;
    private Dictionary<string, WindowSettings> _state = new();

    #endregion
}
