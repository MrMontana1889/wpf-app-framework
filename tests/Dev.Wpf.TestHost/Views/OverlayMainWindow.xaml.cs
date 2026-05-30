// OverlayMainWindow.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Wpf.Views;

namespace Dev.Wpf.TestHost.Views;

/// <summary>
/// Validation harness window for end-to-end overlay interaction in a real UI host.
/// </summary>
public partial class OverlayMainWindow : BaseWindow
{
    public OverlayMainWindow(IWindowPersistenceService persistenceManager)
        : base(persistenceManager)
    {
        InitializeComponent();
    }
}
