using System;
using System.Threading.Tasks;
using VOID.APP.Models.Settings;

namespace VOID.APP.Services.Interfaces.ISettings;

public interface ISettingsService
{
    Task<AppSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
