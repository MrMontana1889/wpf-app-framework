// AboutDialogViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Dev.Core.ViewModels;

public partial class AboutDialogViewModel : ObservableObject
{
    public AboutDialogViewModel(string applicationLabel, string version)
    {
        ApplicationLabel = applicationLabel;
        Version = version;
    }

    public string ApplicationLabel { get; }
    public string Version { get; }

    [ObservableProperty]
    private bool? dialogResult;

    [RelayCommand]
    private void Close() => DialogResult = true;
}
