// OverlayRootPanel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services.Mode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev.Wpf.Controls;

/// <summary>
/// Root container for interaction overlays that centralizes keyboard interaction behavior.
/// </summary>
public class OverlayRootPanel : ContentControl
{
    static OverlayRootPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OverlayRootPanel),
            new FrameworkPropertyMetadata(typeof(OverlayRootPanel)));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is IInteractionOverlay overlay)
        {
            if (overlay.CancelCommand?.CanExecute(null) == true)
            {
                overlay.CancelCommand.Execute(null);
                e.Handled = true;
            }
        }

        base.OnKeyDown(e);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Loaded += (_, _) => Focus();
    }
}
