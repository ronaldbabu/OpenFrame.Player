using OpenFrame.Player.Core.Enums;
using OpenFrame.Player.Core.Interfaces;
using OpenFrame.Player.UI.Services;
using OpenFrame.Player.UI.ViewModels;
using Xunit;

namespace OpenFrame.Player.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void OpenCommand_WhenDialogIsCanceled_LeavesStateUnchanged()
    {
        var player = new FakePlayerService();
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(null));

        viewModel.OpenCommand.Execute(null);

        Assert.Null(viewModel.SelectedMediaPath);
        Assert.Equal("No media selected", viewModel.SelectedMediaName);
        Assert.Null(viewModel.ErrorMessage);
        Assert.Equal(0, player.OpenCallCount);
    }

    [Fact]
    public void OpenCommand_WhenFileExists_OpensMediaAndUpdatesState()
    {
        var path = CreateTempMediaFile();
        var player = new FakePlayerService { DurationMillisecondsValue = 10_000 };
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(path));

        try
        {
            viewModel.OpenCommand.Execute(null);

            Assert.Equal(path, viewModel.SelectedMediaPath);
            Assert.Equal(Path.GetFileName(path), viewModel.SelectedMediaName);
            Assert.Equal(PlaybackState.Playing, viewModel.PlaybackState);
            Assert.Equal(10_000, viewModel.DurationMilliseconds);
            Assert.Null(viewModel.ErrorMessage);
            Assert.Equal(1, player.OpenCallCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void OpenCommand_WhenFileIsMissing_ShowsFriendlyError()
    {
        var player = new FakePlayerService();
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(@"C:\missing\movie.mp4"));

        viewModel.OpenCommand.Execute(null);

        Assert.Equal("The selected media file could not be found.", viewModel.ErrorMessage);
        Assert.Equal(0, player.OpenCallCount);
    }

    [Fact]
    public void PlayPauseCommand_TogglesBetweenPauseAndPlay()
    {
        var path = CreateTempMediaFile();
        var player = new FakePlayerService();
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(path));

        try
        {
            viewModel.OpenCommand.Execute(null);

            viewModel.PlayPauseCommand.Execute(null);

            Assert.Equal(1, player.PauseCallCount);
            Assert.Equal(PlaybackState.Paused, viewModel.PlaybackState);

            viewModel.PlayPauseCommand.Execute(null);

            Assert.Equal(1, player.PlayCallCount);
            Assert.Equal(PlaybackState.Playing, viewModel.PlaybackState);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void StopCommand_StopsPlaybackAndResetsPosition()
    {
        var path = CreateTempMediaFile();
        var player = new FakePlayerService { PositionMillisecondsValue = 5_000 };
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(path));

        try
        {
            viewModel.OpenCommand.Execute(null);
            viewModel.PositionMilliseconds = 5_000;

            viewModel.StopCommand.Execute(null);

            Assert.Equal(1, player.StopCallCount);
            Assert.Equal(PlaybackState.Stopped, viewModel.PlaybackState);
            Assert.Equal(0, viewModel.PositionMilliseconds);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Volume_WhenChanged_ClampsAndUpdatesPlayer()
    {
        var player = new FakePlayerService();
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(null));

        viewModel.Volume = 150;

        Assert.Equal(100, viewModel.Volume);
        Assert.Equal(100, player.Volume);

        viewModel.Volume = -10;

        Assert.Equal(0, viewModel.Volume);
        Assert.Equal(0, player.Volume);
    }

    [Fact]
    public void Position_WhenMediaIsOpen_SeeksPlayer()
    {
        var path = CreateTempMediaFile();
        var player = new FakePlayerService { DurationMillisecondsValue = 10_000 };
        var viewModel = new MainWindowViewModel(player, new FakeFileDialogService(path));

        try
        {
            viewModel.OpenCommand.Execute(null);

            viewModel.PositionMilliseconds = 3_000;

            Assert.Equal(3_000, viewModel.PositionMilliseconds);
            Assert.Equal(3_000, player.PositionMilliseconds);
            Assert.Equal(1, player.SeekCallCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateTempMediaFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");
        File.WriteAllText(path, "test");
        return path;
    }

    private sealed class FakeFileDialogService(string? path) : IFileDialogService
    {
        public string? OpenMediaFile() => path;
    }

    private sealed class FakePlayerService : IPlayerService
    {
        public int OpenCallCount { get; private set; }

        public int PlayCallCount { get; private set; }

        public int PauseCallCount { get; private set; }

        public int StopCallCount { get; private set; }

        public int SeekCallCount { get; private set; }

        public bool IsPlaying => State == PlaybackState.Playing;

        public PlaybackState State { get; private set; } = PlaybackState.Stopped;

        public long PositionMilliseconds => PositionMillisecondsValue;

        public long PositionMillisecondsValue { get; set; }

        public long DurationMilliseconds => DurationMillisecondsValue;

        public long DurationMillisecondsValue { get; set; }

        public int Volume { get; private set; } = 80;

        public string? CurrentMediaPath { get; private set; }

        public void Open(string path)
        {
            OpenCallCount++;
            CurrentMediaPath = path;
            State = PlaybackState.Playing;
        }

        public void Play()
        {
            PlayCallCount++;
            State = PlaybackState.Playing;
        }

        public void Pause()
        {
            PauseCallCount++;
            State = PlaybackState.Paused;
        }

        public void Stop()
        {
            StopCallCount++;
            State = PlaybackState.Stopped;
            PositionMillisecondsValue = 0;
        }

        public void Seek(long milliseconds)
        {
            SeekCallCount++;
            PositionMillisecondsValue = milliseconds;
        }

        public void SetVolume(int volume)
            => Volume = volume;

        public void Dispose()
        {
        }
    }
}
