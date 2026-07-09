using LibVLCSharp.Shared;
using OpenFrame.Player.Core.Enums;
using OpenFrame.Player.Core.Interfaces;
using VLC = LibVLCSharp.Shared.Core;

namespace OpenFrame.Player.Infrastructure.Players;

public sealed class VlcPlayerService : IPlayerService
{
    public LibVLC LibVLC { get; }

    public MediaPlayer MediaPlayer { get; }

    public bool IsPlaying => MediaPlayer.IsPlaying;

    public PlaybackState State { get; private set; } = PlaybackState.Stopped;

    public long PositionMilliseconds => MediaPlayer.Time;

    public long DurationMilliseconds => MediaPlayer.Length;

    public int Volume => MediaPlayer.Volume;

    public string? CurrentMediaPath { get; private set; }

    public VlcPlayerService()
    {
        VLC.Initialize();

        LibVLC = new LibVLC();
        MediaPlayer = new MediaPlayer(LibVLC);
    }

    public void Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Choose a media file before trying to play it.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The selected media file could not be found.", path);
        }

        using var media = new Media(LibVLC, path);

        if (!MediaPlayer.Play(media))
        {
            throw new InvalidOperationException("The selected media file could not be opened.");
        }

        CurrentMediaPath = path;
        State = PlaybackState.Playing;
    }

    public void Play()
    {
        MediaPlayer.Play();
        State = PlaybackState.Playing;
    }

    public void Pause()
    {
        MediaPlayer.Pause();
        State = PlaybackState.Paused;
    }

    public void Stop()
    {
        MediaPlayer.Stop();
        State = PlaybackState.Stopped;
    }

    public void Seek(long milliseconds)
        => MediaPlayer.Time = Math.Max(0, milliseconds);

    public void SetVolume(int volume)
        => MediaPlayer.Volume = Math.Clamp(volume, 0, 100);

    public void Dispose()
    {
        MediaPlayer.Dispose();
        LibVLC.Dispose();
    }
}
