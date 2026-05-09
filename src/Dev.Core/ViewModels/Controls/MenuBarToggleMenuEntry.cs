// MenuBarToggleMenuEntry.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Windows.Input;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents the "Menu Bar" toggle entry that appears as the first item
/// in a toolbar's context menu, allowing the user to show or hide the shell menu bar.
/// This entry observes the authoritative Menu Bar visibility state and derives
/// its checked state dynamically, ensuring the toggle always reflects live user intent.
/// </summary>
public sealed class MenuBarToggleMenuEntry
{
    private readonly IMenuBarToggleStateProvider _stateProvider;
    private readonly ICommand _toggleCommand;

    public MenuBarToggleMenuEntry(IMenuBarToggleStateProvider stateProvider, ICommand toggleCommand)
    {
        ArgumentNullException.ThrowIfNull(stateProvider);
        ArgumentNullException.ThrowIfNull(toggleCommand);
        _stateProvider = stateProvider;
        _toggleCommand = toggleCommand;
    }

    /// <summary>
    /// Gets the current Menu Bar checked state, derived from the authoritative
    /// user preference held by the state provider. Always reflects live state;
    /// no cached or copied values exist on this entry.
    /// </summary>
    public bool IsChecked => _stateProvider.IsMenuBarUserVisible;

    /// <summary>
    /// Gets the command to toggle menu bar visibility.
    /// </summary>
    public ICommand ToggleCommand => _toggleCommand;
}
