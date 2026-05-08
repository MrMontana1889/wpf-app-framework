// PromptOrchestrationServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Prompts;
using NSubstitute;
using NUnit.Framework;

namespace Dev.Core.Tests.Prompts;

[TestFixture]
public class PromptOrchestrationServiceTests
{
    private PromptRegistry _registry = null!;
    private string _profileDir = null!;
    private PromptSuppressionService _suppressionService = null!;
    private IPromptPresenter _presenter = null!;
    private PromptOrchestrationService _orchestrator = null!;

    // Definitions used across tests
    private static readonly PromptId SuppressibleId = new("Build.VisualStudioRunning");
    private static readonly PromptId NonSuppressibleId = new("Workspace.ResetStrategyConfirmation");

    [SetUp]
    public void SetUp()
    {
        _registry = new PromptRegistry();
        _profileDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_profileDir);
        _suppressionService = new PromptSuppressionService(_profileDir);
        _presenter = Substitute.For<IPromptPresenter>();
        _orchestrator = new PromptOrchestrationService(_registry, _suppressionService, _presenter);

        _registry.Register(new PromptDefinition(
            SuppressibleId,
            "Build Running",
            "Visual Studio is currently running.",
            PromptButtonSet.YesNo,
            PromptResult.No,
            allowSuppression: true,
            suppressionText: "Do not prompt again"));

        _registry.Register(new PromptDefinition(
            NonSuppressibleId,
            "Reset Strategy",
            "Are you sure you want to reset?",
            PromptButtonSet.OkCancel,
            PromptResult.Cancel,
            allowSuppression: false,
            suppressionText: null));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_profileDir))
            Directory.Delete(_profileDir, recursive: true);
    }

    // ── Suppression bypass ────────────────────────────────────────────────────

    [Test]
    public void Execute_WhenSuppressedResponseExists_BypassesPresenter()
    {
        var storedResponse = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);
        _suppressionService.PersistSuppression(SuppressibleId, storedResponse);

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        _presenter.DidNotReceive().Present(Arg.Any<BoundPromptRequest>());
    }

    [Test]
    public void Execute_WhenSuppressedResponseExists_ReturnsSuppressedResult()
    {
        var storedResponse = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);
        _suppressionService.PersistSuppression(SuppressibleId, storedResponse);

        var result = _orchestrator.Execute(new PromptRequest(SuppressibleId));

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(PromptResult.Yes));
            Assert.That(result.IsFromUserInteraction, Is.False);
        });
    }

    // ── Non-suppressed presentation path ─────────────────────────────────────

    [Test]
    public void Execute_WhenNoSuppressedResponse_InvokesPresenter()
    {
        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: false));

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        _presenter.Received(1).Present(Arg.Any<BoundPromptRequest>());
    }

    [Test]
    public void Execute_PresenterResponse_IsReturnedToCallerUnchanged()
    {
        var presenterResponse = PromptResponse.FromUserInteraction(PromptResult.No, suppressChecked: false);
        _presenter.Present(Arg.Any<BoundPromptRequest>()).Returns(presenterResponse);

        var result = _orchestrator.Execute(new PromptRequest(SuppressibleId));

        Assert.That(result, Is.SameAs(presenterResponse));
    }

    [Test]
    public void Execute_PresenterReceivesBoundRequestWithCorrectDefinition()
    {
        BoundPromptRequest? captured = null;
        _presenter.Present(Arg.Do<BoundPromptRequest>(r => captured = r))
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: false));

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Definition.Id, Is.EqualTo(SuppressibleId));
    }

    // ── AllowSuppression gate ─────────────────────────────────────────────────

    [Test]
    public void Execute_NonSuppressiblePrompt_AlwaysInvokesPresenter()
    {
        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Ok, suppressChecked: false));

        _orchestrator.Execute(new PromptRequest(NonSuppressibleId));

        _presenter.Received(1).Present(Arg.Any<BoundPromptRequest>());
    }

    [Test]
    public void Execute_NonSuppressiblePrompt_DoesNotQuerySuppressionService()
    {
        var mockSuppression = Substitute.For<IPromptSuppressionService>();
        var orchestrator = new PromptOrchestrationService(_registry, mockSuppression, _presenter);

        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Ok, suppressChecked: false));

        orchestrator.Execute(new PromptRequest(NonSuppressibleId));

        mockSuppression.DidNotReceive().TryGetSuppressedResponse(Arg.Any<PromptId>());
    }

    // ── Conditional persistence after presentation ────────────────────────────

    [Test]
    public void Execute_EligibleResponse_PersistsSuppression()
    {
        var eligibleResponse = PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: true);
        _presenter.Present(Arg.Any<BoundPromptRequest>()).Returns(eligibleResponse);

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        // Verify persistence by consulting the suppression service directly
        var stored = _suppressionService.TryGetSuppressedResponse(SuppressibleId);
        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!.Result, Is.EqualTo(PromptResult.Yes));
    }

    [Test]
    public void Execute_CancelResponse_IsNotPersisted()
    {
        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Cancel, suppressChecked: true));

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        Assert.That(_suppressionService.TryGetSuppressedResponse(SuppressibleId), Is.Null);
    }

    [Test]
    public void Execute_SuppressCheckedFalseResponse_IsNotPersisted()
    {
        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromUserInteraction(PromptResult.Yes, suppressChecked: false));

        _orchestrator.Execute(new PromptRequest(SuppressibleId));

        Assert.That(_suppressionService.TryGetSuppressedResponse(SuppressibleId), Is.Null);
    }

    // ── Presenter contract enforcement ───────────────────────────────────────

    [Test]
    public void Execute_WhenPresenterReturnsNone_ThrowsInvalidOperationException()
    {
        _presenter.Present(Arg.Any<BoundPromptRequest>())
            .Returns(PromptResponse.FromNonInteractiveSource(PromptResult.None, suppressChecked: false));

        Assert.Throws<InvalidOperationException>(() =>
            _orchestrator.Execute(new PromptRequest(SuppressibleId)));
    }

    // ── Request parameters are forwarded ─────────────────────────────────────

    [Test]
    public void Execute_RequestParameters_AreForwardedToBoundRequest()
    {
        var parameters = new object?[] { "RepoA", 7 };
        BoundPromptRequest? captured = null;
        _presenter.Present(Arg.Do<BoundPromptRequest>(r => captured = r))
            .Returns(PromptResponse.FromUserInteraction(PromptResult.No, suppressChecked: false));

        _orchestrator.Execute(new PromptRequest(SuppressibleId, parameters));

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Parameters, Is.EqualTo(parameters));
    }
}
