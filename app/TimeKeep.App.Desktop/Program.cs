using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Splat;
using TimeKeep.App.Desktop.Services;
using TimeKeep.App.Services;

namespace TimeKeep.App.Desktop;

internal class Program
{
	[STAThread]
	public static void Main(string[] args) =>
		BuildAvaloniaApp()
            .AfterSetup(Setup)
			.StartWithClassicDesktopLifetime(args);

	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

    private static void Setup(AppBuilder appBuilder)
    {
		Locator.CurrentMutable.RegisterLazySingleton<ISettingsService>(() => new SettingsService());
    }
}
