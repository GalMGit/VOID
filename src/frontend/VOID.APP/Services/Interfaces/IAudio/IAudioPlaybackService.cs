using System;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.APP.Services.Interfaces.IAudio;

public interface IAudioPlaybackService : IDisposable
{
    bool IsPlaying { get; }
    Task PlayAsync( string url, CancellationToken ct = default);
    void Stop();
}