// ToolbarSettingsServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using NUnit.Framework;

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

    private static ToolbarItem Item(string id, bool isVisible = true) =>
        new(
            new ToolbarItemId(id),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText(id, null), null),
            ToolbarItemDisplayIntent.IconAndText,
            isVisible: isVisible,
            command: new RelayCommandStub());

    // --- Persistence ---

    [Test]
    public void Save_CreatesSettingsFile()
    {
        var items = new[] { Item("Build"), Item("Pull") };
        var toolbarId = new ToolbarId("MyToolbar");

        _service.Save(toolbarId, items);

        var path = Path.Combine(_testDirectory, "toolbar-settings.json");
        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void Save_ThenLoad_RestoresVisibility()
    {
        var saved = new[] { Item("Build", true), Item("Pull", false), Item("TMR", true) };
        var toolbarId = new ToolbarId("MyToolbar");
        _service.Save(toolbarId, saved);

        var newService = new ToolbarSettingsService(_testDirectory);
        var loaded = new[] { Item("Build"), Item("Pull"), Item("TMR") };
        newService.Load(toolbarId, loaded);

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

        _service.Load(new ToolbarId("NonExistent"), items);

        Assert.That(items.All(i => i.IsVisible), Is.True);
    }

    [Test]
    public void Load_UnknownItem_LeavesOtherItemsUnchanged()
    {
        var saved = new[] { Item("Build", false) };
        var toolbarId = new ToolbarId("MyToolbar");
        _service.Save(toolbarId, saved);

        var newService = new ToolbarSettingsService(_testDirectory);
        var loaded = new[] { Item("Build"), Item("NewButton", true) };
        newService.Load(toolbarId, loaded);

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
        _service.Save(new ToolbarId("Build"),   buildItems);
        _service.Save(new ToolbarId("Project"), projectItems);

        var newService = new ToolbarSettingsService(_testDirectory);

        var loadedBuild   = new[] { Item("Build") };
        var loadedProject = new[] { Item("New"), Item("Open") };
        newService.Load(new ToolbarId("Build"),   loadedBuild);
        newService.Load(new ToolbarId("Project"), loadedProject);

        Assert.Multiple(() =>
        {
            Assert.That(loadedBuild[0].IsVisible,   Is.False);
            Assert.That(loadedProject[0].IsVisible, Is.True);
            Assert.That(loadedProject[1].IsVisible, Is.False);
        });
    }

    // --- Semantic key (ToolbarItemId) ---

    [Test]
    public void Save_KeyedByToolbarItemId_NotLabel()
    {
        // Two items with same "label-like" value but different IDs
        var itemA = Item("cmd.build", false);
        var toolbarId = new ToolbarId("Build");
        _service.Save(toolbarId, [itemA]);

        var newService = new ToolbarSettingsService(_testDirectory);
        var reloaded = new[] { Item("cmd.build") };
        newService.Load(toolbarId, reloaded);

        Assert.That(reloaded[0].IsVisible, Is.False);
    }

    // -------------------------------------------------------------------------
    // Test doubles
    // -------------------------------------------------------------------------

    private sealed class RelayCommandStub : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
