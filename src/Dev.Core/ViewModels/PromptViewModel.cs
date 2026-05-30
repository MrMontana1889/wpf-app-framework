// PromptViewModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Prompts;
using System.Globalization;

namespace Dev.Core.ViewModels;

public partial class PromptViewModel : ObservableObject
{
    private BoundPromptRequest? _request;
    private Action? _requestClose;

    public PromptViewModel()
    {
        _request = null;
    }

    // ── Input binding ────────────────────────────────────────────────────────

    [ObservableProperty]
    private string titleText = string.Empty;

    [ObservableProperty]
    private string messageText = string.Empty;

    [ObservableProperty]
    private bool showYesButton = false;

    [ObservableProperty]
    private bool showNoButton = false;

    [ObservableProperty]
    private bool showOkButton = false;

    [ObservableProperty]
    private bool showCancelButton = false;

    [ObservableProperty]
    private bool showSuppressCheckbox = false;

    [ObservableProperty]
    private string suppressCheckboxText = string.Empty;

    [ObservableProperty]
    private PromptResult selectedResult;

    [ObservableProperty]
    private bool suppressCheckboxChecked;

    [ObservableProperty]
    private bool? dialogResult;

    // ── Initialization ───────────────────────────────────────────────────────

    public void Initialize(BoundPromptRequest request, Action? requestClose = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        _requestClose = requestClose;
        _request = request;

        // Apply title and message templates
        TitleText = ApplyTemplate(request.Definition.TitleTemplate, request.Parameters);
        MessageText = ApplyTemplate(request.Definition.MessageTemplate, request.Parameters);

        // Configure button visibility
        UpdateButtonVisibility(request.Definition.ButtonSet);

        // Configure suppression checkbox
        if (request.Definition.AllowSuppression)
        {
            ShowSuppressCheckbox = true;
            SuppressCheckboxText = request.Definition.SuppressionText;
        }
        else
        {
            ShowSuppressCheckbox = false;
        }

        // Set initial focus based on default result
        SelectedResult = request.Definition.DefaultResult;
        SuppressCheckboxChecked = false;
    }

    // ── Button commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private void Yes() => CompleteDialog(PromptResult.Yes);

    [RelayCommand]
    private void No() => CompleteDialog(PromptResult.No);

    [RelayCommand]
    private void Ok() => CompleteDialog(PromptResult.Ok);

    [RelayCommand]
    private void Cancel() => CompleteDialog(PromptResult.Cancel);

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void UpdateButtonVisibility(PromptButtonSet buttonSet)
    {
        // Hide all by default
        ShowYesButton = false;
        ShowNoButton = false;
        ShowOkButton = false;
        ShowCancelButton = false;

        // Show based on button set
        switch (buttonSet)
        {
            case PromptButtonSet.OkCancel:
                ShowOkButton = true;
                ShowCancelButton = true;
                break;

            case PromptButtonSet.YesNo:
                ShowYesButton = true;
                ShowNoButton = true;
                break;

            case PromptButtonSet.YesNoCancel:
                ShowYesButton = true;
                ShowNoButton = true;
                ShowCancelButton = true;
                break;
        }
    }

    private string ApplyTemplate(string template, IReadOnlyList<object?> parameters)
    {
        if (parameters.Count == 0)
            return template;

        try
        {
            return string.Format(CultureInfo.CurrentCulture, template, parameters.ToArray());
        }
        catch
        {
            // If formatting fails, return unformatted template
            return template;
        }
    }

    private void CompleteDialog(PromptResult result)
    {
        SelectedResult = result;
        DialogResult = true;
        _requestClose?.Invoke();
    }

    /// <summary>
    /// Returns a <see cref="PromptResponse"/> based on current UI state.
    /// This is called after the dialog closes to capture the user's decision.
    /// </summary>
    public PromptResponse GetResponse()
    {
        return PromptResponse.FromUserInteraction(SelectedResult, SuppressCheckboxChecked);
    }
}
