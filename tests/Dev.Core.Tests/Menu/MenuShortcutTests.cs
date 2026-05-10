// MenuShortcutTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using NUnit.Framework;

namespace Dev.Core.Tests.Menu;

[TestFixture]
public class MenuShortcutTests
{
    [Test]
    public void Equals_WithSameModifiersAndKey_ReturnsTrue()
    {
        var left = new MenuShortcut(MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift, MenuShortcutKey.N);
        var right = new MenuShortcut(MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift, MenuShortcutKey.N);

        Assert.That(left.Equals(right), Is.True);
    }

    [Test]
    public void Equals_WithDifferentKey_ReturnsFalse()
    {
        var left = new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.N);
        var right = new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.O);

        Assert.That(left.Equals(right), Is.False);
    }

    [Test]
    public void GetHashCode_WithSameValues_Matches()
    {
        var left = new MenuShortcut(MenuShortcutModifiers.Alt, MenuShortcutKey.F5);
        var right = new MenuShortcut(MenuShortcutModifiers.Alt, MenuShortcutKey.F5);

        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
    }

    [Test]
    public void ToString_WithNoModifiers_ReturnsKeyName()
    {
        var shortcut = new MenuShortcut(MenuShortcutModifiers.None, MenuShortcutKey.Enter);

        Assert.That(shortcut.ToString(), Is.EqualTo("Enter"));
    }
}
