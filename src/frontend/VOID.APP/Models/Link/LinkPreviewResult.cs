using System;

namespace VOID.APP.Models.Link;

public class LinkPreviewResult
{
    public bool IsValid { get; init; }

    public bool IsImage { get; init; }

    public bool IsVideo { get; init; }

    public bool IsWebsite { get; init; }

    public string? Url { get; init; }

    public static LinkPreviewResult Invalid() => new();

    public static LinkPreviewResult Unknown() => new()
    {
        IsValid = true
    };

    public static LinkPreviewResult Image(string url) => new()
    {
        IsValid = true,
        IsImage = true,
        Url = url
    };

    public static LinkPreviewResult Video(string url) => new()
    {
        IsValid = true,
        IsVideo = true,
        Url = url
    };
    

    public static LinkPreviewResult Website(string url) => new()
    {
        IsValid = true,
        IsWebsite = true,
        Url = url
    };
}