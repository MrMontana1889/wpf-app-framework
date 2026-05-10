// ToolbarDisplayIntentToVisibilityConverterTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Wpf.Converters;
using NUnit.Framework;
using System.Globalization;
using System.Windows;

namespace Dev.Wpf.Tests.Converters;

[TestFixture]
public sealed class ToolbarDisplayIntentToVisibilityConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    [TestCase(ToolbarItemDisplayIntent.IconOnly, "Icon", Visibility.Visible)]
    [TestCase(ToolbarItemDisplayIntent.IconOnly, "Text", Visibility.Collapsed)]
    [TestCase(ToolbarItemDisplayIntent.TextOnly, "Icon", Visibility.Collapsed)]
    [TestCase(ToolbarItemDisplayIntent.TextOnly, "Text", Visibility.Visible)]
    [TestCase(ToolbarItemDisplayIntent.IconAndText, "Icon", Visibility.Visible)]
    [TestCase(ToolbarItemDisplayIntent.IconAndText, "Text", Visibility.Visible)]
    public void Convert_ProjectsIntentAsExpected(ToolbarItemDisplayIntent intent, string target, Visibility expected)
    {
        var converter = new ToolbarDisplayIntentToVisibilityConverter();

        var visibility = converter.Convert(intent, typeof(Visibility), target, Culture);

        Assert.That(visibility, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_UnknownTarget_ReturnsCollapsed()
    {
        var converter = new ToolbarDisplayIntentToVisibilityConverter();

        var visibility = converter.Convert(ToolbarItemDisplayIntent.IconAndText, typeof(Visibility), "Unknown", Culture);

        Assert.That(visibility, Is.EqualTo(Visibility.Collapsed));
    }
}
