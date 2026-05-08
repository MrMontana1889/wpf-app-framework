// MultiSelectBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="MultiSelectBehavior"/> covering lifecycle, the
/// <see cref="TreeViewControl.SelectedNodes"/> attachment point, and the
/// <see cref="TreeViewControl.SelectionChanged"/> /
/// <see cref="TreeViewControl.MultiSelectionChanged"/> event surface.
///
/// Input-driven paths (Ctrl+Click, Shift+Click, right-click) require a live
/// WPF visual tree and are covered by integration tests; these unit tests
/// verify the control's internal API that the behavior calls.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class MultiSelectBehaviorTests
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
        var behavior = new MultiSelectBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new MultiSelectBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new MultiSelectBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // SetSelectedNodes attachment point
    // -----------------------------------------------------------------------

    [Test]
    public void SetSelectedNodes_SingleNode_UpdatesSelectedNodesProperty()
    {
        var node = new TreeNodeModel("1", "Node");

        _control.SetSelectedNodes([node]);

        Assert.That(_control.SelectedNodes, Has.Count.EqualTo(1));
        Assert.That(_control.SelectedNodes[0], Is.SameAs(node));
    }

    [Test]
    public void SetSelectedNodes_MultipleNodes_AllPresentInOrder()
    {
        var a = new TreeNodeModel("a", "A");
        var b = new TreeNodeModel("b", "B");
        var c = new TreeNodeModel("c", "C");

        _control.SetSelectedNodes([a, b, c]);

        Assert.That(_control.SelectedNodes, Has.Count.EqualTo(3));
        Assert.That(_control.SelectedNodes[0], Is.SameAs(a));
        Assert.That(_control.SelectedNodes[1], Is.SameAs(b));
        Assert.That(_control.SelectedNodes[2], Is.SameAs(c));
    }

    [Test]
    public void SetSelectedNodes_EmptyList_ClearsSelectedNodes()
    {
        var node = new TreeNodeModel("1", "Node");
        _control.SetSelectedNodes([node]);

        _control.SetSelectedNodes([]);

        Assert.That(_control.SelectedNodes, Is.Empty);
    }

    [Test]
    public void SelectedNodes_DefaultIsEmpty()
    {
        Assert.That(_control.SelectedNodes, Is.Empty);
    }

    // -----------------------------------------------------------------------
    // SelectionChanged event (single-node selection)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseSelectionChanged_FiresEventWithOldAndNewNode()
    {
        var oldNode = new TreeNodeModel("old", "Old");
        var newNode = new TreeNodeModel("new", "New");
        TreeSelectionChangedEventArgs? received = null;
        _control.SelectionChanged += (_, e) => received = e;

        _control.RaiseSelectionChanged(oldNode, newNode);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.OldNode, Is.SameAs(oldNode));
        Assert.That(received.NewNode, Is.SameAs(newNode));
    }

    [Test]
    public void RaiseSelectionChanged_WithNullOldNode_OldNodeIsNull()
    {
        var newNode = new TreeNodeModel("new", "New");
        TreeSelectionChangedEventArgs? received = null;
        _control.SelectionChanged += (_, e) => received = e;

        _control.RaiseSelectionChanged(null, newNode);

        Assert.That(received!.OldNode, Is.Null);
        Assert.That(received.NewNode, Is.SameAs(newNode));
    }

    [Test]
    public void RaiseSelectionChanged_WithNullNewNode_NewNodeIsNull()
    {
        var oldNode = new TreeNodeModel("old", "Old");
        TreeSelectionChangedEventArgs? received = null;
        _control.SelectionChanged += (_, e) => received = e;

        _control.RaiseSelectionChanged(oldNode, null);

        Assert.That(received!.OldNode, Is.SameAs(oldNode));
        Assert.That(received.NewNode, Is.Null);
    }

    [Test]
    public void RaiseSelectionChanged_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() => _control.RaiseSelectionChanged(null, node));
    }

    // -----------------------------------------------------------------------
    // MultiSelectionChanged event (Ctrl+Click / Shift+Click / Ctrl+A)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseMultiSelectionChanged_FiresWithAddedAndRemovedNodes()
    {
        var added   = new TreeNodeModel("a", "Added");
        var removed = new TreeNodeModel("r", "Removed");
        TreeMultiSelectionChangedEventArgs? received = null;
        _control.MultiSelectionChanged += (_, e) => received = e;

        _control.RaiseMultiSelectionChanged([added], [removed]);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.AddedNodes,     Has.Count.EqualTo(1));
        Assert.That(received.AddedNodes[0],   Is.SameAs(added));
        Assert.That(received.RemovedNodes,    Has.Count.EqualTo(1));
        Assert.That(received.RemovedNodes[0], Is.SameAs(removed));
    }

    [Test]
    public void RaiseMultiSelectionChanged_MultipleAdded_AllPresent()
    {
        var a = new TreeNodeModel("a", "A");
        var b = new TreeNodeModel("b", "B");
        TreeMultiSelectionChangedEventArgs? received = null;
        _control.MultiSelectionChanged += (_, e) => received = e;

        _control.RaiseMultiSelectionChanged([a, b], []);

        Assert.That(received!.AddedNodes, Has.Count.EqualTo(2));
        Assert.That(received.RemovedNodes, Is.Empty);
    }

    [Test]
    public void RaiseMultiSelectionChanged_EmptyLists_EventStillFires()
    {
        TreeMultiSelectionChangedEventArgs? received = null;
        _control.MultiSelectionChanged += (_, e) => received = e;

        _control.RaiseMultiSelectionChanged([], []);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.AddedNodes,   Is.Empty);
        Assert.That(received.RemovedNodes,  Is.Empty);
    }

    [Test]
    public void RaiseMultiSelectionChanged_WhenNoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
            _control.RaiseMultiSelectionChanged([], []));
    }

    // -----------------------------------------------------------------------
    // SelectionMode dependency property
    // -----------------------------------------------------------------------

    [Test]
    public void SelectionMode_DefaultIsSingle()
    {
        Assert.That(_control.SelectionMode, Is.EqualTo(TreeSelectionMode.Single));
    }

    [Test]
    public void SelectionMode_CanBeSetToMultiple()
    {
        _control.SelectionMode = TreeSelectionMode.Multiple;
        Assert.That(_control.SelectionMode, Is.EqualTo(TreeSelectionMode.Multiple));
    }

    [Test]
    public void SelectionMode_CanBeSetToNone()
    {
        _control.SelectionMode = TreeSelectionMode.None;
        Assert.That(_control.SelectionMode, Is.EqualTo(TreeSelectionMode.None));
    }

    [Test]
    public void SelectionMode_RoundTrip_RetainsLastValue()
    {
        _control.SelectionMode = TreeSelectionMode.Multiple;
        _control.SelectionMode = TreeSelectionMode.Single;
        Assert.That(_control.SelectionMode, Is.EqualTo(TreeSelectionMode.Single));
    }
}
