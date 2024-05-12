using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TimeKeep.App.Events;

namespace TimeKeep.App.Views;

public partial class MainView : UserControl
{
    private readonly Stack<(Type, object?[]?)> navigationStack = new([(typeof(EntriesView), null)]);

	public MainView()
	{
		InitializeComponent();
    }

    private void HamburgerMenu_Tapped(object? sender, TappedEventArgs e)
    {
        SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
    }

    private void OnNavigationRequested(object? sender, NavigateEventArgs e)
    {
        navigationStack.Push((e.Destination, e.Arguments));
        Frame.Content = Activator.CreateInstance(e.Destination, e.Arguments);
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (navigationStack.Count > 1)
        {
            navigationStack.Pop();
            var (dest, args) = navigationStack.Peek();
            Frame.Content = Activator.CreateInstance(dest, args);
        }
    }
}
