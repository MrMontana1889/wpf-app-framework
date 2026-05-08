// ToolbarItemModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a single command entry in a toolbar, with a stable identity name,
/// a display label, and an independently controllable visibility state.
/// </summary>
public sealed class ToolbarItemModel : ToolbarEntryModel
{
    public ToolbarItemModel(
        ICommand command,
        string name,
        string label,
        bool includeInMenuBar = true,
        string? logicalGroup = null)
        : base(name, label)
    {
        Command = command;
        IncludeInMenuBar = includeInMenuBar;
        LogicalGroup = logicalGroup;
    }

    /// <summary>
    /// The command to execute when the toolbar button is clicked.
    /// </summary>
    public ICommand Command { get; }

    /// <summary>
    /// Controls whether this toolbar item should be projected into the shell menu bar.
    /// </summary>
    public bool IncludeInMenuBar { get; }

    /// <summary>
    /// Optional logical group key used by toolbar projection to emit menu separators.
    /// </summary>
    public string? LogicalGroup { get; }
}
