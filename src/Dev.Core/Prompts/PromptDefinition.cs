// PromptDefinition.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Immutable static metadata for a prompt identity.
/// </summary>
public sealed class PromptDefinition
{
    public PromptId Id { get; }

    public string TitleTemplate { get; }

    public string MessageTemplate { get; }

    public PromptButtonSet ButtonSet { get; }

    public PromptResult DefaultResult { get; }

    public bool AllowSuppression { get; }

    public string SuppressionText { get; }

    public PromptDefinition(
        PromptId id,
        string titleTemplate,
        string messageTemplate,
        PromptButtonSet buttonSet,
        PromptResult defaultResult,
        bool allowSuppression,
        string? suppressionText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titleTemplate);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageTemplate);

        if (allowSuppression)
            ArgumentException.ThrowIfNullOrWhiteSpace(suppressionText);

        if (!IsDefaultResultValidForButtonSet(buttonSet, defaultResult))
            throw new ArgumentException(
                $"Default result '{defaultResult}' is invalid for button set '{buttonSet}'.",
                nameof(defaultResult));

        Id = id;
        TitleTemplate = titleTemplate;
        MessageTemplate = messageTemplate;
        ButtonSet = buttonSet;
        DefaultResult = defaultResult;
        AllowSuppression = allowSuppression;
        SuppressionText = suppressionText ?? string.Empty;
    }

    private static bool IsDefaultResultValidForButtonSet(PromptButtonSet buttonSet, PromptResult defaultResult)
    {
        return buttonSet switch
        {
            PromptButtonSet.OkCancel => defaultResult is PromptResult.Ok or PromptResult.Cancel,
            PromptButtonSet.YesNo => defaultResult is PromptResult.Yes or PromptResult.No,
            PromptButtonSet.YesNoCancel => defaultResult is PromptResult.Yes or PromptResult.No or PromptResult.Cancel,
            _ => false,
        };
    }
}
