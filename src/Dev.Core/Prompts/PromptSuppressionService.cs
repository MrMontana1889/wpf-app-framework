// PromptSuppressionService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Text.Json;

namespace Dev.Core.Prompts;

/// <summary>
/// Disk-backed, user-profile-scoped implementation of <see cref="IPromptSuppressionService"/>.
/// </summary>
public sealed class PromptSuppressionService : IPromptSuppressionService
{
    private const string FileName = "prompt-suppressions.json";

    private readonly string _filePath;
    private readonly Action<string> _log;

    // Mutable in-memory store: promptId.Value → PromptResult
    private Dictionary<string, string> _store;

    public PromptSuppressionService(string profileDirectory, Action<string>? log = null)
    {
        ArgumentNullException.ThrowIfNull(profileDirectory);

        _filePath = Path.Combine(profileDirectory, FileName);
        _log = log ?? (_ => { });
        _store = LoadFromDisk();
    }

    public PromptResponse? TryGetSuppressedResponse(PromptId promptId)
    {
        if (!_store.TryGetValue(promptId.Value, out var resultName))
            return null;

        if (!Enum.TryParse<PromptResult>(resultName, ignoreCase: false, out var result))
        {
            _log($"[PromptSuppressionService] Unrecognised stored result '{resultName}' for prompt '{promptId}'. Entry ignored.");
            return null;
        }

        // A stored result must always be a concrete interactive result.
        // Guard defensively against corrupted or pre-existing data.
        if (result is PromptResult.None or PromptResult.Cancel)
        {
            _log($"[PromptSuppressionService] Stored result '{result}' for prompt '{promptId}' is not persistable. Entry ignored.");
            return null;
        }

        return PromptResponse.FromNonInteractiveSource(result, suppressChecked: false);
    }

    public void PersistSuppression(PromptId promptId, PromptResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!PromptSuppressionEligibility.IsEligible(response))
            throw new ArgumentException(
                $"Response is not eligible for suppression persistence (result: {response.Result}, " +
                $"suppressChecked: {response.SuppressChecked}, isFromUserInteraction: {response.IsFromUserInteraction}).",
                nameof(response));

        _store[promptId.Value] = response.Result.ToString();
        SaveToDisk();
    }

    public void ClearSuppression(PromptId promptId)
    {
        if (_store.Remove(promptId.Value))
            SaveToDisk();
    }

    public void ClearAll()
    {
        if (_store.Count == 0)
            return;

        _store.Clear();
        SaveToDisk();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private Dictionary<string, string> LoadFromDisk()
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, string>();

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (data is null)
            {
                _log($"[PromptSuppressionService] Deserialization returned null for '{_filePath}'. Starting with empty store.");
                return new Dictionary<string, string>();
            }

            return data;
        }
        catch (Exception ex)
        {
            _log($"[PromptSuppressionService] Failed to load suppressions from '{_filePath}': {ex.Message}. Starting with empty store.");
            return new Dictionary<string, string>();
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var json = JsonSerializer.Serialize(_store, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _log($"[PromptSuppressionService] Failed to save suppressions to '{_filePath}': {ex.Message}. In-memory state is unchanged.");
        }
    }
}
