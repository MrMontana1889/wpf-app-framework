// ToolbarHostControlTemplateTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dev.Wpf.Tests.Templates;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ToolbarHostControlTemplateTests
{
    [Test]
    public void Template_ContainsToolBarTray_AndToolBar()
    {
        var control = new ToolbarHostControl();

        using var host = new TemplateTestHost(control);

        var tray = TemplateTestHost.FindChild<ToolBarTray>(control);
        var toolbar = TemplateTestHost.FindChild<ToolBar>(control);

        Assert.Multiple(() =>
        {
            Assert.That(tray, Is.Not.Null);
            Assert.That(toolbar, Is.Not.Null);
        });
    }

    [Test]
    public void GroupedProjection_Disabled_RendersSingleToolBar()
    {
        var items = new[]
        {
            CreateButtonItem("Build.Null1", order: 0, tooltip: "null-1", logicalGroup: null),
            CreateButtonItem("Build.A1", order: 1, tooltip: "a-1", logicalGroup: "A"),
            CreateButtonItem("Build.Null2", order: 2, tooltip: "null-2", logicalGroup: null),
            CreateButtonItem("Build.B1", order: 3, tooltip: "b-1", logicalGroup: "B")
        };

        var control = new ToolbarHostControl
        {
            EnableGroupedToolbarProjection = false,
            ItemsSource = items
        };

        using var host = new TemplateTestHost(control);

        var toolbars = TemplateTestHost.FindAllChildren<ToolBar>(control).ToList();
        Assert.That(toolbars, Has.Count.EqualTo(1));

        var buttonTooltips = TemplateTestHost.FindAllChildren<Button>(toolbars[0])
            .Select(button => button.ToolTip)
            .Cast<string>()
            .ToList();

        Assert.That(buttonTooltips, Is.EqualTo(new[] { "null-1", "a-1", "null-2", "b-1" }));
    }

    [Test]
    public void GroupedProjection_Enabled_PartitionsByLogicalGroup_WithNullGroup()
    {
        var items = new[]
        {
            CreateButtonItem("Build.Null1", order: 0, tooltip: "null-1", logicalGroup: null),
            CreateButtonItem("Build.A1", order: 1, tooltip: "a-1", logicalGroup: "A"),
            CreateButtonItem("Build.Null2", order: 2, tooltip: "null-2", logicalGroup: null),
            CreateButtonItem("Build.A2", order: 3, tooltip: "a-2", logicalGroup: "A"),
            CreateButtonItem("Build.B1", order: 4, tooltip: "b-1", logicalGroup: "B")
        };

        var control = new ToolbarHostControl
        {
            EnableGroupedToolbarProjection = true,
            ItemsSource = items
        };

        using var host = new TemplateTestHost(control);

        var toolbars = TemplateTestHost.FindAllChildren<ToolBar>(control).ToList();
        Assert.That(toolbars, Has.Count.EqualTo(3));

        var firstGroup = TemplateTestHost.FindAllChildren<Button>(toolbars[0]).Select(button => button.ToolTip).Cast<string>().ToList();
        var secondGroup = TemplateTestHost.FindAllChildren<Button>(toolbars[1]).Select(button => button.ToolTip).Cast<string>().ToList();
        var thirdGroup = TemplateTestHost.FindAllChildren<Button>(toolbars[2]).Select(button => button.ToolTip).Cast<string>().ToList();

        Assert.That(firstGroup, Is.EqualTo(new[] { "null-1", "null-2" }));
        Assert.That(secondGroup, Is.EqualTo(new[] { "a-1", "a-2" }));
        Assert.That(thirdGroup, Is.EqualTo(new[] { "b-1" }));
    }

    [Test]
    public void GroupedProjection_Enabled_WithSingleLogicalGroup_RendersSingleToolBar()
    {
        var items = new[]
        {
            CreateButtonItem("Build.One", order: 0, tooltip: "one", logicalGroup: "A"),
            CreateButtonItem("Build.Two", order: 1, tooltip: "two", logicalGroup: "A")
        };

        var control = new ToolbarHostControl
        {
            EnableGroupedToolbarProjection = true,
            ItemsSource = items
        };

        using var host = new TemplateTestHost(control);

        var toolbars = TemplateTestHost.FindAllChildren<ToolBar>(control).ToList();
        Assert.That(toolbars, Has.Count.EqualTo(1));
    }

    [Test]
    public void GroupedProjection_Enabled_OmitsEmptyGroupsAfterRegistryVisibilityFiltering()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemA = new ToolbarItemId("Build.A");
            var itemB = new ToolbarItemId("Build.B");
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemA, itemB]));
            registry.SetItemVisibility(toolbarId, itemB, false);

            var control = new ToolbarHostControl
            {
                EnableGroupedToolbarProjection = true,
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[]
                {
                    CreateButtonItem(itemA.Value, order: 0, tooltip: "a", logicalGroup: "A"),
                    CreateButtonItem(itemB.Value, order: 1, tooltip: "b", logicalGroup: "B")
                }
            };

            using var host = new TemplateTestHost(control);

            var toolbars = TemplateTestHost.FindAllChildren<ToolBar>(control).ToList();
            Assert.That(toolbars, Has.Count.EqualTo(1));

            var buttonTooltips = TemplateTestHost.FindAllChildren<Button>(toolbars[0])
                .Select(button => button.ToolTip)
                .Cast<string>()
                .ToList();

            Assert.That(buttonTooltips, Is.EqualTo(new[] { "a" }));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void ButtonProjection_BindsVisibilityAndEnablement()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Build", "Run build")),
            ToolbarItemDisplayIntent.IconAndText,
            isVisible: false,
            isEnabled: false,
            command: new TestCommand());

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { item },
        };

        using var host = new TemplateTestHost(control);

        var button = TemplateTestHost.FindChild<Button>(control);

        Assert.That(button, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(button!.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(button.IsEnabled, Is.False);
        });
    }

    [Test]
    public void ButtonProjection_UsesIconProvider_WhenDisplayIntentShowsIcon()
    {
        var expectedIcon = new DrawingImage();
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Build", "Run build"), new IconKey("Build")),
            ToolbarItemDisplayIntent.IconOnly,
            command: new TestCommand());

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { item },
            IconProvider = new StubIconProvider(expectedIcon),
        };

        using var host = new TemplateTestHost(control);

        var image = TemplateTestHost.FindChild<Image>(control);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Source, Is.SameAs(expectedIcon));
    }

    [Test]
    public void ButtonProjection_ReplacedItems_RefreshesLabelText()
    {
        var items = new ObservableCollection<ToolbarItem>
        {
            new(
                new ToolbarItemId("Selection.Cycle"),
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Selection: Single")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand())
        };

        var control = new ToolbarHostControl
        {
            ItemsSource = items,
        };

        using var host = new TemplateTestHost(control);

        var initialButton = TemplateTestHost.FindChild<Button>(control);
        var initialText = TemplateTestHost.FindChild<TextBlock>(initialButton!);

        Assert.That(initialText, Is.Not.Null);
        Assert.That(initialText!.Text, Is.EqualTo("Selection: Single"));

        items.Clear();
        items.Add(
            new ToolbarItem(
                new ToolbarItemId("Selection.Cycle"),
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Selection: Multiple")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand()));

        control.UpdateLayout();

        var updatedButton = TemplateTestHost.FindChild<Button>(control);
        var updatedText = TemplateTestHost.FindChild<TextBlock>(updatedButton!);

        Assert.That(updatedText, Is.Not.Null);
        Assert.That(updatedText!.Text, Is.EqualTo("Selection: Multiple"));
    }

    [Test]
    public void HostRightClick_WithRegistry_AddsCustomizeSubMenuForCurrentToolbarItems()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");
            var item = new ToolbarItem(
                itemId,
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand());
            var definition = new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]);
            registry.RegisterDefinition(definition);

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[] { item }
            };

            using var host = new TemplateTestHost(control);
            var toolbar = TemplateTestHost.FindChild<ToolBar>(control);
            Assert.That(toolbar, Is.Not.Null);
            var toolbarHost = toolbar!;

            var menuOpened = OpenToolbarContextMenu(control, toolbarHost);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);
            var nonNullContextMenu = contextMenu!;
            var customizeEntry = nonNullContextMenu.Items.OfType<MenuItem>().Single(m => Equals(m.Header, "Customize..."));

            Assert.That(customizeEntry.Items.OfType<MenuItem>().Any(m => Equals(m.Header, "Run")), Is.True);
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_OnToolbarChrome_OpensCustomizeMenu()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");
            var item = new ToolbarItem(
                itemId,
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand());
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[] { item }
            };

            using var host = new TemplateTestHost(control);
            var toolbarTray = TemplateTestHost.FindChild<ToolBarTray>(control);
            Assert.That(toolbarTray, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarTray!);
            Assert.That(menuOpened, Is.True, "Right-click on empty toolbar chrome should open host menu");

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);
            Assert.That(contextMenu!.Items.OfType<MenuItem>().Any(m => Equals(m.Header, "Customize...")), Is.True);
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_WithMenuBarDefinition_IncludesMenuBarAndSeparator()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var menuBarId = new ToolbarId("MenuBar");
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");

            registry.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Primary Toolbar", itemIds: [itemId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[]
                {
                    new ToolbarItem(
                        itemId,
                        ToolbarItemKind.Button,
                        new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                        ToolbarItemDisplayIntent.TextOnly,
                        command: new TestCommand())
                }
            };

            using var host = new TemplateTestHost(control);
            var toolbarTray = TemplateTestHost.FindChild<ToolBarTray>(control);
            Assert.That(toolbarTray, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarTray!);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);

            var items = contextMenu!.Items.Cast<object>().ToList();
            Assert.That(items[0], Is.TypeOf<MenuItem>());
            Assert.That(((MenuItem)items[0]).Header, Is.EqualTo("Menu Bar"));
            Assert.That(items[1], Is.TypeOf<Separator>());
            Assert.That(items[2], Is.TypeOf<MenuItem>());
            Assert.That(((MenuItem)items[2]).Header, Is.EqualTo("Primary Toolbar"));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_WithoutMenuBarDefinition_DoesNotInsertRowSeparator()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");

            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Primary Toolbar", itemIds: [itemId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[]
                {
                    new ToolbarItem(
                        itemId,
                        ToolbarItemKind.Button,
                        new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                        ToolbarItemDisplayIntent.TextOnly,
                        command: new TestCommand())
                }
            };

            using var host = new TemplateTestHost(control);
            var toolbarTray = TemplateTestHost.FindChild<ToolBarTray>(control);
            Assert.That(toolbarTray, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarTray!);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);

            var items = contextMenu!.Items.Cast<object>().ToList();
            Assert.That(items.OfType<Separator>().Count(), Is.EqualTo(1), "Only the Customize separator should be present when Menu Bar is absent.");
            Assert.That(items[0], Is.TypeOf<MenuItem>());
            Assert.That(((MenuItem)items[0]).Header, Is.EqualTo("Primary Toolbar"));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_MenuBarEntry_TogglesRegistryVisibility()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var menuBarId = new ToolbarId("MenuBar");
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");

            registry.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Primary Toolbar", itemIds: [itemId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[]
                {
                    new ToolbarItem(
                        itemId,
                        ToolbarItemKind.Button,
                        new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                        ToolbarItemDisplayIntent.TextOnly,
                        command: new TestCommand())
                }
            };

            using var host = new TemplateTestHost(control);
            var toolbarTray = TemplateTestHost.FindChild<ToolBarTray>(control);
            Assert.That(toolbarTray, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarTray!);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);

            var menuBarEntry = contextMenu!.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "Menu Bar"));
            Assert.That(menuBarEntry.IsChecked, Is.True);

            menuBarEntry.IsChecked = false;
            menuBarEntry.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            Assert.That(registry.IsVisible(menuBarId), Is.False);

            var reloaded = new ToolbarRegistryService(appDataPath);
            reloaded.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));
            Assert.That(reloaded.IsVisible(menuBarId), Is.False);
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_UsesOverriddenMenuBarIdForGrouping()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var configuredMenuBarId = new ToolbarId("Shell.MainMenu");
            var defaultMenuBarId = new ToolbarId("MenuBar");
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");

            registry.RegisterDefinition(new ToolbarDefinition(defaultMenuBarId, "Legacy Menu Bar", canHide: true, defaultVisible: true));
            registry.RegisterDefinition(new ToolbarDefinition(configuredMenuBarId, "Configured Menu Bar", canHide: true, defaultVisible: true));
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Primary Toolbar", itemIds: [itemId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                MenuBarId = configuredMenuBarId,
                ItemsSource = new[]
                {
                    new ToolbarItem(
                        itemId,
                        ToolbarItemKind.Button,
                        new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                        ToolbarItemDisplayIntent.TextOnly,
                        command: new TestCommand())
                }
            };

            using var host = new TemplateTestHost(control);
            var toolbarTray = TemplateTestHost.FindChild<ToolBarTray>(control);
            Assert.That(toolbarTray, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarTray!);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);

            var items = contextMenu!.Items.Cast<object>().ToList();
            Assert.That(((MenuItem)items[0]).Header, Is.EqualTo("Configured Menu Bar"));
            Assert.That(items[1], Is.TypeOf<Separator>());
            Assert.That(items.OfType<MenuItem>().Any(item => Equals(item.Header, "Legacy Menu Bar")), Is.True,
                "Unconfigured menu bar definitions should remain in the toolbar section.");
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void HostRightClick_OnComboBox_ShowsToolbarCustomizeMenu()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var comboId = new ToolbarItemId("Build.Config");
            var comboItem = new ToolbarItem(
                comboId,
                ToolbarItemKind.ComboBox,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Configuration")),
                ToolbarItemDisplayIntent.TextOnly,
                selectionItems: new[] { "Debug", "Release" },
                selectedValue: "Debug");
            registry.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [comboId]));

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[] { comboItem }
            };

            using var host = new TemplateTestHost(control);
            var comboBox = TemplateTestHost.FindChild<ComboBox>(control);
            Assert.That(comboBox, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, comboBox!);
            Assert.That(menuOpened, Is.True, "Right-click on ComboBox within toolbar should open host menu");

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);
            Assert.That(contextMenu!.Items.OfType<MenuItem>().Any(m => Equals(m.Header, "Customize...")), Is.True);
            Assert.That(contextMenu.Items.OfType<MenuItem>().Any(m => Equals(m.Header, "Cut")), Is.False,
                "Host menu should be toolbar customization menu, not default control text-edit menu");
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void CustomizeToggle_UpdatesRegistryAndProjectionVisibilityImmediately()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId = new ToolbarItemId("Build.Run");
            var item = new ToolbarItem(
                itemId,
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand());
            var definition = new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]);
            registry.RegisterDefinition(definition);

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[] { item }
            };

            using var host = new TemplateTestHost(control);
            var toolbar = TemplateTestHost.FindChild<ToolBar>(control);
            var button = TemplateTestHost.FindChild<Button>(control);
            Assert.That(toolbar, Is.Not.Null);
            Assert.That(button, Is.Not.Null);
            var toolbarHost = toolbar!;
            Assert.That(button!.Visibility, Is.EqualTo(Visibility.Visible));

            var menuOpened = OpenToolbarContextMenu(control, toolbarHost);
            Assert.That(menuOpened, Is.True);

            var contextMenu = control.ContextMenu;
            Assert.That(contextMenu, Is.Not.Null);
            var nonNullContextMenu = contextMenu!;
            var customizeEntry = nonNullContextMenu.Items.OfType<MenuItem>().Single(m => Equals(m.Header, "Customize..."));
            var itemEntry = customizeEntry.Items.OfType<MenuItem>().Single(m => Equals(m.Header, "Run"));

            itemEntry.IsChecked = false;
            itemEntry.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            control.UpdateLayout();

            // Registry should show item as not visible
            Assert.That(registry.IsItemVisible(toolbarId, itemId), Is.False);
            
            // Button should be hidden due to filter
            var updatedButton = TemplateTestHost.FindChild<Button>(control);
            Assert.That(updatedButton, Is.Null, "Button should be filtered out when not visible in registry");
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void ItemsSourceMutation_Reset_PreservesRegistryVisibilityFiltering()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId1 = new ToolbarItemId("Build.Item1");
            var itemId2 = new ToolbarItemId("Build.Item2");
            
            var definition = new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId1, itemId2]);
            registry.RegisterDefinition(definition);

            // Initial items
            var item1 = new ToolbarItem(
                itemId1,
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Item 1", "Tooltip 1")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand());
            var item2 = new ToolbarItem(
                itemId2,
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Item 2", "Tooltip 2")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand());

            var items = new ObservableCollection<ToolbarItem> { item1, item2 };

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = items
            };

            using var host = new TemplateTestHost(control);

            // Hide item 1
            registry.SetItemVisibility(toolbarId, itemId1, false);
            control.UpdateLayout();

            // Verify item 1 is hidden
            var buttons = TemplateTestHost.FindAllChildren<Button>(control).ToList();
            Assert.That(buttons, Has.Count.EqualTo(1), "Only item 2 should be visible");
            Assert.That(buttons[0].ToolTip, Is.EqualTo("Tooltip 2"));

            for (var rebuildIndex = 1; rebuildIndex <= 2; rebuildIndex++)
            {
                // Rebuild the same ItemsSource instance (Clear triggers Reset)
                var newItem1 = new ToolbarItem(
                    itemId1,
                    ToolbarItemKind.Button,
                    new ToolbarItemSemanticMetadata(new ToolbarItemText($"Item 1 (rebuilt {rebuildIndex})", $"Tooltip 1 (rebuilt {rebuildIndex})")),
                    ToolbarItemDisplayIntent.TextOnly,
                    command: new TestCommand());
                var newItem2 = new ToolbarItem(
                    itemId2,
                    ToolbarItemKind.Button,
                    new ToolbarItemSemanticMetadata(new ToolbarItemText($"Item 2 (rebuilt {rebuildIndex})", $"Tooltip 2 (rebuilt {rebuildIndex})")),
                    ToolbarItemDisplayIntent.TextOnly,
                    command: new TestCommand());

                items.Clear();
                items.Add(newItem1);
                items.Add(newItem2);
                control.UpdateLayout();

                Assert.That(control.ItemsSource, Is.SameAs(items), "Test must mutate the same ItemsSource instance");

                // Verify filtering is still applied - item 1 should remain hidden
                var buttonsAfterRebuild = TemplateTestHost.FindAllChildren<Button>(control).ToList();
                Assert.That(buttonsAfterRebuild, Has.Count.EqualTo(1), "After rebuild, only item 2 should still be visible");
                Assert.That(buttonsAfterRebuild[0].ToolTip, Is.EqualTo($"Tooltip 2 (rebuilt {rebuildIndex})"));
            }
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    [Test]
    public void ItemsSourceReplaced_FilterAppliedToNewCollection()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataPath);

        try
        {
            var registry = new ToolbarRegistryService(appDataPath);
            var toolbarId = new ToolbarId("Build");
            var itemId1 = new ToolbarItemId("Build.Hidden");
            var itemId2 = new ToolbarItemId("Build.Visible");
            
            var definition = new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId1, itemId2]);
            registry.RegisterDefinition(definition);

            // Set visibility before creating control
            registry.SetItemVisibility(toolbarId, itemId1, false);
            registry.SetItemVisibility(toolbarId, itemId2, true);

            var item1 = new ToolbarItem(itemId1, ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Hidden", "Hidden tooltip")),
                ToolbarItemDisplayIntent.TextOnly, command: new TestCommand());
            var item2 = new ToolbarItem(itemId2, ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Visible", "Visible tooltip")),
                ToolbarItemDisplayIntent.TextOnly, command: new TestCommand());

            var control = new ToolbarHostControl
            {
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new ObservableCollection<ToolbarItem> { item1, item2 }
            };

            using var host = new TemplateTestHost(control);

            // First ItemsSource: hidden item should be filtered
            var buttons1 = TemplateTestHost.FindAllChildren<Button>(control).ToList();
            Assert.That(buttons1, Has.Count.EqualTo(1));
            Assert.That(buttons1[0].ToolTip, Is.EqualTo("Visible tooltip"));

            // Replace ItemsSource with new collection
            var newItem1 = new ToolbarItem(itemId1, ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Hidden (v2)", "Hidden tooltip (v2)")),
                ToolbarItemDisplayIntent.TextOnly, command: new TestCommand());
            var newItem2 = new ToolbarItem(itemId2, ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Visible (v2)", "Visible tooltip (v2)")),
                ToolbarItemDisplayIntent.TextOnly, command: new TestCommand());

            control.ItemsSource = new ObservableCollection<ToolbarItem> { newItem1, newItem2 };
            control.UpdateLayout();

            // Second ItemsSource: filter should still be applied
            var buttons2 = TemplateTestHost.FindAllChildren<Button>(control).ToList();
            Assert.That(buttons2, Has.Count.EqualTo(1), "Filter should be reapplied to new ItemsSource");
            Assert.That(buttons2[0].ToolTip, Is.EqualTo("Visible tooltip (v2)"));
        }
        finally
        {
            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);
        }
    }

    private sealed class StubIconProvider : IIconProvider
    {
        private readonly ImageSource _image;

        public StubIconProvider(ImageSource image)
        {
            _image = image;
        }

        public ImageSource? GetIcon(string iconKey) => _image;
    }

    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }
    }

    private static ToolbarItem CreateButtonItem(string id, int order, string tooltip, string? logicalGroup)
    {
        return new ToolbarItem(
            new ToolbarItemId(id),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText(id, tooltip)),
            ToolbarItemDisplayIntent.TextOnly,
            order: order,
            command: new TestCommand(),
            logicalGroup: logicalGroup);
    }

    private static bool OpenToolbarContextMenu(ToolbarHostControl control, DependencyObject originalSource)
    {
        var method = typeof(ToolbarHostControl).GetMethod("TryShowToolbarContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        var result = method!.Invoke(control, [originalSource, new Point(8, 8)]);
        Assert.That(result, Is.Not.Null);
        return (bool)result!;
    }
}
