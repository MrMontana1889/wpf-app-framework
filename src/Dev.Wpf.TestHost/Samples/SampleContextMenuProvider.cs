// SampleContextMenuProvider.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using CommunityToolkit.Mvvm.Input;
using Dev.Core.Tree;
using System.Windows;

namespace Dev.Wpf.TestHost.Samples;

/// <summary>
/// Sample <see cref="ITreeContextMenuProvider"/> for the test host.
/// Demonstrates how an application wires context menus into
/// <c>TreeViewControl.ContextMenuProvider</c>.
/// </summary>
public sealed class SampleContextMenuProvider : ITreeContextMenuProvider
{
    public IReadOnlyList<MenuItemModel> BuildMenu(IReadOnlyList<TreeNodeModel> selectedNodes)
    {
        var items = new List<MenuItemModel>
        {
            new("Expand All",   new RelayCommand(() => SetExpanded(selectedNodes, true)),
                IsEnabled: selectedNodes.Any(n => n.IsExpandable)),

            new("Collapse All", new RelayCommand(() => SetExpanded(selectedNodes, false)),
                IsEnabled: selectedNodes.Any(n => n.IsExpandable)),
        };

        items.Add(selectedNodes.Count == 1
            ? new MenuItemModel(
                $"Copy \"{selectedNodes[0].Label}\"",
                new RelayCommand(() => Clipboard.SetText(selectedNodes[0].Label)))
            : new MenuItemModel(
                $"Copy {selectedNodes.Count} labels",
                new RelayCommand(() =>
                {
                    var text = string.Join(Environment.NewLine,
                        selectedNodes.Select(n => n.Label));
                    Clipboard.SetText(text);
                })));

        return items;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static void SetExpanded(IReadOnlyList<TreeNodeModel> nodes, bool expanded)
    {
        foreach (var node in nodes)
            SetExpandedRecursive(node, expanded);
    }

    private static void SetExpandedRecursive(TreeNodeModel node, bool expanded)
    {
        if (node.IsExpandable)
            node.IsExpanded = expanded;

        foreach (var child in node.Children)
            SetExpandedRecursive(child, expanded);
    }
}
