// TreeNodeContainerTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using Dev.Wpf.Tests.Templates;
using NUnit.Framework;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dev.Wpf.Tests.Controls;

/// <summary>
/// Structural tests for <see cref="TreeNodeContainer"/>.
///
/// These tests verify class-level structure independently of interactive
/// behavior:
/// <list type="bullet">
///   <item>Inheritance from <see cref="ContentControl"/>.</item>
///   <item>All five <c>[TemplatePart]</c> attribute declarations are present
///         on the class and carry the correct name string and element
///         type (verified via reflection without template application).</item>
///   <item>Each attribute name matches the corresponding public
///         <c>Part*</c> constant so the two representations are
///         always in sync.</item>
///   <item>The default style key is set to
///         <see cref="TreeViewControl.NodeContainerStyleKey"/>.</item>
///   <item>The <see cref="TreeNodeContainer.Node"/> computed property
///         returns the correct value for various <c>DataContext</c>
///         states.</item>
///   <item>All five template parts resolve to the correct element types
///         after a synchronous template-application pass.</item>
/// </list>
///
/// Visual behavior, data-trigger effects, and binding semantics are covered by
/// <see cref="Dev.Wpf.Tests.Templates.TreeNodeContainerTemplateTests"/>.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class TreeNodeContainerTests
{
    // -----------------------------------------------------------------------
    // Cached reflection state (evaluated once for the whole fixture)
    // -----------------------------------------------------------------------

    // The DefaultStyleKeyProperty DP is protected in FrameworkElement, so
    // we read it through reflection. The lookup happens once at class-init time.
    private static readonly DependencyProperty _defaultStyleKeyDp =
        (DependencyProperty)typeof(FrameworkElement)
            .GetField(
                "DefaultStyleKeyProperty",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!
            .GetValue(null)!;

    // [TemplatePart] attributes declared on the class — used by all
    // attribute-declaration tests without repeating the reflection call.
    private static readonly TemplatePartAttribute[] _parts =
        typeof(TreeNodeContainer)
            .GetCustomAttributes<TemplatePartAttribute>(inherit: false)
            .ToArray();

    // -----------------------------------------------------------------------
    // Per-test hosted container (required for template-structure tests)
    // -----------------------------------------------------------------------

    private TreeViewControl?   _treeView  = null!;
    private TreeNodeModel?     _rootNode  = null!;
    private TemplateTestHost?  _host      = null!;
    private TreeNodeContainer? _container = null!;

    [SetUp]
    public void SetUp()
    {
        _rootNode = new TreeNodeModel("root", "Root", isExpandable: true);
        _treeView = new TreeViewControl { ItemsSource = new[] { _rootNode } };
        _host     = new TemplateTestHost(_treeView);

        _container = _treeView.ItemContainerGenerator
            .ContainerFromItem(_rootNode) as TreeNodeContainer;

        Assert.That(_container, Is.Not.Null,
            "Precondition: TreeViewControl must generate a TreeNodeContainer for the root item.");

        _container!.ApplyTemplate();
        _container.UpdateLayout();
    }

    [TearDown]
    public void TearDown() => _host?.Dispose();

    // -----------------------------------------------------------------------
    // Type hierarchy
    // -----------------------------------------------------------------------

    [Test]
    public void InheritsDirectlyFrom_ContentControl()
    {
        Assert.That(typeof(TreeNodeContainer).BaseType,
            Is.EqualTo(typeof(ContentControl)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — quantity
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_DeclarationCount_IsFive()
    {
        Assert.That(_parts.Length, Is.EqualTo(5),
            "TreeNodeContainer must declare exactly five [TemplatePart] attributes.");
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — expand/collapse glyph
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_ExpandCollapseGlyph_IsDeclared()
    {
        Assert.That(FindPart(TreeNodeContainer.PartExpandCollapseGlyph),
            Is.Not.Null,
            $"Expected [TemplatePart(Name = \"{TreeNodeContainer.PartExpandCollapseGlyph}\")] on TreeNodeContainer.");
    }

    [Test]
    public void TemplatePart_ExpandCollapseGlyph_DeclaredType_IsToggleButton()
    {
        Assert.That(FindPart(TreeNodeContainer.PartExpandCollapseGlyph)!.Type,
            Is.EqualTo(typeof(ToggleButton)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — checkbox
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_CheckBox_IsDeclared()
    {
        Assert.That(FindPart(TreeNodeContainer.PartCheckBox),
            Is.Not.Null,
            $"Expected [TemplatePart(Name = \"{TreeNodeContainer.PartCheckBox}\")] on TreeNodeContainer.");
    }

    [Test]
    public void TemplatePart_CheckBox_DeclaredType_IsCheckBox()
    {
        Assert.That(FindPart(TreeNodeContainer.PartCheckBox)!.Type,
            Is.EqualTo(typeof(CheckBox)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — icon presenter
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_IconPresenter_IsDeclared()
    {
        Assert.That(FindPart(TreeNodeContainer.PartIconPresenter),
            Is.Not.Null,
            $"Expected [TemplatePart(Name = \"{TreeNodeContainer.PartIconPresenter}\")] on TreeNodeContainer.");
    }

    [Test]
    public void TemplatePart_IconPresenter_DeclaredType_IsContentPresenter()
    {
        Assert.That(FindPart(TreeNodeContainer.PartIconPresenter)!.Type,
            Is.EqualTo(typeof(ContentPresenter)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — label presenter
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_LabelPresenter_IsDeclared()
    {
        Assert.That(FindPart(TreeNodeContainer.PartLabelPresenter),
            Is.Not.Null,
            $"Expected [TemplatePart(Name = \"{TreeNodeContainer.PartLabelPresenter}\")] on TreeNodeContainer.");
    }

    [Test]
    public void TemplatePart_LabelPresenter_DeclaredType_IsTextBlock()
    {
        Assert.That(FindPart(TreeNodeContainer.PartLabelPresenter)!.Type,
            Is.EqualTo(typeof(TextBlock)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute declarations — children presenter
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_ChildrenPresenter_IsDeclared()
    {
        Assert.That(FindPart(TreeNodeContainer.PartChildrenPresenter),
            Is.Not.Null,
            $"Expected [TemplatePart(Name = \"{TreeNodeContainer.PartChildrenPresenter}\")] on TreeNodeContainer.");
    }

    [Test]
    public void TemplatePart_ChildrenPresenter_DeclaredType_IsItemsControl()
    {
        // The declared type is ItemsControl (per the TreeViewDesign contract).
        // The concrete element placed by the template is TreeChildItemsControl,
        // which is an ItemsControl subclass.
        Assert.That(FindPart(TreeNodeContainer.PartChildrenPresenter)!.Type,
            Is.EqualTo(typeof(ItemsControl)));
    }

    // -----------------------------------------------------------------------
    // [TemplatePart] attribute names match the public Part* constants
    // -----------------------------------------------------------------------

    [Test]
    public void TemplatePart_AttributeNames_MatchPartNameConstants()
    {
        var attributeNames = _parts
            .Select(a => a.Name)
            .OrderBy(n => n)
            .ToArray();

        var constantNames = new[]
        {
            TreeNodeContainer.PartExpandCollapseGlyph,
            TreeNodeContainer.PartCheckBox,
            TreeNodeContainer.PartIconPresenter,
            TreeNodeContainer.PartLabelPresenter,
            TreeNodeContainer.PartChildrenPresenter,
        }
        .OrderBy(n => n)
        .ToArray();

        Assert.That(attributeNames, Is.EqualTo(constantNames),
            "Every [TemplatePart] Name must match a public Part* constant and vice versa.");
    }

    // -----------------------------------------------------------------------
    // Default style key
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultStyleKey_IsNodeContainerStyleKey()
    {
        var metadata = _defaultStyleKeyDp.GetMetadata(typeof(TreeNodeContainer));

        Assert.That(metadata.DefaultValue,
            Is.EqualTo(TreeViewControl.NodeContainerStyleKey),
            "The DefaultStyleKey override must point to TreeViewControl.NodeContainerStyleKey " +
            "so the control template is resolved from the correct resource entry in Generic.xaml.");
    }

    // -----------------------------------------------------------------------
    // Node convenience property
    // -----------------------------------------------------------------------

    [Test]
    public void Node_ReturnsNull_WhenDataContextIsNull()
    {
        var container = new TreeNodeContainer { DataContext = null };
        Assert.That(container.Node, Is.Null);
    }

    [Test]
    public void Node_ReturnsModel_WhenDataContextIsTreeNodeModel()
    {
        var model     = new TreeNodeModel("id", "Label");
        var container = new TreeNodeContainer { DataContext = model };
        Assert.That(container.Node, Is.SameAs(model));
    }

    [Test]
    public void Node_ReturnsNull_WhenDataContextIsUnrelatedType()
    {
        var container = new TreeNodeContainer { DataContext = new object() };
        Assert.That(container.Node, Is.Null);
    }

    // -----------------------------------------------------------------------
    // Template part existence after template application
    // -----------------------------------------------------------------------

    [Test]
    public void AfterTemplateApplication_AllFivePartsExist()
    {
        var partNames = new[]
        {
            TreeNodeContainer.PartExpandCollapseGlyph,
            TreeNodeContainer.PartCheckBox,
            TreeNodeContainer.PartIconPresenter,
            TreeNodeContainer.PartLabelPresenter,
            TreeNodeContainer.PartChildrenPresenter,
        };

        foreach (var name in partNames)
        {
            Assert.That(
                _container!.Template.FindName(name, _container),
                Is.Not.Null,
                $"Template part '{name}' was not found after template application.");
        }
    }

    [Test]
    public void AfterTemplateApplication_ExpandCollapseGlyph_IsToggleButton()
    {
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartExpandCollapseGlyph, _container);
        Assert.That(part, Is.InstanceOf<ToggleButton>());
    }

    [Test]
    public void AfterTemplateApplication_CheckBox_IsCheckBox()
    {
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartCheckBox, _container);
        Assert.That(part, Is.InstanceOf<CheckBox>());
    }

    [Test]
    public void AfterTemplateApplication_IconPresenter_IsContentPresenter()
    {
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartIconPresenter, _container);
        Assert.That(part, Is.InstanceOf<ContentPresenter>());
    }

    [Test]
    public void AfterTemplateApplication_LabelPresenter_IsTextBlock()
    {
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartLabelPresenter, _container);
        Assert.That(part, Is.InstanceOf<TextBlock>());
    }

    [Test]
    public void AfterTemplateApplication_ChildrenPresenter_IsItemsControl()
    {
        // The concrete type is TreeChildItemsControl; tests that it satisfies
        // the ItemsControl contract declared by the [TemplatePart] attribute.
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container);
        Assert.That(part, Is.InstanceOf<ItemsControl>());
    }

    [Test]
    public void AfterTemplateApplication_ChildrenPresenter_IsTreeChildItemsControl()
    {
        // Asserts the concrete type placed in the template is the recursive
        // child host, not a generic ItemsControl.
        var part = _container!.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container);
        Assert.That(part, Is.InstanceOf<TreeChildItemsControl>());
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static TemplatePartAttribute? FindPart(string name) =>
        _parts.FirstOrDefault(a => a.Name == name);
}
