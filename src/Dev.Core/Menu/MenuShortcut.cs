// MenuShortcut.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Menu;

/// <summary>
/// Structured, UI-agnostic semantic shortcut descriptor.
/// </summary>
public readonly struct MenuShortcut : IEquatable<MenuShortcut>
{
    public MenuShortcutModifiers Modifiers { get; }

    public MenuShortcutKey Key { get; }

    public MenuShortcut(MenuShortcutModifiers modifiers, MenuShortcutKey key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public bool Equals(MenuShortcut other) =>
        Modifiers == other.Modifiers && Key == other.Key;

    public override bool Equals(object? obj) =>
        obj is MenuShortcut other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine((int)Modifiers, (int)Key);

    public override string ToString() =>
        Modifiers == MenuShortcutModifiers.None
            ? Key.ToString()
            : $"{Modifiers}+{Key}";

    public static bool operator ==(MenuShortcut left, MenuShortcut right) => left.Equals(right);

    public static bool operator !=(MenuShortcut left, MenuShortcut right) => !left.Equals(right);
}
