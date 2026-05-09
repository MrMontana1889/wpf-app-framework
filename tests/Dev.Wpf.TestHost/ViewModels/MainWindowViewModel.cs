// MainWindowViewModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dev.Core.Tree;
using Dev.Wpf.Controls;
using Dev.Wpf.Themes;
using Dev.Wpf.TestHost.Samples;

namespace Dev.Wpf.TestHost.ViewModels;

/// <summary>
/// ViewModel for <c>MainWindow</c>. Owns all toggle state that controls which
/// <c>TreeViewControl</c> features are active, and exposes the sample data and
/// context menu provider.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        // Apply default light theme on startup
        // In BBApp.Next this would delegate to IThemeService via DI
        ThemeManager.ApplyTheme("Light");
    }

    // -----------------------------------------------------------------------
    // Sample data and providers (never change at runtime)
    // -----------------------------------------------------------------------

    /// <summary>Root nodes of the sample tree.</summary>
    public ObservableCollection<TreeNodeModel> RootNodes { get; } = 
        new ObservableCollection<TreeNodeModel>(SampleTreeBuilder.Build());

    /// <summary>Context menu provider wired to the tree.</summary>
    public SampleContextMenuProvider ContextMenuProvider { get; } = new();

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
        ThemeManager.ApplyTheme(theme);
    }

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
}
