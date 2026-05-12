// ToolbarMenuItemStyleSelector.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Core.ViewModels.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

/// <summary>
/// Selects the appropriate <see cref="Style"/> for items in a toolbar
/// context menu: <see cref="ToggleStyle"/> for
/// <see cref="ToolbarDefinition"/> entries, and <see cref="CustomizeStyle"/>
/// for <see cref="ToolbarCustomizeMenuEntry"/> entries.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ToolbarMenuItemStyleSelector : StyleSelector
{
    public Style? ToggleStyle { get; set; }
    public Style? CustomizeStyle { get; set; }

    public override Style? SelectStyle(object item, DependencyObject container) => item switch
    {
        ToolbarDefinition => ToggleStyle,
        ToolbarCustomizeMenuEntry => CustomizeStyle,
        _ => base.SelectStyle(item, container)
    };
}
