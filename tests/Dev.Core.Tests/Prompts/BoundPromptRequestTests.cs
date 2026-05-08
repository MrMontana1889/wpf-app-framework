// BoundPromptRequestTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class BoundPromptRequestTests
{
    [Test]
    public void Constructor_WithMatchingIds_BindsDefinitionAndParameters()
    {
        var request = new PromptRequest(
            new PromptId("Build.VisualStudioRunning"),
            parameters: new object?[] { "RepoA", 42 },
            presentationOverrides: new PromptPresentationOverrides("Title", "Message"));

        var definition = new PromptDefinition(
            new PromptId("Build.VisualStudioRunning"),
            "Title {0}",
            "Message {1}",
            PromptButtonSet.YesNo,
            PromptResult.No,
            allowSuppression: true,
            suppressionText: "Do not prompt again");

        var bound = new BoundPromptRequest(request, definition);

        Assert.Multiple(() =>
        {
            Assert.That(bound.Definition, Is.SameAs(definition));
            Assert.That(bound.Parameters, Is.EqualTo(request.Parameters));
            Assert.That(bound.PresentationOverrides, Is.SameAs(request.PresentationOverrides));
        });
    }

    [Test]
    public void Constructor_WithMismatchedIds_Throws()
    {
        var request = new PromptRequest(new PromptId("Build.VisualStudioRunning"));
        var definition = new PromptDefinition(
            new PromptId("Environment.CustomStrategyDuplicate"),
            "Title",
            "Message",
            PromptButtonSet.OkCancel,
            PromptResult.Ok,
            allowSuppression: false,
            suppressionText: null);

        Assert.Throws<InvalidOperationException>(() => _ = new BoundPromptRequest(request, definition));
    }
}
