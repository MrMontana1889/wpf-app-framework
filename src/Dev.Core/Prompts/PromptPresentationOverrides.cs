// PromptPresentationOverrides.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Prompts;

/// <summary>
/// Optional request-time overrides that can affect presentation only.
/// </summary>
public sealed class PromptPresentationOverrides
{
    public string? TitleTemplateOverride { get; }

    public string? MessageTemplateOverride { get; }

    public PromptPresentationOverrides(string? titleTemplateOverride = null, string? messageTemplateOverride = null)
    {
        TitleTemplateOverride = titleTemplateOverride;
        MessageTemplateOverride = messageTemplateOverride;
    }
}
