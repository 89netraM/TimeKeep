using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TimeKeep.App.Events;

namespace TimeKeep.App.Views;

public partial class MainView : UserControl
{
    private readonly Stack<Type> navigationStack = new();

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
        navigationStack.Push(Frame.Content!.GetType());
        Frame.Content = Activator.CreateInstance(e.Destination);
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (navigationStack.TryPop(out var dest))
        {
            Frame.Content = Activator.CreateInstance(dest);
        }
    }
}
