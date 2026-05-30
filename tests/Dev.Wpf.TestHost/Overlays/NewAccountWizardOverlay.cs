// NewAccountWizardOverlay.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Services.Mode;
using System.Windows.Input;

namespace Dev.Wpf.TestHost.Overlays;

/// <summary>
/// Minimal three-step wizard overlay used to validate stateful overlay workflows.
/// </summary>
public sealed class NewAccountWizardOverlay : ObservableObject, IInteractionOverlay<NewAccountData>
{
    private const int FirstStep = 0;
    private const int LastStep = 2;

    private Action<NewAccountData>? _callback;
    private int _stepIndex;

    private readonly RelayCommand _nextCommand;
    private readonly RelayCommand _backCommand;

    public NewAccountWizardOverlay()
    {
        _nextCommand = new RelayCommand(() => StepIndex++, () => StepIndex < LastStep);
        _backCommand = new RelayCommand(() => StepIndex--, () => StepIndex > FirstStep);
        FinishCommand = new RelayCommand(() => _callback?.Invoke(Data));
        CancelCommand = new RelayCommand(() => _callback?.Invoke(Data));
    }

    public int StepIndex
    {
        get => _stepIndex;
        private set
        {
            var clamped = Math.Clamp(value, FirstStep, LastStep);
            if (SetProperty(ref _stepIndex, clamped))
            {
                _nextCommand.NotifyCanExecuteChanged();
                _backCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public NewAccountData Data { get; } = new();

    public ICommand NextCommand => _nextCommand;

    public ICommand BackCommand => _backCommand;

    public ICommand FinishCommand { get; }

    public ICommand CancelCommand { get; }

    public void SetResultCallback(Action<NewAccountData> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callback = callback;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
}
