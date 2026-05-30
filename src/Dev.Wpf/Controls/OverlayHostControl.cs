// OverlayHostControl.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services.Mode;
using System.Collections.Generic;
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
    private bool _isBackgroundIsolationApplied;
    private readonly List<UIElement> _disabledSiblingElements = new();

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

    private static readonly DependencyPropertyKey HasActiveOverlayPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasActiveOverlay),
            typeof(bool),
            typeof(OverlayHostControl),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasActiveOverlayProperty = HasActiveOverlayPropertyKey.DependencyProperty;

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

    /// <summary>
    /// Gets a value indicating whether an overlay is currently active.
    /// </summary>
    public bool HasActiveOverlay => (bool)GetValue(HasActiveOverlayProperty);

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
        RestoreBackgroundInteractivity();
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

        var hasActiveOverlay = topOverlay is not null;
        if (HasActiveOverlay != hasActiveOverlay)
            SetValue(HasActiveOverlayPropertyKey, hasActiveOverlay);

        if (hasActiveOverlay)
            ApplyBackgroundIsolation();
        else
            RestoreBackgroundInteractivity();

        if (overlayChanged && topOverlay is not null)
            Keyboard.Focus(this);

        _lastProjectedOverlay = topOverlay;
    }

    private void ApplyBackgroundIsolation()
    {
        if (_isBackgroundIsolationApplied)
            return;

        if (Parent is not Panel panel)
            return;

        _disabledSiblingElements.Clear();

        foreach (UIElement child in panel.Children)
        {
            if (ReferenceEquals(child, this))
                continue;

            if (!child.IsEnabled)
                continue;

            child.IsEnabled = false;
            _disabledSiblingElements.Add(child);
        }

        _isBackgroundIsolationApplied = true;
    }

    private void RestoreBackgroundInteractivity()
    {
        if (!_isBackgroundIsolationApplied && _disabledSiblingElements.Count == 0)
            return;

        foreach (var element in _disabledSiblingElements)
            element.IsEnabled = true;

        _disabledSiblingElements.Clear();
        _isBackgroundIsolationApplied = false;
    }
}