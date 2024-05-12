using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeKeep.App.Events;

public static class Navigation
{
    public static RoutedEvent<RoutedEventArgs> BackEvent =
        RoutedEvent.Register<RoutedEventArgs>("Back", RoutingStrategies.Bubble, typeof(Navigation));

    public static void AddBackEventHandler(Control element, EventHandler<RoutedEventArgs> handler)
    {
        element.AddHandler(BackEvent, handler);
    }

    public static RoutedEvent<NavigateEventArgs> NavigateEvent =
        RoutedEvent.Register<NavigateEventArgs>("Navigate", RoutingStrategies.Bubble, typeof(Navigation));

    public static void AddNavigationEventHandler(Control element, EventHandler<NavigateEventArgs> handler)
    {
        element.AddHandler(NavigateEvent, handler);
    }
}

public class NavigateEventArgs : RoutedEventArgs
{
    public required Type Destination { get; init; }
    public object?[]? Arguments { get; init; }
}
