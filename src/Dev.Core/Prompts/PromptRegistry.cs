// PromptRegistry.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// In-memory registry enforcing unique prompt identities.
/// </summary>
public sealed class PromptRegistry : IPromptRegistry
{
    private readonly Dictionary<PromptId, PromptDefinition> _definitions = new();

    public IReadOnlyCollection<PromptDefinition> Definitions => _definitions.Values;

    public void Register(PromptDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Prompt '{definition.Id}' is already registered.");
    }

    public PromptDefinition GetDefinition(PromptId promptId)
    {
        if (!_definitions.TryGetValue(promptId, out var definition))
            throw new KeyNotFoundException($"Prompt '{promptId}' is not registered.");

        return definition;
    }

    public bool TryGetDefinition(PromptId promptId, out PromptDefinition? definition)
    {
        return _definitions.TryGetValue(promptId, out definition);
    }
}
