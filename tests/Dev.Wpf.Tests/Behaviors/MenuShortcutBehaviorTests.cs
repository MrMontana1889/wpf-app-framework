// MenuShortcutBehaviorTests.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CoreMenuItem = Dev.Core.Menu.MenuItem;
using Dev.Core.Menu;
using Dev.Core.Toolbar;
using Dev.Wpf.Behaviors;
using Dev.Wpf.Controls;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev.Wpf.Tests.Behaviors;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class MenuShortcutBehaviorTests
{
    [Test]
    public void CreateKeyGesture_MapsSemanticShortcutToWpfGesture()
    {
        var gesture = MenuShortcutBehavior.CreateKeyGesture(
            new MenuShortcut(
                MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift,
                MenuShortcutKey.N));

        Assert.Multiple(() =>
        {
            Assert.That(gesture.Key, Is.EqualTo(Key.N));
            Assert.That(gesture.Modifiers, Is.EqualTo(ModifierKeys.Control | ModifierKeys.Shift));
        });
    }

    [Test]
    public void EnabledWindow_RegistersBindingsFromDiscoveredMenuHost()
    {
        var command = new RecordingCommand();
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.Open", "Open", command, MenuShortcutModifiers.Ctrl, MenuShortcutKey.O),
        };

        using var host = CreateWindowHost(items);
        var window = host.Window;

        var binding = window.InputBindings.OfType<KeyBinding>().Single();
        var gesture = (KeyGesture)binding.Gesture;

        Assert.Multiple(() =>
        {
            Assert.That(window.InputBindings, Has.Count.EqualTo(1));
            Assert.That(binding.Command, Is.SameAs(command));
            Assert.That(gesture.Key, Is.EqualTo(Key.O));
            Assert.That(gesture.Modifiers, Is.EqualTo(ModifierKeys.Control));
        });
    }

    [Test]
    public void RegisteredShortcut_InvokesOriginalCommandInstance()
    {
        var command = new RecordingCommand();
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.Save", "Save", command, MenuShortcutModifiers.Ctrl, MenuShortcutKey.S),
        };

        using var host = CreateWindowHost(items);
        var window = host.Window;

        InvokeRegisteredShortcut(window, Key.S, ModifierKeys.Control);

        Assert.That(command.ExecuteCount, Is.EqualTo(1));
    }

    [Test]
    public void CollectionChange_RefreshesRegisteredBindings()
    {
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.New", "New", new RecordingCommand(), MenuShortcutModifiers.Ctrl, MenuShortcutKey.N),
        };

        using var host = CreateWindowHost(items);
        var window = host.Window;

        items.Add(CreateCommandItem("File.Open", "Open", new RecordingCommand(), MenuShortcutModifiers.Ctrl, MenuShortcutKey.O));

        var gestures = window.InputBindings
            .OfType<KeyBinding>()
            .Select(binding => (KeyGesture)binding.Gesture)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(window.InputBindings, Has.Count.EqualTo(2));
            Assert.That(gestures.Any(static gesture => gesture.Key == Key.N && gesture.Modifiers == ModifierKeys.Control), Is.True);
            Assert.That(gestures.Any(static gesture => gesture.Key == Key.O && gesture.Modifiers == ModifierKeys.Control), Is.True);
        });
    }

    [Test]
    public void MultipleShortcuts_RegisterIndependently()
    {
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.New", "New", new RecordingCommand(), MenuShortcutModifiers.Ctrl, MenuShortcutKey.N),
            CreateCommandItem("File.Open", "Open", new RecordingCommand(), MenuShortcutModifiers.Ctrl | MenuShortcutModifiers.Shift, MenuShortcutKey.O),
        };

        using var host = CreateWindowHost(items);
        var window = host.Window;

        Assert.That(window.InputBindings.OfType<KeyBinding>(), Has.Exactly(2).Items);
    }

    [Test]
    public void DisablingBehavior_RemovesRegisteredBindings()
    {
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.Exit", "Exit", new RecordingCommand(), MenuShortcutModifiers.Alt, MenuShortcutKey.F4),
        };

        using var host = CreateWindowHost(items);
        var window = host.Window;

        MenuShortcutBehavior.SetIsEnabled(window, false);

        Assert.That(window.InputBindings, Is.Empty);
    }

    [Test]
    public void ClosingWindow_RemovesRegisteredBindings()
    {
        var items = new ObservableCollection<CoreMenuItem>
        {
            CreateCommandItem("File.Delete", "Delete", new RecordingCommand(), MenuShortcutModifiers.None, MenuShortcutKey.Delete),
        };

        var host = CreateWindowHost(items);
        var window = host.Window;

        window.Close();

        Assert.That(window.InputBindings, Is.Empty);
    }

    private static WindowHost CreateWindowHost(ObservableCollection<CoreMenuItem> items)
    {
        var menuHost = new MenuHostControl
        {
            ItemsSource = items,
        };

        var panel = new DockPanel();
        panel.Children.Add(menuHost);

        var window = new Window
        {
            Content = panel,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None,
            Width = 320,
            Height = 200,
            Visibility = Visibility.Hidden,
        };

        MenuShortcutBehavior.SetIsEnabled(window, true);
        window.Show();
        panel.ApplyTemplate();
        panel.UpdateLayout();

        return new WindowHost(window);
    }

    private static CoreMenuItem CreateCommandItem(
        string id,
        string label,
        ICommand command,
        MenuShortcutModifiers modifiers,
        MenuShortcutKey key)
    {
        return new CoreMenuItem(
            new MenuItemId(id),
            MenuItemKind.Command,
            new MenuItemSemanticMetadata(new ToolbarItemText(label)),
            shortcut: new MenuShortcut(modifiers, key),
            command: command);
    }

    private static void InvokeRegisteredShortcut(Window window, Key key, ModifierKeys modifiers)
    {
        var binding = window.InputBindings
            .OfType<KeyBinding>()
            .Single(candidate =>
            {
                var gesture = (KeyGesture)candidate.Gesture;
                return gesture.Key == key && gesture.Modifiers == modifiers;
            });

        Assert.That(binding.Command, Is.Not.Null);
        Assert.That(binding.Command!.CanExecute(binding.CommandParameter), Is.True);
        binding.Command.Execute(binding.CommandParameter);
    }

    private sealed class WindowHost : IDisposable
    {
        private bool _isClosed;

        public WindowHost(Window window)
        {
            Window = window;
            Window.Closed += OnWindowClosed;
        }

        public Window Window { get; }

        public void Dispose()
        {
            Window.Closed -= OnWindowClosed;

            if (!_isClosed)
                Window.Close();
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _isClosed = true;
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
        }
    }
}