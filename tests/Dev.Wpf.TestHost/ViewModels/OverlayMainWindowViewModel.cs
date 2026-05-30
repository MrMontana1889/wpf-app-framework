// OverlayMainWindowViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Prompts;
using Dev.Core.Services.Mode;
using Dev.Core.ViewModels;
using Dev.Core.Services;
using Dev.Wpf.Views;
using Dev.Wpf.TestHost.Overlays;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Dev.Wpf.TestHost.ViewModels;

/// <summary>
/// ViewModel for the overlay validation harness window.
/// </summary>
public sealed partial class OverlayMainWindowViewModel : ObservableObject
{
    private readonly IModeService _modeService;
    private int _confirmCounter;
    private int _promptCounter;

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

    [RelayCommand]
    private void ShowWizardOverlay()
    {
        var overlay = new NewAccountWizardOverlay();

        _modeService.ShowOverlay(overlay, result =>
        {
            LastOverlayResult = $"Wizard completed: {JsonSerializer.Serialize(result)}";
        });
    }

    [RelayCommand]
    private void ShowPromptDialog()
    {
        _promptCounter++;

        var request = CreateSamplePromptRequest(_promptCounter);
        var viewModel = new PromptViewModel();

        var persistence = new WindowPersistenceService(Path.GetTempPath());
        var dialog = new PromptDialog(viewModel, persistence);

        var owner = Application.Current?.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive);

        if (owner is not null)
            dialog.Owner = owner;

        dialog.Present(request);
        var response = viewModel.GetResponse();

        LastOverlayResult = FormatPromptResult("Dialog", response);
    }

    [RelayCommand]
    private void ShowPromptOverlay()
    {
        _promptCounter++;

        var request = CreateSamplePromptRequest(_promptCounter);
        var overlay = new PromptOverlay(request);

        _modeService.ShowOverlay(overlay, response =>
        {
            LastOverlayResult = FormatPromptResult("Overlay", response);
        });
    }

    private static BoundPromptRequest CreateSamplePromptRequest(int sequence)
    {
        var promptId = new PromptId("testhost.prompt.validation");
        var definition = new PromptDefinition(
            promptId,
            titleTemplate: "Prompt Validation",
            messageTemplate: "Apply sample change #{0} to account '{1}'?",
            buttonSet: PromptButtonSet.YesNoCancel,
            defaultResult: PromptResult.No,
            allowSuppression: true,
            suppressionText: "Do not ask again for this test prompt.");

        var request = new PromptRequest(
            promptId,
            parameters: new object?[] { sequence, "Checking" });

        return new BoundPromptRequest(request, definition);
    }

    private static string FormatPromptResult(string projection, PromptResponse response)
    {
        return $"{projection} prompt result: Result={response.Result}, SuppressChecked={response.SuppressChecked}, FromUser={response.IsFromUserInteraction}";
    }
}
