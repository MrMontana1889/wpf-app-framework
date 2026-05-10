// MenuShortcutModifiers.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Menu;

/// <summary>
/// Semantic modifier keys for a menu shortcut.
/// </summary>
[Flags]
public enum MenuShortcutModifiers
{
    None = 0,
    Ctrl = 1 << 0,
    Shift = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3,
}
