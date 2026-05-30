// OverlayRootPanel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services.Mode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dev.Wpf.Controls;

/// <summary>
/// Root container for interaction overlays that centralizes keyboard interaction behavior.
/// </summary>
public class OverlayRootPanel : ContentControl
{
    public OverlayRootPanel()
    {
        IsVisibleChanged += OnPanelIsVisibleChanged;
    }

    static OverlayRootPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OverlayRootPanel),
            new FrameworkPropertyMetadata(typeof(OverlayRootPanel)));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is IInteractionOverlay overlay)
        {
            if (overlay.CancelCommand?.CanExecute(null) == true)
            {
                overlay.CancelCommand.Execute(null);
                e.Handled = true;
                return;
            }
        }

        base.OnPreviewKeyDown(e);
    }

    private void OnPanelIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (!MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
                    {
                        Focus();
                        Keyboard.Focus(this);
                    }
                }, DispatcherPriority.Input);
            }, DispatcherPriority.Loaded);
        }
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);

        if (e.NewFocus == this)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }, DispatcherPriority.Input);
        }
    }
}
