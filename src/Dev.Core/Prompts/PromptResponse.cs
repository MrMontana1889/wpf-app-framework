// PromptResponse.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Captures decision and suppression metadata for a prompt interaction or bypass.
/// </summary>
public sealed class PromptResponse
{
    public PromptResult Result { get; }

    public bool SuppressChecked { get; }

    public bool IsFromUserInteraction { get; }

    private PromptResponse(PromptResult result, bool suppressChecked, bool isFromUserInteraction)
    {
        Result = result;
        SuppressChecked = suppressChecked;
        IsFromUserInteraction = isFromUserInteraction;
    }

    public static PromptResponse FromUserInteraction(PromptResult result, bool suppressChecked)
    {
        if (result == PromptResult.None)
            throw new ArgumentException("Result.None is reserved for non-interactive responses.", nameof(result));

        return new PromptResponse(result, suppressChecked, isFromUserInteraction: true);
    }

    public static PromptResponse FromNonInteractiveSource(PromptResult result, bool suppressChecked)
    {
        return new PromptResponse(result, suppressChecked, isFromUserInteraction: false);
    }
}
