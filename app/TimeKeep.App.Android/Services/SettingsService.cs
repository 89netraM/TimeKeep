using System.Threading.Tasks;
using TimeKeep.App.Services;
using Xamarin.Essentials;

namespace TimeKeep.App.Android.Services;

public class SettingsService : ISettingsService
{
    public ValueTask<string?> GetSettingAsync(string key) =>
        ValueTask.FromResult((string?)Preferences.Get(key, null));

    public ValueTask SaveSettingAsync(string key, string? value)
    {
        Preferences.Set(key, value);
        return ValueTask.CompletedTask;
    }
}
