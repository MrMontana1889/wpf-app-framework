// App.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Wpf.Services;
using Dev.Wpf.TestHost.ViewModels;
using Dev.Wpf.TestHost.Views;
using System.IO;
using System.Windows;

namespace Dev.Wpf.TestHost;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var themeService = new ThemeService();
        themeService.ApplyTheme("System");

        IWindowPersistenceService persistence = new WindowPersistenceService(Path.GetTempPath());

        MainWindowViewModel viewModel = new MainWindowViewModel(themeService);
        var mainWindow = new MainWindow(persistence) 
        { 
            DataContext = viewModel 
        };

        mainWindow.Show();
    }
}
