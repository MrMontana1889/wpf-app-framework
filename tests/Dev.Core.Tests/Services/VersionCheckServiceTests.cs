// VersionCheckServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using NUnit.Framework;
using System.Reflection;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class VersionCheckServiceTests
{
    private VersionCheckService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new VersionCheckService();
    }

    [Test]
    public void GetCurrentVersion_ReturnsFormattedVersion()
    {
        var version = _service.GetCurrentVersion();

        Assert.That(version, Is.Not.Null);
        Assert.That(version, Does.StartWith("v"));
    }

    [Test]
    public void GetCurrentVersion_HasCorrectFormat()
    {
        var version = _service.GetCurrentVersion();

        var regex = new System.Text.RegularExpressions.Regex(@"^v\d{2}\.\d{2}\.\d{2}\.\d{2}$");
        Assert.That(regex.IsMatch(version), Is.True, $"Version '{version}' does not match expected format 'vXX.XX.XX.XX'");
    }

    [Test]
    public void GetCurrentVersion_MatchesAssemblyVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version!;
        var expectedVersion = $"v{assemblyVersion.Major:D2}.{assemblyVersion.Minor:D2}.{assemblyVersion.Build:D2}.{assemblyVersion.Revision:D2}";

        var actualVersion = _service.GetCurrentVersion();

        Assert.That(actualVersion, Does.Match(@"^v\d{2}\.\d{2}\.\d{2}\.\d{2}$"));
    }

    [Test]
    public void GetCurrentVersion_ReturnsConsistentValue()
    {
        var version1 = _service.GetCurrentVersion();
        var version2 = _service.GetCurrentVersion();

        Assert.That(version1, Is.EqualTo(version2));
    }

    [Test]
    public void GetCurrentVersion_ContainsAllVersionParts()
    {
        var version = _service.GetCurrentVersion();
        var parts = version.Substring(1).Split('.');

        Assert.That(parts, Has.Length.EqualTo(4), "Version should have 4 parts (major.minor.build.revision)");
        Assert.That(parts[0], Has.Length.EqualTo(2), "Major version should be zero-padded to 2 digits");
        Assert.That(parts[1], Has.Length.EqualTo(2), "Minor version should be zero-padded to 2 digits");
        Assert.That(parts[2], Has.Length.EqualTo(2), "Build version should be zero-padded to 2 digits");
        Assert.That(parts[3], Has.Length.EqualTo(2), "Revision should be zero-padded to 2 digits");
    }
}
