// MenuItemSemanticMetadataTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using Dev.Core.Toolbar;
using NUnit.Framework;

namespace Dev.Core.Tests.Menu;

[TestFixture]
public class MenuItemSemanticMetadataTests
{
    [Test]
    public void Constructor_WithUninitializedText_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new MenuItemSemanticMetadata(default));
    }

    [Test]
    public void Constructor_WithTextAndIcon_AssignsValues()
    {
        var text = new ToolbarItemText("Open", "Open file");
        var icon = new IconKey("open");

        var metadata = new MenuItemSemanticMetadata(text, icon);

        Assert.Multiple(() =>
        {
            Assert.That(metadata.Text, Is.EqualTo(text));
            Assert.That(metadata.IconKey, Is.EqualTo(icon));
        });
    }

    [Test]
    public void Properties_AreReadOnly()
    {
        var textProperty = typeof(MenuItemSemanticMetadata).GetProperty(nameof(MenuItemSemanticMetadata.Text));
        var iconProperty = typeof(MenuItemSemanticMetadata).GetProperty(nameof(MenuItemSemanticMetadata.IconKey));

        Assert.Multiple(() =>
        {
            Assert.That(textProperty, Is.Not.Null);
            Assert.That(textProperty!.CanWrite, Is.False);
            Assert.That(iconProperty, Is.Not.Null);
            Assert.That(iconProperty!.CanWrite, Is.False);
        });
    }
}
