// ToolbarCustomizeMenuEntryTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Windows.Input;

namespace Dev.Core.Tests.ViewModels.Controls;

[TestFixture]
public class ToolbarCustomizeMenuEntryTests
{
    // --- Construction ---

    [Test]
    public void Constructor_CreatesInstance()
    {
        var entry = new ToolbarCustomizeMenuEntry(Substitute.For<ICommand>());

        Assert.That(entry, Is.Not.Null);
    }

    [Test]
    public void Command_ReturnsConstructorArgument()
    {
        var command = Substitute.For<ICommand>();

        var entry = new ToolbarCustomizeMenuEntry(command);

        Assert.That(entry.Command, Is.SameAs(command));
    }
}
