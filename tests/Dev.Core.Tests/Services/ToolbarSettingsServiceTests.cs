// ToolbarSettingsServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarSettingsServiceTests
{
    private string _testDirectory = null!;
    private ToolbarSettingsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _service = new ToolbarSettingsService(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    private static ToolbarItemModel Item(string name, bool isVisible = true) =>
        new(Substitute.For<ICommand>(), name, name) { IsVisible = isVisible };

    // --- Persistence ---

    [Test]
    public void Save_CreatesSettingsFile()
    {
        var items = new[] { Item("Build"), Item("Pull") };

        _service.Save("MyToolbar", items);

        var path = Path.Combine(_testDirectory, "toolbar-settings.json");
        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void Save_ThenLoad_RestoresVisibility()
    {
        var saved = new[] { Item("Build", true), Item("Pull", false), Item("TMR", true) };
        _service.Save("MyToolbar", saved);

        var newService = new ToolbarSettingsService(_testDirectory);
        var loaded = new[] { Item("Build"), Item("Pull"), Item("TMR") };
        newService.Load("MyToolbar", loaded);

        Assert.Multiple(() =>
        {
            Assert.That(loaded[0].IsVisible, Is.True);
            Assert.That(loaded[1].IsVisible, Is.False);
            Assert.That(loaded[2].IsVisible, Is.True);
        });
    }

    [Test]
    public void Load_UnknownToolbar_LeavesItemsUnchanged()
    {
        var items = new[] { Item("Build", true), Item("Pull", true) };

        _service.Load("NonExistent", items);

        Assert.That(items.All(i => i.IsVisible), Is.True);
    }

    [Test]
    public void Load_UnknownItem_LeavesOtherItemsUnchanged()
    {
        var saved = new[] { Item("Build", false) };
        _service.Save("MyToolbar", saved);

        var newService = new ToolbarSettingsService(_testDirectory);
        var loaded = new[] { Item("Build"), Item("NewButton", true) };
        newService.Load("MyToolbar", loaded);

        Assert.Multiple(() =>
        {
            Assert.That(loaded[0].IsVisible, Is.False);
            Assert.That(loaded[1].IsVisible, Is.True);
        });
    }

    [Test]
    public void Save_MultipleToolbars_PersistsIndependently()
    {
        var buildItems    = new[] { Item("Build", false) };
        var projectItems  = new[] { Item("New", true), Item("Open", false) };
        _service.Save("Build",   buildItems);
        _service.Save("Project", projectItems);

        var newService = new ToolbarSettingsService(_testDirectory);

        var loadedBuild   = new[] { Item("Build") };
        var loadedProject = new[] { Item("New"), Item("Open") };
        newService.Load("Build",   loadedBuild);
        newService.Load("Project", loadedProject);

        Assert.Multiple(() =>
        {
            Assert.That(loadedBuild[0].IsVisible,   Is.False);
            Assert.That(loadedProject[0].IsVisible, Is.True);
            Assert.That(loadedProject[1].IsVisible, Is.False);
        });
    }
}
