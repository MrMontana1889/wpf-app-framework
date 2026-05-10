// ToolbarIdTests.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarIdTests
{
    [Test]
    public void Constructor_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new ToolbarId(null!));
    }

    [Test]
    public void Constructor_Whitespace_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarId("   "));
    }

    [Test]
    public void Constructor_TrimmedValue_Accepts()
    {
        var id = new ToolbarId("Build.Toolbar");

        Assert.That(id.Value, Is.EqualTo("Build.Toolbar"));
    }

    [Test]
    public void Equality_SameValue_IsTrue()
    {
        var left = new ToolbarId("Build.Toolbar");
        var right = new ToolbarId("Build.Toolbar");

        Assert.That(left, Is.EqualTo(right));
        Assert.That(left == right, Is.True);
    }
}
