// TreeViewControlTemplateTests.cs
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
/// Template integrity tests for <see cref="TreeViewControl"/>.
///
/// These tests verify the control's dependency-property metadata (set in its
/// static constructor), the virtualization configuration inherited by all
/// instances, the container-override contract, and the runtime template
/// structure after a synchronous layout pass.
///
/// Visual appearance is not tested here.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class TreeViewControlTemplateTests
{
    // The control under test — recreated for each test to keep state clean.
    private TreeViewControl _control = null!;

    [SetUp]
    public void SetUp() => _control = new TreeViewControl();

    // -----------------------------------------------------------------------
    // Virtualization metadata — set via OverrideMetadata, no visual tree needed
    // -----------------------------------------------------------------------

    [Test]
    public void VirtualizingPanel_IsVirtualizing_IsTrue()
    {
        Assert.That(VirtualizingPanel.GetIsVirtualizing(_control), Is.True);
    }

    [Test]
    public void VirtualizingPanel_VirtualizationMode_IsRecycling()
    {
        Assert.That(
            VirtualizingPanel.GetVirtualizationMode(_control),
            Is.EqualTo(VirtualizationMode.Recycling));
    }

    [Test]
    public void ScrollViewer_CanContentScroll_IsFalse()
    {
        Assert.That(ScrollViewer.GetCanContentScroll(_control), Is.False);
    }

    // -----------------------------------------------------------------------
    // IsItemItsOwnContainerOverride
    // -----------------------------------------------------------------------

    private static readonly MethodInfo IsItemItsOwnContainerMethod =
        typeof(TreeViewControl).GetMethod(
            "IsItemItsOwnContainerOverride",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Test]
    public void IsItemItsOwnContainerOverride_TreeNodeModel_ReturnsFalse()
    {
        var node   = new TreeNodeModel("1", "Node");
        var result = (bool)IsItemItsOwnContainerMethod.Invoke(_control, [node])!;

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsItemItsOwnContainerOverride_TreeNodeContainer_ReturnsTrue()
    {
        var container = new TreeNodeContainer();
        var result    = (bool)IsItemItsOwnContainerMethod.Invoke(_control, [container])!;

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsItemItsOwnContainerOverride_ArbitraryString_ReturnsFalse()
    {
        var result = (bool)IsItemItsOwnContainerMethod.Invoke(_control, ["not a container"])!;

        Assert.That(result, Is.False);
    }

    // -----------------------------------------------------------------------
    // GetContainerForItemOverride
    // -----------------------------------------------------------------------

    private static readonly MethodInfo GetContainerMethod =
        typeof(TreeViewControl).GetMethod(
            "GetContainerForItemOverride",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Test]
    public void GetContainerForItemOverride_ReturnsTreeNodeContainer()
    {
        var container = GetContainerMethod.Invoke(_control, null);

        Assert.That(container, Is.InstanceOf<TreeNodeContainer>());
    }

    [Test]
    public void GetContainerForItemOverride_SetsParentTreeView()
    {
        var container = (TreeNodeContainer)GetContainerMethod.Invoke(_control, null)!;

        Assert.That(container.ParentTreeView, Is.SameAs(_control));
    }

    [Test]
    public void GetContainerForItemOverride_EachCallReturnsNewInstance()
    {
        var c1 = GetContainerMethod.Invoke(_control, null);
        var c2 = GetContainerMethod.Invoke(_control, null);

        Assert.That(c1, Is.Not.SameAs(c2));
    }

    // -----------------------------------------------------------------------
    // Template structure — requires a real window for template application
    // -----------------------------------------------------------------------

    [Test]
    public void Template_IsNotNullAfterApply()
    {
        using var host = new TemplateTestHost(_control);

        Assert.That(_control.Template, Is.Not.Null);
    }

    [Test]
    public void Template_ContainsScrollViewer()
    {
        using var host = new TemplateTestHost(_control);

        var scrollViewer = TemplateTestHost.FindChild<ScrollViewer>(_control);

        Assert.That(scrollViewer, Is.Not.Null);
    }

    [Test]
    public void Template_ContainsItemsPresenter()
    {
        using var host = new TemplateTestHost(_control);

        var presenter = TemplateTestHost.FindChild<ItemsPresenter>(_control);

        Assert.That(presenter, Is.Not.Null);
    }

    [Test]
    public void Template_ScrollViewer_CanContentScroll_IsFalse()
    {
        using var host = new TemplateTestHost(_control);

        var scrollViewer = TemplateTestHost.FindChild<ScrollViewer>(_control)!;

        Assert.That(ScrollViewer.GetCanContentScroll(scrollViewer), Is.False);
    }

    // -----------------------------------------------------------------------
    // ItemsPanel — VirtualizingStackPanel
    // -----------------------------------------------------------------------

    [Test]
    public void ItemsPanel_IsVirtualizingStackPanel()
    {
        // Add a node so the panel is realized.
        _control.ItemsSource = new[] { new TreeNodeModel("1", "One") };
        using var host = new TemplateTestHost(_control);

        var panel = TemplateTestHost.FindChild<VirtualizingStackPanel>(_control);

        Assert.That(panel, Is.Not.Null);
    }

    [Test]
    public void ItemsPanel_VirtualizingStackPanel_IsVirtualizing()
    {
        _control.ItemsSource = new[] { new TreeNodeModel("1", "One") };
        using var host = new TemplateTestHost(_control);

        var panel = TemplateTestHost.FindChild<VirtualizingStackPanel>(_control)!;

        Assert.That(VirtualizingPanel.GetIsVirtualizing(panel), Is.True);
    }

    [Test]
    public void ItemsPanel_VirtualizingStackPanel_IsItemsHost()
    {
        _control.ItemsSource = new[] { new TreeNodeModel("1", "One") };
        using var host = new TemplateTestHost(_control);

        var panel = TemplateTestHost.FindChild<VirtualizingStackPanel>(_control)!;

        // VirtualizingStackPanel with IsItemsHost="True" is the items panel.
        // Recycling mode is configured at the control level via OverrideMetadata
        // (see VirtualizingPanel_VirtualizationMode_IsRecycling test).
        Assert.That(panel.IsItemsHost, Is.True);
    }

    // -----------------------------------------------------------------------
    // Container generation via ItemsSource
    // -----------------------------------------------------------------------

    [Test]
    public void ItemsSource_GeneratesTreeNodeContainerPerItem()
    {
        var node = new TreeNodeModel("1", "One");
        _control.ItemsSource = new[] { node };
        using var host = new TemplateTestHost(_control);

        var container = _control.ItemContainerGenerator.ContainerFromItem(node);

        Assert.That(container, Is.InstanceOf<TreeNodeContainer>());
    }

    [Test]
    public void ItemsSource_MultipleItems_EachGetsOwnContainer()
    {
        var a = new TreeNodeModel("a", "A");
        var b = new TreeNodeModel("b", "B");
        _control.ItemsSource = new[] { a, b };
        using var host = new TemplateTestHost(_control);

        var ca = _control.ItemContainerGenerator.ContainerFromItem(a);
        var cb = _control.ItemContainerGenerator.ContainerFromItem(b);

        Assert.That(ca, Is.Not.Null);
        Assert.That(cb, Is.Not.Null);
        Assert.That(ca, Is.Not.SameAs(cb));
    }

    [Test]
    public void GeneratedContainer_HasParentTreeViewSet()
    {
        var node = new TreeNodeModel("1", "One");
        _control.ItemsSource = new[] { node };
        using var host = new TemplateTestHost(_control);

        var container = (TreeNodeContainer)_control.ItemContainerGenerator.ContainerFromItem(node);

        Assert.That(container.ParentTreeView, Is.SameAs(_control));
    }
}
