// PromptSuppressionEligibilityTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptSuppressionEligibilityTests
{
    [Test]
    public void IsEligible_UserInteractionWithSuppressCheckedAndYes_ReturnsTrue()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.True);
    }

    [Test]
    public void IsEligible_UserInteractionWithSuppressCheckedAndNo_ReturnsTrue()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.No, suppressChecked: true);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.True);
    }

    [Test]
    public void IsEligible_UserInteractionWithSuppressCheckedAndOk_ReturnsTrue()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Ok, suppressChecked: true);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.True);
    }

    [Test]
    public void IsEligible_UserInteractionWithSuppressCheckedAndCancel_ReturnsFalse()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Cancel, suppressChecked: true);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.False);
    }

    [Test]
    public void IsEligible_NonInteractiveWithResultNone_ReturnsFalse()
    {
        var response = PromptResponse.FromNonInteractiveSource(PromptResult.None, suppressChecked: false);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.False);
    }

    [Test]
    public void IsEligible_UserInteractionSuppressCheckedFalse_ReturnsFalse()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: false);
        Assert.That(PromptSuppressionEligibility.IsEligible(response), Is.False);
    }
}
