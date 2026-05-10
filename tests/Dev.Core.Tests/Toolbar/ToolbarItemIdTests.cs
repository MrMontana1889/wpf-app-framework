// ToolbarItemIdTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarItemIdTests
{
    [Test]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new ToolbarItemId(null!));
    }

    [Test]
    public void Constructor_WithWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemId("   "));
    }

    [Test]
    public void Constructor_WithLeadingOrTrailingWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemId(" Build.Run "));
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var left = new ToolbarItemId("Build.Run");
        var right = new ToolbarItemId("Build.Run");

        Assert.That(left.Equals(right), Is.True);
    }

    [Test]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var left = new ToolbarItemId("Build.Run");
        var right = new ToolbarItemId("Build.Clean");

        Assert.That(left.Equals(right), Is.False);
    }

    [Test]
    public void GetHashCode_WithSameValue_Matches()
    {
        var left = new ToolbarItemId("Build.Run");
        var right = new ToolbarItemId("Build.Run");

        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
    }

    [Test]
    public void ToString_ReturnsOriginalValue()
    {
        var id = new ToolbarItemId("Build.Run");

        Assert.That(id.ToString(), Is.EqualTo("Build.Run"));
    }

    [Test]
    public void Type_ImplementsIEquatable()
    {
        Assert.That(typeof(IEquatable<ToolbarItemId>).IsAssignableFrom(typeof(ToolbarItemId)), Is.True);
    }

    [Test]
    public void ValueProperty_IsReadOnly()
    {
        var property = typeof(ToolbarItemId).GetProperty(nameof(ToolbarItemId.Value));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }
}
