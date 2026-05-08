// ExpandCollapseBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="ExpandCollapseBehavior"/> covering lifecycle and the
/// <see cref="TreeViewControl.NodeExpanded"/>,
/// <see cref="TreeViewControl.NodeCollapsed"/>, and
/// <see cref="TreeViewControl.NodeActivated"/> event surface.
///
/// The behavior routes ToggleButton.Checked/Unchecked and mouse double-click
/// events — both require a live WPF visual tree — so these tests verify the
/// control's internal raise methods that the behavior delegates to.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ExpandCollapseBehaviorTests
{
    private TreeViewControl _control = null!;

    [SetUp]
    public void SetUp() => _control = new TreeViewControl();

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------

    [Test]
    public void Attach_DoesNotThrow()
    {
        var behavior = new ExpandCollapseBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new ExpandCollapseBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new ExpandCollapseBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // NodeExpanded event
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeExpanded_FiresEventWithCorrectNode()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        NodeExpandedEventArgs? received = null;
        _control.NodeExpanded += (_, e) => received = e;

        _control.RaiseNodeExpanded(node);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.Node, Is.SameAs(node));
    }

    [Test]
    public void RaiseNodeExpanded_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        Assert.DoesNotThrow(() => _control.RaiseNodeExpanded(node));
    }

    [Test]
    public void RaiseNodeExpanded_DoesNotMutateNodeIsExpanded()
    {
        // The behavior sets IsExpanded via the ToggleButton two-way binding;
        // RaiseNodeExpanded is a pure event raise and must not touch the model.
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        Assert.That(node.IsExpanded, Is.False);

        _control.RaiseNodeExpanded(node);

        Assert.That(node.IsExpanded, Is.False);
    }

    [Test]
    public void RaiseNodeExpanded_FiresForLeafNodeWithoutError()
    {
        // Leaf nodes should not be expanded in practise, but the event
        // raise must not throw if the behavior calls it with a non-expandable node.
        var leaf = new TreeNodeModel("leaf", "Leaf", isExpandable: false);
        Assert.DoesNotThrow(() => _control.RaiseNodeExpanded(leaf));
    }

    // -----------------------------------------------------------------------
    // NodeCollapsed event
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCollapsed_FiresEventWithCorrectNode()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true) { IsExpanded = true };
        NodeCollapsedEventArgs? received = null;
        _control.NodeCollapsed += (_, e) => received = e;

        _control.RaiseNodeCollapsed(node);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.Node, Is.SameAs(node));
    }

    [Test]
    public void RaiseNodeCollapsed_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        Assert.DoesNotThrow(() => _control.RaiseNodeCollapsed(node));
    }

    [Test]
    public void RaiseNodeCollapsed_DoesNotMutateNodeIsExpanded()
    {
        // Same principle as expand: the binding drives state; the event
        // raise must not touch the model.
        var node = new TreeNodeModel("1", "Node", isExpandable: true) { IsExpanded = true };

        _control.RaiseNodeCollapsed(node);

        Assert.That(node.IsExpanded, Is.True);
    }

    // -----------------------------------------------------------------------
    // NodeActivated event (double-click)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeActivated_FiresEventWithCorrectNode()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeActivatedEventArgs? received = null;
        _control.NodeActivated += (_, e) => received = e;

        _control.RaiseNodeActivated(node);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.Node, Is.SameAs(node));
    }

    [Test]
    public void RaiseNodeActivated_FiresForLeafNode()
    {
        var leaf = new TreeNodeModel("leaf", "Leaf", isExpandable: false);
        NodeActivatedEventArgs? received = null;
        _control.NodeActivated += (_, e) => received = e;

        _control.RaiseNodeActivated(leaf);

        Assert.That(received!.Node, Is.SameAs(leaf));
    }

    [Test]
    public void RaiseNodeActivated_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() => _control.RaiseNodeActivated(node));
    }

    // -----------------------------------------------------------------------
    // Expanded / collapsed are independent events on the same control
    // -----------------------------------------------------------------------

    [Test]
    public void ExpandedAndCollapsed_BothSubscribed_OnlyExpandedFires()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        bool expandedFired  = false;
        bool collapsedFired = false;
        _control.NodeExpanded  += (_, _) => expandedFired  = true;
        _control.NodeCollapsed += (_, _) => collapsedFired = true;

        _control.RaiseNodeExpanded(node);

        Assert.That(expandedFired,  Is.True);
        Assert.That(collapsedFired, Is.False);
    }

    [Test]
    public void ExpandedAndCollapsed_BothSubscribed_OnlyCollapsedFires()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        bool expandedFired  = false;
        bool collapsedFired = false;
        _control.NodeExpanded  += (_, _) => expandedFired  = true;
        _control.NodeCollapsed += (_, _) => collapsedFired = true;

        _control.RaiseNodeCollapsed(node);

        Assert.That(expandedFired,  Is.False);
        Assert.That(collapsedFired, Is.True);
    }
}
