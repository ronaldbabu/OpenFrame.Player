namespace OpenFrame.Player.Core.Interfaces;

public interface IPlayerService
{
    void Open(string path);

    void Play();

    void Pause();

    void Stop();

    void Seek(long milliseconds);

    void SetVolume(int volume);

    bool IsPlaying { get; }
}