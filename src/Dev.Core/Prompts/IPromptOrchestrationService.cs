// IPromptOrchestrationService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Orchestrates the full lifecycle of a prompt: suppression lookup,
/// optional presentation, and conditional persistence.
/// </summary>
public interface IPromptOrchestrationService
{
    /// <summary>
    /// Executes the prompt identified by <paramref name="request"/>.
    /// Returns the caller's response in all cases, whether interactive or suppressed.
    /// </summary>
    PromptResponse Execute(PromptRequest request);
}
