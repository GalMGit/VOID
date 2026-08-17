using System;
using System.Threading.Tasks;
using VOID.APP.Models.Link;

namespace VOID.APP.Services.Interfaces.ILink;

public interface ILinkPreviewService
{
    Task<LinkPreviewResult> AnalyzeUrlAsync(string url);
}
