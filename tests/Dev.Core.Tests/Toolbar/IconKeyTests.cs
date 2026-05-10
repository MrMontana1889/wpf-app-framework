// IconKeyTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class IconKeyTests
{
    [Test]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new IconKey(null!));
    }

    [Test]
    public void Constructor_WithWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new IconKey("   "));
    }

    [Test]
    public void Constructor_WithLeadingOrTrailingWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new IconKey(" save "));
    }

    [Test]
    public void Constructor_WithArbitraryToken_PreservesExactValue()
    {
        var key = new IconKey("fluent:save-24");

        Assert.That(key.Value, Is.EqualTo("fluent:save-24"));
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var left = new IconKey("save");
        var right = new IconKey("save");

        Assert.That(left.Equals(right), Is.True);
    }

    [Test]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var left = new IconKey("save");
        var right = new IconKey("delete");

        Assert.That(left.Equals(right), Is.False);
    }

    [Test]
    public void GetHashCode_WithSameValue_Matches()
    {
        var left = new IconKey("save");
        var right = new IconKey("save");

        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
    }

    [Test]
    public void ToString_ReturnsValue()
    {
        var key = new IconKey("save");

        Assert.That(key.ToString(), Is.EqualTo("save"));
    }

    [Test]
    public void ValueProperty_IsReadOnly()
    {
        var property = typeof(IconKey).GetProperty(nameof(IconKey.Value));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanWrite, Is.False);
    }
}
