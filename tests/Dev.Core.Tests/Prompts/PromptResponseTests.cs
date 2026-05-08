// PromptResponseTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptResponseTests
{
    [Test]
    public void FromUserInteraction_WithResultNone_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = PromptResponse.FromUserInteraction(PromptResult.None, suppressChecked: false));
    }

    [Test]
    public void FromUserInteraction_WithConcreteResult_CreatesInteractiveResponse()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);

        Assert.Multiple(() =>
        {
            Assert.That(response.Result, Is.EqualTo(PromptResult.Yes));
            Assert.That(response.SuppressChecked, Is.True);
            Assert.That(response.IsFromUserInteraction, Is.True);
        });
    }

    [Test]
    public void FromNonInteractiveSource_AllowsResultNone()
    {
        var response = PromptResponse.FromNonInteractiveSource(PromptResult.None, suppressChecked: false);

        Assert.Multiple(() =>
        {
            Assert.That(response.Result, Is.EqualTo(PromptResult.None));
            Assert.That(response.SuppressChecked, Is.False);
            Assert.That(response.IsFromUserInteraction, Is.False);
        });
    }
}
