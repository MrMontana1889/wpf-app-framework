// ToolbarItemSemanticMetadataTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Toolbar;

[TestFixture]
public class ToolbarItemSemanticMetadataTests
{
    [Test]
    public void Constructor_WithUninitializedText_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new ToolbarItemSemanticMetadata(default));
    }

    [Test]
    public void Constructor_WithTextAndIcon_AssignsValues()
    {
        var text = new ToolbarItemText("Build", "Run build");
        var icon = new IconKey("build");

        var metadata = new ToolbarItemSemanticMetadata(text, icon);

        Assert.Multiple(() =>
        {
            Assert.That(metadata.Text, Is.EqualTo(text));
            Assert.That(metadata.IconKey, Is.EqualTo(icon));
        });
    }

    [Test]
    public void Constructor_WithoutIcon_AllowsNullIconKey()
    {
        var metadata = new ToolbarItemSemanticMetadata(new ToolbarItemText("Build", "Run build"));

        Assert.That(metadata.IconKey, Is.Null);
    }

    [Test]
    public void Properties_AreReadOnly()
    {
        var textProperty = typeof(ToolbarItemSemanticMetadata).GetProperty(nameof(ToolbarItemSemanticMetadata.Text));
        var iconProperty = typeof(ToolbarItemSemanticMetadata).GetProperty(nameof(ToolbarItemSemanticMetadata.IconKey));

        Assert.Multiple(() =>
        {
            Assert.That(textProperty, Is.Not.Null);
            Assert.That(textProperty!.CanWrite, Is.False);
            Assert.That(iconProperty, Is.Not.Null);
            Assert.That(iconProperty!.CanWrite, Is.False);
        });
    }
}
