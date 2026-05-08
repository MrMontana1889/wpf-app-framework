// ToolbarControl.xaml.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Dev.Wpf.Controls;

public partial class ToolbarControl : UserControl
{
    public static readonly DependencyProperty CanCustomizeProperty =
        DependencyProperty.Register(
            nameof(CanCustomize),
            typeof(bool),
            typeof(ToolbarControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AllToolbarsProperty =
        DependencyProperty.Register(
            nameof(AllToolbars),
            typeof(IReadOnlyList<ToolbarModel>),
            typeof(ToolbarControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MenuBarToggleEntryProperty =
        DependencyProperty.Register(
            nameof(MenuBarToggleEntry),
            typeof(MenuBarToggleMenuEntry),
            typeof(ToolbarControl),
            new PropertyMetadata(null));

    public ToolbarControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets whether the toolbar exposes a right-click context menu
    /// that allows the user to customize button visibility. Default is <c>true</c>.
    /// </summary>
    public bool CanCustomize
    {
        get => (bool)GetValue(CanCustomizeProperty);
        set => SetValue(CanCustomizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the full list of registered toolbars used to build the
    /// context menu. Typically bound to the main ViewModel's Toolbars property.
    /// </summary>
    public IReadOnlyList<ToolbarModel>? AllToolbars
    {
        get => (IReadOnlyList<ToolbarModel>?)GetValue(AllToolbarsProperty);
        set => SetValue(AllToolbarsProperty, value);
    }

    /// <summary>
    /// Gets or sets the menu bar toggle entry for the context menu.
    /// Typically bound to a menu bar toggle entry created by the main ViewModel.
    /// </summary>
    public MenuBarToggleMenuEntry? MenuBarToggleEntry
    {
        get => (MenuBarToggleMenuEntry?)GetValue(MenuBarToggleEntryProperty);
        set => SetValue(MenuBarToggleEntryProperty, value);
    }
}
