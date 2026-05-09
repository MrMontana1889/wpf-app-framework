// MainWindow.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Services;
using Dev.Wpf.Views;
using System.Windows;

namespace Dev.Wpf.TestHost.Views;

/// <summary>
/// Interaction logic for MainWindow.
/// <para>
/// All TreeViewControl interaction is handled via MVVM:
/// <list type="bullet">
///   <item>Event routing via <see cref="Behaviors.EventToCommandBehavior"/></item>
///   <item>SelectedNodes binding via <see cref="Behaviors.SelectedNodesBindingBehavior"/></item>
///   <item>Theme changes via ViewModel property hooks</item>
/// </list>
/// </para>
/// <para>
/// This code-behind contains <strong>zero business logic</strong> — only the required
/// <see cref="InitializeComponent"/> call. All interaction logic resides in the ViewModel
/// and attached behaviors.
/// </para>
/// </summary>
public partial class MainWindow : BaseWindow
{
    public MainWindow(IWindowPersistenceService persistenceManager)
        : base(persistenceManager)
    {
        InitializeComponent();
    }
}
