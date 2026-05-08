// PromptDefinitionTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptDefinitionTests
{
    [Test]
    public void Constructor_WithValidData_AssignsProperties()
    {
        var definition = new PromptDefinition(
            new PromptId("Build.VisualStudioRunning"),
            "Build Running",
            "Visual Studio is currently running.",
            PromptButtonSet.YesNo,
            PromptResult.No,
            allowSuppression: true,
            suppressionText: "Do not prompt again");

        Assert.Multiple(() =>
        {
            Assert.That(definition.Id.Value, Is.EqualTo("Build.VisualStudioRunning"));
            Assert.That(definition.TitleTemplate, Is.EqualTo("Build Running"));
            Assert.That(definition.MessageTemplate, Is.EqualTo("Visual Studio is currently running."));
            Assert.That(definition.ButtonSet, Is.EqualTo(PromptButtonSet.YesNo));
            Assert.That(definition.DefaultResult, Is.EqualTo(PromptResult.No));
            Assert.That(definition.AllowSuppression, Is.True);
            Assert.That(definition.SuppressionText, Is.EqualTo("Do not prompt again"));
        });
    }

    [Test]
    public void Constructor_WithInvalidDefaultForButtonSet_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new PromptDefinition(
                new PromptId("Build.VisualStudioRunning"),
                "Build Running",
                "Visual Studio is currently running.",
                PromptButtonSet.YesNo,
                PromptResult.Ok,
                allowSuppression: false,
                suppressionText: null));
    }

    [Test]
    public void Constructor_WhenSuppressionAllowedWithoutSuppressionText_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _ = new PromptDefinition(
                new PromptId("Build.VisualStudioRunning"),
                "Build Running",
                "Visual Studio is currently running.",
                PromptButtonSet.YesNo,
                PromptResult.No,
                allowSuppression: true,
                suppressionText: " "));
    }

    [Test]
    public void Type_HasNoPublicPropertySetters()
    {
        var hasPublicSetters = typeof(PromptDefinition)
            .GetProperties()
            .Any(property => property.SetMethod is { IsPublic: true });

        Assert.That(hasPublicSetters, Is.False);
    }
}
