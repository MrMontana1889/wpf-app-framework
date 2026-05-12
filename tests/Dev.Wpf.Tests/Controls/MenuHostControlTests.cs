// MenuHostControlTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using Dev.Wpf.Tests.Templates;
using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Tests.Controls;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class MenuHostControlTests
{
    [Test]
    public void MenuHostControl_TracksRegistryVisibility()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var menuBarId = new ToolbarId("MenuBar");
            registry.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));

            var control = new MenuHostControl
            {
                ToolbarRegistry = registry
            };

            using var host = new TemplateTestHost(control);

            Assert.That(control.Visibility, Is.EqualTo(Visibility.Visible));

            registry.SetVisibility(menuBarId, false);

            Assert.That(control.Visibility, Is.EqualTo(Visibility.Collapsed));

            registry.SetVisibility(menuBarId, true);

            Assert.That(control.Visibility, Is.EqualTo(Visibility.Visible));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void MenuHostControl_AutoDiscoversRegistry_FromLoadedTree()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var menuBarId = new ToolbarId("MenuBar");
            registry.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: false));

            var toolbarHost = new ToolbarHostControl
            {
                ToolbarRegistry = registry
            };

            var menuHost = new MenuHostControl
            {
            };

            var root = new Grid();
            root.Children.Add(menuHost);
            root.Children.Add(toolbarHost);

            using var host = new TemplateTestHost(root);

            Assert.That(menuHost.Visibility, Is.EqualTo(Visibility.Collapsed));

            registry.SetVisibility(menuBarId, true);

            Assert.That(menuHost.Visibility, Is.EqualTo(Visibility.Visible));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void MenuHostControl_UsesOverriddenMenuBarId()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var defaultMenuBarId = new ToolbarId("MenuBar");
            var configuredMenuBarId = new ToolbarId("Shell.MainMenu");

            registry.RegisterDefinition(new ToolbarDefinition(defaultMenuBarId, "Default Menu Bar", canHide: true, defaultVisible: true));
            registry.RegisterDefinition(new ToolbarDefinition(configuredMenuBarId, "Configured Menu Bar", canHide: true, defaultVisible: false));

            var control = new MenuHostControl
            {
                ToolbarRegistry = registry,
                MenuBarId = configuredMenuBarId
            };

            using var host = new TemplateTestHost(control);

            Assert.That(control.Visibility, Is.EqualTo(Visibility.Collapsed));

            registry.SetVisibility(configuredMenuBarId, true);

            Assert.That(control.Visibility, Is.EqualTo(Visibility.Visible));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

}