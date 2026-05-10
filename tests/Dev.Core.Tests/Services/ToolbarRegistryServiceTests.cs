// ToolbarRegistryServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Toolbar;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarRegistryServiceTests
{
    private string _testDirectory = null!;
    private ToolbarRegistryService _service = null!;
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

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _dialogService = Substitute.For<IDialogService>();
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
    public void Register_AddsToolbarToList()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");

        _service.Register(toolbar);

        Assert.That(_service.Toolbars, Has.Count.EqualTo(1));
        Assert.That(_service.Toolbars[0], Is.SameAs(toolbar));
    }

    [Test]
    public void Register_MultipleToolbars_PreservesOrder()
    {
        var project = new TestToolbarModel(_dialogService, "Project");
        var build   = new TestToolbarModel(_dialogService, "Build");

        _service.Register(project);
        _service.Register(build);

        Assert.That(_service.Toolbars.Select(t => t.Name),
            Is.EqualTo(new[] { "Project", "Build" }));
    }

    [Test]
    public void Register_NoSavedData_LeavesVisibilityAtDefault()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");

        _service.Register(toolbar);

        Assert.That(toolbar.IsToolbarVisible, Is.True);
    }

    [Test]
    public void RegisterDefinition_AddsDefinition()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", canHide: true, defaultVisible: true);

        _service.RegisterDefinition(definition);

        Assert.That(_service.ToolbarDefinitions, Has.Count.EqualTo(1));
        Assert.That(_service.ToolbarDefinitions[0].Id, Is.EqualTo(new ToolbarId("Build")));
    }

    [Test]
    public void RegisterDefinition_DuplicateId_Throws()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build");
        _service.RegisterDefinition(definition);

        Assert.Throws<InvalidOperationException>(() => _service.RegisterDefinition(definition));
    }

    [Test]
    public void RegisterDefinition_DefaultVisibleFalse_AppliesWhenNoPersistedValue()
    {
        var definition = new ToolbarDefinition(new ToolbarId("Build"), "Build", defaultVisible: false);

        _service.RegisterDefinition(definition);

        Assert.That(_service.IsVisible(new ToolbarId("Build")), Is.False);
    }

    // --- Persistence ---

    [Test]
    public void SaveVisibility_CreatesFile()
    {
        _service.Register(new TestToolbarModel(_dialogService, "Build"));

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
    public void SaveAndLoad_RestoresVisibility()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        _service.Register(toolbar);
        toolbar.IsToolbarVisible = false;
        _service.SaveVisibility();

        var newService = new ToolbarRegistryService(_testDirectory);
        var restoredToolbar = new TestToolbarModel(_dialogService, "Build");
        newService.Register(restoredToolbar);

        Assert.That(restoredToolbar.IsToolbarVisible, Is.False);
    }

    [Test]
    public void Register_RestoresTrueVisibility()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        _service.Register(toolbar);
        _service.SaveVisibility();

        var newService = new ToolbarRegistryService(_testDirectory);
        var restoredToolbar = new TestToolbarModel(_dialogService, "Build");
        newService.Register(restoredToolbar);

        Assert.That(restoredToolbar.IsToolbarVisible, Is.True);
    }

    [Test]
    public void SaveAndLoad_MultipleToolbars_IndependentVisibility()
    {
        var project = new TestToolbarModel(_dialogService, "Project");
        var build   = new TestToolbarModel(_dialogService, "Build");
        _service.Register(project);
        _service.Register(build);
        project.IsToolbarVisible = false;
        build.IsToolbarVisible   = true;
        _service.SaveVisibility();

        var newService       = new ToolbarRegistryService(_testDirectory);
        var restoredProject  = new TestToolbarModel(_dialogService, "Project");
        var restoredBuild    = new TestToolbarModel(_dialogService, "Build");
        newService.Register(restoredProject);
        newService.Register(restoredBuild);

        Assert.Multiple(() =>
        {
            Assert.That(restoredProject.IsToolbarVisible, Is.False);
            Assert.That(restoredBuild.IsToolbarVisible,   Is.True);
        });
    }

    [Test]
    public void SaveAndLoad_ByToolbarId_RestoresVisibilityWithoutToolbarModel()
    {
        var toolbarId = new ToolbarId("Build.Toolbar");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: true));
        _service.SetVisibility(toolbarId, isVisible: false);

        var newService = new ToolbarRegistryService(_testDirectory);
        newService.RegisterDefinition(new ToolbarDefinition(toolbarId, "Build Toolbar", defaultVisible: true));

        Assert.That(newService.IsVisible(toolbarId), Is.False);
    }

    // --- Auto-save ---

    [Test]
    public void IsToolbarVisible_Change_AutoSavesToDisk()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        _service.Register(toolbar);

        toolbar.IsToolbarVisible = false;

        var newService = new ToolbarRegistryService(_testDirectory);
        var reloaded   = new TestToolbarModel(_dialogService, "Build");
        newService.Register(reloaded);

        Assert.That(reloaded.IsToolbarVisible, Is.False);
    }

    [Test]
    public void SetVisibility_ByToolbarId_UpdatesRegisteredToolbar()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        _service.Register(toolbar);

        _service.SetVisibility(new ToolbarId("Build"), isVisible: false);

        Assert.That(toolbar.IsToolbarVisible, Is.False);
    }

    // --- ToggleVisibilityCommand ---

    [Test]
    public void ToggleVisibilityCommand_TogglesVisibility()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");

        toolbar.ToggleVisibilityCommand.Execute(null);

        Assert.That(toolbar.IsToolbarVisible, Is.False);
    }

    [Test]
    public void ToggleVisibilityCommand_WhenCanHideFalse_DoesNotToggle()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        _service.Register(toolbar, canHide: false);

        toolbar.ToggleVisibilityCommand.Execute(null);

        Assert.That(toolbar.IsToolbarVisible, Is.True);
    }

    [Test]
    public void ToggleVisibilityCommand_TogglesBackToVisible()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");
        toolbar.IsToolbarVisible = false;

        toolbar.ToggleVisibilityCommand.Execute(null);

        Assert.That(toolbar.IsToolbarVisible, Is.True);
    }

    // --- canHide registration policy ---

    [Test]
    public void Register_DefaultCanHide_LeavesCanHideTrue()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Build");

        _service.Register(toolbar);

        Assert.That(toolbar.CanHide, Is.True);
    }

    [Test]
    public void Register_WithCanHideFalse_SetsCanHideOnToolbar()
    {
        var toolbar = new TestToolbarModel(_dialogService, "Project");

        _service.Register(toolbar, canHide: false);

        Assert.That(toolbar.CanHide, Is.False);
    }

    [Test]
    public void SetVisibility_WhenCanHideFalse_StaysVisible()
    {
        var toolbarId = new ToolbarId("Project");
        _service.RegisterDefinition(new ToolbarDefinition(toolbarId, "Project", canHide: false, defaultVisible: false));

        _service.SetVisibility(toolbarId, isVisible: false);

        Assert.That(_service.IsVisible(toolbarId), Is.True);
    }
}
