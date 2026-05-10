// MenuItemId.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Menu;

/// <summary>
/// Strongly typed semantic identity for a menu item.
/// </summary>
public readonly struct MenuItemId : IEquatable<MenuItemId>
{
    public string Value { get; }

    public MenuItemId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Menu item id cannot be empty or whitespace.", nameof(value));

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Menu item id cannot include leading or trailing whitespace.", nameof(value));

        Value = value;
    }

    public bool Equals(MenuItemId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is MenuItemId other && Equals(other);

    public override int GetHashCode() =>
        Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool operator ==(MenuItemId left, MenuItemId right) => left.Equals(right);

    public static bool operator !=(MenuItemId left, MenuItemId right) => !left.Equals(right);
}
