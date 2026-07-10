using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrame.Player.Core.Enums;
using OpenFrame.Player.Core.Interfaces;
using OpenFrame.Player.UI.Services;
using Serilog;

namespace OpenFrame.Player.UI.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IPlayerService _playerService;
    private readonly IFileDialogService _fileDialogService;

    private string? _selectedMediaPath;
    private string _selectedMediaName = "No media selected";
    private string? _errorMessage;
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private long _positionMilliseconds;
    private long _durationMilliseconds;
    private int _volume = 80;

    public MainWindowViewModel(IPlayerService playerService, IFileDialogService fileDialogService)
    {
        _playerService = playerService;
        _fileDialogService = fileDialogService;

        OpenCommand = new RelayCommand(OpenMedia);
        PlayPauseCommand = new RelayCommand(PlayPause, () => HasMedia);
        StopCommand = new RelayCommand(Stop, () => HasMedia && PlaybackState != PlaybackState.Stopped);
        SeekCommand = new RelayCommand(Seek, () => HasMedia);

        _playerService.SetVolume(_volume);
    }

    public IRelayCommand OpenCommand { get; }

    public IRelayCommand PlayPauseCommand { get; }

    public IRelayCommand StopCommand { get; }

    public IRelayCommand SeekCommand { get; }

    public string? SelectedMediaPath
    {
        get => _selectedMediaPath;
        private set
        {
            if (SetProperty(ref _selectedMediaPath, value))
            {
                OnPropertyChanged(nameof(HasMedia));
                OnPropertyChanged(nameof(StatusMessage));
                NotifyCommandStateChanged();
            }
        }
    }

    public string SelectedMediaName
    {
        get => _selectedMediaName;
        private set => SetProperty(ref _selectedMediaName, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasMedia => !string.IsNullOrWhiteSpace(SelectedMediaPath);

    public PlaybackState PlaybackState
    {
        get => _playbackState;
        private set
        {
            if (SetProperty(ref _playbackState, value))
            {
                OnPropertyChanged(nameof(PlayPauseText));
                OnPropertyChanged(nameof(PlayPauseIcon));
                OnPropertyChanged(nameof(StatusMessage));
                NotifyCommandStateChanged();
            }
        }
    }

    public string PlayPauseText => PlaybackState == PlaybackState.Playing ? "Pause" : "Play";

    public string PlayPauseIcon => PlaybackState == PlaybackState.Playing ? "\uE769" : "\uE768";

    public string StatusMessage
        => HasMedia
            ? $"{PlaybackState} - {FormatTime(PositionMilliseconds)} / {FormatTime(DurationMilliseconds)}"
            : "Open a media file to begin";

    public long PositionMilliseconds
    {
        get => _positionMilliseconds;
        set => SetPosition(value, seekPlayer: true);
    }

    public long DurationMilliseconds
    {
        get => _durationMilliseconds;
        private set
        {
            if (SetProperty(ref _durationMilliseconds, Math.Max(0, value)))
            {
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public int Volume
    {
        get => _volume;
        set
        {
            var clampedVolume = Math.Clamp(value, 0, 100);

            if (SetProperty(ref _volume, clampedVolume))
            {
                _playerService.SetVolume(clampedVolume);
            }
        }
    }

    public void RefreshFromPlayer()
    {
        if (!HasMedia)
        {
            return;
        }

        PlaybackState = _playerService.State;
        DurationMilliseconds = _playerService.DurationMilliseconds;
        SetPosition(_playerService.PositionMilliseconds, seekPlayer: false);
    }

    private void OpenMedia()
    {
        var selectedPath = _fileDialogService.OpenMediaFile();

        if (selectedPath is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedPath) || !File.Exists(selectedPath))
        {
            SetError("The selected media file could not be found.");
            return;
        }

        try
        {
            _playerService.Open(selectedPath);

            SelectedMediaPath = selectedPath;
            SelectedMediaName = Path.GetFileName(selectedPath);
            PlaybackState = _playerService.State;
            DurationMilliseconds = _playerService.DurationMilliseconds;
            SetPosition(_playerService.PositionMilliseconds, seekPlayer: false);
            ErrorMessage = null;
        }
        catch (Exception exception) when (exception is ArgumentException or FileNotFoundException or InvalidOperationException)
        {
            Log.Error(exception, "Failed to open media file {MediaPath}", selectedPath);
            SetError(exception.Message);
        }
    }

    private void PlayPause()
    {
        if (!HasMedia)
        {
            SetError("Open a media file before trying to play it.");
            return;
        }

        try
        {
            if (PlaybackState == PlaybackState.Playing)
            {
                _playerService.Pause();
            }
            else
            {
                _playerService.Play();
            }

            PlaybackState = _playerService.State;
            ErrorMessage = null;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to change playback state");
            SetError("Playback could not be changed.");
        }
    }

    private void Stop()
    {
        if (!HasMedia)
        {
            return;
        }

        try
        {
            _playerService.Stop();
            PlaybackState = _playerService.State;
            SetPosition(0, seekPlayer: false);
            ErrorMessage = null;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to stop playback");
            SetError("Playback could not be stopped.");
        }
    }

    private void Seek()
        => SetPosition(PositionMilliseconds, seekPlayer: true);

    private void SetPosition(long value, bool seekPlayer)
    {
        var upperBound = DurationMilliseconds > 0 ? DurationMilliseconds : long.MaxValue;
        var clampedPosition = Math.Clamp(value, 0, upperBound);

        if (SetProperty(ref _positionMilliseconds, clampedPosition) && seekPlayer && HasMedia)
        {
            _playerService.Seek(clampedPosition);
        }

        OnPropertyChanged(nameof(StatusMessage));
    }

    private void SetError(string message)
        => ErrorMessage = message;

    private void NotifyCommandStateChanged()
    {
        PlayPauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        SeekCommand.NotifyCanExecuteChanged();
    }

    private static string FormatTime(long milliseconds)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        return time.TotalHours >= 1
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");
    }
}
