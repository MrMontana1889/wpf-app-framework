// ToolbarItemCustomizeEntry.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using CommunityToolkit.Mvvm.ComponentModel;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// A working-copy entry used by <c>CustomizeToolbarViewModel</c> to represent
/// one toolbar item whose visibility can be toggled before being applied.
/// </summary>
public partial class ToolbarItemCustomizeEntry : ObservableObject
{
    [ObservableProperty]
    private bool isChecked;

    /// <summary>
    /// The culture-invariant identifier, used to match back to the source <c>ToolbarItemModel</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The display text shown in the customize dialog. May be localized.
    /// </summary>
    public string Label { get; }

    public ToolbarItemCustomizeEntry(string name, string label, bool isChecked)
    {
        Name = name;
        Label = label;
        this.isChecked = isChecked;
    }
}
