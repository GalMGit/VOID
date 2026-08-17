using System;
using System.Threading;
using System.Threading.Tasks;

namespace VOID.APP.Services.Interfaces.IAudio;

public interface IAudioRecordingService : IDisposable
{
    bool IsRecording { get; } 
    Task StartRecordingAsync(CancellationToken ct = default); 
    Task<string> StopRecordingAsync(CancellationToken ct = default);
}