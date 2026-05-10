// MainWindowViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Menu;
using Dev.Core.Services;
using Dev.Core.Toolbar;
using Dev.Core.Tree;
using Dev.Wpf.Controls;
using Dev.Wpf.TestHost.Samples;
using System.Collections.ObjectModel;

namespace Dev.Wpf.TestHost.ViewModels;

/// <summary>
/// ViewModel for <c>MainWindow</c>. Owns all toggle state that controls which
/// <c>TreeViewControl</c> features are active, and exposes the sample data and
/// context menu provider.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    public MainWindowViewModel(IThemeService themeService)
    {
        // Apply default light theme on startup
        // In BBApp.Next this would delegate to IThemeService via DI
        _themeService = themeService;
        IsDarkTheme = true;

        RebuildMenuItems();
        RebuildToolbarItems();
    }

    private IThemeService ThemeService => _themeService;

    // -----------------------------------------------------------------------
    // Sample data and providers (never change at runtime)
    // -----------------------------------------------------------------------

    /// <summary>Root nodes of the sample tree.</summary>
    public ObservableCollection<TreeNodeModel> RootNodes { get; } = 
        new ObservableCollection<TreeNodeModel>(SampleTreeBuilder.Build());

    /// <summary>Context menu provider wired to the tree.</summary>
    public SampleContextMenuProvider ContextMenuProvider { get; } = new();

    /// <summary>
    /// Semantic toolbar composition projected by <c>ToolbarHostControl</c>.
    /// </summary>
    public ObservableCollection<MenuItem> MainMenuItems { get; } = [];

    /// <summary>
    /// Semantic toolbar composition projected by <c>ToolbarHostControl</c>.
    /// </summary>
    public ObservableCollection<ToolbarItem> PrimaryToolbarItems { get; } = [];

    public ObservableCollection<ToolbarItem> SecondaryToolbarItems { get; } = [];

    /// <summary>
    /// Diagnostic combo-box choices for toolbar projection validation.
    /// </summary>
    public IReadOnlyList<object> ToolbarChoices { get; } = ["Choice 1", "Choice 2", "Choice 3"];

    // -----------------------------------------------------------------------
    // Feature toggles (bound to toolbar ToggleButtons)
    // -----------------------------------------------------------------------

    /// <summary>Shows or hides the tri-state checkbox column on each node.</summary>
    [ObservableProperty]
    private bool showCheckboxes;

    /// <summary>Enables or disables drag-and-drop reordering.</summary>
    [ObservableProperty]
    private bool canDragDrop;

    /// <summary>
    /// Current selection mode. Cycled through None → Single → Multiple by
    /// <see cref="ToggleSelectionModeCommand"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionModeLabel))]
    private TreeSelectionMode selectionMode = TreeSelectionMode.Single;

    [ObservableProperty]
    private bool toolbarOptionA;

    [ObservableProperty]
    private object? toolbarChoice = "Choice 1";

    /// <summary>
    /// Placeholder dark-theme flag. Phase D will replace this with a full
    /// <c>ThemedIconProvider</c> / ResourceDictionary swap.
    /// </summary>
    [ObservableProperty]
    private bool isDarkTheme;

    partial void OnIsDarkThemeChanged(bool value)
    {
        // Apply theme change via ThemeManager
        // In BBApp.Next this would delegate to IThemeService
        var theme = value ? "Dark" : "Light";
        ThemeService.ApplyTheme(theme);

        RebuildToolbarItems();
    }

    partial void OnShowCheckboxesChanged(bool value) => RebuildToolbarItems();

    partial void OnCanDragDropChanged(bool value) => RebuildToolbarItems();

    partial void OnSelectionModeChanged(TreeSelectionMode value) => RebuildToolbarItems();

    partial void OnToolbarOptionAChanged(bool value) => RebuildToolbarItems();

    partial void OnToolbarChoiceChanged(object? value) => RebuildToolbarItems();

    // -----------------------------------------------------------------------
    // Status (updated by MainWindow code-behind when SelectedNodes changes)
    // -----------------------------------------------------------------------

    /// <summary>Human-readable summary of the current selection, shown in the status bar.</summary>
    [ObservableProperty]
    private string statusText = "Ready — select a node to begin.";

    /// <summary>
    /// Mirror of <c>TreeViewControl.SelectedNodes</c>. Set by MainWindow
    /// code-behind via <c>DependencyPropertyDescriptor</c>.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<TreeNodeModel> selectedNodes = [];

    partial void OnSelectedNodesChanged(IReadOnlyList<TreeNodeModel> value)
    {
        StatusText = value.Count switch
        {
            0 => "No selection.",
            1 => $"Selected: {value[0].Label}",
            _ => $"{value.Count} nodes selected.",
        };
    }

    // -----------------------------------------------------------------------
    // Computed helpers
    // -----------------------------------------------------------------------

    /// <summary>Short label for the current selection mode, shown in the toolbar button.</summary>
    public string SelectionModeLabel => SelectionMode switch
    {
        TreeSelectionMode.None     => "Selection: None",
        TreeSelectionMode.Single   => "Selection: Single",
        TreeSelectionMode.Multiple => "Selection: Multiple",
        _                          => "Selection: Single",
    };

    // -----------------------------------------------------------------------
    // Commands
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cycles <see cref="SelectionMode"/> through Single → Multiple → None → Single.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectionMode()
    {
        SelectionMode = SelectionMode switch
        {
            TreeSelectionMode.Single   => TreeSelectionMode.Multiple,
            TreeSelectionMode.Multiple => TreeSelectionMode.None,
            _                          => TreeSelectionMode.Single,
        };
    }

    [RelayCommand]
    private void ToggleShowCheckboxes() => ShowCheckboxes = !ShowCheckboxes;

    [RelayCommand]
    private void ToggleCanDragDrop() => CanDragDrop = !CanDragDrop;

    [RelayCommand]
    private void ToggleDarkTheme() => IsDarkTheme = !IsDarkTheme;

    [RelayCommand]
    private void ToggleToolbarOptionA() => ToolbarOptionA = !ToolbarOptionA;

    [RelayCommand]
    private static void ExitMenu()
    {
    }

    [RelayCommand]
    private static void AboutMenu()
    {
    }

    /// <summary>
    /// Handles a drag-and-drop operation by moving nodes from their current
    /// location to the target location.
    /// <para>
    /// Called by <see cref="NodesDroppedCommand"/> when TreeViewControl.NodesDropped fires.
    /// </para>
    /// </summary>
    [RelayCommand]
    private void NodesDropped(NodesDroppedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        HandleNodesDrop(e.DroppedNodes, e.TargetNode, e.InsertionIndex);
    }

    // -----------------------------------------------------------------------
    // Drag & Drop handling
    // -----------------------------------------------------------------------

    /// <summary>
    /// Handles a drag-and-drop operation by moving nodes from their current
    /// location to the target location.
    /// </summary>
    public void HandleNodesDrop(IReadOnlyList<TreeNodeModel> droppedNodes, TreeNodeModel? targetNode, int insertionIndex)
    {
        // Remove nodes from their current parents
        foreach (var node in droppedNodes)
        {
            var parent = FindParent(node, RootNodes);
            if (parent is not null)
            {
                parent.Children.Remove(node);
            }
            else
            {
                // Node was at root level
                RootNodes.Remove(node);
            }
        }

        // Calculate safe insertion index after removal
        // (removal may have changed the collection size)
        int safeIndex;
        if (targetNode is not null)
        {
            safeIndex = Math.Min(insertionIndex, targetNode.Children.Count);
        }
        else
        {
            safeIndex = Math.Min(insertionIndex, RootNodes.Count);
        }

        // Add nodes to target
        if (targetNode is not null)
        {
            // Drop onto a node - add to its children
            foreach (var node in droppedNodes)
            {
                targetNode.Children.Insert(safeIndex++, node);
            }

            // Expand the target to show the new children
            if (!targetNode.IsExpanded)
                targetNode.IsExpanded = true;
        }
        else
        {
            // Drop onto root
            foreach (var node in droppedNodes)
            {
                RootNodes.Insert(safeIndex++, node);
            }
        }
    }

    /// <summary>
    /// Finds the parent of the given node in the tree.
    /// Returns null if the node is at root level.
    /// </summary>
    private static TreeNodeModel? FindParent(TreeNodeModel target, IEnumerable<TreeNodeModel> roots)
    {
        foreach (var root in roots)
        {
            if (root.Children.Contains(target))
                return root;

            var parent = FindParent(target, root.Children);
            if (parent is not null)
                return parent;
        }

        return null;
    }

    private void RebuildMenuItems()
    {
        var items = new[]
        {
            new MenuItem(
                new MenuItemId("TestHost.Menu.File"),
                MenuItemKind.Submenu,
                new MenuItemSemanticMetadata(new ToolbarItemText("_File")),
                children:
                [
                    new MenuItem(
                        new MenuItemId("TestHost.Menu.File.Exit"),
                        MenuItemKind.Command,
                        new MenuItemSemanticMetadata(new ToolbarItemText("Exit")),
                        command: ExitMenuCommand),
                ]),

            new MenuItem(
                new MenuItemId("TestHost.Menu.View"),
                MenuItemKind.Submenu,
                new MenuItemSemanticMetadata(new ToolbarItemText("_View")),
                children:
                [
                    new MenuItem(
                        new MenuItemId("TestHost.Menu.View.Dummy"),
                        MenuItemKind.Command,
                        new MenuItemSemanticMetadata(
                            new ToolbarItemText("Dummy Item", "Cycles the shared selection mode command.")),
                        shortcut: new MenuShortcut(MenuShortcutModifiers.Ctrl, MenuShortcutKey.D),
                        command: ToggleSelectionModeCommand),
                ]),

            new MenuItem(
                new MenuItemId("TestHost.Menu.Help"),
                MenuItemKind.Submenu,
                new MenuItemSemanticMetadata(new ToolbarItemText("_Help")),
                children:
                [
                    new MenuItem(
                        new MenuItemId("TestHost.Menu.Help.About"),
                        MenuItemKind.Command,
                        new MenuItemSemanticMetadata(new ToolbarItemText("About")),
                        command: AboutMenuCommand),
                ]),
        };

        MainMenuItems.Clear();
        foreach (var item in items)
            MainMenuItems.Add(item);
    }

    private void RebuildToolbarItems()
    {
        var items = new[]
        {
            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.SelectionLabel"),
                ToolbarItemKind.Label,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Selection:")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 10),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.SelectionCycle"),
                ToolbarItemKind.Button,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        SelectionModeLabel,
                        "Click to cycle: Single -> Multiple -> None -> Single")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 20,
                command: ToggleSelectionModeCommand),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.Separator1"),
                ToolbarItemKind.Separator,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Separator")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 30),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.ShowCheckboxes"),
                ToolbarItemKind.ToggleButton,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        "Checkboxes",
                        "Show or hide tri-state checkboxes on each node")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 40,
                command: ToggleShowCheckboxesCommand,
                isChecked: ShowCheckboxes),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.Separator2"),
                ToolbarItemKind.Separator,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Separator")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 50),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.DragDrop"),
                ToolbarItemKind.ToggleButton,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        "Drag & Drop",
                        "Enable or disable drag-and-drop reordering")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 60,
                command: ToggleCanDragDropCommand,
                isChecked: CanDragDrop),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.Separator3"),
                ToolbarItemKind.Separator,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Separator")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 70),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.DarkTheme"),
                ToolbarItemKind.ToggleButton,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        "Dark Theme",
                        "Placeholder dark-theme toggle")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 80,
                command: ToggleDarkThemeCommand,
                isChecked: IsDarkTheme),
        };

        PrimaryToolbarItems.Clear();
        foreach (var item in items)
            PrimaryToolbarItems.Add(item);

        var items2 = new[]
        {
            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.OptionA"),
                ToolbarItemKind.CheckBox,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        "Option A",
                        "Test CheckBox in ToolBar")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 100,
                command: ToggleToolbarOptionACommand,
                isChecked: ToolbarOptionA),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.Separator5"),
                ToolbarItemKind.Separator,
                new ToolbarItemSemanticMetadata(new ToolbarItemText("Separator")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 110),

            new ToolbarItem(
                new ToolbarItemId("TestHost.Toolbar.Choices"),
                ToolbarItemKind.ComboBox,
                new ToolbarItemSemanticMetadata(
                    new ToolbarItemText(
                        "Choices",
                        "Test ComboBox in ToolBar")),
                ToolbarItemDisplayIntent.TextOnly,
                order: 120,
                selectionItems: ToolbarChoices,
                selectedValue: ToolbarChoice),
        };

        SecondaryToolbarItems.Clear();
        foreach (var item in items2)
            SecondaryToolbarItems.Add(item);
    }
}
