// ToolbarCustomizeMenuEntry.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Windows.Input;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents the "Customize…" entry that appears at the bottom of a
/// toolbar's context menu, separated from the toolbar-visibility items.
/// </summary>
public sealed class ToolbarCustomizeMenuEntry
{
    public ICommand Command { get; }

    public ToolbarCustomizeMenuEntry(ICommand command)
    {
        Command = command;
    }
}
