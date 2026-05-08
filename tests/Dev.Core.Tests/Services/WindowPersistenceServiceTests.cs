// WindowPersistenceServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class WindowPersistenceServiceTests
{
    private string _testDirectory = null!;
    private WindowPersistenceService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _service = new WindowPersistenceService(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Test]
    public void Constructor_ThrowsException_WhenPathIsNull()
    {
        Assert.Throws<ArgumentException>(() => new WindowPersistenceService(null!));
    }

    [Test]
    public void Constructor_ThrowsException_WhenPathIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new WindowPersistenceService(string.Empty));
    }

    [Test]
    public void Constructor_ThrowsException_WhenPathIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new WindowPersistenceService("   "));
    }

    [Test]
    public void Constructor_CreatesDirectory_WhenItDoesNotExist()
    {
        var newDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var service = new WindowPersistenceService(newDir);
            Assert.That(Directory.Exists(newDir), Is.True);
        }
        finally
        {
            if (Directory.Exists(newDir))
                Directory.Delete(newDir, true);
        }
    }

    [Test]
    public void LoadWindowState_ReturnsNull_WhenKeyDoesNotExist()
    {
        var result = _service.LoadWindowState("NonExistentKey");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SaveWindowState_StoresWindowSettings()
    {
        var settings = new WindowSettings(800, 600, 100, 50);

        _service.SaveWindowState("TestWindow", settings);
        var loaded = _service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Width, Is.EqualTo(800));
        Assert.That(loaded.Height, Is.EqualTo(600));
        Assert.That(loaded.Left, Is.EqualTo(100));
        Assert.That(loaded.Top, Is.EqualTo(50));
    }

    [Test]
    public void SaveWindowState_WithExtras_StoresAllData()
    {
        var extras = new Dictionary<string, double>
        {
            ["WindowState"] = 2.0,
            ["CustomValue"] = 42.0
        };
        var settings = new WindowSettings(1024, 768, 200, 100, extras);

        _service.SaveWindowState("TestWindow", settings);
        var loaded = _service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Extras, Is.Not.Null);
        Assert.That(loaded.Extras, Has.Count.EqualTo(2));
        Assert.That(loaded.Extras!["WindowState"], Is.EqualTo(2.0));
        Assert.That(loaded.Extras["CustomValue"], Is.EqualTo(42.0));
    }

    [Test]
    public void SaveWindowState_OverwritesExistingKey()
    {
        var settings1 = new WindowSettings(800, 600, 100, 50);
        var settings2 = new WindowSettings(1024, 768, 200, 100);

        _service.SaveWindowState("TestWindow", settings1);
        _service.SaveWindowState("TestWindow", settings2);
        var loaded = _service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Width, Is.EqualTo(1024));
        Assert.That(loaded.Height, Is.EqualTo(768));
    }

    [Test]
    public void SaveWindowState_PersistsAcrossInstances()
    {
        var settings = new WindowSettings(800, 600, 100, 50);
        _service.SaveWindowState("TestWindow", settings);

        var newService = new WindowPersistenceService(_testDirectory);
        var loaded = newService.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Width, Is.EqualTo(800));
        Assert.That(loaded.Height, Is.EqualTo(600));
    }

    [Test]
    public void ResetWindowState_RemovesStoredSettings()
    {
        var settings = new WindowSettings(800, 600, 100, 50);
        _service.SaveWindowState("TestWindow", settings);

        _service.ResetWindowState("TestWindow");
        var loaded = _service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Null);
    }

    [Test]
    public void ResetWindowState_DoesNotThrow_WhenKeyDoesNotExist()
    {
        Assert.DoesNotThrow(() => _service.ResetWindowState("NonExistentKey"));
    }

    [Test]
    public void ResetWindowState_PersistsAcrossInstances()
    {
        var settings = new WindowSettings(800, 600, 100, 50);
        _service.SaveWindowState("TestWindow", settings);
        _service.ResetWindowState("TestWindow");

        var newService = new WindowPersistenceService(_testDirectory);
        var loaded = newService.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Null);
    }

    [Test]
    public void SaveWindowState_HandlesMultipleKeys()
    {
        var settings1 = new WindowSettings(800, 600, 100, 50);
        var settings2 = new WindowSettings(1024, 768, 200, 100);

        _service.SaveWindowState("Window1", settings1);
        _service.SaveWindowState("Window2", settings2);

        var loaded1 = _service.LoadWindowState("Window1");
        var loaded2 = _service.LoadWindowState("Window2");

        Assert.That(loaded1!.Width, Is.EqualTo(800));
        Assert.That(loaded2!.Width, Is.EqualTo(1024));
    }

    [Test]
    public void LoadWindowState_HandlesCorruptedFile()
    {
        var filePath = Path.Combine(_testDirectory, "WindowState.json");
        File.WriteAllText(filePath, "{ corrupted json content");

        var service = new WindowPersistenceService(_testDirectory);
        var loaded = service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Null);
    }

    [Test]
    public void Constructor_HandlesNullJsonContent()
    {
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var filePath = Path.Combine(testDir, "WindowState.json");
            File.WriteAllText(filePath, "null");

            var service = new WindowPersistenceService(testDir);
            var loaded = service.LoadWindowState("TestWindow");

            Assert.That(loaded, Is.Null);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void WindowSettings_Record_SupportsValueEquality()
    {
        var settings1 = new WindowSettings(800, 600, 100, 50);
        var settings2 = new WindowSettings(800, 600, 100, 50);

        Assert.That(settings1, Is.EqualTo(settings2));
    }

    [Test]
    public void WindowSettings_WithNullExtras_IsValid()
    {
        var settings = new WindowSettings(800, 600, 100, 50, null);

        _service.SaveWindowState("TestWindow", settings);
        var loaded = _service.LoadWindowState("TestWindow");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Extras, Is.Null);
    }

    [Test]
    public void WindowSettings_WidthProperty_ReturnsCorrectValue()
    {
        var settings = new WindowSettings(1024, 768, 50, 100);

        Assert.That(settings.Width, Is.EqualTo(1024));
    }

    [Test]
    public void WindowSettings_HeightProperty_ReturnsCorrectValue()
    {
        var settings = new WindowSettings(1024, 768, 50, 100);

        Assert.That(settings.Height, Is.EqualTo(768));
    }

    [Test]
    public void WindowSettings_LeftProperty_ReturnsCorrectValue()
    {
        var settings = new WindowSettings(1024, 768, 50, 100);

        Assert.That(settings.Left, Is.EqualTo(50));
    }

    [Test]
    public void WindowSettings_TopProperty_ReturnsCorrectValue()
    {
        var settings = new WindowSettings(1024, 768, 50, 100);

        Assert.That(settings.Top, Is.EqualTo(100));
    }

    [Test]
    public void WindowSettings_ExtrasProperty_ReturnsCorrectValue()
    {
        var extras = new Dictionary<string, double>
        {
            ["Key1"] = 42.0,
            ["Key2"] = 99.5
        };
        var settings = new WindowSettings(800, 600, 100, 50, extras);

        Assert.That(settings.Extras, Is.Not.Null);
        Assert.That(settings.Extras, Has.Count.EqualTo(2));
        Assert.That(settings.Extras!["Key1"], Is.EqualTo(42.0));
        Assert.That(settings.Extras["Key2"], Is.EqualTo(99.5));
    }

    [Test]
    public void WindowSettings_ExtrasProperty_CanBeNull()
    {
        var settings = new WindowSettings(800, 600, 100, 50, null);

        Assert.That(settings.Extras, Is.Null);
    }

    [Test]
    public void WindowSettings_ExtrasProperty_DefaultsToNull()
    {
        var settings = new WindowSettings(800, 600, 100, 50);

        Assert.That(settings.Extras, Is.Null);
    }

    [Test]
    public void WindowSettings_AllProperties_WorkTogether()
    {
        var extras = new Dictionary<string, double> { ["Test"] = 123.45 };
        var settings = new WindowSettings(1920, 1080, 200, 150, extras);

        Assert.That(settings.Width, Is.EqualTo(1920));
        Assert.That(settings.Height, Is.EqualTo(1080));
        Assert.That(settings.Left, Is.EqualTo(200));
        Assert.That(settings.Top, Is.EqualTo(150));
        Assert.That(settings.Extras, Is.Not.Null);
        Assert.That(settings.Extras!["Test"], Is.EqualTo(123.45));
    }

    [Test]
    public void WindowSettings_WithExpression_CreatesNewInstance()
    {
        var original = new WindowSettings(800, 600, 100, 50);
        var modified = original with { Width = 1024, Height = 768 };

        Assert.That(modified.Width, Is.EqualTo(1024));
        Assert.That(modified.Height, Is.EqualTo(768));
        Assert.That(modified.Left, Is.EqualTo(100));
        Assert.That(modified.Top, Is.EqualTo(50));
        Assert.That(original.Width, Is.EqualTo(800), "Original should be unchanged");
    }

    [Test]
    public void WindowSettings_WithExpression_CanModifyLeft()
    {
        var original = new WindowSettings(800, 600, 100, 50);
        var modified = original with { Left = 250 };

        Assert.That(modified.Left, Is.EqualTo(250));
        Assert.That(modified.Width, Is.EqualTo(800));
        Assert.That(modified.Height, Is.EqualTo(600));
        Assert.That(modified.Top, Is.EqualTo(50));
        Assert.That(original.Left, Is.EqualTo(100), "Original should be unchanged");
    }

    [Test]
    public void WindowSettings_WithExpression_CanModifyTop()
    {
        var original = new WindowSettings(800, 600, 100, 50);
        var modified = original with { Top = 300 };

        Assert.That(modified.Top, Is.EqualTo(300));
        Assert.That(modified.Width, Is.EqualTo(800));
        Assert.That(modified.Height, Is.EqualTo(600));
        Assert.That(modified.Left, Is.EqualTo(100));
        Assert.That(original.Top, Is.EqualTo(50), "Original should be unchanged");
    }

    [Test]
    public void WindowSettings_WithExpression_CanModifyExtras()
    {
        var originalExtras = new Dictionary<string, double> { ["Key1"] = 1.0 };
        var original = new WindowSettings(800, 600, 100, 50, originalExtras);

        var newExtras = new Dictionary<string, double> { ["Key2"] = 2.0 };
        var modified = original with { Extras = newExtras };

        Assert.That(modified.Extras, Has.Count.EqualTo(1));
        Assert.That(modified.Extras!["Key2"], Is.EqualTo(2.0));
        Assert.That(original.Extras!["Key1"], Is.EqualTo(1.0), "Original should be unchanged");
    }
}
