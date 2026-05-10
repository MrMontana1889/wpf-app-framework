// ToolbarItemTextTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarItemTextTests
{
    [Test]
    public void Constructor_WithNullLabel_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new ToolbarItemText(null!));
    }

    [Test]
    public void Constructor_WithWhitespaceLabel_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemText("   "));
    }

    [Test]
    public void Constructor_WithLeadingOrTrailingLabelWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemText(" Build "));
    }

    [Test]
    public void Constructor_WithWhitespaceAssistiveText_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemText("Build", "   "));
    }

    [Test]
    public void Constructor_WithValidValues_PreservesExactText()
    {
        var text = new ToolbarItemText("Build", "Run a build for the current configuration.");

        Assert.Multiple(() =>
        {
            Assert.That(text.Label, Is.EqualTo("Build"));
            Assert.That(text.AssistiveText, Is.EqualTo("Run a build for the current configuration."));
        });
    }

    [Test]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var left = new ToolbarItemText("Build", "Run build");
        var right = new ToolbarItemText("Build", "Run build");

        Assert.That(left.Equals(right), Is.True);
    }

    [Test]
    public void Equals_WithDifferentAssistiveText_ReturnsFalse()
    {
        var left = new ToolbarItemText("Build", "Run build");
        var right = new ToolbarItemText("Build", "Build project output");

        Assert.That(left.Equals(right), Is.False);
    }

    [Test]
    public void GetHashCode_WithSameValues_Matches()
    {
        var left = new ToolbarItemText("Build", "Run build");
        var right = new ToolbarItemText("Build", "Run build");

        Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
    }

    [Test]
    public void ToString_ReturnsLabel()
    {
        var text = new ToolbarItemText("Build", "Run build");

        Assert.That(text.ToString(), Is.EqualTo("Build"));
    }

    [Test]
    public void Properties_AreReadOnly()
    {
        var labelProperty = typeof(ToolbarItemText).GetProperty(nameof(ToolbarItemText.Label));
        var assistiveTextProperty = typeof(ToolbarItemText).GetProperty(nameof(ToolbarItemText.AssistiveText));

        Assert.Multiple(() =>
        {
            Assert.That(labelProperty, Is.Not.Null);
            Assert.That(labelProperty!.CanWrite, Is.False);
            Assert.That(assistiveTextProperty, Is.Not.Null);
            Assert.That(assistiveTextProperty!.CanWrite, Is.False);
        });
    }
}
