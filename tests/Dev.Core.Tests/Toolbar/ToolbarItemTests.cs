// ToolbarItemTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarItemTests
{
    private static ToolbarItemSemanticMetadata CreateSemanticMetadata() =>
        new(new ToolbarItemText("Build", "Run build"), new IconKey("build"));

    [Test]
    public void Constructor_ComposesPhase1aAnd1bTypes()
    {
        var id = new ToolbarItemId("Build.Run");
        var metadata = CreateSemanticMetadata();
        var command = new TestCommand();

        var item = new ToolbarItem(
            id,
            ToolbarItemKind.Button,
            metadata,
            ToolbarItemDisplayIntent.IconAndText,
            order: 12,
            command: command);

        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo(id));
            Assert.That(item.Kind, Is.EqualTo(ToolbarItemKind.Button));
            Assert.That(item.SemanticMetadata, Is.EqualTo(metadata));
            Assert.That(item.DisplayIntent, Is.EqualTo(ToolbarItemDisplayIntent.IconAndText));
            Assert.That(item.Order, Is.EqualTo(12));
            Assert.That(item.Command, Is.SameAs(command));
        });
    }

    [Test]
    public void Constructor_WithDefaultId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                default,
                ToolbarItemKind.Label,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.TextOnly));
    }

    [Test]
    public void Constructor_WithNullSemanticMetadata_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Run"),
                ToolbarItemKind.Label,
                null!,
                ToolbarItemDisplayIntent.TextOnly));
    }

    [Test]
    public void Constructor_ButtonWithoutCommand_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Run"),
                ToolbarItemKind.Button,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.IconAndText));
    }

    [Test]
    public void Constructor_ToggleButtonWithoutCommand_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Toggle"),
                ToolbarItemKind.ToggleButton,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.IconOnly,
                isChecked: true));
    }

    [Test]
    public void Constructor_ToggleButtonWithoutCheckedState_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Toggle"),
                ToolbarItemKind.ToggleButton,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.IconOnly,
                command: new TestCommand()));
    }

    [Test]
    public void Constructor_CheckBoxWithoutCheckedState_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Check"),
                ToolbarItemKind.CheckBox,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.TextOnly));
    }

    [Test]
    public void Constructor_LabelWithCommand_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Label"),
                ToolbarItemKind.Label,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand()));
    }

    [Test]
    public void Constructor_SeparatorWithCommand_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Separator"),
                ToolbarItemKind.Separator,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.IconOnly,
                command: new TestCommand()));
    }

    [Test]
    public void Constructor_ComboBoxRequiresSelectionItems()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Config"),
                ToolbarItemKind.ComboBox,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.TextOnly));
    }

    [Test]
    public void Constructor_ComboBoxWithSelectionItems_AssignsSelectionState()
    {
        var items = new[] { "Debug", "Release" };

        var combo = new ToolbarItem(
            new ToolbarItemId("Build.Config"),
            ToolbarItemKind.ComboBox,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.TextOnly,
            selectionItems: items,
            selectedValue: "Release");

        Assert.Multiple(() =>
        {
            Assert.That(combo.SelectionItems, Is.Not.Null);
            Assert.That(combo.SelectionItems, Is.EqualTo(items));
            Assert.That(combo.SelectedValue, Is.EqualTo("Release"));
        });
    }

    [Test]
    public void Constructor_ComboBoxWithSelectedValueOutsideItems_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarItem(
                new ToolbarItemId("Build.Config"),
                ToolbarItemKind.ComboBox,
                CreateSemanticMetadata(),
                ToolbarItemDisplayIntent.TextOnly,
                selectionItems: new[] { "Debug", "Release" },
                selectedValue: "Profile"));
    }

    [Test]
    public void SetChecked_ForToggleButton_UpdatesState()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Toggle"),
            ToolbarItemKind.ToggleButton,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.IconOnly,
            command: new TestCommand(),
            isChecked: false);

        item.SetChecked(true);

        Assert.That(item.IsChecked, Is.True);
    }

    [Test]
    public void SetChecked_ForUnsupportedKind_Throws()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            command: new TestCommand());

        Assert.Throws<InvalidOperationException>(() => item.SetChecked(true));
    }

    [Test]
    public void SetSelectedValue_ForComboBox_UpdatesSelection()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Config"),
            ToolbarItemKind.ComboBox,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.TextOnly,
            selectionItems: new[] { "Debug", "Release" },
            selectedValue: "Debug");

        item.SetSelectedValue("Release");

        Assert.That(item.SelectedValue, Is.EqualTo("Release"));
    }

    [Test]
    public void SetSelectedValue_ForUnsupportedKind_Throws()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            command: new TestCommand());

        Assert.Throws<InvalidOperationException>(() => item.SetSelectedValue("Release"));
    }

    [Test]
    public void VisibilityAndEnablement_DefaultToTrue_AndCanBeUpdated()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            command: new TestCommand());

        Assert.Multiple(() =>
        {
            Assert.That(item.IsVisible, Is.True);
            Assert.That(item.IsEnabled, Is.True);
        });

        item.IsVisible = false;
        item.IsEnabled = false;

        Assert.Multiple(() =>
        {
            Assert.That(item.IsVisible, Is.False);
            Assert.That(item.IsEnabled, Is.False);
        });
    }

    [Test]
    public void Identity_RemainsStable_WhenStateChanges()
    {
        var id = new ToolbarItemId("Build.Toggle");
        var item = new ToolbarItem(
            id,
            ToolbarItemKind.ToggleButton,
            CreateSemanticMetadata(),
            ToolbarItemDisplayIntent.IconOnly,
            command: new TestCommand(),
            isChecked: false);

        item.IsVisible = false;
        item.IsEnabled = false;
        item.SetChecked(true);

        Assert.That(item.Id, Is.EqualTo(id));
    }

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
