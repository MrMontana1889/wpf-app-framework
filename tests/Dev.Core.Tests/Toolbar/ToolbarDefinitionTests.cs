// ToolbarDefinitionTests.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarDefinitionTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var definition = new ToolbarDefinition(
            id: new ToolbarId("Build.Toolbar"),
            displayName: "Build Toolbar",
            canHide: false,
            defaultVisible: true);

        Assert.Multiple(() =>
        {
            Assert.That(definition.Id, Is.EqualTo(new ToolbarId("Build.Toolbar")));
            Assert.That(definition.DisplayName, Is.EqualTo("Build Toolbar"));
            Assert.That(definition.CanHide, Is.False);
            Assert.That(definition.DefaultVisible, Is.True);
        });
    }

    [Test]
    public void Constructor_DefaultToolbarId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarDefinition(default, "Build Toolbar"));
    }

    [Test]
    public void Constructor_InvalidDisplayName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new ToolbarDefinition(new ToolbarId("Build.Toolbar"), "   "));
    }
}
