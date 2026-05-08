// ToolbarSelectionOptionModel.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a selectable option for a toolbar selection entry.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ToolbarSelectionOptionModel(string Label, object Value);