// ToolbarSelectionItemModel.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using CommunityToolkit.Mvvm.ComponentModel;

namespace Dev.Core.ViewModels.Controls;

/// <summary>
/// Represents a selection-style toolbar entry rendered as a value selector.
/// </summary>
public sealed partial class ToolbarSelectionItemModel : ToolbarEntryModel
{
    private readonly Action<object?>? _onSelectionChanged;
    private bool _suppressChangeCallback;

    public ToolbarSelectionItemModel(
        string name,
        string label,
        IReadOnlyList<ToolbarSelectionOptionModel> options,
        object? initialValue,
        Action<object?>? onSelectionChanged = null,
        bool showLabel = true)
        : base(name, label)
    {
        Options = options;
        _onSelectionChanged = onSelectionChanged;
        selectedValue = initialValue;
        ShowLabel = showLabel;
    }

    public IReadOnlyList<ToolbarSelectionOptionModel> Options { get; }

    public bool ShowLabel { get; }

    [ObservableProperty]
    private object? selectedValue;

    partial void OnSelectedValueChanged(object? value)
    {
        if (_suppressChangeCallback)
        {
            return;
        }

        _onSelectionChanged?.Invoke(value);
    }

    public void SetSelectedValueWithoutNotifying(object? value)
    {
        _suppressChangeCallback = true;
        SelectedValue = value;
        _suppressChangeCallback = false;
    }
}