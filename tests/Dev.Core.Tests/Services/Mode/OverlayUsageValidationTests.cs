// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayUsageValidationTests.cs" company="MrMontana1889">
//   Copyright (c) 2026 MrMontana1889.  See LICENSE
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Dev.Core.Services.Mode;
using NUnit.Framework;

namespace Dev.Core.Tests.Services.Mode;

[TestFixture]
public class OverlayUsageValidationTests
{
    private ModeService service = null!;

    private sealed class TestInteractionOverlay : IInteractionOverlay<string>
    {
        private Action<string>? resultCallback;

        public int EnterCount { get; private set; }

        public int ExitCount { get; private set; }

        public int ResolveCount { get; private set; }

        public void OnEnter()
        {
            EnterCount++;
        }

        public void OnExit()
        {
            ExitCount++;
        }

        public void SetResultCallback(Action<string> callback)
        {
            resultCallback = callback;
        }

        public void Resolve(string result)
        {
            ResolveCount++;
            resultCallback?.Invoke(result);
        }
    }

    [SetUp]
    public void SetUp()
    {
        service = new ModeService();
    }

    [Test]
    public void ShowOverlayResolve_InvokesCallbackAndRemovesOverlay()
    {
        var overlay = new TestInteractionOverlay();
        string? received = null;

        service.ShowOverlay(overlay, result => received = result);
        overlay.Resolve("done");

        Assert.That(received, Is.EqualTo("done"));
        Assert.That(service.ActiveOverlays, Does.Not.Contain(overlay));
        Assert.That(overlay.ExitCount, Is.EqualTo(1));
    }

    [Test]
    public void MultipleOverlays_OnlyTopOverlayResolvesUntilPopped()
    {
        var bottomOverlay = new TestInteractionOverlay();
        var topOverlay = new TestInteractionOverlay();
        var callbacks = new List<string>();

        service.ShowOverlay(bottomOverlay, result => callbacks.Add($"bottom:{result}"));
        service.ShowOverlay(topOverlay, result => callbacks.Add($"top:{result}"));

        bottomOverlay.Resolve("ignored");
        topOverlay.Resolve("accepted");
        bottomOverlay.Resolve("accepted-after-pop");

        Assert.That(callbacks, Is.EqualTo(new[] { "top:accepted", "bottom:accepted-after-pop" }));
        Assert.That(service.ActiveOverlays, Is.Empty);
    }

    [Test]
    public void ResolveCalledMultipleTimes_InvokesCallbackOnlyOnce()
    {
        var overlay = new TestInteractionOverlay();
        var callbackCount = 0;

        service.ShowOverlay(overlay, _ => callbackCount++);

        overlay.Resolve("first");
        overlay.Resolve("second");

        Assert.That(callbackCount, Is.EqualTo(1));
        Assert.That(service.ActiveOverlays, Is.Empty);
    }

    [Test]
    public void CloseOverlay_DoesNotInvokeCallback()
    {
        var overlay = new TestInteractionOverlay();
        var callbackCount = 0;

        service.ShowOverlay(overlay, _ => callbackCount++);
        service.CloseOverlay(overlay);

        Assert.That(callbackCount, Is.EqualTo(0));
        Assert.That(service.ActiveOverlays, Is.Empty);
        Assert.That(overlay.ExitCount, Is.EqualTo(1));
    }
}