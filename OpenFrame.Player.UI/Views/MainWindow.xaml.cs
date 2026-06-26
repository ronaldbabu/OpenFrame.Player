using System.Windows;
using OpenFrame.Player.Infrastructure.Players;

namespace OpenFrame.Player.UI
{
    public partial class MainWindow : Window
    {
        private readonly VlcPlayerService _player;

        public MainWindow()
        {
            InitializeComponent();

            _player = new VlcPlayerService();

            VideoView.MediaPlayer = _player.MediaPlayer;

            _player.Open(@"C:\Users\Ajay\Downloads\sample.mp4");

        }
    }
}