// IconKey.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Toolbar;

/// <summary>
/// Opaque semantic identifier for an icon.
/// The key carries no information about icon storage, format, loading, or rendering.
/// </summary>
public readonly struct IconKey : IEquatable<IconKey>
{
    public string Value { get; }

    public IconKey(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Icon key cannot be empty or whitespace.", nameof(value));

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Icon key cannot include leading or trailing whitespace.", nameof(value));

        Value = value;
    }

    public bool Equals(IconKey other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is IconKey other && Equals(other);

    public override int GetHashCode() =>
        Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool operator ==(IconKey left, IconKey right) => left.Equals(right);

    public static bool operator !=(IconKey left, IconKey right) => !left.Equals(right);
}
