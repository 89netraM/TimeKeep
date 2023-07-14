using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Splat;
using TimeKeep.App.Android.Services;
using TimeKeep.App.Services;

namespace TimeKeep.App.Android;

[Activity(
	Label = "TimeKeep",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : AvaloniaMainActivity<App>
{
	protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
	{
		return base.CustomizeAppBuilder(builder)
            .AfterSetup(Setup)
			.WithInterFont()
			.UseReactiveUI();
    }

    private static void Setup(AppBuilder appBuilder)
    {
        Locator.CurrentMutable.RegisterLazySingleton<ISettingsService>(() => new SettingsService());
    }
}
