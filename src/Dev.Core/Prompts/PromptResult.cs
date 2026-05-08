// PromptResult.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Prompts;

/// <summary>
/// Prompt decision result captured by the contract layer.
/// </summary>
public enum PromptResult
{
    None,
    Yes,
    No,
    Ok,
    Cancel,
}
