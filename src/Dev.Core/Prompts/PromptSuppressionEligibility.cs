// PromptSuppressionEligibility.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Determines whether a response is eligible for suppression persistence.
/// </summary>
public static class PromptSuppressionEligibility
{
    private static readonly HashSet<PromptResult> NonPersistableResults = new()
    {
        PromptResult.None,
        PromptResult.Cancel,
    };

    /// <summary>
    /// Returns true when a response may be persisted as a suppressed decision.
    /// </summary>
    public static bool IsEligible(PromptResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.IsFromUserInteraction
            && response.SuppressChecked
            && !NonPersistableResults.Contains(response.Result);
    }
}
