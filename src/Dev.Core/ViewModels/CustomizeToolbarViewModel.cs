// CustomizeToolbarViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Services;
using Dev.Core.ViewModels.Controls;

namespace Dev.Core.ViewModels;

/// <summary>
/// ViewModel for the Customize Toolbar dialog. Presents a working copy of the
/// toolbar items so changes are only applied when the user clicks Apply.
/// </summary>
public partial class CustomizeToolbarViewModel : ObservableObject
{
    private readonly ToolbarModel _toolbarModel;
    private readonly IToolbarSettingsService _toolbarSettings;

    [ObservableProperty]
    private bool? dialogResult;

    public string ToolbarName { get; }

    public IReadOnlyList<ToolbarItemCustomizeEntry> Items { get; }

    public CustomizeToolbarViewModel(ToolbarModel toolbarModel, IToolbarSettingsService toolbarSettings)
    {
        _toolbarModel = toolbarModel;
        _toolbarSettings = toolbarSettings;

        ToolbarName = toolbarModel.Name;
        Items = toolbarModel.Items
            .OfType<ToolbarItemModel>()
            .Select(i => new ToolbarItemCustomizeEntry(i.Name, i.Label, i.IsVisible))
            .ToList();
    }

    [RelayCommand]
    private void Apply()
    {
        foreach (var entry in Items)
        {
            var item = _toolbarModel.Items
                .OfType<ToolbarItemModel>()
                .FirstOrDefault(i => i.Name == entry.Name);
            if (item is not null)
                item.IsVisible = entry.IsChecked;
        }

        _toolbarSettings.Save(ToolbarName, _toolbarModel.Items.OfType<ToolbarItemModel>());
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
