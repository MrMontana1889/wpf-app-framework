// ToolbarMenuBarProjection.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.ViewModels.Controls;

namespace Dev.Core.Services;

/// <summary>
/// Projects registered toolbar metadata into a shell menu bar model.
/// </summary>
public static class ToolbarMenuBarProjection
{
    public static IReadOnlyList<MenuBarMenuModel> Project(IReadOnlyList<ToolbarModel> toolbars)
    {
        ArgumentNullException.ThrowIfNull(toolbars);

        var menus = new List<MenuBarMenuModel>(toolbars.Count);

        foreach (var toolbar in toolbars)
        {
            var menuItems = ProjectToolbarItems(toolbar);
            menus.Add(new MenuBarMenuModel(toolbar.Name, menuItems));
        }

        return menus;
    }

    private static IReadOnlyList<MenuBarEntryModel> ProjectToolbarItems(ToolbarModel toolbar)
    {
        var eligibleItems = toolbar.Items
            .OfType<ToolbarItemModel>()
            .Where(item => item.IncludeInMenuBar)
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

            result.Add(new MenuBarCommandItemModel(item.Label, item.Command));

            previousGroup = item.LogicalGroup;
            hasProjectedItem = true;
        }

        return result;
    }
}
