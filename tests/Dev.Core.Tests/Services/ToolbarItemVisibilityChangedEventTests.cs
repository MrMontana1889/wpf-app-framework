// ToolbarItemVisibilityChangedEventTests.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services;
using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarItemVisibilityChangedEventTests
{
    [Test]
    public void Constructor_SetsToolbarIdItemIdAndVisibility()
    {
        var toolbarId = new ToolbarId("Build");
        var itemId = new ToolbarItemId("Build.Run");

        var args = new ToolbarItemVisibilityChangedEventArgs(toolbarId, itemId, isVisible: true);

        Assert.Multiple(() =>
        {
            Assert.That(args.ToolbarId, Is.EqualTo(toolbarId));
            Assert.That(args.ItemId, Is.EqualTo(itemId));
            Assert.That(args.IsVisible, Is.True);
        });
    }

    [Test]
    public void Constructor_PreservesFalseVisibility()
    {
        var args = new ToolbarItemVisibilityChangedEventArgs(
            new ToolbarId("Project"),
            new ToolbarItemId("Project.Open"),
            isVisible: false);

        Assert.That(args.IsVisible, Is.False);
    }
}
