// ToolbarModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Services;
using System.Collections.ObjectModel;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Base ViewModel for a generic toolbar. Provides an observable collection
/// of <see cref="ToolbarEntryModel"/> entries that drive <c>ToolbarControl</c>.
/// Concrete toolbar models inherit from this class and populate
/// <see cref="Items"/> in their constructor.
/// </summary>
public abstract partial class ToolbarModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    /// <summary>
    /// The unique name identifying this toolbar, used as the persistence key.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The ordered list of items rendered by the toolbar control.
    /// </summary>
    public ObservableCollection<ToolbarEntryModel> Items { get; } = new();

    /// <summary>
    /// Controls whether the toolbar row is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool isToolbarVisible = true;

    /// <summary>
    /// Gets whether this toolbar can be hidden by the user.
    /// When <c>false</c> the toolbar is always visible and the context-menu
    /// entry for it is disabled.
    /// </summary>
    public bool CanHide { get; internal set; } = true;

    protected ToolbarModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    /// <summary>
    /// Opens the toolbar customization dialog.
    /// </summary>
    [RelayCommand]
    protected virtual void Customize()
    {
        _dialogService.ShowCustomizeToolbarDialog(this);
    }

    /// <summary>
    /// Toggles <see cref="IsToolbarVisible"/>. Has no effect when
    /// <see cref="CanHide"/> is <c>false</c>.
    /// </summary>
    [RelayCommand]
    private void ToggleVisibility()
    {
        if (CanHide)
            IsToolbarVisible = !IsToolbarVisible;
    }
}
