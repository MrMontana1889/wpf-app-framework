// MenuItemIdTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using NUnit.Framework;

namespace Dev.Core.Tests.Menu;

[TestFixture]
public class MenuItemIdTests
{
    [Test]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new MenuItemId(null!));
    }

    [Test]
    public void Constructor_WithWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new MenuItemId("   "));
    }

    [Test]
    public void Constructor_WithLeadingOrTrailingWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new MenuItemId(" File.Open "));
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var left = new MenuItemId("File.Open");
        var right = new MenuItemId("File.Open");

        Assert.That(left.Equals(right), Is.True);
    }

    [Test]
    public void GetHashCode_WithSameValue_Matches()
    {
        var left = new MenuItemId("File.Open");
        var right = new MenuItemId("File.Open");

        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
    }
}
