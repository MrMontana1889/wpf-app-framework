// KeyboardNavigationBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="KeyboardNavigationBehavior"/> covering lifecycle and
/// the event surface it uses: <see cref="TreeViewControl.NodeActivated"/>,
/// <see cref="TreeViewControl.NodeExpanded"/>, and
/// <see cref="TreeViewControl.NodeCollapsed"/>.
///
/// Key-press routing (PreviewKeyDown) requires a live WPF focus and keyboard
/// state; these tests verify the control-level raise methods that the behavior
/// delegates to after resolving the focused node.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class KeyboardNavigationBehaviorTests
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
        var behavior = new KeyboardNavigationBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new KeyboardNavigationBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new KeyboardNavigationBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // NodeActivated event (Enter key)
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
    public void RaiseNodeActivated_FiresForExpandableNode()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        NodeActivatedEventArgs? received = null;
        _control.NodeActivated += (_, e) => received = e;

        _control.RaiseNodeActivated(node);

        Assert.That(received!.Node, Is.SameAs(node));
    }

    [Test]
    public void RaiseNodeActivated_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() => _control.RaiseNodeActivated(node));
    }

    // -----------------------------------------------------------------------
    // NodeExpanded event (Right arrow)
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

    // -----------------------------------------------------------------------
    // NodeCollapsed event (Left arrow)
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
        var node = new TreeNodeModel("1", "Node", isExpandable: true) { IsExpanded = true };
        Assert.DoesNotThrow(() => _control.RaiseNodeCollapsed(node));
    }

    // -----------------------------------------------------------------------
    // Activation is distinct from expand/collapse
    // -----------------------------------------------------------------------

    [Test]
    public void Activation_DoesNotFireExpandedOrCollapsed()
    {
        var node = new TreeNodeModel("1", "Node", isExpandable: true);
        bool expandedFired  = false;
        bool collapsedFired = false;
        _control.NodeExpanded  += (_, _) => expandedFired  = true;
        _control.NodeCollapsed += (_, _) => collapsedFired = true;

        _control.RaiseNodeActivated(node);

        Assert.That(expandedFired,  Is.False);
        Assert.That(collapsedFired, Is.False);
    }
}
