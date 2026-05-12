// ToolbarDefinition.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// Immutable semantic definition for a toolbar.
/// Carries the full item list alongside visibility metadata.
/// </summary>
public sealed class ToolbarDefinition
{
    public ToolbarId Id { get; }

    public string DisplayName { get; }

    public bool CanHide { get; }

    public bool DefaultVisible { get; }

    /// <summary>
    /// The ordered list of semantic toolbar item IDs that belong to this toolbar.
    /// Items are provided by the application via ItemsSource; this list is for metadata only.
    /// </summary>
    public IReadOnlyList<ToolbarItemId> ItemIds { get; }

    public ToolbarDefinition(
        ToolbarId id,
        string displayName,
        bool canHide = true,
        bool defaultVisible = true,
        IReadOnlyList<ToolbarItemId>? itemIds = null)
    {
        if (id == default)
            throw new ArgumentException("Toolbar id must be initialized.", nameof(id));

        ArgumentNullException.ThrowIfNull(displayName);

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Toolbar display name cannot be empty or whitespace.", nameof(displayName));

        if (!string.Equals(displayName, displayName.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Toolbar display name cannot include leading or trailing whitespace.", nameof(displayName));

        Id = id;
        DisplayName = displayName;
        CanHide = canHide;
        DefaultVisible = defaultVisible;
        ItemIds = itemIds is null ? [] : itemIds.ToArray();
    }
}