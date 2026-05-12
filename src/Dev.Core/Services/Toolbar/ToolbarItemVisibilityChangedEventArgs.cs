// ToolbarItemVisibilityChangedEventArgs.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;

namespace Dev.Core.Services;

/// <summary>
/// Raised when a toolbar item's visibility changes in the registry.
/// </summary>
public sealed class ToolbarItemVisibilityChangedEventArgs : EventArgs
{
    public ToolbarId ToolbarId { get; }
    public ToolbarItemId ItemId { get; }
    public bool IsVisible { get; }

    public ToolbarItemVisibilityChangedEventArgs(ToolbarId toolbarId, ToolbarItemId itemId, bool isVisible)
    {
        ToolbarId = toolbarId;
        ItemId = itemId;
        IsVisible = isVisible;
    }
}
