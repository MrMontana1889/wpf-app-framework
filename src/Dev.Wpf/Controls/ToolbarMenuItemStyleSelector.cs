// ToolbarMenuItemStyleSelector.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

/// <summary>
/// Selects the appropriate <see cref="Style"/> for items in a toolbar
/// context menu: <see cref="MenuBarToggleStyle"/> for
/// <see cref="MenuBarToggleMenuEntry"/>, <see cref="ToggleStyle"/> for
/// <see cref="ToolbarModel"/> entries, and <see cref="CustomizeStyle"/>
/// for <see cref="ToolbarCustomizeMenuEntry"/> entries.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ToolbarMenuItemStyleSelector : StyleSelector
{
    public Style? MenuBarToggleStyle { get; set; }
    public Style? ToggleStyle { get; set; }
    public Style? CustomizeStyle { get; set; }

    public override Style? SelectStyle(object item, DependencyObject container) => item switch
    {
        MenuBarToggleMenuEntry => MenuBarToggleStyle,
        ToolbarModel => ToggleStyle,
        ToolbarCustomizeMenuEntry => CustomizeStyle,
        _ => base.SelectStyle(item, container)
    };
}
