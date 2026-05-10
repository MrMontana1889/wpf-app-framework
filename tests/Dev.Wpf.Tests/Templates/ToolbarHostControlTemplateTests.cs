// ToolbarHostControlTemplateTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dev.Wpf.Tests.Templates;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ToolbarHostControlTemplateTests
{
    [Test]
    public void Template_ContainsToolBarTray_AndToolBar()
    {
        var control = new ToolbarHostControl();

        using var host = new TemplateTestHost(control);

        var tray = TemplateTestHost.FindChild<ToolBarTray>(control);
        var toolbar = TemplateTestHost.FindChild<ToolBar>(control);

        Assert.Multiple(() =>
        {
            Assert.That(tray, Is.Not.Null);
            Assert.That(toolbar, Is.Not.Null);
        });
    }

    [Test]
    public void ButtonProjection_BindsVisibilityAndEnablement()
    {
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Build", "Run build")),
            ToolbarItemDisplayIntent.IconAndText,
            isVisible: false,
            isEnabled: false,
            command: new TestCommand());

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { item },
        };

        using var host = new TemplateTestHost(control);

        var button = TemplateTestHost.FindChild<Button>(control);

        Assert.That(button, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(button!.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(button.IsEnabled, Is.False);
        });
    }

    [Test]
    public void ButtonProjection_UsesIconProvider_WhenDisplayIntentShowsIcon()
    {
        var expectedIcon = new DrawingImage();
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            new ToolbarItemSemanticMetadata(new ToolbarItemText("Build", "Run build"), new IconKey("Build")),
            ToolbarItemDisplayIntent.IconOnly,
            command: new TestCommand());

        var control = new ToolbarHostControl
        {
            ItemsSource = new[] { item },
            IconProvider = new StubIconProvider(expectedIcon),
        };

        using var host = new TemplateTestHost(control);

        var image = TemplateTestHost.FindChild<Image>(control);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Source, Is.SameAs(expectedIcon));
    }

    [Test]
    public void ButtonProjection_ReplacedItems_RefreshesLabelText()
    {
        var items = new ObservableCollection<ToolbarItem>
        {
            new(
                new ToolbarItemId("Selection.Cycle"),
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Selection: Single")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand())
        };

        var control = new ToolbarHostControl
        {
            ItemsSource = items,
        };

        using var host = new TemplateTestHost(control);

        var initialButton = TemplateTestHost.FindChild<Button>(control);
        var initialText = TemplateTestHost.FindChild<TextBlock>(initialButton!);

        Assert.That(initialText, Is.Not.Null);
        Assert.That(initialText!.Text, Is.EqualTo("Selection: Single"));

        items.Clear();
        items.Add(
            new ToolbarItem(
                new ToolbarItemId("Selection.Cycle"),
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Selection: Multiple")),
                ToolbarItemDisplayIntent.TextOnly,
                command: new TestCommand()));

        control.UpdateLayout();

        var updatedButton = TemplateTestHost.FindChild<Button>(control);
        var updatedText = TemplateTestHost.FindChild<TextBlock>(updatedButton!);

        Assert.That(updatedText, Is.Not.Null);
        Assert.That(updatedText!.Text, Is.EqualTo("Selection: Multiple"));
    }

    private sealed class StubIconProvider : IIconProvider
    {
        private readonly ImageSource _image;

        public StubIconProvider(ImageSource image)
        {
            _image = image;
        }

        public ImageSource? GetIcon(string iconKey) => _image;
    }

    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }
    }
}
