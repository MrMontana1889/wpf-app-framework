// ToolbarRegistryServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarRegistryServiceTests
{
    private string _testDirectory = null!;
    private ToolbarRegistryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _service = new ToolbarRegistryService(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    // --- Registration ---

    [Test]
    public void RegisterDefinition_AddsDefinition()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", canHide: true, defaultVisible: true);

        _service.RegisterDefinition(definition);

        Assert.That(_service.ToolbarDefinitions, Has.Count.EqualTo(1));
        Assert.That(_service.ToolbarDefinitions[0].Id, Is.EqualTo(new ToolbarId("Build")));
    }

    [Test]
    public void RegisterDefinition_PreservesRegistrationOrder()
    {
        _service.RegisterDefinition(new ToolbarDefinition(new ToolbarId("Project"), "Project"));
        _service.RegisterDefinition(new ToolbarDefinition(new ToolbarId("Build"), "Build"));

        Assert.That(
            _service.ToolbarDefinitions.Select(d => d.Id.Value),
            Is.EqualTo(new[] { "Project", "Build" }));
    }

    [Test]
    public void RegisterDefinition_DuplicateId_Throws()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build");
        _service.RegisterDefinition(definition);

        Assert.Throws<InvalidOperationException>(() => _service.RegisterDefinition(definition));
    }

    [Test]
    public void RegisterDefinition_DefaultVisibleFalse_InitializesInvisible()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", defaultVisible: false);

        _service.RegisterDefinition(definition);

        Assert.That(_service.IsVisible(new ToolbarId("Build")), Is.False);
    }

    [Test]
    public void RegisterDefinition_DefaultVisibleTrue_InitializesVisible()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", defaultVisible: true);

        _service.RegisterDefinition(definition);

        Assert.That(_service.IsVisible(new ToolbarId("Build")), Is.True);
    }

    [Test]
    public void IsVisible_UnregisteredId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _service.IsVisible(new ToolbarId("Unknown")));
    }

    [Test]
    public void SetVisibility_UnregisteredId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _service.SetVisibility(new ToolbarId("Unknown"), false));
    }

    // --- Visibility semantics ---

    [Test]
    public void SetVisibility_ToFalse_VisibilityBecomesHidden()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));

        _service.SetVisibility(toolbarId, isVisible: false);

        Assert.That(_service.IsVisible(toolbarId), Is.False);
    }

    [Test]
    public void SetVisibility_ToTrue_VisibilityBecomesVisible()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: false));

        _service.SetVisibility(toolbarId, isVisible: true);

        Assert.That(_service.IsVisible(toolbarId), Is.True);
    }

    [Test]
    public void SetVisibility_WhenCanHideFalse_RemainsVisible()
    {
        var toolbarId = new ToolbarId("Project");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Project", canHide: false, defaultVisible: true));

        _service.SetVisibility(toolbarId, isVisible: false);

        Assert.That(_service.IsVisible(toolbarId), Is.True);
    }

    // --- Persistence ---

    [Test]
    public void SaveVisibility_CreatesFile()
    {
        _service.RegisterDefinition(new ToolbarDefinition(new ToolbarId("Build"), "Build"));

        _service.SaveVisibility();

        var path = Path.Combine(_testDirectory, "toolbar-registry.json");
        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void SaveVisibility_PersistsByToolbarIdKey()
    {
        var toolbarId = new ToolbarId("Build.Toolbar");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: true));
        _service.SetVisibility(toolbarId, isVisible: false);

        var path = Path.Combine(_testDirectory, "toolbar-registry.json");
        var json = File.ReadAllText(path);

        Assert.That(json, Does.Contain("\"Build.Toolbar\""));
    }

    [Test]
    public void SaveAndLoad_ByToolbarId_RestoresHiddenVisibility()
    {
        var toolbarId = new ToolbarId("Build.Toolbar");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: true));
        _service.SetVisibility(toolbarId, isVisible: false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: true));

        Assert.That(newService.IsVisible(toolbarId), Is.False);
    }

    [Test]
    public void SaveAndLoad_ByToolbarId_RestoresVisibleVisibility()
    {
        var toolbarId = new ToolbarId("Build.Toolbar");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: false));
        _service.SetVisibility(toolbarId, isVisible: true);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: false));

        Assert.That(newService.IsVisible(toolbarId), Is.True);
    }

    [Test]
    public void SaveAndLoad_MultipleToolbars_IndependentVisibility()
    {
        var projectId = new ToolbarId("Project");
        var buildId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(projectId, "Project", defaultVisible: true));
        _service.RegisterDefinition(new ToolbarDefinition(buildId, "Build", defaultVisible: true));
        _service.SetVisibility(projectId, isVisible: false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(projectId, "Project", defaultVisible: true));
        newService.RegisterDefinition(new ToolbarDefinition(buildId, "Build", defaultVisible: true));

        Assert.Multiple(() =>
        {
            Assert.That(newService.IsVisible(projectId), Is.False);
            Assert.That(newService.IsVisible(buildId), Is.True);
        });
    }

    [Test]
    public void SetVisibility_AutoSavesToDisk()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));

        _service.SetVisibility(toolbarId, isVisible: false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));
        Assert.That(newService.IsVisible(toolbarId), Is.False);
    }

    // --- VisibilityChanged Event ---

    [Test]
    public void SetVisibility_RaisesVisibilityChangedEvent()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));

        var eventRaised = false;
        ToolbarVisibilityChangedEventArgs? capturedArgs = null;
        _service.VisibilityChanged += (_, e) =>
        {
            if (e.ToolbarId == toolbarId)
            {
                eventRaised = true;
                capturedArgs = e;
            }
        };

        _service.SetVisibility(toolbarId, isVisible: false);

        Assert.That(eventRaised, Is.True);
        Assert.That(capturedArgs?.IsVisible, Is.False);
    }

    [Test]
    public void SetVisibility_RaisesVisibilityChangedEvent_WithVisibleTrue()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: false));

        ToolbarVisibilityChangedEventArgs? capturedArgs = null;
        _service.VisibilityChanged += (_, e) => capturedArgs = e;

        _service.SetVisibility(toolbarId, isVisible: true);

        Assert.That(capturedArgs?.IsVisible, Is.True);
    }

    [Test]
    public void SetVisibility_SameVisibility_DoesNotRaiseEvent()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));

        var eventRaised = false;
        _service.VisibilityChanged += (_, e) =>
        {
            if (e.ToolbarId == toolbarId)
                eventRaised = true;
        };

        // Set to same value as default (true)
        _service.SetVisibility(toolbarId, isVisible: true);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void RegisterDefinition_DoesNotRaiseVisibilityChangedEvent()
    {
        var toolbarId = new ToolbarId("Build");
        var eventRaised = false;
        _service.VisibilityChanged += (_, _) => eventRaised = true;

        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", defaultVisible: true));

        Assert.That(eventRaised, Is.False);
    }

    // --- MenuBar as first-class toolbar (no special casing) ---

    [Test]
    public void RegisterDefinition_MenuBarToolbar_TreatedAsRegularDefinition()
    {
        var menuBarId = new ToolbarId("MenuBar");
        var definition = new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true);

        _service.RegisterDefinition(definition);

        Assert.That(_service.ToolbarDefinitions, Has.Count.EqualTo(1));
        Assert.That(_service.IsVisible(menuBarId), Is.True);
    }

    [Test]
    public void SetVisibility_MenuBarToolbar_CanBeHidden()
    {
        var menuBarId = new ToolbarId("MenuBar");
        _service.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));

        _service.SetVisibility(menuBarId, isVisible: false);

        Assert.That(_service.IsVisible(menuBarId), Is.False);
    }

    [Test]
    public void SaveAndLoad_MenuBarVisibility_RoundTrips()
    {
        var menuBarId = new ToolbarId("MenuBar");
        _service.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));
        _service.SetVisibility(menuBarId, isVisible: false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(menuBarId, "Menu Bar", canHide: true, defaultVisible: true));

        Assert.That(newService.IsVisible(menuBarId), Is.False);
    }

    // --- ToolbarDefinitions includes Items ---

    [Test]
    public void RegisterDefinition_WithItemIds_ExposesItemIdsThroughDefinition()
    {
        var itemId = new ToolbarItemId("Build.Button");
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", itemIds: [itemId]);

        _service.RegisterDefinition(definition);

        Assert.That(_service.ToolbarDefinitions[0].ItemIds, Has.Count.EqualTo(1));
        Assert.That(_service.ToolbarDefinitions[0].ItemIds[0], Is.EqualTo(itemId));
    }

    [Test]
    public void RegisterDefinition_NoItemIds_DefinitionHasEmptyItemIdsList()
    {
        var toolbarId = new ToolbarId("Build");
        var definition = new ToolbarDefinition(toolbarId, "Build");

        _service.RegisterDefinition(definition);

        Assert.That(_service.ToolbarDefinitions[0].ItemIds, Is.Empty);
    }

    // --- Item-level visibility semantics ---

    [Test]
    public void RegisterDefinition_ItemVisibility_DefaultsFromDefinition()
    {
        var toolbarId = new ToolbarId("Build");
        var hiddenItemId = new ToolbarItemId("Build.Hidden");
        var visibleItemId = new ToolbarItemId("Build.Visible");

        // Persist some visibility state first
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [hiddenItemId, visibleItemId]));
        _service.SetItemVisibility(toolbarId, hiddenItemId, false);
        _service.SetItemVisibility(toolbarId, visibleItemId, true);

        // Create new service and re-register definition - should restore visibility
        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [hiddenItemId, visibleItemId]));

        Assert.Multiple(() =>
        {
            Assert.That(newService.IsItemVisible(toolbarId, hiddenItemId), Is.False);
            Assert.That(newService.IsItemVisible(toolbarId, visibleItemId), Is.True);
        });
    }

    [Test]
    public void SetItemVisibility_UserOverride_UpdatesVisibilityState()
    {
        var toolbarId = new ToolbarId("Build");
        var itemId = new ToolbarItemId("Build.Button");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));

        _service.SetItemVisibility(toolbarId, itemId, false);

        Assert.That(_service.IsItemVisible(toolbarId, itemId), Is.False);
    }

    [Test]
    public void SaveAndLoad_ItemVisibility_ByToolbarItemId_RoundTrips()
    {
        var toolbarId = new ToolbarId("Build.Toolbar");
        var itemId = new ToolbarItemId("Build.Button");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));
        _service.SetItemVisibility(toolbarId, itemId, false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));

        Assert.That(newService.IsItemVisible(toolbarId, itemId), Is.False);
    }

    [Test]
    public void SetItemVisibility_RaisesItemVisibilityChangedEvent()
    {
        var toolbarId = new ToolbarId("Build");
        var itemId = new ToolbarItemId("Build.Button");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));

        ToolbarItemVisibilityChangedEventArgs? captured = null;
        _service.ItemVisibilityChanged += (_, e) => captured = e;

        _service.SetItemVisibility(toolbarId, itemId, false);

        Assert.Multiple(() =>
        {
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.ToolbarId, Is.EqualTo(toolbarId));
            Assert.That(captured.ItemId, Is.EqualTo(itemId));
            Assert.That(captured.IsVisible, Is.False);
        });
    }

    [Test]
    public void SetItemVisibility_SameValue_DoesNotRaiseItemVisibilityChangedEvent()
    {
        var toolbarId = new ToolbarId("Build");
        var itemId = new ToolbarItemId("Build.Button");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [itemId]));

        var eventRaised = false;
        _service.ItemVisibilityChanged += (_, _) => eventRaised = true;

        _service.SetItemVisibility(toolbarId, itemId, true);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void IsItemVisible_UnregisteredItem_DefaultsToTrue()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [new ToolbarItemId("Build.Button")]));

        // Unregistered items should default to visible
        Assert.That(_service.IsItemVisible(toolbarId, new ToolbarItemId("Build.Missing")), Is.True);
    }

    [Test]
    public void SetItemVisibility_UnregisteredItem_CreatesNewEntry()
    {
        var toolbarId = new ToolbarId("Build");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [new ToolbarItemId("Build.Button")]));

        // Should not throw; should create new visibility entry
        _service.SetItemVisibility(toolbarId, new ToolbarItemId("Build.Missing"), false);

        Assert.That(_service.IsItemVisible(toolbarId, new ToolbarItemId("Build.Missing")), Is.False);
    }

    [Test]
    public void ItemVisibility_RebuildScenario_PreservesPersistentState()
    {
        var toolbarId = new ToolbarId("Build");
        var item1Id = new ToolbarItemId("Build.Item1");
        var item2Id = new ToolbarItemId("Build.Item2");

        // First definition with 2 items
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [item1Id, item2Id]));
        _service.SetItemVisibility(toolbarId, item1Id, false);
        _service.SetItemVisibility(toolbarId, item2Id, false);

        // Simulate rebuild: new service instance
        var newService = new ToolbarRegistryService(_testDirectory);

        // Re-register with same items
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [item1Id, item2Id]));

        // Visibility should be restored
        Assert.Multiple(() =>
        {
            Assert.That(newService.IsItemVisible(toolbarId, item1Id), Is.False);
            Assert.That(newService.IsItemVisible(toolbarId, item2Id), Is.False);
        });
    }

    [Test]
    public void ItemVisibility_RebuildScenario_NewItemsDefaultVisible()
    {
        var toolbarId = new ToolbarId("Build");
        var item1Id = new ToolbarItemId("Build.Item1");
        var item2Id = new ToolbarItemId("Build.Item2");
        var item3Id = new ToolbarItemId("Build.Item3");

        // First definition with 2 items
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [item1Id, item2Id]));
        _service.SetItemVisibility(toolbarId, item1Id, false);

        // Simulate rebuild: new service instance
        var newService = new ToolbarRegistryService(_testDirectory);

        // Re-register with 3 items (one is new)
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build", itemIds: [item1Id, item2Id, item3Id]));

        // Old item's visibility is restored, new item defaults to visible
        Assert.Multiple(() =>
        {
            Assert.That(newService.IsItemVisible(toolbarId, item1Id), Is.False);
            Assert.That(newService.IsItemVisible(toolbarId, item2Id), Is.True);
            Assert.That(newService.IsItemVisible(toolbarId, item3Id), Is.True);
        });
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

    private static ToolbarItem CreateButtonItem(string itemId, string label, bool isVisible = true)
    {
        return new ToolbarItem(
            new ToolbarItemId(itemId),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText(label, null), null),
            ToolbarItemDisplayIntent.IconAndText,
            isVisible: isVisible,
            command: new RelayCommandStub());
    }
}
