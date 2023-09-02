using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using TimeKeep.App.Services;

namespace TimeKeep.App.ViewModels;

public class SettingsViewModel : ViewModelBase, IActivatableViewModel
{
    [Reactive]
    public bool IsLoading { get; set; } = true;

    [Reactive]
    public string Server { get; set; } = string.Empty;
    [Reactive]
    public string Token { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ViewModelActivator Activator { get; }

    public SettingsViewModel()
    {
        Activator = new ViewModelActivator();
        this.WhenActivated(OnActivation);

        SaveCommand = ReactiveCommand.CreateFromTask(Save, this.ObservableForProperty(s => s.IsLoading, b => !b));
    }

    private void OnActivation(CompositeDisposable disposables)
    {
        var cts = new CancellationTokenSource();
        Disposable.Create(cts.Cancel).DisposeWith(disposables);
        cts.DisposeWith(disposables);
        LoadSettings(cts.Token).DisposeWith(disposables);
    }

    private async Task LoadSettings(CancellationToken ct)
    {
        var settingsService = Locator.Current.GetService<ISettingsService>();
        if (settingsService is null)
        {
            return;
        }
        
        Server = await settingsService.GetSettingAsync(nameof(Server)) ?? string.Empty;
        Token = await settingsService.GetSettingAsync(nameof(Token)) ?? string.Empty;
        IsLoading = false;
    }

    private async Task Save(CancellationToken ct)
    {
        var settingsService = Locator.Current.GetService<ISettingsService>();
        if (settingsService is null)
        {
            return;
        }

        IsLoading = true;

        await settingsService.SaveSettingAsync(nameof(Server), Server);
        await settingsService.SaveSettingAsync(nameof(Token), Token);

        IsLoading = false;
    }
}
