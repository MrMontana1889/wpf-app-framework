// ToolbarVisibilityChangedEventTests.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services;
using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Services;

[TestFixture]
public class ToolbarVisibilityChangedEventTests
{
    [Test]
    public void Constructor_SetsToolbarIdAndVisibility()
    {
        var toolbarId = new ToolbarId("Build");
        var isVisible = true;

        var args = new ToolbarVisibilityChangedEventArgs(toolbarId, isVisible);

        Assert.Multiple(() =>
        {
            Assert.That(args.ToolbarId, Is.EqualTo(toolbarId));
            Assert.That(args.IsVisible, Is.EqualTo(isVisible));
        });
    }

    [Test]
    public void Constructor_PreservesVisibilityFalse()
    {
        var toolbarId = new ToolbarId("Project");
        var isVisible = false;

        var args = new ToolbarVisibilityChangedEventArgs(toolbarId, isVisible);

        Assert.That(args.IsVisible, Is.False);
    }
}
