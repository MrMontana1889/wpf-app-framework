// MenuItemTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using Dev.Core.Toolbar;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.Menu;

[TestFixture]
public class MenuItemTests
{
    [Test]
    public void Constructor_ComposesSemanticModel_AsExpected()
    {
        var id = new MenuItemId("File.Open");
        var metadata = CreateMetadata("Open", "Open file");
        var shortcut = new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.O);
        var command = new TestCommand();

        var item = new MenuItem(
            id,
            MenuItemKind.Command,
            metadata,
            shortcut: shortcut,
            command: command,
            isEnabled: false,
            isVisible: false);

        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo(id));
            Assert.That(item.Kind, Is.EqualTo(MenuItemKind.Command));
            Assert.That(item.SemanticMetadata, Is.EqualTo(metadata));
            Assert.That(item.Shortcut, Is.EqualTo(shortcut));
            Assert.That(item.Command, Is.SameAs(command));
            Assert.That(item.IsEnabled, Is.False);
            Assert.That(item.IsVisible, Is.False);
            Assert.That(item.Children, Is.Empty);
        });
    }

    [Test]
    public void Constructor_CommandKindWithoutCommand_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new MenuItem(
                new MenuItemId("File.Open"),
                MenuItemKind.Command,
                CreateMetadata("Open")));
    }

    [Test]
    public void Constructor_CommandKindWithChildren_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new MenuItem(
                new MenuItemId("File.Open"),
                MenuItemKind.Command,
                CreateMetadata("Open"),
                command: new TestCommand(),
                children: [CreateCommandChild()]));
    }

    [Test]
    public void Constructor_CheckableKindWithoutCheckedState_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new MenuItem(
                new MenuItemId("View.ShowLineNumbers"),
                MenuItemKind.Checkable,
                CreateMetadata("Show Line Numbers")));
    }

    [Test]
    public void Constructor_SeparatorForbidsCommandAndShortcut_Throws()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                _ = new MenuItem(
                    new MenuItemId("File.Separator1"),
                    MenuItemKind.Separator,
                    CreateMetadata("Separator"),
                    command: new TestCommand()));

            Assert.Throws<ArgumentException>(() =>
                _ = new MenuItem(
                    new MenuItemId("File.Separator1"),
                    MenuItemKind.Separator,
                    CreateMetadata("Separator"),
                    shortcut: new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.S)));
        });
    }

    [Test]
    public void Constructor_SubmenuRequiresChildren_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new MenuItem(
                new MenuItemId("File"),
                MenuItemKind.Submenu,
                CreateMetadata("File")));
    }

    [Test]
    public void Constructor_SubmenuWithChildren_AssignsHierarchy()
    {
        var open = CreateCommandChild("File.Open", "Open", MenuShortcutKey.O);
        var save = CreateCommandChild("File.Save", "Save", MenuShortcutKey.S);

        var file = new MenuItem(
            new MenuItemId("File"),
            MenuItemKind.Submenu,
            CreateMetadata("File"),
            children: [open, save]);

        Assert.That(file.Children.Select(child => child.Id.Value), Is.EqualTo(new[] { "File.Open", "File.Save" }));
    }

    [Test]
    public void Constructor_SubmenuWithNestedSubmenu_AllowsHierarchy()
    {
        var exportPdf = CreateCommandChild("File.Export.Pdf", "Export PDF", MenuShortcutKey.P);
        var export = new MenuItem(
            new MenuItemId("File.Export"),
            MenuItemKind.Submenu,
            CreateMetadata("Export"),
            children: [exportPdf]);

        var file = new MenuItem(
            new MenuItemId("File"),
            MenuItemKind.Submenu,
            CreateMetadata("File"),
            children: [CreateCommandChild("File.Open", "Open", MenuShortcutKey.O), export]);

        Assert.That(file.Children[1].Children[0].Id.Value, Is.EqualTo("File.Export.Pdf"));
    }

    [Test]
    public void Constructor_CopiesChildren_ToProtectImmutability()
    {
        var sourceChildren = new List<MenuItem>
        {
            CreateCommandChild("File.Open", "Open", MenuShortcutKey.O)
        };

        var file = new MenuItem(
            new MenuItemId("File"),
            MenuItemKind.Submenu,
            CreateMetadata("File"),
            children: sourceChildren);

        sourceChildren.Add(CreateCommandChild("File.Save", "Save", MenuShortcutKey.S));

        Assert.That(file.Children, Has.Count.EqualTo(1));
    }

    [Test]
    public void Constructor_WithDefaultId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new MenuItem(
                default,
                MenuItemKind.Command,
                CreateMetadata("Open"),
                command: new TestCommand()));
    }

    private static MenuItem CreateCommandChild(
        string id = "File.Open",
        string label = "Open",
        MenuShortcutKey key = MenuShortcutKey.O)
    {
        return new MenuItem(
            new MenuItemId(id),
            MenuItemKind.Command,
            CreateMetadata(label),
            shortcut: new MenuShortcut(MenuShortcutModifiers.Ctrl, key),
            command: new TestCommand());
    }

    private static MenuItemSemanticMetadata CreateMetadata(string label, string? assistiveText = null) =>
        new(new ToolbarItemText(label, assistiveText));

    private sealed class TestCommand : ICommand
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
