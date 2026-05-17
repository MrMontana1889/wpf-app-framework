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
    public void Template_ContainsRootItemsHost()
    {
        var control = new ToolbarHostControl();

        using var host = new TemplateTestHost(control);

        var itemsHost = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
        Assert.That(itemsHost, Is.Not.Null);
    }

    [Test]
    public void Projection_RendersOrderedFlatItems()
    {
        var items = new[]
        {
            CreateButtonItem("Build.Null1", order: 0, tooltip: "null-1", logicalGroup: null),
            CreateButtonItem("Build.A1", order: 1, tooltip: "a-1", logicalGroup: "A"),
            CreateButtonItem("Build.Null2", order: 2, tooltip: "null-2", logicalGroup: null),
            CreateButtonItem("Build.B1", order: 3, tooltip: "b-1", logicalGroup: "B")
        };

        var control = new ToolbarHostControl { ItemsSource = items };

        using var host = new TemplateTestHost(control);

        var buttonTooltips = TemplateTestHost.FindAllChildren<Button>(control)
            .Select(button => button.ToolTip)
            .Cast<string>()
            .ToList();

        Assert.That(buttonTooltips, Is.EqualTo(new[] { "null-1", "a-1", "null-2", "b-1" }));
    }

    [Test]
    public void Projection_IgnoresLogicalGroupAndPreservesOrder()
    {
        var items = new[]
        {
            CreateButtonItem("Build.Null1", order: 0, tooltip: "null-1", logicalGroup: null),
            CreateButtonItem("Build.A1", order: 1, tooltip: "a-1", logicalGroup: "A"),
            CreateButtonItem("Build.Null2", order: 2, tooltip: "null-2", logicalGroup: null),
            CreateButtonItem("Build.A2", order: 3, tooltip: "a-2", logicalGroup: "A"),
            CreateButtonItem("Build.B1", order: 4, tooltip: "b-1", logicalGroup: "B")
        };

        var control = new ToolbarHostControl { ItemsSource = items };

        using var host = new TemplateTestHost(control);

        var buttonTooltips = TemplateTestHost.FindAllChildren<Button>(control)
            .Select(button => button.ToolTip)
            .Cast<string>()
            .ToList();

        Assert.That(buttonTooltips, Is.EqualTo(new[] { "null-1", "a-1", "null-2", "a-2", "b-1" }));
    }

    [Test]
    public void Projection_OmitsHiddenItemsAfterRegistryVisibilityFiltering()
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
                ToolbarRegistry = registry,
                ToolbarId = toolbarId,
                ItemsSource = new[]
                {
                    CreateButtonItem(itemA.Value, order: 0, tooltip: "a", logicalGroup: "A"),
                    CreateButtonItem(itemB.Value, order: 1, tooltip: "b", logicalGroup: "B")
                }
            };

            using var host = new TemplateTestHost(control);

            var buttonTooltips = TemplateTestHost.FindAllChildren<Button>(control)
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
    public void ToolbarWrapPanel_WrapsButtons_WhenWidthIsConstrained()
    {
        var items = CreateWideItems();

        var control = new ToolbarHostControl
        {
            Width = 120,
            ItemsSource = items
        };

        using var host = new TemplateTestHost(control);

        var slots = GetProjectedButtonSlots(control);

        var distinctRows = slots
            .Select(slot => slot.Top)
            .DistinctBy(value => Math.Round(value, 1))
            .Count();
        Assert.That(distinctRows, Is.GreaterThan(1));
    }

    [Test]
    public void Projection_PreservesButtonOrderingAcrossWrappedRows()
    {
        var items = CreateWideItems();

        var control = new ToolbarHostControl
        {
            Width = 120,
            ItemsSource = items
        };

        using var host = new TemplateTestHost(control);

        var orderedLabels = GetProjectedButtonSlots(control)
            .OrderBy(slot => slot.Top)
            .ThenBy(slot => slot.Left)
            .Select(slot => slot.Label)
            .ToList();

        Assert.That(
            orderedLabels,
            Is.EqualTo(new[]
            {
                "alpha",
                "beta",
                "gamma",
                "delta",
                "epsilon"
            }));
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
    public void DropDownProjection_RendersRootMenuItem_WithCommandMappedChildren()
    {
        var childCommand = new TestCommand();
        var nestedChildCommand = new TestCommand();

        var nestedChild = new ToolbarItem(
            new ToolbarItemId("Build.NestedLeaf"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Nested Leaf")),
            ToolbarItemDisplayIntent.TextOnly,
            command: nestedChildCommand);

        var childWithChildren = new ToolbarItem(
            new ToolbarItemId("Build.NestedParent"),
            ToolbarItemKind.SplitDropDown,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Nested Parent")),
            ToolbarItemDisplayIntent.TextOnly,
            command: new TestCommand(),
            children: new[] { nestedChild });

        var directChild = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Run")),
            ToolbarItemDisplayIntent.TextOnly,
            command: childCommand);

        var dropDown = new ToolbarItem(
            new ToolbarItemId("Build.Actions"),
            ToolbarItemKind.DropDown,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Actions")),
            ToolbarItemDisplayIntent.TextOnly,
            children: new[] { directChild, childWithChildren });

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { dropDown },
        };

        using var host = new TemplateTestHost(control);

        var rootMenuItem = TemplateTestHost.FindChild<MenuItem>(control);

        Assert.That(rootMenuItem, Is.Not.Null);
        Assert.That(rootMenuItem!.Header, Is.EqualTo("Actions"));
        Assert.That(rootMenuItem.Command, Is.Null);

        rootMenuItem.IsSubmenuOpen = true;
        control.UpdateLayout();

        var firstChildContainer = rootMenuItem.ItemContainerGenerator.ContainerFromIndex(0) as MenuItem;
        Assert.That(firstChildContainer, Is.Not.Null);
        Assert.That(firstChildContainer!.Header, Is.EqualTo("Run"));
        Assert.That(firstChildContainer.Command, Is.SameAs(childCommand));

        var secondChildContainer = rootMenuItem.ItemContainerGenerator.ContainerFromIndex(1) as MenuItem;
        Assert.That(secondChildContainer, Is.Not.Null);

        secondChildContainer!.IsSubmenuOpen = true;
        control.UpdateLayout();

        var nestedChildContainer = secondChildContainer.ItemContainerGenerator.ContainerFromIndex(0) as MenuItem;
        Assert.That(nestedChildContainer, Is.Not.Null);
        Assert.That(nestedChildContainer!.Header, Is.EqualTo("Nested Leaf"));
        Assert.That(nestedChildContainer.Command, Is.SameAs(nestedChildCommand));
    }

    [Test]
    public void SplitDropDownProjection_MainButtonClick_ExecutesPrimaryCommand()
    {
        var primaryCommand = new TestCommand();

        var splitDropDown = new ToolbarItem(
            new ToolbarItemId("Build.Split"),
            ToolbarItemKind.SplitDropDown,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Split")),
            ToolbarItemDisplayIntent.TextOnly,
            command: primaryCommand,
            children: new[]
            {
                new ToolbarItem(
                    new ToolbarItemId("Build.Child"),
                    ToolbarItemKind.Button,
                    new ToolbarItemSemanticMetadata(new ToolbarItemText("Child")),
                    ToolbarItemDisplayIntent.TextOnly,
                    command: new TestCommand())
            });

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { splitDropDown },
        };

        using var host = new TemplateTestHost(control);

        var button = TemplateTestHost.FindAllChildren<Button>(control)
            .FirstOrDefault(candidate => ReferenceEquals(candidate.Command, primaryCommand));

        Assert.That(button, Is.Not.Null);

        Assert.That(button!.Command, Is.SameAs(primaryCommand));

        button.Command!.Execute(null);

        Assert.That(primaryCommand.ExecuteCount, Is.EqualTo(1));
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
            var toolbarHost = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarHost, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarHost!);
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
            var toolbarChrome = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarChrome, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarChrome!);
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
            var toolbarChrome = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarChrome, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarChrome!);
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
            var toolbarChrome = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarChrome, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarChrome!);
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
            var toolbarChrome = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarChrome, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarChrome!);
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
            var toolbarChrome = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarChrome, Is.Not.Null);

            var menuOpened = OpenToolbarContextMenu(control, toolbarChrome!);
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
            var button = TemplateTestHost.FindChild<Button>(control);
            Assert.That(button, Is.Not.Null);
            var toolbarHost = TemplateTestHost.FindNamedChild<ItemsControl>(control, "PART_ItemsHost");
            Assert.That(toolbarHost, Is.Not.Null);
            Assert.That(button!.Visibility, Is.EqualTo(Visibility.Visible));

            var menuOpened = OpenToolbarContextMenu(control, toolbarHost!);
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
        public int ExecuteCount { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _ = parameter;
            ExecuteCount++;
        }
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

    private static IReadOnlyList<ToolbarItem> CreateWideItems()
    {
        return new[]
        {
            CreateButtonItem("alpha-with-very-long-caption", 0, "alpha", "A"),
            CreateButtonItem("beta-with-very-long-caption", 1, "beta", "B"),
            CreateButtonItem("gamma-with-very-long-caption", 2, "gamma", "C"),
            CreateButtonItem("delta-with-very-long-caption", 3, "delta", "D"),
            CreateButtonItem("epsilon-with-very-long-caption", 4, "epsilon", "E")
        };
    }

    private static bool OpenToolbarContextMenu(ToolbarHostControl control, DependencyObject originalSource)
    {
        var method = typeof(ToolbarHostControl).GetMethod("TryShowToolbarContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        var result = method!.Invoke(control, [originalSource, new Point(8, 8)]);
        Assert.That(result, Is.Not.Null);
        return (bool)result!;
    }

    private static List<(double Left, double Top, double Right, string Label)> GetProjectedButtonSlots(ToolbarHostControl control)
    {
        return TemplateTestHost
            .FindAllChildren<Button>(control)
            .Select(button =>
            {
                var topLeft = button.TransformToAncestor(control).Transform(new Point(0, 0));

                return (
                    Left: topLeft.X,
                    Top: topLeft.Y,
                    Right: topLeft.X + button.ActualWidth,
                    Label: (string)button.ToolTip);
            })
            .ToList();
    }
}
