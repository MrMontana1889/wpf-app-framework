// PromptRegistryTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptRegistryTests
{
    [Test]
    public void Register_WithDuplicatePromptId_Throws()
    {
        var registry = new PromptRegistry();
        var definition = CreateDefinition("Environment.CustomStrategyDuplicate");

        registry.Register(definition);

        Assert.Throws<InvalidOperationException>(() => registry.Register(definition));
    }

    [Test]
    public void Register_ThenGetDefinition_ReturnsDefinition()
    {
        var registry = new PromptRegistry();
        var definition = CreateDefinition("Build.VisualStudioRunning");

        registry.Register(definition);

        var resolved = registry.GetDefinition(new PromptId("Build.VisualStudioRunning"));

        Assert.That(resolved, Is.SameAs(definition));
    }

    [Test]
    public void GetDefinition_WhenUnknown_Throws()
    {
        var registry = new PromptRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.GetDefinition(new PromptId("Unknown.Prompt")));
    }

    [Test]
    public void Definitions_ContainsRegisteredEntries()
    {
        var registry = new PromptRegistry();
        registry.Register(CreateDefinition("Environment.CustomStrategyDuplicate"));
        registry.Register(CreateDefinition("Build.VisualStudioRunning"));

        Assert.That(registry.Definitions.Select(definition => definition.Id.Value),
            Is.EquivalentTo(new[]
            {
                "Environment.CustomStrategyDuplicate",
                "Build.VisualStudioRunning",
            }));
    }

    private static PromptDefinition CreateDefinition(string id)
    {
        return new PromptDefinition(
            new PromptId(id),
            "Title",
            "Message",
            PromptButtonSet.OkCancel,
            PromptResult.Ok,
            allowSuppression: true,
            suppressionText: "Do not prompt again");
    }
}
