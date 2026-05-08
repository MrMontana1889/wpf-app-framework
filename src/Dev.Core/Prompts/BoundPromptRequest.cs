// BoundPromptRequest.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Explicit binding between a request and its resolved static definition.
/// </summary>
public sealed class BoundPromptRequest
{
    public PromptDefinition Definition { get; }

    public IReadOnlyList<object?> Parameters { get; }

    public PromptPresentationOverrides? PresentationOverrides { get; }

    public BoundPromptRequest(PromptRequest request, PromptDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(definition);

        if (request.PromptId != definition.Id)
            throw new InvalidOperationException(
                $"Cannot bind request for '{request.PromptId}' to definition '{definition.Id}'.");

        Definition = definition;
        Parameters = request.Parameters;
        PresentationOverrides = request.PresentationOverrides;
    }
}
