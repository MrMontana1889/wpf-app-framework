// ToolbarSelectionOptionModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Diagnostics.CodeAnalysis;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a selectable option for a toolbar selection entry.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ToolbarSelectionOptionModel(string Label, object Value);