using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using VOID.APP.Services.Interfaces.IAudio;

namespace VOID.APP.Services.Implementations.Audio;

public sealed class SoundFlowAudioPlaybackService
    : IAudioPlaybackService
{
    private readonly MiniAudioEngine _engine;
    private readonly HttpClient _httpClient;

    private AudioPlaybackDevice? _playbackDevice;
    private SoundPlayer? _player;
    private Stream? _audioStream;

    public bool IsPlaying =>
        _player is not null;

    public SoundFlowAudioPlaybackService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
        _engine = new MiniAudioEngine();
    }

    public async Task PlayAsync(
        string url,
        CancellationToken ct = default)
    {
        Stop();

        var response =
            await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

        response.EnsureSuccessStatusCode();

        var memoryStream = new MemoryStream();

        await response.Content.CopyToAsync(
            memoryStream,
            ct);

        memoryStream.Position = 0;

        _audioStream = memoryStream;

        _engine.UpdateAudioDevicesInfo();

        var deviceInfo =
            _engine.PlaybackDevices
                .FirstOrDefault(x => x.IsDefault);

        var format = AudioFormat.DvdHq;

        _playbackDevice =
            _engine.InitializePlaybackDevice(
                deviceInfo,
                format);

        var provider =
            new StreamDataProvider(
                _engine,
                format,
                _audioStream);

        _player =
            new SoundPlayer(
                _engine,
                format,
                provider);

        _playbackDevice.MasterMixer
            .AddComponent(_player);

        _playbackDevice.Start();

        _player.Play();
    }

    public void Stop()
    {
        try
        {
            _player?.Stop();
        }
        catch
        {
            // ignore
        }

        try
        {
            _playbackDevice?.Stop();
        }
        catch
        {
            // ignore
        }

        _player?.Dispose();
        _player = null;

        _playbackDevice?.Dispose();
        _playbackDevice = null;

        _audioStream?.Dispose();
        _audioStream = null;
    }

    public void Dispose()
    {
        Stop();
        _engine.Dispose();
    }
}
