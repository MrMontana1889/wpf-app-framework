// ToolbarMenuBarProjection.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;
using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services;

/// <summary>
/// Projects registered semantic toolbar definitions and items into a shell menu bar model.
/// </summary>
public static class ToolbarMenuBarProjection
{
    public static IReadOnlyList<MenuBarMenuModel> Project(
        IReadOnlyList<ToolbarDefinition> definitions,
        IEnumerable<ToolbarItem> items)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(items);

        var itemsById = items.ToDictionary(i => i.Id);
        var menus = new List<MenuBarMenuModel>(definitions.Count);

        foreach (var definition in definitions)
        {
            var menuItems = ProjectToolbarItems(definition, itemsById);
            menus.Add(new MenuBarMenuModel(definition.DisplayName, menuItems));
        }

        return menus;
    }

    private static IReadOnlyList<MenuBarEntryModel> ProjectToolbarItems(
        ToolbarDefinition definition,
        Dictionary<ToolbarItemId, ToolbarItem> itemsById)
    {
        var eligibleItems = definition.ItemIds
            .Where(itemId => itemsById.TryGetValue(itemId, out var item)
                && item.IncludeInMenuBar
                && item.Command is not null)
            .Select(itemId => itemsById[itemId])
            .ToList();

        var result = new List<MenuBarEntryModel>(eligibleItems.Count);
        string? previousGroup = null;
        var hasProjectedItem = false;

        foreach (var item in eligibleItems)
        {
            if (hasProjectedItem
                && !string.Equals(previousGroup, item.LogicalGroup, StringComparison.Ordinal)
                && !(previousGroup is null && item.LogicalGroup is null))
            {
                result.Add(new MenuBarSeparatorItemModel());
            }

            result.Add(new MenuBarCommandItemModel(item.SemanticMetadata.Text.Label, item.Command!));

            previousGroup = item.LogicalGroup;
            hasProjectedItem = true;
        }

        return result;
    }
}
