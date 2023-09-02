using System;
using System.Reactive.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using TimeKeep.App.Events;
using TimeKeep.App.ViewModels;

namespace TimeKeep.App.Views;

public partial class AddEntryView : ReactiveUserControl<AddEntryViewModel>
{
    public AddEntryView()
    {
        DataContext = ViewModel = new AddEntryViewModel();

        InitializeComponent();
    }

    private void OnAddCategoryClicked(object? sender, RoutedEventArgs e) =>
        AddCategory();

    private void OnAddCategoryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddCategory();
        }
    }

    private async void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(CategoriesBox.Text))
        {
            return;
        }

        await ViewModel!.AddCategoryCommand.Execute(CategoriesBox.Text).ToTask();
        CategoriesBox.Text = string.Empty;
    }

    private void OnEndTimeIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { IsChecked: true })
        {
            ViewModel!.EndTime = DateTimeOffset.Now;
        }
        else
        {
            ViewModel!.EndTime = null;
        }
    }

    private async void OnAddClick(object? sender, RoutedEventArgs e)
    {
        var result = await ViewModel!.AddEntryCommand
            .Execute()
            .ToTask();

        if (result is AddEntrySuccess)
        {
            RaiseBackEvent();
        }
        else if (result is AddEntryFailure { Message: var errorMessage })
        {
            errorFlyoutMessage = errorMessage;
            ErrorFlyout?.ShowAt(this);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) =>
        RaiseBackEvent();

    private void RaiseBackEvent() =>
        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = Navigation.BackEvent,
            Source = this,
        });

    private Flyout? ErrorFlyout =>
        Resources.TryGetValue(nameof(ErrorFlyout), out var resource) && resource is Flyout flyout ? flyout : null;

    private string? errorFlyoutMessage = null;

    private void OnErrorMessageBlockAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) =>
        (sender as TextBlock)!.Text = errorFlyoutMessage;

    private void OnErrorFyloutOK(object? sender, RoutedEventArgs e) =>
        ErrorFlyout?.Hide();
}
