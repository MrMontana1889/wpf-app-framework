// ToolbarRegistryService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using System.Text.Json;

namespace Dev.Core.Services;

/// <summary>
/// Persists and restores toolbar row-level (show / hide) visibility state
/// to a JSON file on disk, keyed by <see cref="ToolbarId"/>.
/// Semantic definitions are the only registration path.
/// </summary>
public class ToolbarRegistryService : IToolbarRegistryService
{
    private readonly string _settingsFilePath;
    private readonly List<ToolbarDefinition> _definitions = new();
    private readonly Dictionary<ToolbarId, ToolbarDefinition> _definitionsById = new();
    private readonly Dictionary<ToolbarId, bool> _visibilityById = new();
    private readonly Dictionary<(ToolbarId ToolbarId, ToolbarItemId ItemId), bool> _itemVisibilityByCompositeId = new();
    private Dictionary<string, bool> _persistedVisibilityData = new(StringComparer.Ordinal);
    private Dictionary<string, Dictionary<string, bool>> _persistedItemVisibilityData = new(StringComparer.Ordinal);

    public event EventHandler<ToolbarVisibilityChangedEventArgs>? VisibilityChanged;
    public event EventHandler<ToolbarItemVisibilityChangedEventArgs>? ItemVisibilityChanged;

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

        RegisterItems(definition);
    }

    public bool IsVisible(ToolbarId toolbarId)
    {
        if (!_definitionsById.ContainsKey(toolbarId))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        return _visibilityById[toolbarId];
    }

    public bool IsItemVisible(ToolbarId toolbarId, ToolbarItemId itemId)
    {
        if (!_definitionsById.ContainsKey(toolbarId))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        var key = (toolbarId, itemId);
        
        // Return existing visibility if tracked, otherwise default to true
        if (_itemVisibilityByCompositeId.TryGetValue(key, out var isVisible))
            return isVisible;

        // Item hasn't been seen before; assume visible by default
        return true;
    }

    public void SetVisibility(ToolbarId toolbarId, bool isVisible)
    {
        if (!_definitionsById.TryGetValue(toolbarId, out var definition))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        var effectiveVisibility = definition.CanHide ? isVisible : true;
        if (_visibilityById.TryGetValue(toolbarId, out var currentVisibility) && currentVisibility == effectiveVisibility)
            return;

        _visibilityById[toolbarId] = effectiveVisibility;
        SaveVisibility();
        VisibilityChanged?.Invoke(this, new ToolbarVisibilityChangedEventArgs(toolbarId, effectiveVisibility));
    }

    public void SetItemVisibility(ToolbarId toolbarId, ToolbarItemId itemId, bool isVisible)
    {
        if (!_definitionsById.ContainsKey(toolbarId))
            throw new InvalidOperationException($"Toolbar '{toolbarId}' is not registered.");

        var key = (toolbarId, itemId);
        
        // Check if visibility is already at target state
        if (_itemVisibilityByCompositeId.TryGetValue(key, out var currentVisibility) && currentVisibility == isVisible)
            return;

        // Store visibility state (registry owns only state, not items)
        _itemVisibilityByCompositeId[key] = isVisible;
        SaveVisibility();
        ItemVisibilityChanged?.Invoke(this, new ToolbarItemVisibilityChangedEventArgs(toolbarId, itemId, isVisible));
    }

    public void SaveVisibility()
    {
        foreach (var (toolbarId, isVisible) in _visibilityById)
            _persistedVisibilityData[toolbarId.Value] = isVisible;

        foreach (var ((toolbarId, itemId), isVisible) in _itemVisibilityByCompositeId)
        {
            if (!_persistedItemVisibilityData.TryGetValue(toolbarId.Value, out var toolbarItems))
            {
                toolbarItems = new Dictionary<string, bool>(StringComparer.Ordinal);
                _persistedItemVisibilityData[toolbarId.Value] = toolbarItems;
            }

            toolbarItems[itemId.Value] = isVisible;
        }

        var payload = new ToolbarRegistryState
        {
            Toolbars = _persistedVisibilityData,
            Items = _persistedItemVisibilityData
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_settingsFilePath))
            return;

        var json = File.ReadAllText(_settingsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            // Backward compatibility: previous format was Dictionary<string, bool>.
            var legacyData = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            if (legacyData is not null)
            {
                _persistedVisibilityData = legacyData;
                return;
            }
        }
        catch (JsonException)
        {
            // Not legacy format.
        }

        try
        {
            var state = JsonSerializer.Deserialize<ToolbarRegistryState>(json);
            if (state is null)
                return;

            _persistedVisibilityData = state.Toolbars is null
                ? new Dictionary<string, bool>(StringComparer.Ordinal)
                : new Dictionary<string, bool>(state.Toolbars, StringComparer.Ordinal);

            _persistedItemVisibilityData = state.Items is null
                ? new Dictionary<string, Dictionary<string, bool>>(StringComparer.Ordinal)
                : state.Items.ToDictionary(
                    pair => pair.Key,
                    pair => new Dictionary<string, bool>(pair.Value, StringComparer.Ordinal),
                    StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            // Corrupt payload: ignore and continue with defaults.
        }
    }

    private void RegisterItems(ToolbarDefinition definition)
    {
        foreach (var itemId in definition.ItemIds)
        {
            var key = (definition.Id, itemId);

            // Initialize item visibility from persisted state if available
            var itemVisible = true; // default to visible
            if (_persistedItemVisibilityData.TryGetValue(definition.Id.Value, out var itemMap)
                && itemMap.TryGetValue(itemId.Value, out var persistedItemVisible))
            {
                itemVisible = persistedItemVisible;
            }

            // Only register if not already present
            _itemVisibilityByCompositeId.TryAdd(key, itemVisible);
        }
    }

    private sealed class ToolbarRegistryState
    {
        public Dictionary<string, bool> Toolbars { get; set; } = new(StringComparer.Ordinal);
        public Dictionary<string, Dictionary<string, bool>> Items { get; set; } = new(StringComparer.Ordinal);
    }
}
