// ToolbarMenuBarProjectionTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using NUnit.Framework;
using System.Windows.Input;
using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarMenuBarProjectionTests
{
    private sealed class TestCommand : ICommand
    {
        private bool _canExecute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            _ = parameter;
            return _canExecute;
        }

        public void Execute(object? parameter)
        {
            _ = parameter;
            ExecuteCount++;
        }

        public int ExecuteCount { get; private set; }

        public void SetCanExecute(bool canExecute)
        {
            if (_canExecute == canExecute)
            {
                return;
            }

            _canExecute = canExecute;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static ToolbarItem ButtonItem(string id, string label, ICommand? command = null, bool includeInMenuBar = true, string? logicalGroup = null) =>
        new(
            new ToolbarItemId(id),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText(label)),
            ToolbarItemDisplayIntent.IconAndText,
            command: command ?? new TestCommand(),
            includeInMenuBar: includeInMenuBar,
            logicalGroup: logicalGroup);

    [Test]
    public void Project_CreatesOneTopLevelMenuPerToolbar_AndIncludesEligibleItems()
    {
        var buildItem = ButtonItem("build.cmd", "Build");
        var internalItem = ButtonItem("internal.cmd", "Internal", includeInMenuBar: false);
        var filterItem = ButtonItem("filter.cmd", "Filter");

        var toolbarA = new ToolbarDefinition(
            new ToolbarId("Build"),
            "Build",
            itemIds: [buildItem.Id, internalItem.Id]);

        var toolbarB = new ToolbarDefinition(
            new ToolbarId("Advanced"),
            "Advanced",
            itemIds: [filterItem.Id]);

        var result = ToolbarMenuBarProjection.Project([toolbarA, toolbarB], [buildItem, internalItem, filterItem]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Label, Is.EqualTo("Build"));
            Assert.That(result[1].Label, Is.EqualTo("Advanced"));
            Assert.That(result[0].Items.OfType<MenuBarCommandItemModel>().Select(i => i.Label), Is.EqualTo(new[] { "Build" }));
            Assert.That(result[1].Items.OfType<MenuBarCommandItemModel>().Select(i => i.Label), Is.EqualTo(new[] { "Filter" }));
        });
    }

    [Test]
    public void Project_PreservesToolbarItemOrderInMenu()
    {
        var item1 = ButtonItem("one.cmd", "One");
        var item2 = ButtonItem("two.cmd", "Two");
        var item3 = ButtonItem("three.cmd", "Three");

        var toolbar = new ToolbarDefinition(
            new ToolbarId("Build"),
            "Build",
            itemIds: [item1.Id, item2.Id, item3.Id]);

        var result = ToolbarMenuBarProjection.Project([toolbar], [item1, item2, item3]);

        var labels = result[0].Items
            .OfType<MenuBarCommandItemModel>()
            .Select(item => item.Label)
            .ToList();

        Assert.That(labels, Is.EqualTo(new[] { "One", "Two", "Three" }));
    }

    [Test]
    public void Project_InsertsSeparatorsOnLogicalGroupBoundaries_AndKeepsUngroupedContiguous()
    {
        var itemA = ButtonItem("a.cmd", "A", logicalGroup: "G1");
        var itemB = ButtonItem("b.cmd", "B", logicalGroup: "G1");
        var itemC = ButtonItem("c.cmd", "C", logicalGroup: "G2");
        var itemD = ButtonItem("d.cmd", "D");
        var itemE = ButtonItem("e.cmd", "E");

        var toolbar = new ToolbarDefinition(
            new ToolbarId("Build"),
            "Build",
            itemIds: [itemA.Id, itemB.Id, itemC.Id, itemD.Id, itemE.Id]);

        var result = ToolbarMenuBarProjection.Project([toolbar], [itemA, itemB, itemC, itemD, itemE]);
        var menuItems = result[0].Items;

        Assert.Multiple(() =>
        {
            Assert.That(menuItems[0], Is.TypeOf<MenuBarCommandItemModel>());
            Assert.That(menuItems[1], Is.TypeOf<MenuBarCommandItemModel>());
            Assert.That(menuItems[2], Is.TypeOf<MenuBarSeparatorItemModel>());
            Assert.That(menuItems[3], Is.TypeOf<MenuBarCommandItemModel>());
            Assert.That(menuItems[4], Is.TypeOf<MenuBarSeparatorItemModel>());
            Assert.That(menuItems[5], Is.TypeOf<MenuBarCommandItemModel>());
            Assert.That(menuItems[6], Is.TypeOf<MenuBarCommandItemModel>());
        });
    }

    [Test]
    public void Project_PreservesCommandInstance_AndCanExecuteStateIsShared()
    {
        var command = new TestCommand();
        command.SetCanExecute(false);

        var item = ButtonItem("build.cmd", "Build", command: command);
        var toolbar = new ToolbarDefinition(
            new ToolbarId("Build"),
            "Build",
            itemIds: [item.Id]);

        var result = ToolbarMenuBarProjection.Project([toolbar], [item]);
        var projectedItem = result[0].Items.OfType<MenuBarCommandItemModel>().Single();

        Assert.That(projectedItem.Command, Is.SameAs(command));
        Assert.That(projectedItem.Command.CanExecute(null), Is.False);

        command.SetCanExecute(true);

        Assert.That(projectedItem.Command.CanExecute(null), Is.True);
    }

    [Test]
    public void Project_EmptyItemsList_ProducesMenuWithNoItems()
    {
        var toolbar = new ToolbarDefinition(new ToolbarId("Build"), "Build");

        var result = ToolbarMenuBarProjection.Project([toolbar], []);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Items, Is.Empty);
        });
    }

    [Test]
    public void Project_AllItemsExcluded_ProducesMenuWithNoItems()
    {
        var item = ButtonItem("a.cmd", "A", includeInMenuBar: false);
        var toolbar = new ToolbarDefinition(
            new ToolbarId("Build"),
            "Build",
            itemIds: [item.Id]);

        var result = ToolbarMenuBarProjection.Project([toolbar], [item]);

        Assert.That(result[0].Items, Is.Empty);
    }
}
