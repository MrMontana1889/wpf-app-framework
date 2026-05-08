// ModeServiceTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Core.Services.Mode;
using Dev.Core.ViewModels.Controls;
using NSubstitute;
using NUnit.Framework;

namespace Dev.Core.Tests.Services.Mode;

[TestFixture]
public class ModeServiceTests
{
    private ModeService _service = null!;

    // -------------------------------------------------------------------------
    // Test doubles
    // -------------------------------------------------------------------------

    private sealed class StubToolbarModel : ToolbarModel
    {
        public StubToolbarModel() : base(Substitute.For<IDialogService>()) { }
        public override string Name => "StubToolbar";
    }

    private sealed class StubFeatureMode : IFeatureMode
    {
        public string ModeId { get; }
        public ToolbarModel? PrimaryToolbar { get; }
        public int EnterCount { get; private set; }
        public int ExitCount { get; private set; }
        public int ApplyCount { get; private set; }
        public int CancelCount { get; private set; }

        /// <summary>
        /// Controls what <see cref="TryApply"/> returns. Default is <c>true</c>.
        /// </summary>
        public bool ApplyResult { get; set; } = true;

        /// <summary>
        /// Set to <c>true</c> if Apply was called and returned <c>true</c>.
        /// Simulates committed state to verify transient state is only committed on success.
        /// </summary>
        public bool StateCommitted { get; private set; }

        public StubFeatureMode(string modeId, ToolbarModel? primaryToolbar = null)
        {
            ModeId = modeId;
            PrimaryToolbar = primaryToolbar;
        }

        public void OnEnter() => EnterCount++;
        public void OnExit() => ExitCount++;

        public Task<bool> TryApplyAsync()
        {
            ApplyCount++;
            if (ApplyResult)
                StateCommitted = true;
            return Task.FromResult(ApplyResult);
        }

        public void Cancel()
        {
            CancelCount++;
            // Transient state discarded; StateCommitted is intentionally NOT set.
        }
    }

    [SetUp]
    public void SetUp()
    {
        _service = new ModeService();
    }

    // =========================================================================
    // Baseline mode switching
    // =========================================================================

    [Test]
    public void ActiveBaselineMode_DefaultsToSimple()
    {
        Assert.That(_service.ActiveBaselineMode, Is.EqualTo(BaselineMode.Simple));
    }

    [Test]
    public void SetBaselineMode_ToAdvanced_ChangesActiveMode()
    {
        _service.SetBaselineMode(BaselineMode.Advanced);

        Assert.That(_service.ActiveBaselineMode, Is.EqualTo(BaselineMode.Advanced));
    }

    [Test]
    public void SetBaselineMode_BackToSimple_ChangesActiveMode()
    {
        _service.SetBaselineMode(BaselineMode.Advanced);
        _service.SetBaselineMode(BaselineMode.Simple);

        Assert.That(_service.ActiveBaselineMode, Is.EqualTo(BaselineMode.Simple));
    }

    [Test]
    public void SetBaselineMode_FiresBaselineModeChangedEvent()
    {
        BaselineModeChangedEventArgs? receivedArgs = null;
        _service.BaselineModeChanged += (_, e) => receivedArgs = e;

        _service.SetBaselineMode(BaselineMode.Advanced);

        Assert.That(receivedArgs, Is.Not.Null);
        Assert.That(receivedArgs!.Previous, Is.EqualTo(BaselineMode.Simple));
        Assert.That(receivedArgs.Current, Is.EqualTo(BaselineMode.Advanced));
    }

    [Test]
    public void SetBaselineMode_SameMode_DoesNotFireEvent()
    {
        var fired = false;
        _service.BaselineModeChanged += (_, _) => fired = true;

        _service.SetBaselineMode(BaselineMode.Simple);

        Assert.That(fired, Is.False);
    }

    [Test]
    public void SetBaselineMode_SameMode_DoesNotChangeActiveMode()
    {
        _service.SetBaselineMode(BaselineMode.Simple);

        Assert.That(_service.ActiveBaselineMode, Is.EqualTo(BaselineMode.Simple));
    }

