using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TimeKeep.App.Services;

namespace TimeKeep.App.Desktop.Services;

public class SettingsService : ISettingsService
{
    public async ValueTask<string?> GetSettingAsync(string key)
    {
        var settings = await GetSettingsFile();
        return settings.TryGetValue(key, out var value) ? value : null;
    }

    public async ValueTask SaveSettingAsync(string key, string? value)
    {
        var settings = await GetSettingsFile();
        if (value is null)
        {
            settings.Remove(key);
        }
        else
        {
            settings[key] = value;
        }

        await SaveSettingsFile(settings);
    }

    private async Task<IDictionary<string, string>> GetSettingsFile()
    {
        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            return await JsonSerializer.DeserializeAsync<IDictionary<string, string>>(stream) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private async Task SaveSettingsFile(IDictionary<string, string> settings)
    {
        await using var stream = File.Create(SettingsPath);
        await JsonSerializer.SerializeAsync(stream, settings);
    }

    private string SettingsPath =>
        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "settings.json");
}
