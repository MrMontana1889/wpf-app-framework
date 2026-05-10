// MenuShortcutToTextConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using System.Globalization;
using System.Windows.Data;

namespace Dev.Wpf.Converters;

/// <summary>
/// Converts a structured <see cref="MenuShortcut"/> to display text (for example Ctrl+Shift+N).
/// </summary>
[ValueConversion(typeof(MenuShortcut), typeof(string))]
public sealed class MenuShortcutToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MenuShortcut shortcut)
            return string.Empty;

        var parts = new List<string>(5);

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Ctrl))
            parts.Add("Ctrl");

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Shift))
            parts.Add("Shift");

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Alt))
            parts.Add("Alt");

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Meta))
            parts.Add("Meta");

        parts.Add(shortcut.Key.ToString());

        return string.Join("+", parts);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
