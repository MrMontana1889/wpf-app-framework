// ToolbarVisibilityChangedEventArgs.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Toolbar;

namespace Dev.Core.Services;

/// <summary>
/// Raised when a toolbar's visibility changes in the registry.
/// </summary>
public sealed class ToolbarVisibilityChangedEventArgs : EventArgs
{
    public ToolbarId ToolbarId { get; }
    public bool IsVisible { get; }

    public ToolbarVisibilityChangedEventArgs(ToolbarId toolbarId, bool isVisible)
    {
        ToolbarId = toolbarId;
        IsVisible = isVisible;
    }
}
