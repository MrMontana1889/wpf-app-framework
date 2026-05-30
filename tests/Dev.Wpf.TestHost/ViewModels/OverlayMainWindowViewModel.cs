// OverlayMainWindowViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Services.Mode;
using Dev.Wpf.TestHost.Overlays;

namespace Dev.Wpf.TestHost.ViewModels;

/// <summary>
/// ViewModel for the overlay validation harness window.
/// </summary>
public sealed partial class OverlayMainWindowViewModel : ObservableObject
{
    private readonly IModeService _modeService;
    private int _confirmCounter;

    public OverlayMainWindowViewModel()
    {
        _modeService = new ModeService();
        LastOverlayResult = "No overlay result yet.";
    }

    public IModeService ModeService => _modeService;

    [ObservableProperty]
    private string lastOverlayResult;

    [RelayCommand]
    private void ShowConfirmOverlay()
    {
        _confirmCounter++;
        var overlay = new ConfirmOverlay($"Confirm test action #{_confirmCounter}?");

        _modeService.ShowOverlay(overlay, confirmed =>
        {
            LastOverlayResult = confirmed
                ? $"Action #{_confirmCounter} confirmed."
                : $"Action #{_confirmCounter} canceled.";
        });
    }
}
