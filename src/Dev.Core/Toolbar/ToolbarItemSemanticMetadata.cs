// ToolbarItemSemanticMetadata.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// Immutable semantic metadata for toolbar presentation text and optional icon identity.
/// Contains no UI, rendering, or behavior concerns.
/// </summary>
public sealed record ToolbarItemSemanticMetadata
{
    public ToolbarItemText Text { get; }

    public IconKey? IconKey { get; }

    public ToolbarItemSemanticMetadata(ToolbarItemText text, IconKey? iconKey = null)
    {
        if (text.Label is null)
            throw new ArgumentException("Toolbar item text must be initialized.", nameof(text));

        Text = text;
        IconKey = iconKey;
    }
}
