// MenuItem.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Windows.Input;

namespace Dev.Core.Menu;

/// <summary>
/// UI-agnostic Core aggregate for a semantic menu item.
/// </summary>
public sealed class MenuItem
{
    public MenuItemId Id { get; }

    public MenuItemKind Kind { get; }

    public MenuItemSemanticMetadata SemanticMetadata { get; }

    public MenuShortcut? Shortcut { get; }

    public ICommand? Command { get; }

    public object? CommandParameter { get; }

    public Func<object?>? CommandParameterProvider { get; }

    public bool IsEnabled { get; }

    public bool IsVisible { get; }

    public bool? IsChecked { get; }

    public IReadOnlyList<MenuItem> Children { get; }

    public MenuItem(
        MenuItemId id,
        MenuItemKind kind,
        MenuItemSemanticMetadata semanticMetadata,
        MenuShortcut? shortcut = null,
        ICommand? command = null,
        object? commandParameter = null,
        Func<object?>? commandParameterProvider = null,
        bool isEnabled = true,
        bool isVisible = true,
        bool? isChecked = null,
        IReadOnlyList<MenuItem>? children = null)
    {
        if (id.Value is null)
            throw new ArgumentException("Menu item id must be initialized.", nameof(id));

        ArgumentNullException.ThrowIfNull(semanticMetadata);

        if (children?.Any(static child => child is null) == true)
            throw new ArgumentException("Menu children cannot contain null entries.", nameof(children));

        if (commandParameter is not null && commandParameterProvider is not null)
            throw new ArgumentException("CommandParameter and CommandParameterProvider must not both be non-null.", nameof(commandParameterProvider));

        ValidateKindConfiguration(kind, shortcut, command, isChecked, children);

        Id = id;
        Kind = kind;
        SemanticMetadata = semanticMetadata;
        Shortcut = shortcut;
        Command = command;
        CommandParameter = commandParameter;
        CommandParameterProvider = commandParameterProvider;
        IsEnabled = isEnabled;
        IsVisible = isVisible;
        IsChecked = isChecked;
        Children = children is null ? Array.Empty<MenuItem>() : children.ToArray();
    }

    private static void ValidateKindConfiguration(
        MenuItemKind kind,
        MenuShortcut? shortcut,
        ICommand? command,
        bool? isChecked,
        IReadOnlyList<MenuItem>? children)
    {
        var hasChildren = children is { Count: > 0 };

        switch (kind)
        {
            case MenuItemKind.Command:
                if (command is null)
                    throw new ArgumentException("Command menu items require a command.", nameof(command));

                if (isChecked is not null)
                    throw new ArgumentException("Command menu items do not support checked state.", nameof(isChecked));

                if (hasChildren)
                    throw new ArgumentException("Command menu items do not support children.", nameof(children));

                break;

            case MenuItemKind.Checkable:
                if (isChecked is null)
                    throw new ArgumentException("Checkable menu items require an initial checked state.", nameof(isChecked));

                if (hasChildren)
                    throw new ArgumentException("Checkable menu items do not support children.", nameof(children));

                break;

            case MenuItemKind.Separator:
                if (command is not null)
                    throw new ArgumentException("Separator menu items do not support command association.", nameof(command));

                if (shortcut is not null)
                    throw new ArgumentException("Separator menu items do not support shortcuts.", nameof(shortcut));

                if (isChecked is not null)
                    throw new ArgumentException("Separator menu items do not support checked state.", nameof(isChecked));

                if (hasChildren)
                    throw new ArgumentException("Separator menu items do not support children.", nameof(children));

                break;

            case MenuItemKind.Submenu:
                if (command is not null)
                    throw new ArgumentException("Submenu menu items do not support command association.", nameof(command));

                if (shortcut is not null)
                    throw new ArgumentException("Submenu menu items do not support shortcuts.", nameof(shortcut));

                if (isChecked is not null)
                    throw new ArgumentException("Submenu menu items do not support checked state.", nameof(isChecked));

                if (!hasChildren)
                    throw new ArgumentException("Submenu menu items require one or more children.", nameof(children));

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported menu item kind.");
        }
    }
}
