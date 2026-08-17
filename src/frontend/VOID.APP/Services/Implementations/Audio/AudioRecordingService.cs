using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Structs;
using VOID.APP.Services.Interfaces.IAudio;

namespace VOID.APP.Services.Implementations.Audio;

public sealed class SoundFlowAudioRecordingService
    : IAudioRecordingService
{
    private readonly MiniAudioEngine _engine;

    private AudioCaptureDevice? _captureDevice;
    private Recorder? _recorder;

    private string? _currentFilePath;

    public bool IsRecording =>
        _recorder is not null;

    public SoundFlowAudioRecordingService()
    {
        _engine = new MiniAudioEngine();
    }

    public Task StartRecordingAsync(
        CancellationToken ct = default)
    {
        if (IsRecording)
            throw new InvalidOperationException(
                "Запись уже идёт.");

        ct.ThrowIfCancellationRequested();

        _engine.UpdateAudioDevicesInfo();

        var deviceInfo = _engine.CaptureDevices
            .FirstOrDefault(x => x.IsDefault);

        var format = new AudioFormat
        {
            SampleRate = 48000,
            Channels = 1,
            Format = SampleFormat.F32,
            Layout = ChannelLayout.Mono
        };

        _captureDevice =
            _engine.InitializeCaptureDevice(
                deviceInfo,
                format);

        var recordingsDirectory = Path.Combine(
            Path.GetTempPath(),
            "VOID",
            "Recordings");

        Directory.CreateDirectory(
            recordingsDirectory);

        _currentFilePath = Path.Combine(
            recordingsDirectory,
            $"{Guid.NewGuid()}.wav");

        _recorder = new Recorder(
            _captureDevice,
            _currentFilePath,
            "wav");

        _captureDevice.Start();

        var result =
            _recorder.StartRecording(null);

        if (!result.IsSuccess)
        {
            Cleanup();

            throw new InvalidOperationException(
                $"Не удалось начать запись: {result.Error}");
        }

        return Task.CompletedTask;
    }

    public async Task<string> StopRecordingAsync(
        CancellationToken ct = default)
    {
        if (_recorder is null ||
            _captureDevice is null ||
            _currentFilePath is null)
        {
            throw new InvalidOperationException(
                "Запись не запущена.");
        }

        ct.ThrowIfCancellationRequested();

        var recorder = _recorder;
        var captureDevice = _captureDevice;
        var filePath = _currentFilePath;

        try
        {
            var result =
                await recorder.StopRecordingAsync();

            captureDevice.Stop();

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Не удалось сохранить запись: {result.Error}");
            }

            if (!System.IO.File.Exists(filePath))
            {
                throw new InvalidOperationException(
                    "Файл записи не был создан.");
            }

            return filePath;
        }
        finally
        {
            _recorder = null;
            _captureDevice = null;
            _currentFilePath = null;

            recorder.Dispose();
            captureDevice.Dispose();
        }
    }

    private void Cleanup()
    {
        try
        {
            _captureDevice?.Stop();
        }
        catch
        {
            // ignore
        }

        _recorder?.Dispose();
        _captureDevice?.Dispose();

        _recorder = null;
        _captureDevice = null;
        _currentFilePath = null;
    }

    public void Dispose()
    {
        Cleanup();
        _engine.Dispose();
    }
}