    // =========================================================================
    // Feature mode entry
    // =========================================================================

    [Test]
    public void EnterFeatureMode_SetsActiveFeatureMode()
    {
        var mode = new StubFeatureMode("TestMode");

        _service.EnterFeatureMode(mode);

        Assert.That(_service.ActiveFeatureMode, Is.SameAs(mode));
    }

    [Test]
    public void EnterFeatureMode_SetsIsFeatureModeActiveTrue()
    {
        var mode = new StubFeatureMode("TestMode");

        _service.EnterFeatureMode(mode);

        Assert.That(_service.IsFeatureModeActive, Is.True);
    }

    [Test]
    public void EnterFeatureMode_CallsOnEnter()
    {
        var mode = new StubFeatureMode("TestMode");

        _service.EnterFeatureMode(mode);

        Assert.That(mode.EnterCount, Is.EqualTo(1));
    }

    [Test]
    public void EnterFeatureMode_FiresFeatureModeChangedEvent_WithIsEnteringTrue()
    {
        var mode = new StubFeatureMode("TestMode");
        FeatureModeChangedEventArgs? receivedArgs = null;
        _service.FeatureModeChanged += (_, e) => receivedArgs = e;

        _service.EnterFeatureMode(mode);

        Assert.That(receivedArgs, Is.Not.Null);
        Assert.That(receivedArgs!.Mode, Is.SameAs(mode));
        Assert.That(receivedArgs.IsEntering, Is.True);
    }

    // =========================================================================
    // Feature mode exit
    // =========================================================================

    [Test]
    public void ExitFeatureMode_ClearsActiveFeatureMode()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.ExitFeatureMode();

