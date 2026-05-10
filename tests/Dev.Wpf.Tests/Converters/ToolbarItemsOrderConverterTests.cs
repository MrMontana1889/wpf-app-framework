// ToolbarItemsOrderConverterTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Wpf.Converters;
using NUnit.Framework;
using System.Globalization;

namespace Dev.Wpf.Tests.Converters;

[TestFixture]
public sealed class ToolbarItemsOrderConverterTests
{
    [Test]
    public void Convert_SortsByOrderAscending()
    {
        var converter = new ToolbarItemsOrderConverter();
        var items = new[]
        {
            CreateLabelItem("third", 30),
            CreateLabelItem("first", 10),
            CreateLabelItem("second", 20),
        };

        var ordered = (IReadOnlyList<ToolbarItem>)converter.Convert(items, typeof(IReadOnlyList<ToolbarItem>), string.Empty, CultureInfo.InvariantCulture);

        Assert.That(ordered.Select(i => i.Id.Value), Is.EqualTo(new[] { "first", "second", "third" }));
    }

    private static ToolbarItem CreateLabelItem(string id, int order)
    {
        return new ToolbarItem(
            new ToolbarItemId(id),
            ToolbarItemKind.Label,
            new ToolbarItemSemanticMetadata(new ToolbarItemText(id)),
            ToolbarItemDisplayIntent.TextOnly,
            order: order);
    }
}
