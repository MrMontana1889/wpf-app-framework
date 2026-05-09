// ICoreService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

public interface ICoreService
{
    /// <summary>
    /// Gets the window persistence service for saving/restoring window state.
    /// </summary>
    IWindowPersistenceService WindowPersistence { get; }
    /// <summary>
    /// Gets the version checking service.
    /// </summary>
    IVersionCheckService VersionCheck { get; }
    /// <summary>
    /// Gets the dialog service for showing application dialogs.
    /// </summary>
    IDialogService Dialogs { get; }
    /// <summary>
    /// Gets the application settings service.
    /// </summary>
    IApplicationSettingsService Settings { get; }
    /// <summary>
    /// Gets the toolbar registry service for managing toolbar visibility.
    /// </summary>
    IToolbarRegistryService ToolbarRegistry { get; }
}
