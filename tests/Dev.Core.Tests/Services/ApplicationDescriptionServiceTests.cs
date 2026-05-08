// ApplicationDescriptionServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ApplicationDescriptionServiceTests
{
    // --- Construction ---

    [Test]
    public void Constructor_CreatesInstance()
    {
        var service = new ApplicationDescriptionService("MyApp");

        Assert.That(service, Is.Not.Null);
    }

    // --- ApplicationName ---

    [Test]
    public void ApplicationName_ReturnsValuePassedToConstructor()
    {
        var service = new ApplicationDescriptionService("BentleyBuildApp.Next");

        Assert.That(service.ApplicationName, Is.EqualTo("BentleyBuildApp.Next"));
    }

    [Test]
    public void ApplicationName_PreservesValueExactly()
    {
        const string name = "  My App v2 ";
        var service = new ApplicationDescriptionService(name);

        Assert.That(service.ApplicationName, Is.EqualTo(name));
    }

    [Test]
    public void ApplicationName_AllowsEmptyString()
    {
        var service = new ApplicationDescriptionService(string.Empty);

        Assert.That(service.ApplicationName, Is.EqualTo(string.Empty));
    }

    // --- Interface contract ---

    [Test]
    public void ImplementsIApplicationDescriptionService()
    {
        var service = new ApplicationDescriptionService("App");

        Assert.That(service, Is.InstanceOf<IApplicationDescriptionService>());
    }
}
