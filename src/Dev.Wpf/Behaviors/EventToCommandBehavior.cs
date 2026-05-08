// EventToCommandBehavior.cs
// Copyright (c) 2026 Bentley Systems, Incorporated. All Rights Reserved.

using Microsoft.Xaml.Behaviors;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Dev.Wpf.Behaviors;

/// <summary>
/// Routes a <see cref="RoutedEvent"/> on a <see cref="FrameworkElement"/>
/// to an <see cref="ICommand"/> in the element's <see cref="FrameworkElement.DataContext"/>.
/// <para>
/// This behavior enables MVVM-compliant event → command routing without code-behind.
/// The event's <see cref="RoutedEventArgs"/> is passed as the command parameter.
/// </para>
/// </summary>
/// <example>
/// <code><![CDATA[
/// <TreeViewControl ItemsSource="{Binding Nodes}">
///     <i:Interaction.Behaviors>
///         <behaviors:EventToCommandBehavior
///             EventName="NodesDropped"
///             Command="{Binding NodesDroppedCommand}" />
///     </i:Interaction.Behaviors>
/// </TreeViewControl>
/// ]]></code>
/// </example>
[ExcludeFromCodeCoverage]
public sealed class EventToCommandBehavior : Behavior<FrameworkElement>
{
    // -----------------------------------------------------------------------
    // EventName DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty EventNameProperty =
        DependencyProperty.Register(
            nameof(EventName),
            typeof(string),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null, OnEventNameChanged));

    /// <summary>
    /// The name of the routed event to observe (e.g., "NodesDropped").
    /// Must match a routed event defined on the associated element's type.
    /// </summary>
    public string? EventName
    {
        get => (string?)GetValue(EventNameProperty);
        set => SetValue(EventNameProperty, value);
    }

    // -----------------------------------------------------------------------
    // Command DP
    // -----------------------------------------------------------------------

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// The command to execute when the event fires.
    /// The event's <see cref="RoutedEventArgs"/> is passed as the command parameter.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // -----------------------------------------------------------------------
    // Event routing logic
    // -----------------------------------------------------------------------

    private Delegate? _eventHandler;

    protected override void OnAttached()
    {
        base.OnAttached();
        RegisterEvent();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        UnregisterEvent();
    }

    private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (EventToCommandBehavior)d;
        behavior.UnregisterEvent();
        behavior.RegisterEvent();
    }

    private void RegisterEvent()
    {
        if (AssociatedObject is null || string.IsNullOrWhiteSpace(EventName))
            return;

        var eventInfo = AssociatedObject.GetType().GetEvent(EventName);
        if (eventInfo is null)
            return;

        // Create a delegate that matches the event's handler type
        var handlerType = eventInfo.EventHandlerType;
        if (handlerType is null)
            return;

        var methodInfo = GetType().GetMethod(nameof(OnEvent), 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (methodInfo is null)
            return;

        _eventHandler = Delegate.CreateDelegate(handlerType, this, methodInfo);
        eventInfo.AddEventHandler(AssociatedObject, _eventHandler);
    }

    private void UnregisterEvent()
    {
        if (AssociatedObject is null || _eventHandler is null || string.IsNullOrWhiteSpace(EventName))
            return;

        var eventInfo = AssociatedObject.GetType().GetEvent(EventName);
        eventInfo?.RemoveEventHandler(AssociatedObject, _eventHandler);
        _eventHandler = null;
    }

    private void OnEvent(object sender, EventArgs e)
    {
        if (Command is null || !Command.CanExecute(e))
            return;

        Command.Execute(e);
    }
}
