// ToolbarItemTemplateSelectorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Toolbar;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Windows;

namespace Dev.Wpf.Tests.Controls;

[TestFixture]
public sealed class ToolbarItemTemplateSelectorTests
{
    [Test]
    public void SelectTemplate_Button_ReturnsButtonTemplate()
    {
        var buttonTemplate = new DataTemplate();
        var selector = CreateSelector(buttonTemplate: buttonTemplate);

        var template = selector.SelectTemplate(CreateButtonItem(), new DependencyObject());

        Assert.That(template, Is.SameAs(buttonTemplate));
    }

    [Test]
    public void SelectTemplate_DropDown_ReturnsDropDownTemplate()
    {
        var dropDownTemplate = new DataTemplate();
        var selector = CreateSelector(dropDownTemplate: dropDownTemplate);

        var template = selector.SelectTemplate(CreateDropDownItem(), new DependencyObject());

        Assert.That(template, Is.SameAs(dropDownTemplate));
    }

    [Test]
    public void SelectTemplate_SplitDropDown_ReturnsSplitDropDownTemplate()
    {
        var splitDropDownTemplate = new DataTemplate();
        var selector = CreateSelector(splitDropDownTemplate: splitDropDownTemplate);

        var template = selector.SelectTemplate(CreateSplitDropDownItem(), new DependencyObject());

        Assert.That(template, Is.SameAs(splitDropDownTemplate));
    }

    [Test]
    public void SelectTemplate_ToggleButton_ReturnsToggleButtonTemplate()
    {
        var toggleTemplate = new DataTemplate();
        var selector = CreateSelector(toggleButtonTemplate: toggleTemplate);
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Toggle"),
            ToolbarItemKind.ToggleButton,
            CreateMetadata(),
            ToolbarItemDisplayIntent.IconOnly,
            command: new TestCommand(),
            isChecked: true);

        var template = selector.SelectTemplate(item, new DependencyObject());

        Assert.That(template, Is.SameAs(toggleTemplate));
    }

    [Test]
    public void SelectTemplate_CheckBox_ReturnsCheckBoxTemplate()
    {
        var checkBoxTemplate = new DataTemplate();
        var selector = CreateSelector(checkBoxTemplate: checkBoxTemplate);
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Check"),
            ToolbarItemKind.CheckBox,
            CreateMetadata(),
            ToolbarItemDisplayIntent.TextOnly,
            isChecked: false);

        var template = selector.SelectTemplate(item, new DependencyObject());

        Assert.That(template, Is.SameAs(checkBoxTemplate));
    }

    [Test]
    public void SelectTemplate_Label_ReturnsLabelTemplate()
    {
        var labelTemplate = new DataTemplate();
        var selector = CreateSelector(labelTemplate: labelTemplate);
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Label"),
            ToolbarItemKind.Label,
            CreateMetadata(),
            ToolbarItemDisplayIntent.TextOnly);

        var template = selector.SelectTemplate(item, new DependencyObject());

        Assert.That(template, Is.SameAs(labelTemplate));
    }

    [Test]
    public void SelectTemplate_ComboBox_ReturnsComboBoxTemplate()
    {
        var comboTemplate = new DataTemplate();
        var selector = CreateSelector(comboBoxTemplate: comboTemplate);
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Config"),
            ToolbarItemKind.ComboBox,
            CreateMetadata(),
            ToolbarItemDisplayIntent.TextOnly,
            selectionItems: new[] { "Debug", "Release" },
            selectedValue: "Debug");

        var template = selector.SelectTemplate(item, new DependencyObject());

        Assert.That(template, Is.SameAs(comboTemplate));
    }

    [Test]
    public void SelectTemplate_Separator_ReturnsSeparatorTemplate()
    {
        var separatorTemplate = new DataTemplate();
        var selector = CreateSelector(separatorTemplate: separatorTemplate);
        var item = new ToolbarItem(
            new ToolbarItemId("Build.Separator"),
            ToolbarItemKind.Separator,
            CreateMetadata(),
            ToolbarItemDisplayIntent.IconOnly);

        var template = selector.SelectTemplate(item, new DependencyObject());

        Assert.That(template, Is.SameAs(separatorTemplate));
    }

    private static ToolbarItemTemplateSelector CreateSelector(
        DataTemplate? buttonTemplate = null,
        DataTemplate? dropDownTemplate = null,
        DataTemplate? splitDropDownTemplate = null,
        DataTemplate? toggleButtonTemplate = null,
        DataTemplate? checkBoxTemplate = null,
        DataTemplate? labelTemplate = null,
        DataTemplate? comboBoxTemplate = null,
        DataTemplate? separatorTemplate = null)
    {
        return new ToolbarItemTemplateSelector
        {
            ButtonTemplate = buttonTemplate,
            DropDownTemplate = dropDownTemplate,
            SplitDropDownTemplate = splitDropDownTemplate,
            ToggleButtonTemplate = toggleButtonTemplate,
            CheckBoxTemplate = checkBoxTemplate,
            LabelTemplate = labelTemplate,
            ComboBoxTemplate = comboBoxTemplate,
            SeparatorTemplate = separatorTemplate,
        };
    }

    private static ToolbarItem CreateButtonItem() =>
        new(
            new ToolbarItemId("Build.Run"),
            ToolbarItemKind.Button,
            CreateMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            command: new TestCommand());

    private static ToolbarItem CreateDropDownItem() =>
        new(
            new ToolbarItemId("Build.Actions"),
            ToolbarItemKind.DropDown,
            CreateMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            children: [CreateButtonItem()]);

    private static ToolbarItem CreateSplitDropDownItem() =>
        new(
            new ToolbarItemId("Build.Split"),
            ToolbarItemKind.SplitDropDown,
            CreateMetadata(),
            ToolbarItemDisplayIntent.IconAndText,
            command: new TestCommand(),
            children: [CreateButtonItem()]);

    private static ToolbarItemSemanticMetadata CreateMetadata() =>
        new(new ToolbarItemText("Build", "Run build"), new IconKey("build"));

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
