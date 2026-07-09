using System.Windows;
using System.Windows.Threading;
using OpenFrame.Player.Infrastructure.Players;
using OpenFrame.Player.UI.Services;
using OpenFrame.Player.UI.ViewModels;

namespace OpenFrame.Player.UI
{
    public partial class MainWindow : Window
    {
        private readonly VlcPlayerService _player;
        private readonly MainWindowViewModel _viewModel;
        private readonly DispatcherTimer _playbackTimer;

        public MainWindow()
        {
            InitializeComponent();

            _player = new VlcPlayerService();
            _viewModel = new MainWindowViewModel(
                _player,
                new WpfFileDialogService());

            DataContext = _viewModel;
            VideoView.MediaPlayer = _player.MediaPlayer;

            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _playbackTimer.Tick += OnPlaybackTimerTick;
            _playbackTimer.Start();

            Closed += OnClosed;
        }

        private void OnPlaybackTimerTick(object? sender, EventArgs e)
            => _viewModel.RefreshFromPlayer();

        private void OnClosed(object? sender, EventArgs e)
        {
            _playbackTimer.Stop();
            _playbackTimer.Tick -= OnPlaybackTimerTick;
            VideoView.MediaPlayer = null;
            _player.Dispose();
        }
    }
}
