using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Modals.Image;

public partial class ImageWindowViewModel : ModalViewModelBase
{
    [Reactive] public partial string? ImageUrl { get; set; }
    private readonly HttpClient _httpClient;

    public ImageWindowViewModel(string imageUrl)
    {
        ImageUrl = imageUrl;
        _httpClient = App.ServiceProvider!.GetRequiredService<HttpClient>();
    }

    [ReactiveCommand]
    private async Task DownloadImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        var downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "VOID");

        Directory.CreateDirectory(downloadsPath);

        var uri = new Uri(imageUrl);

        using var response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var extension = response.Content.Headers.ContentType?.MediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png"  => ".png",
            "image/webp" => ".webp",
            "image/gif"  => ".gif",
            "image/bmp"  => ".bmp",
            _ => ".jpg"
        };

        var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss_fff}{extension}";
        var filePath = Path.Combine(downloadsPath, fileName);

        await using var source =
            await response.Content.ReadAsStreamAsync();

        await using var destination =
            File.Create(filePath);

        await source.CopyToAsync(destination);
    }
}