using System;
using System.Text.Json.Serialization;

namespace VOID.APP.Models.Settings;

public class AppSettings
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "Dark";
}
