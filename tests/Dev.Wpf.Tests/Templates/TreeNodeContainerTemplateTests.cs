// TreeNodeContainerTemplateTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dev.Wpf.Tests.Templates;

/// <summary>
/// Template integrity tests for <see cref="TreeNodeContainer"/>.
///
/// These tests verify:
/// <list type="bullet">
///   <item>The <c>DefaultStyleKey</c> metadata is set to
///         <see cref="TreeViewControl.NodeContainerStyleKey"/>.</item>
///   <item>All five template part name constants have the expected string values.</item>
///   <item>Each template part resolves to the correct WPF element type after a
///         synchronous layout pass inside a real <see cref="TreeViewControl"/>.</item>
///   <item>Data-trigger driven visibility: <c>PART_ExpandCollapseGlyph</c> is
///         hidden for leaf nodes and visible for expandable nodes;
///         <c>PART_ChildrenPresenter</c> is collapsed until <c>IsExpanded</c>
///         is set to <c>true</c>.</item>
///   <item><c>PART_CheckBox</c> starts collapsed (controlled by Phase D's
///         <c>ShowCheckboxes</c> property).</item>
///   <item><c>PART_LabelPresenter</c> text is bound to
///         <see cref="TreeNodeModel.Label"/>.</item>
/// </list>
///
/// Visual appearance is not tested here.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class TreeNodeContainerTemplateTests
{
    // Shared host — one TreeViewControl hosts a single root node so we can
    // retrieve a realized TreeNodeContainer in each test. The host is
    // recreated per test via [SetUp] / [TearDown] to keep state isolated.

    private TreeViewControl  _control   = null!;
    private TreeNodeModel    _rootNode  = null!;
    private TemplateTestHost _host      = null!;
    private TreeNodeContainer _container = null!;

    [SetUp]
    public void SetUp()
    {
        _rootNode = new TreeNodeModel("root", "Root Node", isExpandable: true);
        _control  = new TreeViewControl();
        _control.ItemsSource = new[] { _rootNode };
        _host     = new TemplateTestHost(_control);

        var c = _control.ItemContainerGenerator.ContainerFromItem(_rootNode) as TreeNodeContainer;
        Assert.That(c, Is.Not.Null, "Precondition: container must be generated before each test.");
        _container = c!;

        // Belt-and-suspenders: ensure the container's own template is applied.
        _container.ApplyTemplate();
        _container.UpdateLayout();
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    // -----------------------------------------------------------------------
    // DefaultStyleKey metadata
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultStyleKey_MatchesNodeContainerStyleKey()
    {
        // Read the DefaultStyleKeyProperty's registered metadata for
        // TreeNodeContainer via reflection (DefaultStyleKeyProperty is
        // protected-internal in PresentationFramework.dll).
        var dpField = typeof(FrameworkElement).GetField(
            "DefaultStyleKeyProperty",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.That(dpField, Is.Not.Null, "Unable to locate DefaultStyleKeyProperty via reflection.");

        var dp       = (DependencyProperty)dpField!.GetValue(null)!;
        var metadata = dp.GetMetadata(typeof(TreeNodeContainer));

        Assert.That(metadata.DefaultValue, Is.EqualTo(TreeViewControl.NodeContainerStyleKey));
    }

    // -----------------------------------------------------------------------
    // Part name constants
    // -----------------------------------------------------------------------

    [Test]
    public void PartName_ExpandCollapseGlyph_IsExpectedString()
    {
        Assert.That(TreeNodeContainer.PartExpandCollapseGlyph, Is.EqualTo("PART_ExpandCollapseGlyph"));
    }

    [Test]
    public void PartName_CheckBox_IsExpectedString()
    {
        Assert.That(TreeNodeContainer.PartCheckBox, Is.EqualTo("PART_CheckBox"));
    }

    [Test]
    public void PartName_IconPresenter_IsExpectedString()
    {
        Assert.That(TreeNodeContainer.PartIconPresenter, Is.EqualTo("PART_IconPresenter"));
    }

    [Test]
    public void PartName_LabelPresenter_IsExpectedString()
    {
        Assert.That(TreeNodeContainer.PartLabelPresenter, Is.EqualTo("PART_LabelPresenter"));
    }

    [Test]
    public void PartName_ChildrenPresenter_IsExpectedString()
    {
        Assert.That(TreeNodeContainer.PartChildrenPresenter, Is.EqualTo("PART_ChildrenPresenter"));
    }

    // -----------------------------------------------------------------------
    // Template is applied
    // -----------------------------------------------------------------------

    [Test]
    public void Template_IsNotNullAfterApply()
    {
        Assert.That(_container.Template, Is.Not.Null);
    }

    // -----------------------------------------------------------------------
    // PART_ExpandCollapseGlyph
    // -----------------------------------------------------------------------

    [Test]
    public void Part_ExpandCollapseGlyph_ExistsInTemplate()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartExpandCollapseGlyph, _container);

        Assert.That(part, Is.Not.Null);
    }

    [Test]
    public void Part_ExpandCollapseGlyph_IsToggleButton()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartExpandCollapseGlyph, _container);

        Assert.That(part, Is.InstanceOf<ToggleButton>());
    }

    [Test]
    public void Part_ExpandCollapseGlyph_IsVisible_ForExpandableNode()
    {
        // _rootNode was created with isExpandable: true.
        var glyph = (ToggleButton)_container.Template.FindName(
            TreeNodeContainer.PartExpandCollapseGlyph, _container)!;

        Assert.That(glyph.Visibility, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Part_ExpandCollapseGlyph_IsHidden_ForLeafNode()
    {
        // Create a separate container backed by a non-expandable node.
        var leafNode = new TreeNodeModel("leaf", "Leaf", isExpandable: false);
        _control.ItemsSource = new[] { leafNode };
        _control.UpdateLayout();

        var container = (TreeNodeContainer)_control.ItemContainerGenerator.ContainerFromItem(leafNode)!;
        container.ApplyTemplate();
        container.UpdateLayout();

        var glyph = (ToggleButton)container.Template.FindName(
            TreeNodeContainer.PartExpandCollapseGlyph, container)!;

        Assert.That(glyph.Visibility, Is.EqualTo(Visibility.Hidden));
    }

    // -----------------------------------------------------------------------
    // PART_CheckBox
    // -----------------------------------------------------------------------

    [Test]
    public void Part_CheckBox_ExistsInTemplate()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartCheckBox, _container);

        Assert.That(part, Is.Not.Null);
    }

    [Test]
    public void Part_CheckBox_IsCheckBox()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartCheckBox, _container);

        Assert.That(part, Is.InstanceOf<CheckBox>());
    }

    [Test]
    public void Part_CheckBox_IsCollapsedByDefault()
    {
        var cb = (CheckBox)_container.Template.FindName(
            TreeNodeContainer.PartCheckBox, _container)!;

        // Phase D will wire ShowCheckboxes. Until then the checkbox is always Collapsed.
        Assert.That(cb.Visibility, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Part_CheckBox_IsThreeState()
    {
        var cb = (CheckBox)_container.Template.FindName(
            TreeNodeContainer.PartCheckBox, _container)!;

        Assert.That(cb.IsThreeState, Is.True);
    }

    // -----------------------------------------------------------------------
    // PART_IconPresenter
    // -----------------------------------------------------------------------

    [Test]
    public void Part_IconPresenter_ExistsInTemplate()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartIconPresenter, _container);

        Assert.That(part, Is.Not.Null);
    }

    [Test]
    public void Part_IconPresenter_IsContentPresenter()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartIconPresenter, _container);

        Assert.That(part, Is.InstanceOf<ContentPresenter>());
    }

    // -----------------------------------------------------------------------
    // PART_LabelPresenter
    // -----------------------------------------------------------------------

    [Test]
    public void Part_LabelPresenter_ExistsInTemplate()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartLabelPresenter, _container);

        Assert.That(part, Is.Not.Null);
    }

    [Test]
    public void Part_LabelPresenter_IsTextBlock()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartLabelPresenter, _container);

        Assert.That(part, Is.InstanceOf<TextBlock>());
    }

    [Test]
    public void Part_LabelPresenter_TextMatchesNodeLabel()
    {
        var label = (TextBlock)_container.Template.FindName(
            TreeNodeContainer.PartLabelPresenter, _container)!;

        // The binding Text="{Binding Label}" must have resolved.
        Assert.That(label.Text, Is.EqualTo(_rootNode.Label));
    }

    [Test]
    public void Part_LabelPresenter_TextUpdatesWhenLabelChanges()
    {
        var label = (TextBlock)_container.Template.FindName(
            TreeNodeContainer.PartLabelPresenter, _container)!;

        _rootNode.Label = "Updated Label";
        _container.UpdateLayout();

        Assert.That(label.Text, Is.EqualTo("Updated Label"));
    }

    // -----------------------------------------------------------------------
    // PART_ChildrenPresenter
    // -----------------------------------------------------------------------

    [Test]
    public void Part_ChildrenPresenter_ExistsInTemplate()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container);

        Assert.That(part, Is.Not.Null);
    }

    [Test]
    public void Part_ChildrenPresenter_IsTreeChildItemsControl()
    {
        var part = _container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container);

        Assert.That(part, Is.InstanceOf<TreeChildItemsControl>());
    }

    [Test]
    public void Part_ChildrenPresenter_IsCollapsed_WhenNotExpanded()
    {
        // _rootNode.IsExpanded starts false.
        var presenter = (TreeChildItemsControl)_container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container)!;

        Assert.That(presenter.Visibility, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Part_ChildrenPresenter_BecomesVisible_WhenExpanded()
    {
        _rootNode.IsExpanded = true;
        _container.UpdateLayout();

        var presenter = (TreeChildItemsControl)_container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container)!;

        Assert.That(presenter.Visibility, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Part_ChildrenPresenter_CollapsesAgain_WhenCollapsed()
    {
        _rootNode.IsExpanded = true;
        _container.UpdateLayout();

        _rootNode.IsExpanded = false;
        _container.UpdateLayout();

        var presenter = (TreeChildItemsControl)_container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container)!;

        Assert.That(presenter.Visibility, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Part_ChildrenPresenter_HasParentTreeViewPropagated()
    {
        var presenter = (TreeChildItemsControl)_container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container)!;

        Assert.That(presenter.ParentTreeView, Is.SameAs(_control));
    }

    [Test]
    public void Part_ChildrenPresenter_ItemsSourceBound_ToNodeChildren()
    {
        var presenter = (TreeChildItemsControl)_container.Template.FindName(
            TreeNodeContainer.PartChildrenPresenter, _container)!;

        // The binding ItemsSource="{Binding Children}" means the presenter's
        // ItemsSource reference should equal the model's Children collection.
        Assert.That(presenter.ItemsSource, Is.SameAs(_rootNode.Children));
    }

    // -----------------------------------------------------------------------
    // All five parts present in a single sweep
    // -----------------------------------------------------------------------

    [Test]
    public void AllFiveTemplateParts_ArePresent()
    {
        var parts = new[]
        {
            TreeNodeContainer.PartExpandCollapseGlyph,
            TreeNodeContainer.PartCheckBox,
            TreeNodeContainer.PartIconPresenter,
            TreeNodeContainer.PartLabelPresenter,
            TreeNodeContainer.PartChildrenPresenter,
        };

        foreach (var name in parts)
        {
            var part = _container.Template.FindName(name, _container);
            Assert.That(part, Is.Not.Null, $"Template part '{name}' was not found.");
        }
    }
}
