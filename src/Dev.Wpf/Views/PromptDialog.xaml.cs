// PromptDialog.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using Dev.Core.Services;
using Dev.Core.ViewModels;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Views;

/// <summary>
/// Dialog for displaying prompts with optional suppression checkbox.
/// Implements <see cref="IPromptPresenter"/> to participate in orchestration.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class PromptDialog : BaseDialog, IPromptPresenter
{
    private readonly PromptDialogViewModel _viewModel;

    public PromptDialog(PromptDialogViewModel viewModel, IWindowPersistenceService windowPersistence)
        : base(windowPersistence)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Presents the prompt and returns the user's decision.
    /// </summary>
    public PromptResponse Present(BoundPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _viewModel.Initialize(request);
        _viewModel.PropertyChanged += OnViewModelDialogCompleted;

        var owner = GetDialogOwner();
        if (owner is not null)
            Owner = owner;

        var shown = ShowDialog();

        _viewModel.PropertyChanged -= OnViewModelDialogCompleted;

        // ShowDialog() returns null if dialog was closed without explicit result.
        // In that case, we treat it as a Cancel, which is consistent with
        // expected behavior when the dialog is dismissed.
        if (shown != true)
            return PromptResponse.FromUserInteraction(PromptResult.Cancel, suppressChecked: false);

        return _viewModel.GetResponse();
    }

    private void OnViewModelDialogCompleted(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PromptDialogViewModel.DialogResult) && _viewModel.DialogResult.HasValue)
            DialogResult = _viewModel.DialogResult.Value;
    }

    private static Window? GetDialogOwner()
    {
        var app = Application.Current;
        if (app is null)
            return null;

        return app.Windows
                  .OfType<Window>()
                  .FirstOrDefault(w => w.IsActive)
               ?? app.MainWindow;
    }
}
