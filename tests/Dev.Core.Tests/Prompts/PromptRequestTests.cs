// PromptRequestTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptRequestTests
{
    [Test]
    public void Constructor_CopiesParametersToPreventExternalMutation()
    {
        var parameters = new object?[] { "RepoA", 5 };
        var request = new PromptRequest(new PromptId("Workspace.ResetStrategyConfirmation"), parameters);

        parameters[0] = "Mutated";

        Assert.That(request.Parameters[0], Is.EqualTo("RepoA"));
    }

    [Test]
    public void Constructor_WithPresentationOverrides_AssignsOverrides()
    {
        var overrides = new PromptPresentationOverrides("Override Title", "Override Message");
        var request = new PromptRequest(
            new PromptId("Workspace.ResetStrategyConfirmation"),
            parameters: null,
            presentationOverrides: overrides);

        Assert.That(request.PresentationOverrides, Is.SameAs(overrides));
    }
}
