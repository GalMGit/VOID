using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VOID.APP.Models.Settings;
using VOID.APP.Services.Interfaces.ISettings;

namespace VOID.APP.Services.Implementations.Settings;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VOID");

        Directory.CreateDirectory(appDataPath);

        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (!System.IO.File.Exists(_settingsPath))
        {
            var settings = new AppSettings
            {
                Theme = "Dark"
            };

            await SaveSettingsAsync(settings);

            return settings;
        }

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(_settingsPath);

            return JsonSerializer.Deserialize<AppSettings>(json)
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await System.IO.File.WriteAllTextAsync(_settingsPath, json);
    }
}