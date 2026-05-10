// MenuItemSemanticMetadata.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;

namespace Dev.Core.Menu;

/// <summary>
/// Immutable semantic metadata for menu text and optional icon identity.
/// </summary>
public sealed record MenuItemSemanticMetadata
{
    public ToolbarItemText Text { get; }

    public IconKey? IconKey { get; }

    public MenuItemSemanticMetadata(ToolbarItemText text, IconKey? iconKey = null)
    {
        if (text.Label is null)
            throw new ArgumentException("Menu item text must be initialized.", nameof(text));

        Text = text;
        IconKey = iconKey;
    }
}
