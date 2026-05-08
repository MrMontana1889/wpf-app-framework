// MenuBarControl.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dev.Wpf.Controls;

/// <summary>
/// Renders a shell-level menu bar as toolbar-backed headers with popup content.
/// Replaces WPF Menu to avoid implicit background painting and system theme leakage.
/// 
/// Provides coordinated interaction behavior: single active menu, click-to-switch,
/// hover-to-switch, and close-on-outside/Esc.
/// </summary>
public sealed class MenuBarControl : Control
{
    public static readonly DependencyProperty MenusSourceProperty =
        DependencyProperty.Register(
            nameof(MenusSource),
            typeof(IReadOnlyList<MenuBarMenuModel>),
            typeof(MenuBarControl),
            new PropertyMetadata(null, OnMenusSourceChanged));

    private StackPanel? _headerPanel;
    private readonly Dictionary<Button, Popup> _popups = new();
    private readonly MenuInteractionCoordinator _coordinator = new();

    public IReadOnlyList<MenuBarMenuModel>? MenusSource
    {
        get => (IReadOnlyList<MenuBarMenuModel>?)GetValue(MenusSourceProperty);
        set => SetValue(MenusSourceProperty, value);
    }

    public MenuBarControl()
    {
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _headerPanel = GetTemplateChild("PART_HeaderPanel") as StackPanel;
        RebuildMenuHeaders();

        // Hook the popup to detect outside clicks
        Window? window = Window.GetWindow(this);
        if (window != null)
        {
            window.PreviewMouseDown += OnWindowPreviewMouseDown;
        }
    }

    private static void OnMenusSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        _ = e;
        ((MenuBarControl)d).RebuildMenuHeaders();
    }

    private void RebuildMenuHeaders()
    {
        if (_headerPanel is null)
        {
            return;
        }

        _headerPanel.Children.Clear();
        _popups.Clear();
        _coordinator.CloseAll();

        if (MenusSource is null)
        {
            return;
        }

        foreach (var menuModel in MenusSource)
        {
            var headerButton = new Button
            {
                Content = menuModel.Label,
                Style = TryFindResource("MenuBarHeaderButtonStyle") as Style,
            };

            headerButton.Click += (s, e) => OnHeaderButtonClicked(headerButton, menuModel);
            headerButton.MouseEnter += (s, e) => OnHeaderButtonMouseEnter(headerButton, menuModel);

            _headerPanel.Children.Add(headerButton);
        }
    }

    private void OnHeaderButtonClicked(Button headerButton, MenuBarMenuModel menuModel)
    {
        var popup = GetOrCreatePopupForMenu(headerButton, menuModel);

        if (_coordinator.IsMenuOpen && _coordinator.OpenButton == headerButton)
        {
            // Clicking the open menu closes it
            _coordinator.CloseAll();
        }
        else
        {
            // Clicking a different menu (or any menu when none is open) switches to it
            _coordinator.OpenMenu(headerButton, popup);
        }
    }

    private void OnHeaderButtonMouseEnter(Button headerButton, MenuBarMenuModel menuModel)
    {
        // Hover only switches menus if a menu is already open
        if (!_coordinator.IsMenuOpen)
        {
            return;
        }

        if (_coordinator.OpenButton == headerButton)
        {
            return; // Already open
        }

        var popup = GetOrCreatePopupForMenu(headerButton, menuModel);
        _coordinator.OpenMenu(headerButton, popup);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _coordinator.CloseAll();
            e.Handled = true;
        }
    }

    private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // If the click is outside this control and its popups, close all menus
        if (!IsClickWithinMenuBar(e.OriginalSource as DependencyObject))
        {
            _coordinator.CloseAll();
        }
    }

    private bool IsClickWithinMenuBar(DependencyObject? clickSource)
    {
        if (clickSource is null)
        {
            return false;
        }

        // Check if the click is within the menu bar control itself
        DependencyObject? current = clickSource;
        while (current != null)
        {
            if (current == this)
            {
                return true;
            }

            // Check if the click is within any open popup
            foreach (var popup in _popups.Values)
            {
                if (popup.IsOpen && popup.Child is DependencyObject popupContent)
                {
                    if (IsDescendantOf(current, popupContent))
                    {
                        return true;
                    }
                }
            }

            current = LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private static bool IsDescendantOf(DependencyObject? child, DependencyObject parent)
    {
        while (child != null)
        {
            if (child == parent)
            {
                return true;
            }

            child = LogicalTreeHelper.GetParent(child);
        }

        return false;
    }

    private Popup GetOrCreatePopupForMenu(Button headerButton, MenuBarMenuModel menuModel)
    {
        if (_popups.TryGetValue(headerButton, out var existingPopup))
        {
            return existingPopup;
        }

        // Create popup
        var popup = new Popup
        {
            Placement = PlacementMode.Bottom,
            PlacementTarget = headerButton,
            AllowsTransparency = true,
        };

        // Create popup border
        var border = new Border
        {
            Style = TryFindResource("MenuPopupBorderStyle") as Style,
        };

        // Create items panel
        var itemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        // Add menu items
        foreach (var entry in menuModel.Items)
        {
            if (entry is MenuBarCommandItemModel commandItem)
            {
                var itemButton = new Button
                {
                    Content = commandItem.Label,
                    Command = commandItem.Command,
                    Style = TryFindResource("MenuPopupItemStyle") as Style,
                };

                // Close the menu when a command is executed
                itemButton.Click += (s, e) => _coordinator.CloseAll();

                itemsPanel.Children.Add(itemButton);
            }
            else if (entry is MenuBarSeparatorItemModel)
            {
                var separator = new Separator
                {
                    Style = TryFindResource("MenuPopupSeparatorStyle") as Style,
                };
                itemsPanel.Children.Add(separator);
            }
        }

        border.Child = itemsPanel;
        popup.Child = border;

        // Close all menus when the popup is closed by other means (e.g., Esc)
        popup.Closed += (s, e) => _coordinator.CloseAll();

        _popups[headerButton] = popup;
        return popup;
    }

    /// <summary>
    /// Tracks which menu (if any) is currently open and coordinates transitions
    /// between menus.
    /// </summary>
    private sealed class MenuInteractionCoordinator
    {
        private Button? _openButton;
        private Popup? _openPopup;

        public Button? OpenButton => _openButton;

        public bool IsMenuOpen => _openButton != null;

        public void OpenMenu(Button headerButton, Popup popup)
        {
            // Close any existing menu first
            if (_openPopup != null)
            {
                _openPopup.IsOpen = false;
            }

            // Open the new menu
            _openButton = headerButton;
            _openPopup = popup;
            popup.IsOpen = true;
        }

        public void CloseAll()
        {
            if (_openPopup != null)
            {
                _openPopup.IsOpen = false;
            }

            _openButton = null;
            _openPopup = null;
        }
    }
}
