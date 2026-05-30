// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InteractionOverlayTests.cs" company="MrMontana1889">
//   Copyright (c) 2026 MrMontana1889.  See LICENSE
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Dev.Core.Services.Mode;
using NUnit.Framework;

namespace Dev.Core.Tests.Services.Mode;

[TestFixture]
public class InteractionOverlayTests
{
    private sealed class StubInteractionOverlay : IInteractionOverlay<string>
    {
        private Action<string>? resultCallback;

        public int EnterCount { get; private set; }

        public int ExitCount { get; private set; }

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

        public void Complete(string result)
        {
            resultCallback?.Invoke(result);
        }
    }

    [Test]
    public void StubInteractionOverlay_ImplementsBaseAndGenericContracts()
    {
        var overlay = new StubInteractionOverlay();

        Assert.That(overlay, Is.InstanceOf<IInteractionOverlay>());
        Assert.That(overlay, Is.InstanceOf<IInteractionOverlay<string>>());
    }

    [Test]
    public void SetResultCallback_InvokesRegisteredCallback_WithStronglyTypedResult()
    {
        var overlay = new StubInteractionOverlay();
        string? receivedResult = null;

        overlay.SetResultCallback(result => receivedResult = result);
        overlay.Complete("overlay-result");

        Assert.That(receivedResult, Is.EqualTo("overlay-result"));
    }

    [Test]
    public void OnEnterAndOnExit_IncrementLifecycleCounters()
    {
        var overlay = new StubInteractionOverlay();

        overlay.OnEnter();
        overlay.OnExit();

        Assert.That(overlay.EnterCount, Is.EqualTo(1));
        Assert.That(overlay.ExitCount, Is.EqualTo(1));
    }
}