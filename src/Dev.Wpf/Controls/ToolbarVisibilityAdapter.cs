// ToolbarVisibilityAdapter.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using Dev.Core.Services;
using Dev.Core.Toolbar;
using System.Windows;

namespace Dev.Wpf.Controls;

/// <summary>
/// Thin adapter that bridges Dev.Core toolbar semantic visibility state
/// into WPF attached properties for binding.
/// 
/// Usage: Set ToolbarVisibilityAdapter.ToolbarRegistry on a container element;
/// then use ToolbarVisibilityAdapter.GetVisibility(toolbarId) on descendant
/// elements to bind to registry-driven visibility.
/// </summary>
public static class ToolbarVisibilityAdapter
{
    private static readonly Dictionary<ToolbarId, bool> VisibilityCache = new();
    private static IToolbarRegistryService? _registry;

    public static readonly DependencyProperty ToolbarRegistryProperty =
        DependencyProperty.RegisterAttached(
            "ToolbarRegistry",
            typeof(IToolbarRegistryService),
            typeof(ToolbarVisibilityAdapter),
            new FrameworkPropertyMetadata(null, OnToolbarRegistryChanged));

    public static IToolbarRegistryService? GetToolbarRegistry(DependencyObject obj) =>
        (IToolbarRegistryService?)obj.GetValue(ToolbarRegistryProperty);

    public static void SetToolbarRegistry(DependencyObject obj, IToolbarRegistryService? value) =>
        obj.SetValue(ToolbarRegistryProperty, value);

    private static void OnToolbarRegistryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is IToolbarRegistryService oldRegistry)
            oldRegistry.VisibilityChanged -= OnRegistryVisibilityChanged;

        if (e.NewValue is IToolbarRegistryService newRegistry)
        {
            _registry = newRegistry;
            newRegistry.VisibilityChanged += OnRegistryVisibilityChanged;

            // Populate initial cache from definitions
            foreach (var definition in newRegistry.ToolbarDefinitions)
            {
                VisibilityCache[definition.Id] = newRegistry.IsVisible(definition.Id);
            }
        }
    }

    private static void OnRegistryVisibilityChanged(object? sender, ToolbarVisibilityChangedEventArgs e)
    {
        VisibilityCache[e.ToolbarId] = e.IsVisible;
    }

    /// <summary>
    /// Gets the current visibility state for a toolbar from the registry.
    /// Returns Visibility.Visible if visible, Visibility.Collapsed if not.
    /// </summary>
    public static Visibility GetVisibility(ToolbarId toolbarId)
    {
        if (VisibilityCache.TryGetValue(toolbarId, out var isVisible))
            return isVisible ? Visibility.Visible : Visibility.Collapsed;

        // Fallback: query registry directly if not cached
        if (_registry?.IsVisible(toolbarId) ?? false)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }
}
