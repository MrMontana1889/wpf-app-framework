// IPromptSuppressionService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Manages persistence and lookup of suppressed prompt responses.
/// </summary>
public interface IPromptSuppressionService
{
    /// <summary>
    /// Returns the stored response for the given prompt, or null if none exists.
    /// </summary>
    PromptResponse? TryGetSuppressedResponse(PromptId promptId);

    /// <summary>
    /// Persists a suppressed response for the given prompt identity.
    /// The response must satisfy all persistence eligibility rules; throws if it does not.
    /// </summary>
    void PersistSuppression(PromptId promptId, PromptResponse response);

    /// <summary>
    /// Removes the stored suppression for a single prompt identity, if present.
    /// </summary>
    void ClearSuppression(PromptId promptId);

    /// <summary>
    /// Removes all stored suppressions.
    /// </summary>
    void ClearAll();
}
