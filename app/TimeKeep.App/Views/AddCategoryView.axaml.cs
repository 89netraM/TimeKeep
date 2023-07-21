using System.Reactive.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using TimeKeep.App.Events;
using TimeKeep.App.ViewModels;

namespace TimeKeep.App.Views;

public partial class AddCategoryView : ReactiveUserControl<AddCategoryViewModel>
{
    public AddCategoryView()
    {
        DataContext = ViewModel = new AddCategoryViewModel();

        InitializeComponent();
    }

    private async void OnAddClick(object? sender, RoutedEventArgs e)
    {
        var result = await ViewModel!.AddCategoryCommand
            .Execute()
            .ToTask();

        if (result is AddCategorySuccess)
        {
            RaiseBackEvent();
        }
        else if (result is AddCategoryFailure { Message: var errorMessage })
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
