// MenuShortcutBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Dev.Core.Menu;
using Dev.Wpf.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Window-level shortcut registration for semantic menu items.
/// </summary>
public static class MenuShortcutBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MenuShortcutBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(MenuShortcutBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

    private static readonly DependencyProperty RegistrationHostProperty =
        DependencyProperty.RegisterAttached(
            "RegistrationHost",
            typeof(WindowRegistrationHost),
            typeof(MenuShortcutBehavior),
            new PropertyMetadata(null));

    public static bool GetIsEnabled(Window element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(Window element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IsEnabledProperty, value);
    }

    public static IEnumerable? GetItemsSource(Window element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (IEnumerable?)element.GetValue(ItemsSourceProperty);
    }

    public static void SetItemsSource(Window element, IEnumerable? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(ItemsSourceProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
            return;

        if ((bool)e.NewValue)
        {
            var host = GetOrCreateRegistrationHost(window);
            host.Attach();
            host.RefreshBindings();
            return;
        }

        var existing = (WindowRegistrationHost?)window.GetValue(RegistrationHostProperty);
        existing?.Detach();
        window.ClearValue(RegistrationHostProperty);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
            return;

        var host = (WindowRegistrationHost?)window.GetValue(RegistrationHostProperty);
        host?.UpdateExplicitItemsSource((IEnumerable?)e.OldValue, (IEnumerable?)e.NewValue);
    }

    private static WindowRegistrationHost GetOrCreateRegistrationHost(Window window)
    {
        var existing = (WindowRegistrationHost?)window.GetValue(RegistrationHostProperty);
        if (existing is not null)
            return existing;

        var created = new WindowRegistrationHost(window);
        window.SetValue(RegistrationHostProperty, created);
        return created;
    }

    internal static KeyGesture CreateKeyGesture(MenuShortcut shortcut)
    {
        var modifiers = ModifierKeys.None;

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Ctrl))
            modifiers |= ModifierKeys.Control;

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Shift))
            modifiers |= ModifierKeys.Shift;

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Alt))
            modifiers |= ModifierKeys.Alt;

        if (shortcut.Modifiers.HasFlag(MenuShortcutModifiers.Meta))
            modifiers |= ModifierKeys.Windows;

        return new KeyGesture(MapKey(shortcut.Key), modifiers);
    }

    internal static Key MapKey(MenuShortcutKey key) => key switch
    {
        MenuShortcutKey.A => Key.A,
        MenuShortcutKey.B => Key.B,
        MenuShortcutKey.C => Key.C,
        MenuShortcutKey.D => Key.D,
        MenuShortcutKey.E => Key.E,
        MenuShortcutKey.F => Key.F,
        MenuShortcutKey.G => Key.G,
        MenuShortcutKey.H => Key.H,
        MenuShortcutKey.I => Key.I,
        MenuShortcutKey.J => Key.J,
        MenuShortcutKey.K => Key.K,
        MenuShortcutKey.L => Key.L,
        MenuShortcutKey.M => Key.M,
        MenuShortcutKey.N => Key.N,
        MenuShortcutKey.O => Key.O,
        MenuShortcutKey.P => Key.P,
        MenuShortcutKey.Q => Key.Q,
        MenuShortcutKey.R => Key.R,
        MenuShortcutKey.S => Key.S,
        MenuShortcutKey.T => Key.T,
        MenuShortcutKey.U => Key.U,
        MenuShortcutKey.V => Key.V,
        MenuShortcutKey.W => Key.W,
        MenuShortcutKey.X => Key.X,
        MenuShortcutKey.Y => Key.Y,
        MenuShortcutKey.Z => Key.Z,
        MenuShortcutKey.F1 => Key.F1,
        MenuShortcutKey.F2 => Key.F2,
        MenuShortcutKey.F3 => Key.F3,
        MenuShortcutKey.F4 => Key.F4,
        MenuShortcutKey.F5 => Key.F5,
        MenuShortcutKey.F6 => Key.F6,
        MenuShortcutKey.F7 => Key.F7,
        MenuShortcutKey.F8 => Key.F8,
        MenuShortcutKey.F9 => Key.F9,
        MenuShortcutKey.F10 => Key.F10,
        MenuShortcutKey.F11 => Key.F11,
        MenuShortcutKey.F12 => Key.F12,
        MenuShortcutKey.Enter => Key.Enter,
        MenuShortcutKey.Escape => Key.Escape,
        MenuShortcutKey.Delete => Key.Delete,
        MenuShortcutKey.Backspace => Key.Back,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unsupported menu shortcut key."),
    };

    private sealed class WindowRegistrationHost
    {
        private readonly Window _window;
        private readonly List<KeyBinding> _registeredBindings = [];
        private readonly List<MenuHostControl> _trackedMenuHosts = [];
        private readonly DependencyPropertyDescriptor _menuItemsSourceDescriptor;
        private INotifyCollectionChanged? _explicitItemsSourceNotifier;
        private bool _isAttached;

        public WindowRegistrationHost(Window window)
        {
            _window = window;
            _menuItemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
                MenuHostControl.ItemsSourceProperty,
                typeof(MenuHostControl));
        }

        public void Attach()
        {
            if (_isAttached)
                return;

            _window.Loaded += OnWindowLoaded;
            _window.Closed += OnWindowClosed;
            SubscribeToExplicitItemsSource(GetItemsSource(_window));
            _isAttached = true;
        }

        public void Detach()
        {
            if (!_isAttached)
                return;

            _window.Loaded -= OnWindowLoaded;
            _window.Closed -= OnWindowClosed;

            UnsubscribeFromExplicitItemsSource();
            UntrackMenuHosts();
            ClearRegisteredBindings();

            _isAttached = false;
        }

        public void UpdateExplicitItemsSource(IEnumerable? oldValue, IEnumerable? newValue)
        {
            UnsubscribeFromExplicitItemsSource();
            SubscribeToExplicitItemsSource(newValue);
            RefreshBindings();
        }

        public void RefreshBindings()
        {
            if (!_isAttached)
                return;

            TrackMenuHosts();
            ClearRegisteredBindings();

            foreach (var item in EnumerateShortcutItems())
            {
                if (item.Shortcut is null || item.Command is null)
                    continue;

                var binding = new KeyBinding(item.Command, CreateKeyGesture(item.Shortcut.Value));
                _registeredBindings.Add(binding);
                _window.InputBindings.Add(binding);
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            RefreshBindings();
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            Detach();
        }

        private IEnumerable<MenuItem> EnumerateShortcutItems()
        {
            var explicitItemsSource = GetItemsSource(_window);
            if (explicitItemsSource is not null)
            {
                foreach (var item in FlattenMenuItems(explicitItemsSource))
                    yield return item;

                yield break;
            }

            foreach (var host in _trackedMenuHosts)
            {
                if (host.ItemsSource is null)
                    continue;

                foreach (var item in FlattenMenuItems(host.ItemsSource))
                    yield return item;
            }
        }

        private static IEnumerable<MenuItem> FlattenMenuItems(IEnumerable itemsSource)
        {
            foreach (var item in itemsSource.OfType<MenuItem>())
            {
                yield return item;

                foreach (var child in FlattenChildren(item.Children))
                    yield return child;
            }
        }

        private static IEnumerable<MenuItem> FlattenChildren(IReadOnlyList<MenuItem> children)
        {
            foreach (var child in children)
            {
                yield return child;

                foreach (var nested in FlattenChildren(child.Children))
                    yield return nested;
            }
        }

        private void TrackMenuHosts()
        {
            UntrackMenuHosts();

            foreach (var host in EnumerateDescendants<MenuHostControl>(_window))
            {
                _trackedMenuHosts.Add(host);
                _menuItemsSourceDescriptor.AddValueChanged(host, OnMenuHostItemsSourceChanged);

                if (host.ItemsSource is INotifyCollectionChanged notifier)
                    notifier.CollectionChanged += OnMenuItemsCollectionChanged;
            }
        }

        private void UntrackMenuHosts()
        {
            foreach (var host in _trackedMenuHosts)
            {
                _menuItemsSourceDescriptor.RemoveValueChanged(host, OnMenuHostItemsSourceChanged);

                if (host.ItemsSource is INotifyCollectionChanged notifier)
                    notifier.CollectionChanged -= OnMenuItemsCollectionChanged;
            }

            _trackedMenuHosts.Clear();
        }

        private void OnMenuHostItemsSourceChanged(object? sender, EventArgs e)
        {
            RefreshBindings();
        }

        private void OnMenuItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshBindings();
        }

        private void SubscribeToExplicitItemsSource(IEnumerable? itemsSource)
        {
            if (itemsSource is INotifyCollectionChanged notifier)
            {
                _explicitItemsSourceNotifier = notifier;
                _explicitItemsSourceNotifier.CollectionChanged += OnMenuItemsCollectionChanged;
            }
        }

        private void UnsubscribeFromExplicitItemsSource()
        {
            if (_explicitItemsSourceNotifier is null)
                return;

            _explicitItemsSourceNotifier.CollectionChanged -= OnMenuItemsCollectionChanged;
            _explicitItemsSourceNotifier = null;
        }

        private void ClearRegisteredBindings()
        {
            foreach (var binding in _registeredBindings)
                _window.InputBindings.Remove(binding);

            _registeredBindings.Clear();
        }

        private static IEnumerable<T> EnumerateDescendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var index = 0; index < childCount; index++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
                if (child is T match)
                    yield return match;

                foreach (var descendant in EnumerateDescendants<T>(child))
                    yield return descendant;
            }
        }
    }
}