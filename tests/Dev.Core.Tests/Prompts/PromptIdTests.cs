// PromptIdTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptIdTests
{
    [Test]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new PromptId(null!));
    }

    [Test]
    public void Constructor_WithWhitespace_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new PromptId("   "));
    }

    [Test]
    public void Constructor_WithoutNamespaceSeparator_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new PromptId("Environment"));
    }

    [Test]
    public void Constructor_WithEmptyNamespaceSegment_Throws()
    {
        Assert.Throws<ArgumentException>(() => _ = new PromptId("Environment..CustomStrategyDuplicate"));
    }

    [Test]
    public void Constructor_WithValidId_PreservesExactValue()
    {
        var id = new PromptId("Environment.CustomStrategyDuplicate");

        Assert.That(id.Value, Is.EqualTo("Environment.CustomStrategyDuplicate"));
    }
}
