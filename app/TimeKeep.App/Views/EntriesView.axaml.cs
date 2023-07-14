using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using TimeKeep.App.Events;
using TimeKeep.App.ViewModels;

namespace TimeKeep.App.Views;

public partial class EntriesView : ReactiveUserControl<EntriesViewModel>
{
    public EntriesView()
    {
        DataContext = ViewModel = new EntriesViewModel();

        InitializeComponent();
    }

    private void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        ViewModel?.RefreshCommand.Execute()
            .DelaySubscription(DateTimeOffset.Now + TimeSpan.FromMilliseconds(500)) // Feels better
            .Subscribe(Observer.Create<Unit>(_ =>
                Dispatcher.UIThread.Invoke(() =>
                    deferral.Complete())));
    }

    private void OnActiveEntryDestroyed(object? sender, EntryDestroyedEventArgs e)
    {
        ViewModel?.DestroyActiveEntryCommand.Execute(e.Entry).Subscribe();
    }

    private void OnTodaysEntryDestroyed(object? sender, EntryDestroyedEventArgs e)
    {
        ViewModel?.DestroyTodaysEntryCommand.Execute(e.Entry).Subscribe();
    }

    private void OnEntryEnded(object? sender, EntryEndedEventArgs e)
    {
        ViewModel?.EndEntryCommand.Execute(e.Entry).Subscribe();
    }

    private void OnAddEntryButtonClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new NavigateEventArgs
        {
            Destination = typeof(AddEntryView),
            RoutedEvent = Navigation.NavigateEvent,
            Source = this,
        });
    }
}
