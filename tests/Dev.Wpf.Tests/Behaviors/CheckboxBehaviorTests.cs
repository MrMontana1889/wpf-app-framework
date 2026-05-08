// CheckboxBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Tree;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Threading;

namespace Dev.Wpf.Tests.Behaviors;

/// <summary>
/// Tests for <see cref="CheckboxBehavior"/> covering lifecycle and the
/// <see cref="TreeViewControl.NodeCheckedChanged"/> event surface.
///
/// The behavior captures the pre-change value on PreviewMouseLeftButtonDown
/// and fires the event after CheckBox.Checked/Unchecked/Indeterminate bubbles.
/// Both phases require a live WPF visual tree; these tests verify the
/// control's internal raise method across all tri-state transitions.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class CheckboxBehaviorTests
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
        var behavior = new CheckboxBehavior();
        Assert.DoesNotThrow(() => behavior.Attach(_control));
    }

    [Test]
    public void Detach_AfterAttach_DoesNotThrow()
    {
        var behavior = new CheckboxBehavior();
        behavior.Attach(_control);
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    [Test]
    public void Detach_WithoutPriorAttach_DoesNotThrow()
    {
        var behavior = new CheckboxBehavior();
        Assert.DoesNotThrow(() => behavior.Detach());
    }

    // -----------------------------------------------------------------------
    // NodeCheckedChanged event — false → true (Checked)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCheckedChanged_FalseToTrue_FiresWithCorrectValues()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: false, newValue: true);

        Assert.That(received, Is.Not.Null);
        Assert.That(received!.Node,     Is.SameAs(node));
        Assert.That(received.OldValue,  Is.EqualTo(false));
        Assert.That(received.NewValue,  Is.EqualTo(true));
    }

    [Test]
    public void RaiseNodeCheckedChanged_FalseToTrue_NodeReferencePreserved()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, false, true);

        Assert.That(received!.Node, Is.SameAs(node));
    }

    // -----------------------------------------------------------------------
    // NodeCheckedChanged event — true → false (Unchecked)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCheckedChanged_TrueToFalse_FiresWithCorrectValues()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: true, newValue: false);

        Assert.That(received!.OldValue, Is.EqualTo(true));
        Assert.That(received.NewValue,  Is.EqualTo(false));
    }

    // -----------------------------------------------------------------------
    // NodeCheckedChanged event — x → null (Indeterminate)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCheckedChanged_TrueToNull_NewValueIsNull()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: true, newValue: null);

        Assert.That(received!.OldValue, Is.EqualTo(true));
        Assert.That(received.NewValue,  Is.Null);
    }

    [Test]
    public void RaiseNodeCheckedChanged_FalseToNull_NewValueIsNull()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: false, newValue: null);

        Assert.That(received!.NewValue, Is.Null);
    }

    // -----------------------------------------------------------------------
    // NodeCheckedChanged event — null → x (from indeterminate)
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCheckedChanged_NullToTrue_OldValueIsNull()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: null, newValue: true);

        Assert.That(received!.OldValue, Is.Null);
        Assert.That(received.NewValue,  Is.EqualTo(true));
    }

    [Test]
    public void RaiseNodeCheckedChanged_NullToFalse_OldValueIsNull()
    {
        var node = new TreeNodeModel("1", "Node");
        NodeCheckedChangedEventArgs? received = null;
        _control.NodeCheckedChanged += (_, e) => received = e;

        _control.RaiseNodeCheckedChanged(node, oldValue: null, newValue: false);

        Assert.That(received!.OldValue, Is.Null);
        Assert.That(received.NewValue,  Is.EqualTo(false));
    }

    // -----------------------------------------------------------------------
    // Guard: no subscribers
    // -----------------------------------------------------------------------

    [Test]
    public void RaiseNodeCheckedChanged_WhenNoSubscribers_DoesNotThrow()
    {
        var node = new TreeNodeModel("1", "Node");
        Assert.DoesNotThrow(() =>
            _control.RaiseNodeCheckedChanged(node, oldValue: false, newValue: true));
    }

    // -----------------------------------------------------------------------
    // ShowCheckboxes dependency property (behavior is gated on this)
    // -----------------------------------------------------------------------

    [Test]
    public void ShowCheckboxes_DefaultIsFalse()
    {
        Assert.That(_control.ShowCheckboxes, Is.False);
    }

    [Test]
    public void ShowCheckboxes_CanBeEnabled()
    {
        _control.ShowCheckboxes = true;
        Assert.That(_control.ShowCheckboxes, Is.True);
    }
}
