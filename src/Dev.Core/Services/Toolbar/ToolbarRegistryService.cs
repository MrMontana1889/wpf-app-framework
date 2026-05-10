// ToolbarRegistryService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using Dev.Core.ViewModels.Controls;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Persists and restores toolbar row-level (show / hide) visibility state
/// to a JSON file on disk, keyed by <see cref="ToolbarId"/>.
/// </summary>
public class ToolbarRegistryService : IToolbarRegistryService
{
    private readonly string _settingsFilePath;
    private readonly List<ToolbarModel> _toolbars = new();
    private readonly List<ToolbarDefinition> _definitions = new();
    private readonly Dictionary<ToolbarId, ToolbarDefinition> _definitionsById = new();
    private readonly Dictionary<ToolbarId, bool> _visibilityById = new();
    private readonly Dictionary<ToolbarId, List<ToolbarModel>> _toolbarsById = new();
    private Dictionary<string, bool> _persistedVisibilityData = new(StringComparer.Ordinal);

    public event EventHandler<ToolbarVisibilityChangedEventArgs>? VisibilityChanged;

    public IReadOnlyList<ToolbarModel> Toolbars => _toolbars.AsReadOnly();

    public IReadOnlyList<ToolbarDefinition> ToolbarDefinitions => _definitions.AsReadOnly();

    public ToolbarRegistryService(string appDataPath)
    {
        _settingsFilePath = Path.Combine(appDataPath, "toolbar-registry.json");
        LoadFromDisk();
    }

    public void RegisterDefinition(ToolbarDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_definitionsById.ContainsKey(definition.Id))
            throw new InvalidOperationException($"Toolbar definition '{definition.Id}' is already registered.");

        _definitions.Add(definition);
        _definitionsById.Add(definition.Id, definition);

        var isVisible = definition.DefaultVisible;
        if (_persistedVisibilityData.TryGetValue(definition.Id.Value, out var persistedVisibility))
            isVisible = persistedVisibility;

        // Policy is authoritative: non-hideable toolbars must remain visible.
        _visibilityById[definition.Id] = definition.CanHide ? isVisible : true;
        ApplyVisibilityToRegisteredToolbars(definition.Id);
    }

    public bool IsVisible(ToolbarId toolbarId)
    {
        if (!_definitionsById.ContainsKey(toolbarId))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        return _visibilityById[toolbarId];
    }

    public void SetVisibility(ToolbarId toolbarId, bool isVisible)
    {
        if (!_definitionsById.TryGetValue(toolbarId, out var definition))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        var effectiveVisibility = definition.CanHide ? isVisible : true;
        if (_visibilityById.TryGetValue(toolbarId, out var currentVisibility) && currentVisibility == effectiveVisibility)
            return;

        _visibilityById[toolbarId] = effectiveVisibility;
        ApplyVisibilityToRegisteredToolbars(toolbarId);
        SaveVisibility();
        VisibilityChanged?.Invoke(this, new ToolbarVisibilityChangedEventArgs(toolbarId, effectiveVisibility));
    }

    public void Register(ToolbarModel toolbar, bool canHide = true)
    {
        ArgumentNullException.ThrowIfNull(toolbar);

        var toolbarId = new ToolbarId(toolbar.Name);
        if (!_definitionsById.TryGetValue(toolbarId, out var definition))
        {
            definition = new ToolbarDefinition(
                id: toolbarId,
                displayName: toolbar.Name,
                canHide: canHide,
                defaultVisible: toolbar.IsToolbarVisible);
            RegisterDefinition(definition);
        }
        else if (definition.CanHide != canHide)
        {
            throw new InvalidOperationException(
                $"Toolbar '{toolbar.Name}' was already registered with CanHide={definition.CanHide}.");
        }

        _toolbars.Add(toolbar);
        if (!_toolbarsById.TryGetValue(toolbarId, out var instances))
        {
            instances = new List<ToolbarModel>();
            _toolbarsById[toolbarId] = instances;
        }

        instances.Add(toolbar);

        toolbar.CanHide = definition.CanHide;
        toolbar.IsToolbarVisible = _visibilityById[toolbarId];

        toolbar.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolbarModel.IsToolbarVisible))
                SetVisibility(toolbarId, toolbar.IsToolbarVisible);
        };
    }

    public void SaveVisibility()
    {
        foreach (var (toolbarId, isVisible) in _visibilityById)
            _persistedVisibilityData[toolbarId.Value] = isVisible;

        var json = JsonSerializer.Serialize(_persistedVisibilityData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }

    private void ApplyVisibilityToRegisteredToolbars(ToolbarId toolbarId)
    {
        if (!_toolbarsById.TryGetValue(toolbarId, out var toolbars))
            return;

        var isVisible = _visibilityById[toolbarId];
        foreach (var toolbar in toolbars)
        {
            if (toolbar.IsToolbarVisible != isVisible)
                toolbar.IsToolbarVisible = isVisible;
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_settingsFilePath))
            return;

        var json = File.ReadAllText(_settingsFilePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
        if (data is not null)
            _persistedVisibilityData = data;
    }
}
