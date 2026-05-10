// ToolbarId.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// Strongly typed semantic identity for a toolbar.
/// </summary>
public readonly struct ToolbarId : IEquatable<ToolbarId>
{
    public string Value { get; }

    public ToolbarId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Toolbar id cannot be empty or whitespace.", nameof(value));

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Toolbar id cannot include leading or trailing whitespace.", nameof(value));

        Value = value;
    }

    public bool Equals(ToolbarId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is ToolbarId other && Equals(other);

    public override int GetHashCode() =>
        Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool operator ==(ToolbarId left, ToolbarId right) => left.Equals(right);

    public static bool operator !=(ToolbarId left, ToolbarId right) => !left.Equals(right);
}