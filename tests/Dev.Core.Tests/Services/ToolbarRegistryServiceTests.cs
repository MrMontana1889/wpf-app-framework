// ToolbarRegistryServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Windows.Input;

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
}
