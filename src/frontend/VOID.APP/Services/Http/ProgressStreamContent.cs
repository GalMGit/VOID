using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.APP.Services.Http;

public sealed class ProgressStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly IProgress<long>? _progress;
    private readonly int _bufferSize;

    public ProgressStreamContent(
        Stream stream,
        IProgress<long>? progress = null,
        int bufferSize = 64 * 1024)
    {
        _stream = stream;
        _progress = progress;
        _bufferSize = bufferSize;

        Headers.ContentLength = stream.Length - stream.Position;
    }

    protected override async Task SerializeToStreamAsync(
        Stream target,
        TransportContext? context)
    {
        var buffer = new byte[_bufferSize];

        long uploaded = 0;

        while (true)
        {
            var read = await _stream.ReadAsync(
                buffer,
                0,
                buffer.Length);

            if (read == 0)
                break;

            await target.WriteAsync(
                buffer,
                0,
                read);

            uploaded += read;

            _progress?.Report(uploaded);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _stream.Length - _stream.Position;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();

        base.Dispose(disposing);
    }
}