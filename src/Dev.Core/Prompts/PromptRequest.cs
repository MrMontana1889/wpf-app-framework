// PromptRequest.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Represents a single invocation of a registered prompt identity.
/// </summary>
public sealed class PromptRequest
{
    private static readonly IReadOnlyList<object?> EmptyParameters = Array.Empty<object?>();

    public PromptId PromptId { get; }

    public IReadOnlyList<object?> Parameters { get; }

    public PromptPresentationOverrides? PresentationOverrides { get; }

    public PromptRequest(
        PromptId promptId,
        IReadOnlyList<object?>? parameters = null,
        PromptPresentationOverrides? presentationOverrides = null)
    {
        PromptId = promptId;
        Parameters = parameters is null ? EmptyParameters : parameters.ToArray();
        PresentationOverrides = presentationOverrides;
    }
}
