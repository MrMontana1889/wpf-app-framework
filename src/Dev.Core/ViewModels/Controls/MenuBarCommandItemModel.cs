// MenuBarCommandItemModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Windows.Input;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a projected command entry in a menu.
/// </summary>
public sealed class MenuBarCommandItemModel : MenuBarEntryModel
{
    public MenuBarCommandItemModel(string label, ICommand command)
    {
        Label = label;
        Command = command;
    }

    public string Label { get; }

    public ICommand Command { get; }
}
