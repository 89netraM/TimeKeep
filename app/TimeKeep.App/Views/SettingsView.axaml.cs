using Avalonia.ReactiveUI;
using TimeKeep.App.ViewModels;

namespace TimeKeep.App.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        DataContext = ViewModel = new SettingsViewModel();

        InitializeComponent();
    }
}