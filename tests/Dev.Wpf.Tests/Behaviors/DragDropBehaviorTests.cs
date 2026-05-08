// DragDropBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="DragDropBehavior"/> covering lifecycle (including the
/// <see cref="System.Windows.UIElement.AllowDrop"/> side-effect), the
/// <see cref="TreeViewControl.CanDragDrop"/> property, and the
/// <see cref="TreeViewControl.NodesDropped"/> event surface.
///
/// The actual drag initiation (MouseMove beyond threshold) and drop-target
/// validation require a live WPF visual tree and a running dispatcher;
/// those paths are covered by integration tests. These unit tests verify the
/// event raise contract that the behavior delegates to on a successful drop.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class DragDropBehaviorTests
{
    private TreeViewControl _control = null!;

    [SetUp]
    public void SetUp() => _control = new TreeViewControl();

    // -----------------------------------------------------------------------
    // Lifecycle — AllowDrop side-effect
    // -----------------------------------------------------------------------

    [Test]
    public void Attach_SetsAllowDropToTrue()
    {
        Assert.That(_control.AllowDrop, Is.False, "Precondition: AllowDrop starts false.");
        var behavior = new DragDropBehavior();

        behavior.Attach(_control);

        Assert.That(_control.AllowDrop, Is.True);
    }

    [Test]
    public void Detach_ClearsAllowDropToFalse()
    {
        var behavior = new DragDropBehavior();
        behavior.Attach(_control);

        behavior.Detach();

        Assert.That(_control.AllowDrop, Is.False);
    }

    [Test]
    public void Attach_DoesNotThrow()
    {
        var behavior = new DragDropBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new DragDropBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new DragDropBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // NodesDropped event — single node, with target
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodesDropped_FiresEventWithDroppedNodeAndTarget()
    {
        var dragged = new TreeNodeModel("drag", "Drag");
        var target  = new TreeNodeModel("tgt",  "Target");
        NodesDroppedEventArgs? received = null;
        _control.NodesDropped += (_, e) => received = e;

        _control.RaiseNodesDropped([dragged], target, insertionIndex: 0);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.DroppedNodes[0], Is.SameAs(dragged));
        Assert.That(received.TargetNode,        Is.SameAs(target));
        Assert.That(received.InsertionIndex,    Is.EqualTo(0));
    }

    [Test]
    public void RaiseNodesDropped_InsertionIndexIsPreserved()
    {
        var node = new TreeNodeModel("1", "Node");
        NodesDroppedEventArgs? received = null;
        _control.NodesDropped += (_, e) => received = e;

        _control.RaiseNodesDropped([node], null, insertionIndex: 3);

        Assert.That(received!.InsertionIndex, Is.EqualTo(3));
    }

    // -----------------------------------------------------------------------
    // NodesDropped event — null target (drop on root)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodesDropped_WithNullTarget_TargetNodeIsNull()
    {
        var node = new TreeNodeModel("1", "Node");
        NodesDroppedEventArgs? received = null;
        _control.NodesDropped += (_, e) => received = e;

        _control.RaiseNodesDropped([node], targetNode: null, insertionIndex: 0);

        Assert.That(received!.TargetNode, Is.Null);
    }

    // -----------------------------------------------------------------------
    // NodesDropped event — multiple dragged nodes
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodesDropped_MultipleNodes_AllPresentInOrder()
    {
        var a = new TreeNodeModel("a", "A");
        var b = new TreeNodeModel("b", "B");
        NodesDroppedEventArgs? received = null;
        _control.NodesDropped += (_, e) => received = e;

        _control.RaiseNodesDropped([a, b], null, 0);

        Assert.That(received!.DroppedNodes, Has.Count.EqualTo(2));
        Assert.That(received.DroppedNodes[0], Is.SameAs(a));
        Assert.That(received.DroppedNodes[1], Is.SameAs(b));
    }

    [Test]
    public void RaiseNodesDropped_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() =>
            _control.RaiseNodesDropped([node], null, 0));
    }

    // -----------------------------------------------------------------------
    // CanDragDrop dependency property
    // -----------------------------------------------------------------------

    [Test]
    public void CanDragDrop_DefaultIsFalse()
    {
        Assert.That(_control.CanDragDrop, Is.False);
    }

    [Test]
    public void CanDragDrop_CanBeEnabled()
    {
        _control.CanDragDrop = true;
        Assert.That(_control.CanDragDrop, Is.True);
    }

    [Test]
    public void CanDragDrop_CanBeDisabledAfterEnabled()
    {
        _control.CanDragDrop = true;
        _control.CanDragDrop = false;
        Assert.That(_control.CanDragDrop, Is.False);
    }
}
