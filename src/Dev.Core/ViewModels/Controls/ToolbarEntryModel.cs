// ToolbarEntryModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using CommunityToolkit.Mvvm.ComponentModel;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Base model for all toolbar entries rendered by ToolbarControl.
/// </summary>
public abstract partial class ToolbarEntryModel : ObservableObject
{
    protected ToolbarEntryModel(string name, string label)
    {
        Name = name;
        Label = label;
    }

    /// <summary>
    /// The culture-invariant identifier for this entry.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The text displayed for this entry.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Controls whether this entry is visible in the toolbar.
    /// </summary>
    [ObservableProperty]
    private bool isVisible = true;
}