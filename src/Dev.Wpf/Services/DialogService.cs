// DialogService.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels;
using Dev.Wpf.Views;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Dev.Wpf.Services;

/// <summary>
/// Base implementation of IDialogService for WPF applications.
/// </summary>
[ExcludeFromCodeCoverage]
public class DialogService : IDialogService
{
    private readonly IApplicationDescriptionService _appDescription;
    private readonly IVersionCheckService _versionCheckService;
    private readonly IWindowPersistenceService _windowPersistence;

    public DialogService(
        IApplicationDescriptionService appDescription,
        IVersionCheckService versionCheckService,
        IWindowPersistenceService windowPersistence)
    {
        _appDescription = appDescription;
        _versionCheckService = versionCheckService;
        _windowPersistence = windowPersistence;
    }

    public void ShowMessage(string title, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        MessageBox.Show(
            Application.Current.MainWindow,
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public void ShowAboutDialog()
    {
        var vm = new AboutDialogViewModel(_appDescription.ApplicationName, _versionCheckService.GetCurrentVersion());
        var dialog = new AboutDialog(vm, _windowPersistence) { Owner = Application.Current.MainWindow };
        dialog.ShowDialog();
    }

    public void ShowAppSettingsDialog()
    {
        MessageBox.Show(Application.Current.MainWindow,
            "Application Settings dialog not yet implemented.",
            "Settings",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public SaveChangesResult ShowSaveChangesPrompt()
    {
        var result = MessageBox.Show(Application.Current.MainWindow,
            "Do you want to save your changes?",
            "Save Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        return result switch
        {
            MessageBoxResult.Yes => SaveChangesResult.Yes,
            MessageBoxResult.No => SaveChangesResult.No,
            _ => SaveChangesResult.Cancel
        };
    }

    public bool ShowOpenFileDialog(out string? filePath)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Build Files (*.build)|*.build|All Files (*.*)|*.*",
            Title = "Open Build File"
        };

        var result = dialog.ShowDialog();
        filePath = result == true ? dialog.FileName : null;
        return result == true;
    }

    public bool ShowSaveFileDialog(out string? filePath)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Build Files (*.build)|*.build|All Files (*.*)|*.*",
            Title = "Save Build File",
            DefaultExt = ".build"
        };

        var result = dialog.ShowDialog();
        filePath = result == true ? dialog.FileName : null;
        return result == true;
    }

    public bool ShowFolderBrowserDialog(out string? folderPath)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Folder"
        };

        var result = dialog.ShowDialog();
        folderPath = result == true ? dialog.FolderName : null;
        return result == true;
    }
}
