using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VOID.APP.Models.Messages;
using VOID.Shared.Contracts.DTOs.Messages;
using ReactiveUI.SourceGenerators;
using VOID.APP.ViewModels.Base.ModalBase;

namespace VOID.APP.ViewModels.Modals.Video;

public partial class VideoWindowViewModel : ModalViewModelBase
{
    [Reactive] public partial string? VideoUrl { get; set; }
    private readonly HttpClient _client;

    public VideoWindowViewModel(
        HttpClient client, 
        Guid messageId)
    {
        _client = client;

        _ = GetUrlAsync(messageId);
    }

    private async Task GetUrlAsync(Guid messageId)
    {
        var dto = await _client.GetFromJsonAsync<VideoUrlDto>(
                    $"messages/{messageId}/media");
        VideoUrl = dto!.Url;
    }
}