        Assert.That(_service.ActiveFeatureMode, Is.Null);
    }

    [Test]
    public void ExitFeatureMode_SetsIsFeatureModeActiveFalse()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.ExitFeatureMode();

        Assert.That(_service.IsFeatureModeActive, Is.False);
    }

    [Test]
    public void ExitFeatureMode_CallsOnExit()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.ExitFeatureMode();

        Assert.That(mode.ExitCount, Is.EqualTo(1));
    }

    [Test]
    public void ExitFeatureMode_FiresFeatureModeChangedEvent_WithIsEnteringFalse()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        FeatureModeChangedEventArgs? receivedArgs = null;
        _service.FeatureModeChanged += (_, e) => receivedArgs = e;

        _service.ExitFeatureMode();

        Assert.That(receivedArgs, Is.Not.Null);
        Assert.That(receivedArgs!.Mode, Is.SameAs(mode));
        Assert.That(receivedArgs.IsEntering, Is.False);
    }

    [Test]
    public void ExitFeatureMode_WhenNoModeActive_IsNoOp()
    {
        Assert.DoesNotThrow(() => _service.ExitFeatureMode());
    }

    [Test]
    public void ExitFeatureMode_WhenNoModeActive_DoesNotFireEvent()
    {
        var fired = false;
        _service.FeatureModeChanged += (_, _) => fired = true;

        _service.ExitFeatureMode();

        Assert.That(fired, Is.False);
    }

    // =========================================================================
    // Prevention of entering a second feature mode
    // =========================================================================

    [Test]
    public void EnterFeatureMode_WhenAlreadyActive_Throws()
    {
        var first = new StubFeatureMode("First");
        var second = new StubFeatureMode("Second");
        _service.EnterFeatureMode(first);

        Assert.Throws<InvalidOperationException>(() => _service.EnterFeatureMode(second));
    }

    [Test]
    public void EnterFeatureMode_WhenAlreadyActive_DoesNotReplaceActiveMode()
    {
        var first = new StubFeatureMode("First");
        var second = new StubFeatureMode("Second");
        _service.EnterFeatureMode(first);

        try { _service.EnterFeatureMode(second); } catch (InvalidOperationException) { }

        Assert.That(_service.ActiveFeatureMode, Is.SameAs(first));
    }

    [Test]
    public void EnterFeatureMode_AfterExit_Succeeds()
    {
        var first = new StubFeatureMode("First");
        var second = new StubFeatureMode("Second");
        _service.EnterFeatureMode(first);
        _service.ExitFeatureMode();

        Assert.DoesNotThrow(() => _service.EnterFeatureMode(second));
        Assert.That(_service.ActiveFeatureMode, Is.SameAs(second));
    }

    // =========================================================================
    // Toolbar visibility changes driven by mode state
    // =========================================================================

    [Test]
    public void EnterFeatureMode_WithPrimaryToolbar_MakesToolbarVisible()
    {
        var toolbar = new StubToolbarModel();
        toolbar.IsToolbarVisible = false;
        var mode = new StubFeatureMode("TestMode", toolbar);

        _service.EnterFeatureMode(mode);

        Assert.That(toolbar.IsToolbarVisible, Is.True);
    }

    [Test]
    public void ExitFeatureMode_WithPrimaryToolbar_HidesToolbar()
    {
        var toolbar = new StubToolbarModel();
        var mode = new StubFeatureMode("TestMode", toolbar);
        _service.EnterFeatureMode(mode);

        _service.ExitFeatureMode();

        Assert.That(toolbar.IsToolbarVisible, Is.False);
    }

    [Test]
    public void EnterFeatureMode_WithNoPrimaryToolbar_DoesNotThrow()
    {
        var mode = new StubFeatureMode("TestMode", primaryToolbar: null);

        Assert.DoesNotThrow(() => _service.EnterFeatureMode(mode));
    }

    // =========================================================================
    // Output visibility assertion while a feature mode is active
    // =========================================================================

    [Test]
    public void IsFeatureModeActive_IsTrueWhileFeatureModeActive_EnforcesOutputVisibility()
    {
        // IsFeatureModeActive is the shell signal used to keep output visible.
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        Assert.That(_service.IsFeatureModeActive, Is.True,
            "Shell must keep output visible whenever IsFeatureModeActive is true.");
    }

    [Test]
    public void IsFeatureModeActive_FalseAfterExit_OutputVisibilityConstraintLifted()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);
        _service.ExitFeatureMode();

        Assert.That(_service.IsFeatureModeActive, Is.False);
    }

    // =========================================================================
    // Shutdown blocking when a feature mode is active
    // =========================================================================

    [Test]
    public void IsShutdownBlocked_FalseByDefault()
    {
        Assert.That(_service.IsShutdownBlocked, Is.False);
    }

    [Test]
    public void IsShutdownBlocked_TrueWhileFeatureModeActive()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        Assert.That(_service.IsShutdownBlocked, Is.True);
    }

    // =========================================================================
    // Restoration of shutdown capability after feature mode exit
    // =========================================================================

    [Test]
    public void IsShutdownBlocked_FalseAfterFeatureModeExits()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.ExitFeatureMode();

        Assert.That(_service.IsShutdownBlocked, Is.False);
    }

    [Test]
    public void IsShutdownBlocked_RestoredAfterReenter_ThenExit()
    {
        var first = new StubFeatureMode("First");
        var second = new StubFeatureMode("Second");

        _service.EnterFeatureMode(first);
        _service.ExitFeatureMode();
        _service.EnterFeatureMode(second);
        _service.ExitFeatureMode();

        Assert.That(_service.IsShutdownBlocked, Is.False);
    }

    // =========================================================================
    // Phase U2 - Apply: feature modes can request Apply intent
    // =========================================================================

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_CallsTryApplyOnMode()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(mode.ApplyCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_WhenNoModeActive_ReturnsFalse()
    {
        var result = await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_WhenNoModeActive_IsNoOp()
    {
        var fired = false;
        _service.FeatureModeChanged += (_, _) => fired = true;

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(fired, Is.False);
    }

    // =========================================================================
    // Phase U2 - Apply failure: mode remains active
    // =========================================================================

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_ReturnsFalse()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        var result = await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_ModeRemainsActive()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(_service.ActiveFeatureMode, Is.SameAs(mode));
        Assert.That(_service.IsFeatureModeActive, Is.True);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_DoesNotCallOnExit()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(mode.ExitCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_DoesNotFireFeatureModeChangedEvent()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        var fired = false;
        _service.FeatureModeChanged += (_, _) => fired = true;

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(fired, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_ShutdownRemainsBlocked()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(_service.IsShutdownBlocked, Is.True);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnFailure_TransientStateNotCommitted()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = false };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(mode.StateCommitted, Is.False);
    }

    // =========================================================================
    // Phase U2 - Apply success: mode exits cleanly
    // =========================================================================

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_ReturnsTrue()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        var result = await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_ClearsActiveMode()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(_service.ActiveFeatureMode, Is.Null);
        Assert.That(_service.IsFeatureModeActive, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_CallsOnExit()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(mode.ExitCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_FiresFeatureModeChangedEvent()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        FeatureModeChangedEventArgs? receivedArgs = null;
        _service.FeatureModeChanged += (_, e) => receivedArgs = e;

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(receivedArgs, Is.Not.Null);
        Assert.That(receivedArgs!.Mode, Is.SameAs(mode));
        Assert.That(receivedArgs.IsEntering, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_RestoresShutdown()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(_service.IsShutdownBlocked, Is.False);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_StateCommitted()
    {
        var mode = new StubFeatureMode("TestMode") { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(mode.StateCommitted, Is.True);
    }

    [Test]
    public async Task TryApplyAndExitFeatureModeAsync_OnSuccess_HidesPrimaryToolbar()
    {
        var toolbar = new StubToolbarModel();
        var mode = new StubFeatureMode("TestMode", toolbar) { ApplyResult = true };
        _service.EnterFeatureMode(mode);

        await _service.TryApplyAndExitFeatureModeAsync();

        Assert.That(toolbar.IsToolbarVisible, Is.False);
    }

    // =========================================================================
    // Phase U2 - Cancel: always exits unconditionally
    // =========================================================================

    [Test]
    public void CancelAndExitFeatureMode_CallsCancelOnMode()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(mode.CancelCount, Is.EqualTo(1));
    }

    [Test]
    public void CancelAndExitFeatureMode_ClearsActiveMode()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(_service.ActiveFeatureMode, Is.Null);
        Assert.That(_service.IsFeatureModeActive, Is.False);
    }

    [Test]
    public void CancelAndExitFeatureMode_CallsOnExit()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(mode.ExitCount, Is.EqualTo(1));
    }

    [Test]
    public void CancelAndExitFeatureMode_FiresFeatureModeChangedEvent()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        FeatureModeChangedEventArgs? receivedArgs = null;
        _service.FeatureModeChanged += (_, e) => receivedArgs = e;

        _service.CancelAndExitFeatureMode();

        Assert.That(receivedArgs, Is.Not.Null);
        Assert.That(receivedArgs!.Mode, Is.SameAs(mode));
        Assert.That(receivedArgs.IsEntering, Is.False);
    }

    [Test]
    public void CancelAndExitFeatureMode_RestoresShutdown()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(_service.IsShutdownBlocked, Is.False);
    }

    [Test]
    public void CancelAndExitFeatureMode_DoesNotCommitState()
    {
        var mode = new StubFeatureMode("TestMode");
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(mode.StateCommitted, Is.False);
    }

    [Test]
    public void CancelAndExitFeatureMode_HidesPrimaryToolbar()
    {
        var toolbar = new StubToolbarModel();
        var mode = new StubFeatureMode("TestMode", toolbar);
        _service.EnterFeatureMode(mode);

        _service.CancelAndExitFeatureMode();

        Assert.That(toolbar.IsToolbarVisible, Is.False);
    }

    [Test]
    public void CancelAndExitFeatureMode_WhenNoModeActive_IsNoOp()
    {
        var fired = false;
        _service.FeatureModeChanged += (_, _) => fired = true;

        Assert.DoesNotThrow(() => _service.CancelAndExitFeatureMode());
        Assert.That(fired, Is.False);
    }
}
