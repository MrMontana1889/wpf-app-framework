// MenuShortcutToTextConverterTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using Dev.Wpf.Converters;
using NUnit.Framework;
using System.Globalization;

namespace Dev.Wpf.Tests.Converters;

[TestFixture]
public sealed class MenuShortcutToTextConverterTests
{
    [TestCase(MenuShortcutModifiers.None, MenuShortcutKey.Enter, "Enter")]
    [TestCase(MenuShortcutModifiers.Ctrl, MenuShortcutKey.N, "Ctrl+N")]
    [TestCase(MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift, MenuShortcutKey.N, "Ctrl+Shift+N")]
    [TestCase(MenuShortcutModifiers.Alt | MenuShortcutModifiers.Meta, MenuShortcutKey.F5, "Alt+Meta+F5")]
    [TestCase(MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift | MenuShortcutModifiers.Alt | MenuShortcutModifiers.Meta, MenuShortcutKey.Delete, "Ctrl+Shift+Alt+Meta+Delete")]
    public void Convert_FormatsShortcutInStableModifierOrder(MenuShortcutModifiers modifiers, MenuShortcutKey key, string expected)
    {
        var converter = new MenuShortcutToTextConverter();

        var text = converter.Convert(new MenuShortcut(modifiers, key), typeof(string), null!, CultureInfo.InvariantCulture);

        Assert.That(text, Is.EqualTo(expected));
    }
}
