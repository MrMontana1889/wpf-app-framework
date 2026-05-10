// ToolbarItemText.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// UI-agnostic text metadata for a toolbar item.
/// </summary>
public readonly struct ToolbarItemText : IEquatable<ToolbarItemText>
{
    /// <summary>
    /// Primary user-facing label or caption for the toolbar item.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Optional descriptive text suitable for tooltip content or accessibility narration.
    /// </summary>
    public string? AssistiveText { get; }

    public ToolbarItemText(string label, string? assistiveText = null)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Toolbar label cannot be empty or whitespace.", nameof(label));

        if (!string.Equals(label, label.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Toolbar label cannot include leading or trailing whitespace.", nameof(label));

        if (assistiveText is not null)
        {
            if (string.IsNullOrWhiteSpace(assistiveText))
                throw new ArgumentException("Assistive text cannot be empty or whitespace when provided.", nameof(assistiveText));

            if (!string.Equals(assistiveText, assistiveText.Trim(), StringComparison.Ordinal))
                throw new ArgumentException("Assistive text cannot include leading or trailing whitespace.", nameof(assistiveText));
        }

        Label = label;
        AssistiveText = assistiveText;
    }

    public bool Equals(ToolbarItemText other) =>
        string.Equals(Label, other.Label, StringComparison.Ordinal)
        && string.Equals(AssistiveText, other.AssistiveText, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is ToolbarItemText other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(
            Label is null ? 0 : StringComparer.Ordinal.GetHashCode(Label),
            AssistiveText is null ? 0 : StringComparer.Ordinal.GetHashCode(AssistiveText));

    public override string ToString() => Label ?? string.Empty;

    public static bool operator ==(ToolbarItemText left, ToolbarItemText right) => left.Equals(right);

    public static bool operator !=(ToolbarItemText left, ToolbarItemText right) => !left.Equals(right);
}
