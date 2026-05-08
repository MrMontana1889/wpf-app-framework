// ContextMenuBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="ContextMenuBehavior"/> covering lifecycle, the
/// <see cref="TreeViewControl.ContextMenuOpening"/> event routing, and the
/// cancel-gate logic used by <see cref="TreeViewControl.RaiseContextMenuOpening"/>.
///
/// The behaviour fires on MouseRightButtonUp which requires a live visual tree;
/// these tests verify the control's internal gate and the event args contract.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ContextMenuBehaviorTests
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
        var behavior = new ContextMenuBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new ContextMenuBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new ContextMenuBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // RaiseContextMenuOpening — cancel gate
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseContextMenuOpening_WithNoSubscribers_ReturnsTrue()
    {
        var node = new TreeNodeModel("1", "Node");

        var result = _control.RaiseContextMenuOpening([node]);

        Assert.That(result, Is.True);
    }

    [Test]
    public void RaiseContextMenuOpening_WhenSubscriberSetsCancel_ReturnsFalse()
    {
        var node = new TreeNodeModel("1", "Node");
        _control.ContextMenuOpening += (_, e) => e.Cancel = true;

        var result = _control.RaiseContextMenuOpening([node]);

        Assert.That(result, Is.False);
    }

    [Test]
    public void RaiseContextMenuOpening_WhenSubscriberDoesNotCancel_ReturnsTrue()
    {
        var node = new TreeNodeModel("1", "Node");
        _control.ContextMenuOpening += (_, _) => { /* no cancellation */ };

        var result = _control.RaiseContextMenuOpening([node]);

        Assert.That(result, Is.True);
    }

    [Test]
    public void RaiseContextMenuOpening_CancelDefaultIsFalse()
    {
        var node = new TreeNodeModel("1", "Node");
        ContextMenuOpeningEventArgs? received = null;
        _control.ContextMenuOpening += (_, e) => received = e;

        _control.RaiseContextMenuOpening([node]);

        Assert.That(received!.Cancel, Is.False);
    }

    // -----------------------------------------------------------------------
    // RaiseContextMenuOpening — event args payload
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseContextMenuOpening_PassesSingleSelectedNode()
    {
        var node = new TreeNodeModel("1", "Node");
        ContextMenuOpeningEventArgs? received = null;
        _control.ContextMenuOpening += (_, e) => received = e;

        _control.RaiseContextMenuOpening([node]);

        Assert.That(received!.SelectedNodes, Has.Count.EqualTo(1));
        Assert.That(received.SelectedNodes[0], Is.SameAs(node));
    }

    [Test]
    public void RaiseContextMenuOpening_PassesMultipleSelectedNodes()
    {
        var a = new TreeNodeModel("a", "A");
        var b = new TreeNodeModel("b", "B");
        var c = new TreeNodeModel("c", "C");
        ContextMenuOpeningEventArgs? received = null;
        _control.ContextMenuOpening += (_, e) => received = e;

        _control.RaiseContextMenuOpening([a, b, c]);

        Assert.That(received!.SelectedNodes, Has.Count.EqualTo(3));
        Assert.That(received.SelectedNodes[0], Is.SameAs(a));
        Assert.That(received.SelectedNodes[1], Is.SameAs(b));
        Assert.That(received.SelectedNodes[2], Is.SameAs(c));
    }

    [Test]
    public void RaiseContextMenuOpening_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() => _control.RaiseContextMenuOpening([node]));
    }

    // -----------------------------------------------------------------------
    // ContextMenuProvider dependency property
    // -----------------------------------------------------------------------

    [Test]
    public void ContextMenuProvider_DefaultIsNull()
    {
        Assert.That(_control.ContextMenuProvider, Is.Null);
    }

    [Test]
    public void ContextMenuProvider_CanBeAssigned()
    {
        var provider = Substitute.For<ITreeContextMenuProvider>();
        _control.ContextMenuProvider = provider;

        Assert.That(_control.ContextMenuProvider, Is.SameAs(provider));
    }

    [Test]
    public void ContextMenuProvider_CanBeCleared()
    {
        var provider = Substitute.For<ITreeContextMenuProvider>();
        _control.ContextMenuProvider = provider;

        _control.ContextMenuProvider = null;

        Assert.That(_control.ContextMenuProvider, Is.Null);
    }

    // -----------------------------------------------------------------------
    // SupportsContextMenu node flag — behaviour suppresses menu when all
    // selected nodes opt out.  Tested indirectly via the guard in
    // ContextMenuBehavior (reaching it requires visual tree),
    // so we verify the model contract here.
    // -----------------------------------------------------------------------

    [Test]
    public void TreeNodeModel_SupportsContextMenu_DefaultIsTrue()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.That(node.SupportsContextMenu, Is.True);
    }

    [Test]
    public void TreeNodeModel_SupportsContextMenu_CanBeOptedOut()
    {
        var node = new TreeNodeModel("1", "Node") { SupportsContextMenu = false };
        Assert.That(node.SupportsContextMenu, Is.False);
    }
}
