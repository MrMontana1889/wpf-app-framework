// MenuHostControlTemplateTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CoreMenuItem = Dev.Core.Menu.MenuItem;
using Dev.Core.Menu;
using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dev.Wpf.Tests.Templates;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class MenuHostControlTemplateTests
{
    [Test]
    public void Template_ContainsNativeMenu()
    {
        var control = new MenuHostControl();

        using var host = new TemplateTestHost(control);

        var menu = TemplateTestHost.FindChild<Menu>(control);

        Assert.That(menu, Is.Not.Null);
    }

    [Test]
    public void Projection_MapsKindsAndHierarchy()
    {
        var fileMenu = new CoreMenuItem(
            new MenuItemId("File"),
            MenuItemKind.Submenu,
            new MenuItemSemanticMetadata(new ToolbarItemText("File")),
            children:
            [
                new CoreMenuItem(
                    new MenuItemId("File.New"),
                    MenuItemKind.Command,
                    new MenuItemSemanticMetadata(new ToolbarItemText("New")),
                    shortcut: new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.N),
                    command: new TestCommand()),
                new CoreMenuItem(
                    new MenuItemId("File.ToggleHidden"),
                    MenuItemKind.Checkable,
                    new MenuItemSemanticMetadata(new ToolbarItemText("Show Hidden")),
                    command: new TestCommand(),
                    isChecked: true),
                new CoreMenuItem(
                    new MenuItemId("File.Separator1"),
                    MenuItemKind.Separator,
                    new MenuItemSemanticMetadata(new ToolbarItemText("Separator"))),
            ]);

        var control = new MenuHostControl
        {
            ItemsSource = new[] { fileMenu },
        };

        using var host = new TemplateTestHost(control);

        var menu = TemplateTestHost.FindChild<Menu>(control);
        Assert.That(menu, Is.Not.Null);
        Assert.That(menu!.Items, Has.Count.EqualTo(1));

        var file = menu.Items[0] as System.Windows.Controls.MenuItem;
        Assert.That(file, Is.Not.Null);
        Assert.That(file!.Header, Is.EqualTo("File"));
        Assert.That(file.Items, Has.Count.EqualTo(3));

        var command = file.Items[0] as System.Windows.Controls.MenuItem;
        var checkable = file.Items[1] as System.Windows.Controls.MenuItem;

        Assert.Multiple(() =>
        {
            Assert.That(command, Is.Not.Null);
            Assert.That(command!.Header, Is.EqualTo("New"));
            Assert.That(command.InputGestureText, Is.EqualTo("Ctrl+N"));

            Assert.That(checkable, Is.Not.Null);
            Assert.That(checkable!.IsCheckable, Is.True);
            Assert.That(checkable.IsChecked, Is.True);

            Assert.That(file.Items[2], Is.InstanceOf<Separator>());
        });
    }

    [Test]
    public void Projection_BindsVisibilityAndEnablement()
    {
        var disabled = new CoreMenuItem(
            new MenuItemId("File.Disabled"),
            MenuItemKind.Command,
            new MenuItemSemanticMetadata(new ToolbarItemText("Disabled")),
            command: new TestCommand(),
            isEnabled: false,
            isVisible: false);

        var control = new MenuHostControl
        {
            ItemsSource = new[] { disabled },
        };

        using var host = new TemplateTestHost(control);

        var menu = TemplateTestHost.FindChild<Menu>(control);
        var item = menu!.Items[0] as System.Windows.Controls.MenuItem;

        Assert.That(item, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(item!.IsEnabled, Is.False);
            Assert.That(item.Visibility, Is.EqualTo(Visibility.Collapsed));
        });
    }

    [Test]
    public void Projection_UsesIconProvider()
    {
        var expected = new DrawingImage();
        var item = new CoreMenuItem(
            new MenuItemId("File.Open"),
            MenuItemKind.Command,
            new MenuItemSemanticMetadata(new ToolbarItemText("Open"), new IconKey("open")),
            command: new TestCommand());

        var control = new MenuHostControl
        {
            ItemsSource = new[] { item },
            IconProvider = new StubIconProvider(expected),
        };

        using var host = new TemplateTestHost(control);

        var menu = TemplateTestHost.FindChild<Menu>(control);
        var projected = menu!.Items[0] as System.Windows.Controls.MenuItem;

        Assert.That(projected, Is.Not.Null);
        Assert.That(projected!.Icon, Is.InstanceOf<Image>());
        Assert.That(((Image)projected.Icon).Source, Is.SameAs(expected));
    }

    [Test]
    public void MultipleHosts_RenderIndependentItems()
    {
        var left = new MenuHostControl
        {
            ItemsSource = new[]
            {
                new CoreMenuItem(
                    new MenuItemId("File"),
                    MenuItemKind.Command,
                    new MenuItemSemanticMetadata(new ToolbarItemText("File")),
                    command: new TestCommand())
            }
        };

        var right = new MenuHostControl
        {
            ItemsSource = new[]
            {
                new CoreMenuItem(
                    new MenuItemId("View"),
                    MenuItemKind.Command,
                    new MenuItemSemanticMetadata(new ToolbarItemText("View")),
                    command: new TestCommand())
            }
        };

        var panel = new StackPanel();
        panel.Children.Add(left);
        panel.Children.Add(right);

        using var host = new TemplateTestHost(panel);

        var leftMenu = TemplateTestHost.FindChild<Menu>(left);
        var rightMenu = TemplateTestHost.FindChild<Menu>(right);

        Assert.Multiple(() =>
        {
            Assert.That(((System.Windows.Controls.MenuItem)leftMenu!.Items[0]).Header, Is.EqualTo("File"));
            Assert.That(((System.Windows.Controls.MenuItem)rightMenu!.Items[0]).Header, Is.EqualTo("View"));
        });
    }

    private sealed class StubIconProvider : IIconProvider
    {
        private readonly ImageSource _source;

        public StubIconProvider(ImageSource source)
        {
            _source = source;
        }

        public ImageSource? GetIcon(string iconKey) => _source;
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
}
