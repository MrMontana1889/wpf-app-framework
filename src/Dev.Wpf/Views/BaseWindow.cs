// BaseWindow.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Dev.Wpf.Views;

[ExcludeFromCodeCoverage]
public class BaseWindow : Window
{
    #region Constructor
    public BaseWindow(IWindowPersistenceService persistenceManager, string? windowKey = null)
    {
        _persistenceManager = persistenceManager ?? throw new ArgumentNullException(nameof(persistenceManager));
        _windowKey = windowKey ?? GetType().Name;

        // Automatically bind to theme brushes
        SetResourceReference(BackgroundProperty, "WindowBackgroundBrush");
        SetResourceReference(ForegroundProperty, "TextBrush");
        SetResourceReference(IconProperty, "AppIcon");

        Loaded += OnLoaded;
        Closing += OnClosing;
        PreviewKeyDown += OnPreviewKeyDown;
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Hook for derived windows to load custom persisted settings.
    /// </summary>
    /// <param name="state">WindowSettings record containing persisted values.</param>
    protected virtual void LoadSettings(Dev.Core.Services.WindowSettings? state)
    {
    }
    #endregion

    #region Protected Properties
    /// <summary>
    /// Controls whether persisted Width/Height are applied on load.
    /// Override to false in dialogs that should size to content.
    /// </summary>
    protected virtual bool ApplySize => true;

    /// <summary>
    /// Controls whether persisted Left/Top are applied on load.
    /// </summary>
    protected virtual bool ApplyPosition => true;

    /// <summary>
    /// Controls whether persisted WindowState is applied on load.
    /// </summary>
    protected virtual bool ApplyWindowState => true;

    /// <summary>
    /// Controls whether Width/Height are saved on close.
    /// Defaults to ApplySize; override if you want asymmetric behavior.
    /// </summary>
    protected virtual bool SaveSize => ApplySize;

    /// <summary>
    /// Controls whether Left/Top are saved on close.
    /// Defaults to ApplyPosition.
    /// </summary>
    protected virtual bool SavePosition => ApplyPosition;

    /// <summary>
    /// Controls whether WindowState is saved on close.
    /// Defaults to ApplyWindowState.
    /// </summary>
    protected virtual bool SaveWindowState => ApplyWindowState;
    #endregion

    #region Private Fields
    protected readonly IWindowPersistenceService _persistenceManager;
    private readonly string _windowKey;
    private bool _resetRequested;
    #endregion

    #region Event Handlers
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            _resetRequested = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_resetRequested)
        {
            _persistenceManager.ResetWindowState(_windowKey);
            return;
        }

        var state = _persistenceManager.LoadWindowState(_windowKey);

        if (state is null)
            return;

        // Apply persisted values conditionally
        if (ApplySize)
        {
            Width = state.Width;
            Height = state.Height;
        }

        if (ApplyPosition)
        {
            Left = state.Left;
            Top = state.Top;
        }

        if (ApplyWindowState && state.Extras?.TryGetValue("WindowState", out var windowStateValue) == true)
            WindowState = (System.Windows.WindowState)(int)windowStateValue;

        LoadSettings(state);
    }
    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Gather values with fallbacks to current or zero
        var width = SaveSize ? Width : 0;
        var height = SaveSize ? Height : 0;
        var left = SavePosition ? Left : 0;
        var top = SavePosition ? Top : 0;

        Dictionary<string, double>? extras = null;
        if (SaveWindowState)
        {
            extras = new Dictionary<string, double>
            {
                ["WindowState"] = (int)WindowState
            };
        }

        var state = new Dev.Core.Services.WindowSettings(width, height, left, top, extras);
        _persistenceManager.SaveWindowState(_windowKey, state);
    }
    #endregion
}
