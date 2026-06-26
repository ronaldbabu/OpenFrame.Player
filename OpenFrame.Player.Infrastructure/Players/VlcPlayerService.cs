using LibVLCSharp.Shared;
using OpenFrame.Player.Core.Interfaces;
using VLC = LibVLCSharp.Shared.Core;

namespace OpenFrame.Player.Infrastructure.Players;

public class VlcPlayerService : IPlayerService
{
    public LibVLC LibVLC { get; }

    public MediaPlayer MediaPlayer { get; }

    public bool IsPlaying => MediaPlayer.IsPlaying;



    public VlcPlayerService()
    {
        VLC.Initialize();

        LibVLC = new LibVLC();

        MediaPlayer = new MediaPlayer(LibVLC);
    }

    public void Open(string path)
    {
        using var media = new Media(LibVLC, path);

        MediaPlayer.Play(media);
    }


    public void Play()
        => MediaPlayer.Play();



    public void Pause()
        => MediaPlayer.Pause();



    public void Stop()
        => MediaPlayer.Stop();



    public void Seek(long ms)
        => MediaPlayer.Time = ms;



    public void SetVolume(int volume)
        => MediaPlayer.Volume = volume;
}