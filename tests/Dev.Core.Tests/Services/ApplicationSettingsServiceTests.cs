// ApplicationSettingsServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Models;
using Dev.Core.Services;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ApplicationSettingsServiceTests
{
    private string _testDirectory = null!;
    private ApplicationSettingsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _service = new ApplicationSettingsService(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Test]
    public void Constructor_CreatesDefaultSettings()
    {
        Assert.That(_service.CurrentSettings, Is.Not.Null);
        Assert.That(_service.CurrentSettings.ThemeOverride, Is.EqualTo("System"));
        Assert.That(_service.CurrentSettings.RecentFilesMaxCount, Is.EqualTo(10));
        Assert.That(_service.CurrentSettings.RecentFiles, Is.Empty);
    }

    [Test]
    public void SaveSettings_CreatesSettingsFile()
    {
        _service.SaveSettings();

        var settingsPath = Path.Combine(_testDirectory, "appsettings.json");
        Assert.That(File.Exists(settingsPath), Is.True);
    }

    [Test]
    public void LoadSettings_LoadsPreviouslySavedSettings()
    {
        _service.CurrentSettings.ThemeOverride = "Dark";
        _service.CurrentSettings.RecentFilesMaxCount = 5;
        _service.SaveSettings();

        var newService = new ApplicationSettingsService(_testDirectory);

        Assert.That(newService.CurrentSettings.ThemeOverride, Is.EqualTo("Dark"));
        Assert.That(newService.CurrentSettings.RecentFilesMaxCount, Is.EqualTo(5));
    }

    [Test]
    public void LoadSettings_WithNoFile_UsesDefaults()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(emptyDir);

        try
        {
            var service = new ApplicationSettingsService(emptyDir);

            Assert.That(service.CurrentSettings.ThemeOverride, Is.EqualTo("System"));
            Assert.That(service.CurrentSettings.RecentFilesMaxCount, Is.EqualTo(10));
            Assert.That(service.CurrentSettings.RecentFiles, Is.Empty);
        }
        finally
        {
            Directory.Delete(emptyDir, true);
        }
    }

    [Test]
    public void LoadSettings_WithNullJsonContent_UsesDefaults()
    {
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var settingsPath = Path.Combine(testDir, "appsettings.json");
            File.WriteAllText(settingsPath, "null");

            var service = new ApplicationSettingsService(testDir);

            Assert.That(service.CurrentSettings.ThemeOverride, Is.EqualTo("System"));
            Assert.That(service.CurrentSettings.RecentFilesMaxCount, Is.EqualTo(10));
            Assert.That(service.CurrentSettings.RecentFiles, Is.Empty);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void AddRecentFile_AddsFileToList()
    {
        _service.AddRecentFile("C:\\test\\file1.txt");

        Assert.That(_service.CurrentSettings.RecentFiles, Has.Count.EqualTo(1));
        Assert.That(_service.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file1.txt"));
    }

    [Test]
    public void AddRecentFile_AddsFileToTopOfList()
    {
        _service.AddRecentFile("C:\\test\\file1.txt");
        _service.AddRecentFile("C:\\test\\file2.txt");

        Assert.That(_service.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file2.txt"));
        Assert.That(_service.CurrentSettings.RecentFiles[1], Is.EqualTo("C:\\test\\file1.txt"));
    }

    [Test]
    public void AddRecentFile_RemovesDuplicates()
    {
        _service.AddRecentFile("C:\\test\\file1.txt");
        _service.AddRecentFile("C:\\test\\file2.txt");
        _service.AddRecentFile("C:\\test\\file1.txt");

        Assert.That(_service.CurrentSettings.RecentFiles, Has.Count.EqualTo(2));
        Assert.That(_service.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file1.txt"));
        Assert.That(_service.CurrentSettings.RecentFiles[1], Is.EqualTo("C:\\test\\file2.txt"));
    }

    [Test]
    public void AddRecentFile_TrimsListToMaxCount()
    {
        _service.CurrentSettings.RecentFilesMaxCount = 3;

        _service.AddRecentFile("C:\\test\\file1.txt");
        _service.AddRecentFile("C:\\test\\file2.txt");
        _service.AddRecentFile("C:\\test\\file3.txt");
        _service.AddRecentFile("C:\\test\\file4.txt");

        Assert.That(_service.CurrentSettings.RecentFiles, Has.Count.EqualTo(3));
        Assert.That(_service.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file4.txt"));
        Assert.That(_service.CurrentSettings.RecentFiles[2], Is.EqualTo("C:\\test\\file2.txt"));
    }

    [Test]
    public void AddRecentFile_SavesSettingsAutomatically()
    {
        _service.AddRecentFile("C:\\test\\file1.txt");

        var newService = new ApplicationSettingsService(_testDirectory);
        Assert.That(newService.CurrentSettings.RecentFiles, Has.Count.EqualTo(1));
        Assert.That(newService.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file1.txt"));
    }

    [Test]
    public void SaveSettings_PersistsRecentFiles()
    {
        _service.AddRecentFile("C:\\test\\file1.txt");
        _service.AddRecentFile("C:\\test\\file2.txt");
        _service.SaveSettings();

        var newService = new ApplicationSettingsService(_testDirectory);

        Assert.That(newService.CurrentSettings.RecentFiles, Has.Count.EqualTo(2));
        Assert.That(newService.CurrentSettings.RecentFiles[0], Is.EqualTo("C:\\test\\file2.txt"));
        Assert.That(newService.CurrentSettings.RecentFiles[1], Is.EqualTo("C:\\test\\file1.txt"));
    }
}
