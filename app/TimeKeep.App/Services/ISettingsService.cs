using System.Threading.Tasks;

namespace TimeKeep.App.Services;

public interface ISettingsService
{
    public ValueTask<string?> GetSettingAsync(string key);
    public ValueTask SaveSettingAsync(string key, string? value);
}
