// ToolbarItemSettings.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services;

/// <summary>
/// Serializable record of a single toolbar item's persisted visibility state.
/// </summary>
public class ToolbarItemSettings
{
    public string Name { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}
