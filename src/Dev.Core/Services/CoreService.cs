// CoreService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

public class CoreService : ICoreService
{
    #region Constructor
    public CoreService(
        IWindowPersistenceService windowPersistenceService,
        IVersionCheckService versionCheckService,
        IDialogService dialogService,
        IApplicationSettingsService settingsService,
        IToolbarRegistryService toolbarRegistry)
    {
        WindowPersistence = windowPersistenceService;
        VersionCheck = versionCheckService;
        Dialogs = dialogService;
        Settings = settingsService;
        ToolbarRegistry = toolbarRegistry;
    }
    #endregion

    #region Public Properties
    public IWindowPersistenceService WindowPersistence { get; }
    public IVersionCheckService VersionCheck { get; }
    public IDialogService Dialogs { get; }
    public IApplicationSettingsService Settings { get; }
    public IToolbarRegistryService ToolbarRegistry { get; }
    #endregion
}
