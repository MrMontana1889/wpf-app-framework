// ToolbarItemSettings.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

namespace Dev.Core.Services;

/// <summary>
/// Serializable record of a single toolbar item's persisted visibility state.
/// </summary>
public class ToolbarItemSettings
{
    public string Name { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}
