// ToolbarItemId.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// Strongly typed semantic identity for a toolbar item.
/// </summary>
public readonly struct ToolbarItemId : IEquatable<ToolbarItemId>
{
    public string Value { get; }

    public ToolbarItemId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Toolbar item id cannot be empty or whitespace.", nameof(value));

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Toolbar item id cannot include leading or trailing whitespace.", nameof(value));

        Value = value;
    }

    public bool Equals(ToolbarItemId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is ToolbarItemId other && Equals(other);

    public override int GetHashCode() =>
        Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool operator ==(ToolbarItemId left, ToolbarItemId right) => left.Equals(right);

    public static bool operator !=(ToolbarItemId left, ToolbarItemId right) => !left.Equals(right);
}
