// PromptSuppressionServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptSuppressionServiceTests
{
    private string _profileDir = null!;
    private PromptSuppressionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _profileDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_profileDir);
        _service = new PromptSuppressionService(_profileDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_profileDir))
            Directory.Delete(_profileDir, recursive: true);
    }

    // ── Persist / lookup happy paths ─────────────────────────────────────────

    [Test]
    public void PersistSuppression_ThenLookup_ReturnsSameResult()
    {
        var id = new PromptId("Build.VisualStudioRunning");
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);

        _service.PersistSuppression(id, response);

        var retrieved = _service.TryGetSuppressedResponse(id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Result, Is.EqualTo(PromptResult.Yes));
    }

    [Test]
    public void PersistSuppression_SurvivesServiceRestart()
    {
        var id = new PromptId("Environment.CustomStrategyDuplicate");
        _service.PersistSuppression(id, PromptResponse.FromUserInteraction(PromptResult.No, suppressChecked: true));

        var reloaded = new PromptSuppressionService(_profileDir);
        var retrieved = reloaded.TryGetSuppressedResponse(id);

        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Result, Is.EqualTo(PromptResult.No));
    }

    [Test]
    public void TryGetSuppressedResponse_WhenNoEntry_ReturnsNull()
    {
        var result = _service.TryGetSuppressedResponse(new PromptId("Unknown.Prompt"));
        Assert.That(result, Is.Null);
    }

    // ── Eligibility enforcement ───────────────────────────────────────────────

    [Test]
    public void PersistSuppression_WithCancelResult_Throws()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Cancel, suppressChecked: true);

        Assert.Throws<ArgumentException>(() =>
            _service.PersistSuppression(new PromptId("Build.VisualStudioRunning"), response));
    }

    [Test]
    public void PersistSuppression_WithSuppressCheckedFalse_Throws()
    {
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: false);

        Assert.Throws<ArgumentException>(() =>
            _service.PersistSuppression(new PromptId("Build.VisualStudioRunning"), response));
    }

    [Test]
    public void PersistSuppression_WithNonInteractiveNone_Throws()
    {
        var response = PromptResponse.FromNonInteractiveSource(PromptResult.None, suppressChecked: false);

        Assert.Throws<ArgumentException>(() =>
            _service.PersistSuppression(new PromptId("Build.VisualStudioRunning"), response));
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    [Test]
    public void ClearSuppression_RemovesEntry()
    {
        var id = new PromptId("Build.VisualStudioRunning");
        _service.PersistSuppression(id, PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true));

        _service.ClearSuppression(id);

        Assert.That(_service.TryGetSuppressedResponse(id), Is.Null);
    }

    [Test]
    public void ClearSuppression_PersistedRemovalSurvivesRestart()
    {
        var id = new PromptId("Build.VisualStudioRunning");
        _service.PersistSuppression(id, PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true));
        _service.ClearSuppression(id);

        var reloaded = new PromptSuppressionService(_profileDir);
        Assert.That(reloaded.TryGetSuppressedResponse(id), Is.Null);
    }

    [Test]
    public void ClearAll_RemovesAllEntries()
    {
        _service.PersistSuppression(new PromptId("Build.VisualStudioRunning"),
            PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true));

        _service.PersistSuppression(new PromptId("Environment.CustomStrategyDuplicate"),
            PromptResponse.FromUserInteraction(PromptResult.Ok, suppressChecked: true));

        _service.ClearAll();

        Assert.Multiple(() =>
        {
            Assert.That(_service.TryGetSuppressedResponse(new PromptId("Build.VisualStudioRunning")), Is.Null);
            Assert.That(_service.TryGetSuppressedResponse(new PromptId("Environment.CustomStrategyDuplicate")), Is.Null);
        });
    }

    // ── Fail-open behavior ────────────────────────────────────────────────────

    [Test]
    public void Constructor_WhenFileIsCorrupted_FailsOpenWithEmptyStore()
    {
        var filePath = Path.Combine(_profileDir, "prompt-suppressions.json");
        File.WriteAllText(filePath, "NOT VALID JSON {{{{");

        var messages = new List<string>();
        var service = new PromptSuppressionService(_profileDir, messages.Add);

        Assert.Multiple(() =>
        {
            Assert.That(service.TryGetSuppressedResponse(new PromptId("Any.Prompt")), Is.Null);
            Assert.That(messages, Has.Count.GreaterThan(0));
        });
    }

    [Test]
    public void Constructor_WhenFileDoesNotExist_StartsFresh()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(emptyDir);

        try
        {
            var service = new PromptSuppressionService(emptyDir);
            Assert.That(service.TryGetSuppressedResponse(new PromptId("Build.VisualStudioRunning")), Is.Null);
        }
        finally
        {
            Directory.Delete(emptyDir, recursive: true);
        }
    }

    [Test]
    public void SaveFailure_DoesNotThrow_AndInMemoryStateUnchanged()
    {
        // Make profile dir read-only so writes fail.
        var readOnlyDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(readOnlyDir);

        var messages = new List<string>();
        var service = new PromptSuppressionService(readOnlyDir, messages.Add);

        // Lock the file path against writes.
        var filePath = Path.Combine(readOnlyDir, "prompt-suppressions.json");
        File.WriteAllText(filePath, "{}");

        using var lockStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var id = new PromptId("Build.VisualStudioRunning");
        var response = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);

        try
        {
            // Must not throw even when save fails.
            Assert.DoesNotThrow(() => service.PersistSuppression(id, response));

            // In-memory state is updated even if disk write fails.
            Assert.That(service.TryGetSuppressedResponse(id), Is.Not.Null);

            // A log message must be emitted.
            Assert.That(messages, Has.Count.GreaterThan(0));
        }
        finally
        {
            lockStream.Dispose();
            Directory.Delete(readOnlyDir, recursive: true);
        }
    }

    // ── Stored result integrity on load ───────────────────────────────────────

    [Test]
    public void TryGetSuppressedResponse_WhenStoredResultIsUnrecognised_FailsOpenAndLogs()
    {
        var filePath = Path.Combine(_profileDir, "prompt-suppressions.json");
        File.WriteAllText(filePath, """{"Build.VisualStudioRunning": "NonExistentResult"}""");

        var messages = new List<string>();
        var service = new PromptSuppressionService(_profileDir, messages.Add);

        var result = service.TryGetSuppressedResponse(new PromptId("Build.VisualStudioRunning"));

        Assert.That(result, Is.Null);
        Assert.That(messages, Has.Count.GreaterThan(0));
    }

    [Test]
    public void TryGetSuppressedResponse_WhenStoredResultIsCancel_FailsOpenAndLogs()
    {
        var filePath = Path.Combine(_profileDir, "prompt-suppressions.json");
        File.WriteAllText(filePath, """{"Build.VisualStudioRunning": "Cancel"}""");

        var messages = new List<string>();
        var service = new PromptSuppressionService(_profileDir, messages.Add);

        var result = service.TryGetSuppressedResponse(new PromptId("Build.VisualStudioRunning"));

        Assert.That(result, Is.Null);
        Assert.That(messages, Has.Count.GreaterThan(0));
    }
}
