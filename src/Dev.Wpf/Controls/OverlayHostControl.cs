// OverlayHostControl.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services.Mode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dev.Wpf.Controls;

/// <summary>
/// Visual projection host for InteractionOverlay state from <see cref="IModeService"/>.
/// Renders only the topmost active overlay using standard WPF data template resolution.
/// </summary>
public sealed class OverlayHostControl : Control
{
    private bool _isRenderingSubscribed;
    private IInteractionOverlay? _lastProjectedOverlay;

    static OverlayHostControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OverlayHostControl),
            new FrameworkPropertyMetadata(typeof(OverlayHostControl)));
    }

    public OverlayHostControl()
    {
        Focusable = true;
        Loaded += (_, _) => AttachProjectionLoop();
        Unloaded += (_, _) => DetachProjectionLoop();
    }

    public static readonly DependencyProperty ModeServiceProperty =
        DependencyProperty.Register(
            nameof(ModeService),
            typeof(IModeService),
            typeof(OverlayHostControl),
            new FrameworkPropertyMetadata(null, OnModeServiceChanged));

    private static readonly DependencyPropertyKey ActiveOverlayPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActiveOverlay),
            typeof(IInteractionOverlay),
            typeof(OverlayHostControl),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ActiveOverlayProperty = ActiveOverlayPropertyKey.DependencyProperty;

    /// <summary>
    /// The mode service that owns active overlays.
    /// </summary>
    public IModeService? ModeService
    {
        get => (IModeService?)GetValue(ModeServiceProperty);
        set => SetValue(ModeServiceProperty, value);
    }

    /// <summary>
    /// The topmost active overlay, or <c>null</c> when no overlays are active.
    /// </summary>
    public IInteractionOverlay? ActiveOverlay => (IInteractionOverlay?)GetValue(ActiveOverlayProperty);

    private static void OnModeServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((OverlayHostControl)d).UpdateProjection();
    }

    private void AttachProjectionLoop()
    {
        if (_isRenderingSubscribed)
            return;

        CompositionTarget.Rendering += OnRendering;
        _isRenderingSubscribed = true;
        UpdateProjection();
    }

    private void DetachProjectionLoop()
    {
        if (!_isRenderingSubscribed)
            return;

        CompositionTarget.Rendering -= OnRendering;
        _isRenderingSubscribed = false;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        UpdateProjection();
    }

    private void UpdateProjection()
    {
        IInteractionOverlay? topOverlay = null;
        var overlays = ModeService?.ActiveOverlays;

        if (overlays is { Count: > 0 })
            topOverlay = overlays[^1];

        var overlayChanged = !ReferenceEquals(_lastProjectedOverlay, topOverlay);

        if (!ReferenceEquals(ActiveOverlay, topOverlay))
            SetValue(ActiveOverlayPropertyKey, topOverlay);

        if (overlayChanged && topOverlay is not null)
            Keyboard.Focus(this);

        _lastProjectedOverlay = topOverlay;
    }
}