// ToolbarContextMenuConverter.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Dev.Wpf.Converters;

/// <summary>
/// Builds the full item list for a toolbar context menu.
/// Expects five bindings: MenuBarToggleEntry (MenuBarToggleMenuEntry),
/// AllToolbars (IEnumerable&lt;ToolbarModel&gt;), CustomizeCommand (ICommand),
/// and CanCustomize (bool).
/// Returns a List&lt;object&gt; containing the menu bar toggle entry first,
/// then all toolbar models, and — when CanCustomize is true — a Separator
/// followed by a <see cref="ToolbarCustomizeMenuEntry"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ToolbarContextMenuConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var items = new List<object>();

        // Add Menu Bar toggle as first item
        if (values[0] is MenuBarToggleMenuEntry menuBarToggle)
        {
            items.Add(menuBarToggle);
            items.Add(new Separator());
        }

        // Add all toolbar toggles
        if (values[1] is IEnumerable<ToolbarModel> toolbars)
        {
            foreach (var toolbar in toolbars)
                items.Add(toolbar);
        }

        // Add customize entry if applicable
        bool canCustomize = values.Length > 3 && values[3] is bool b && b;
        if (canCustomize && values.Length > 2 && values[2] is ICommand customizeCommand)
        {
            items.Add(new Separator());
            items.Add(new ToolbarCustomizeMenuEntry(customizeCommand));
        }

        return items;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
