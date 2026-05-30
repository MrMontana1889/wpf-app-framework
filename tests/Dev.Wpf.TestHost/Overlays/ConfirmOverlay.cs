// ConfirmOverlay.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.Input;
using Dev.Core.Services.Mode;
using System.Windows.Input;

namespace Dev.Wpf.TestHost.Overlays;

/// <summary>
/// Minimal test overlay that returns a boolean confirmation result.
/// </summary>
public sealed class ConfirmOverlay : IInteractionOverlay<bool>
{
    private Action<bool>? _resultCallback;

    public ConfirmOverlay(string message)
    {
        Message = message;
        ConfirmCommand = new RelayCommand(() => _resultCallback?.Invoke(true));
        CancelCommand = new RelayCommand(() => _resultCallback?.Invoke(false));
    }

    public string Message { get; }

    public IRelayCommand ConfirmCommand { get; }

    public ICommand CancelCommand { get; }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void SetResultCallback(Action<bool> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _resultCallback = callback;
    }
}
