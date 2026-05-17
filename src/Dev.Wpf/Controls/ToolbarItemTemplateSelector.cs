// ToolbarItemTemplateSelector.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

/// <summary>
/// Selects the built-in WPF data template for each <see cref="ToolbarItemKind"/>.
/// </summary>
public sealed class ToolbarItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ButtonTemplate { get; set; }
    public DataTemplate? DropDownTemplate { get; set; }
    public DataTemplate? SplitDropDownTemplate { get; set; }
    public DataTemplate? ToggleButtonTemplate { get; set; }
    public DataTemplate? CheckBoxTemplate { get; set; }
    public DataTemplate? LabelTemplate { get; set; }
    public DataTemplate? ComboBoxTemplate { get; set; }
    public DataTemplate? SeparatorTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not ToolbarItem toolbarItem)
            return base.SelectTemplate(item, container);

        return toolbarItem.Kind switch
        {
            ToolbarItemKind.Button => ButtonTemplate,
            ToolbarItemKind.DropDown => DropDownTemplate,
            ToolbarItemKind.SplitDropDown => SplitDropDownTemplate,
            ToolbarItemKind.ToggleButton => ToggleButtonTemplate,
            ToolbarItemKind.CheckBox => CheckBoxTemplate,
            ToolbarItemKind.Label => LabelTemplate,
            ToolbarItemKind.ComboBox => ComboBoxTemplate,
            ToolbarItemKind.Separator => SeparatorTemplate,
            _ => base.SelectTemplate(item, container),
        };
    }
}
