// PromptOverlay.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using Dev.Core.Services.Mode;
using Dev.Core.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev.Wpf.TestHost.Overlays;

/// <summary>
/// Overlay adapter for <see cref="PromptViewModel"/> used by the overlay validation harness.
/// </summary>
public sealed class PromptOverlay : PromptViewModel, IInteractionOverlay<PromptResponse>
{
    private Action<PromptResponse>? _resultCallback;

    public PromptOverlay(BoundPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Initialize(request);
        PropertyChanged += OnPromptPropertyChanged;
    }

    ICommand IInteractionOverlay.CancelCommand => base.CancelCommand;

    public void SetResultCallback(Action<PromptResponse> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _resultCallback = callback;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
        PropertyChanged -= OnPromptPropertyChanged;
    }

    private void OnPromptPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DialogResult) && DialogResult.HasValue)
            _resultCallback?.Invoke(GetResponse());
    }
}
