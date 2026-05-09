// IPromptRegistry.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Prompts;

/// <summary>
/// Authoritative runtime catalog of known prompt definitions.
/// </summary>
public interface IPromptRegistry
{
    IReadOnlyCollection<PromptDefinition> Definitions { get; }

    void Register(PromptDefinition definition);

    PromptDefinition GetDefinition(PromptId promptId);

    bool TryGetDefinition(PromptId promptId, out PromptDefinition? definition);
}
