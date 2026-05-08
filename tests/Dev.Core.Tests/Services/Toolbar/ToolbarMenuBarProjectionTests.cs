// ToolbarMenuBarProjectionTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarMenuBarProjectionTests
{
    private IDialogService _dialogService = null!;

    private sealed class TestToolbarModel : ToolbarModel
    {
        public override string Name { get; }

        public TestToolbarModel(IDialogService dialogService, string name)
            : base(dialogService)
        {
            Name = name;
        }
    }

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

    [SetUp]
    public void SetUp()
    {
        _dialogService = Substitute.For<IDialogService>();
    }

    [Test]
    public void Project_CreatesOneTopLevelMenuPerToolbar_AndIncludesEligibleItems()
    {
        var toolbarA = new TestToolbarModel(_dialogService, "Build");
        toolbarA.Items.Add(new ToolbarItemModel(new TestCommand(), "Build", "Build"));
        toolbarA.Items.Add(new ToolbarItemModel(new TestCommand(), "InternalOnly", "Internal", includeInMenuBar: false));

        var toolbarB = new TestToolbarModel(_dialogService, "Advanced");
        toolbarB.Items.Add(new ToolbarItemModel(new TestCommand(), "Filter", "Filter"));

        var result = ToolbarMenuBarProjection.Project([toolbarA, toolbarB]);

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
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "One", "One"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "Two", "Two"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "Three", "Three"));

        var result = ToolbarMenuBarProjection.Project([toolbar]);

        var labels = result[0].Items
            .OfType<MenuBarCommandItemModel>()
            .Select(item => item.Label)
            .ToList();

        Assert.That(labels, Is.EqualTo(new[] { "One", "Two", "Three" }));
    }

    [Test]
    public void Project_InsertsSeparatorsOnLogicalGroupBoundaries_AndKeepsUngroupedContiguous()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "A", "A", logicalGroup: "G1"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "B", "B", logicalGroup: "G1"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "C", "C", logicalGroup: "G2"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "D", "D"));
        toolbar.Items.Add(new ToolbarItemModel(new TestCommand(), "E", "E"));

        var result = ToolbarMenuBarProjection.Project([toolbar]);
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

        var toolbar = new TestToolbarModel(_dialogService, "Build");
        toolbar.Items.Add(new ToolbarItemModel(command, "Build", "Build"));

        var result = ToolbarMenuBarProjection.Project([toolbar]);
        var projectedItem = result[0].Items.OfType<MenuBarCommandItemModel>().Single();

        Assert.That(projectedItem.Command, Is.SameAs(command));
        Assert.That(projectedItem.Command.CanExecute(null), Is.False);

        command.SetCanExecute(true);

        Assert.That(projectedItem.Command.CanExecute(null), Is.True);
    }
}
