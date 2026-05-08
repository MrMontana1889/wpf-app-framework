// CoreServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using NSubstitute;
using NUnit.Framework;

namespace Dev.Core.Tests;

[TestFixture]
public class CoreServiceTests
{
    private IWindowPersistenceService _mockWindowPersistence = null!;
    private IVersionCheckService _mockVersionCheck = null!;
    private IDialogService _mockDialogs = null!;
    private IApplicationSettingsService _mockSettings = null!;
    private IToolbarRegistryService _mockToolbarRegistry = null!;
    private CoreService _coreService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWindowPersistence = Substitute.For<IWindowPersistenceService>();
        _mockVersionCheck = Substitute.For<IVersionCheckService>();
        _mockDialogs = Substitute.For<IDialogService>();
        _mockSettings = Substitute.For<IApplicationSettingsService>();
        _mockToolbarRegistry = Substitute.For<IToolbarRegistryService>();

        _coreService = new CoreService(
            _mockWindowPersistence,
            _mockVersionCheck,
            _mockDialogs,
            _mockSettings,
            _mockToolbarRegistry);
    }

    [Test]
    public void Constructor_SetsWindowPersistenceProperty()
    {
        Assert.That(_coreService.WindowPersistence, Is.SameAs(_mockWindowPersistence));
    }

    [Test]
    public void Constructor_SetsVersionCheckProperty()
    {
        Assert.That(_coreService.VersionCheck, Is.SameAs(_mockVersionCheck));
    }

    [Test]
    public void Constructor_SetsDialogsProperty()
    {
        Assert.That(_coreService.Dialogs, Is.SameAs(_mockDialogs));
    }

    [Test]
    public void Constructor_SetsSettingsProperty()
    {
        Assert.That(_coreService.Settings, Is.SameAs(_mockSettings));
    }

    [Test]
    public void Constructor_SetsToolbarRegistryProperty()
    {
        Assert.That(_coreService.ToolbarRegistry, Is.SameAs(_mockToolbarRegistry));
    }

    [Test]
    public void CoreService_ImplementsICoreService()
    {
        Assert.That(_coreService, Is.InstanceOf<ICoreService>());
    }

    [Test]
    public void Properties_ReturnNonNull()
    {
        Assert.That(_coreService.WindowPersistence, Is.Not.Null);
        Assert.That(_coreService.VersionCheck, Is.Not.Null);
        Assert.That(_coreService.Dialogs, Is.Not.Null);
        Assert.That(_coreService.Settings, Is.Not.Null);
        Assert.That(_coreService.ToolbarRegistry, Is.Not.Null);
    }
}

