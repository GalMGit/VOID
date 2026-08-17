using System;
using System.Net.Http;
using System.Threading.Tasks;
using VOID.APP.Models.Link;
using VOID.APP.Services.Interfaces.ILink;

namespace VOID.APP.Services.Implementations.Link;

public class LinkPreviewService(HttpClient httpClient) : ILinkPreviewService
{
    public async Task<LinkPreviewResult> AnalyzeUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return LinkPreviewResult.Invalid();

        try
        {
            using var response = await httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead);

            var contentType = response.Content.Headers.ContentType?.MediaType;

            Console.WriteLine($"Content-Type: {contentType}");

            if (contentType == null)
                return LinkPreviewResult.Unknown();
            

            if (contentType.StartsWith("image/"))
                return LinkPreviewResult.Image(url);

            if (contentType.StartsWith("video/"))
                return LinkPreviewResult.Video(url);

            return LinkPreviewResult.Website(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return LinkPreviewResult.Unknown();
        }
    }
}
