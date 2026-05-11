// ToolbarItem.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Windows.Input;

namespace Dev.Core.Toolbar;

/// <summary>
/// UI-agnostic Core model for a toolbar item.
/// Composes identity, kind, semantic metadata, display intent, and lightweight state.
/// </summary>
public sealed class ToolbarItem
{
    public ToolbarItemId Id { get; }

    public ToolbarItemKind Kind { get; }

    public ToolbarItemSemanticMetadata SemanticMetadata { get; }

    public ToolbarItemDisplayIntent DisplayIntent { get; }

    public ICommand? Command { get; }

    public int Order { get; }

    public IReadOnlyList<object>? SelectionItems { get; }

    public bool IsVisible { get; set; }

    public bool IsEnabled { get; set; }

    public bool? IsChecked { get; private set; }

    public object? SelectedValue { get; private set; }

    public ToolbarItem(
        ToolbarItemId id,
        ToolbarItemKind kind,
        ToolbarItemSemanticMetadata semanticMetadata,
        ToolbarItemDisplayIntent displayIntent,
        int order = 0,
        bool isVisible = true,
        bool isEnabled = true,
        ICommand? command = null,
        bool? isChecked = null,
        IReadOnlyList<object>? selectionItems = null,
        object? selectedValue = null)
    {
        if (id.Value is null)
            throw new ArgumentException("Toolbar item id must be initialized.", nameof(id));

        ArgumentNullException.ThrowIfNull(semanticMetadata);

        ValidateKindConfiguration(kind, command, isChecked, selectionItems, selectedValue);

        Id = id;
        Kind = kind;
        SemanticMetadata = semanticMetadata;
        DisplayIntent = displayIntent;
        Command = command;
        Order = order;
        IsVisible = isVisible;
        IsEnabled = isEnabled;
        IsChecked = isChecked;
        SelectedValue = selectedValue;

        SelectionItems = selectionItems is null
            ? null
            : selectionItems.ToArray();
    }

    public void SetChecked(bool isChecked)
    {
        if (Kind is not ToolbarItemKind.ToggleButton and not ToolbarItemKind.CheckBox)
            throw new InvalidOperationException("Checked state is only supported for ToggleButton and CheckBox items.");

        IsChecked = isChecked;
    }

    public void SetSelectedValue(object? selectedValue)
    {
        if (Kind != ToolbarItemKind.ComboBox)
            throw new InvalidOperationException("Selected value is only supported for ComboBox items.");

        if (selectedValue is not null && SelectionItems is not null && !SelectionItems.Contains(selectedValue))
            throw new ArgumentException("Selected value must exist in the combo box item source.", nameof(selectedValue));

        SelectedValue = selectedValue;

        Command?.Execute(selectedValue);
    }

    private static void ValidateKindConfiguration(
        ToolbarItemKind kind,
        ICommand? command,
        bool? isChecked,
        IReadOnlyList<object>? selectionItems,
        object? selectedValue)
    {
        switch (kind)
        {
            case ToolbarItemKind.Button:
                if (command is null)
                    throw new ArgumentException("Button items require a command.", nameof(command));

                if (isChecked is not null)
                    throw new ArgumentException("Button items do not support checked state.", nameof(isChecked));

                if (selectionItems is not null)
                    throw new ArgumentException("Button items do not support selection items.", nameof(selectionItems));

                if (selectedValue is not null)
                    throw new ArgumentException("Button items do not support selected values.", nameof(selectedValue));

                break;

            case ToolbarItemKind.ToggleButton:
                if (command is null)
                    throw new ArgumentException("ToggleButton items require a command.", nameof(command));

                if (isChecked is null)
                    throw new ArgumentException("ToggleButton items require an initial checked state.", nameof(isChecked));

                if (selectionItems is not null)
                    throw new ArgumentException("ToggleButton items do not support selection items.", nameof(selectionItems));

                if (selectedValue is not null)
                    throw new ArgumentException("ToggleButton items do not support selected values.", nameof(selectedValue));

                break;

            case ToolbarItemKind.CheckBox:
                if (isChecked is null)
                    throw new ArgumentException("CheckBox items require an initial checked state.", nameof(isChecked));

                if (selectionItems is not null)
                    throw new ArgumentException("CheckBox items do not support selection items.", nameof(selectionItems));

                if (selectedValue is not null)
                    throw new ArgumentException("CheckBox items do not support selected values.", nameof(selectedValue));

                break;

            case ToolbarItemKind.ComboBox:
                if (isChecked is not null)
                    throw new ArgumentException("ComboBox items do not support checked state.", nameof(isChecked));

                if (selectionItems is null)
                    throw new ArgumentException("ComboBox items require a selection item source.", nameof(selectionItems));

                if (selectedValue is not null && !selectionItems.Contains(selectedValue))
                    throw new ArgumentException("Selected value must exist in the combo box item source.", nameof(selectedValue));

                break;

            case ToolbarItemKind.Label:
            case ToolbarItemKind.Separator:
                if (command is not null)
                    throw new ArgumentException($"{kind} items do not support command association.", nameof(command));

                if (isChecked is not null)
                    throw new ArgumentException($"{kind} items do not support checked state.", nameof(isChecked));

                if (selectionItems is not null)
                    throw new ArgumentException($"{kind} items do not support selection items.", nameof(selectionItems));

                if (selectedValue is not null)
                    throw new ArgumentException($"{kind} items do not support selected values.", nameof(selectedValue));

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported toolbar item kind.");
        }
    }
}
