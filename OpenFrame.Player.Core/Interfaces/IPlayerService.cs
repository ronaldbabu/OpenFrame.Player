using OpenFrame.Player.Core.Enums;

namespace OpenFrame.Player.Core.Interfaces;

public interface IPlayerService : IDisposable
{
    void Open(string path);

    void Play();

    void Pause();

    void Stop();

    void Seek(long milliseconds);

    void SetVolume(int volume);

    bool IsPlaying { get; }

    PlaybackState State { get; }

    long PositionMilliseconds { get; }

    long DurationMilliseconds { get; }

    int Volume { get; }

    string? CurrentMediaPath { get; }
}
