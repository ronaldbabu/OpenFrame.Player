using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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
        private readonly DispatcherTimer _fullScreenControlsTimer;
        private readonly WndProcDelegate _videoWindowProc;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;
        private double _previousLeft;
        private double _previousTop;
        private double _previousWidth;
        private double _previousHeight;
        private IntPtr _videoWindowHandle;
        private IntPtr _previousVideoWindowProc;
        private bool _wasTopmost;
        private bool _isFullScreen;

        public MainWindow()
        {
            InitializeComponent();

            _player = new VlcPlayerService();
            _viewModel = new MainWindowViewModel(
                _player,
                new WpfFileDialogService());

            DataContext = _viewModel;
            VideoView.MediaPlayer = _player.MediaPlayer;
            _videoWindowProc = VideoWindowProc;

            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _playbackTimer.Tick += OnPlaybackTimerTick;
            _playbackTimer.Start();

            _fullScreenControlsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _fullScreenControlsTimer.Tick += OnFullScreenControlsTimerTick;

            Closed += OnClosed;
            VideoView.Loaded += OnVideoViewLoaded;
        }

        private void OnPlaybackTimerTick(object? sender, EventArgs e)
            => _viewModel.RefreshFromPlayer();

        private void OnToggleFullScreenClick(object sender, RoutedEventArgs e)
            => ToggleFullScreen();

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }

            if (e.Key == Key.Escape && _isFullScreen)
            {
                ExitFullScreen();
                e.Handled = true;
            }
        }

        private void OnSeekSliderPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Slider slider || !slider.IsEnabled || slider.ActualWidth <= 0)
            {
                return;
            }

            if (e.OriginalSource is Thumb)
            {
                return;
            }

            var clickPosition = e.GetPosition(slider).X;
            var percent = Math.Clamp(clickPosition / slider.ActualWidth, 0, 1);
            var targetValue = slider.Minimum + ((slider.Maximum - slider.Minimum) * percent);

            slider.Value = targetValue;
            e.Handled = true;
        }

        private void OnVideoSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TogglePlayPauseFromVideoSurface();
            e.Handled = true;
        }

        private void TogglePlayPauseFromVideoSurface()
        {
            if (_viewModel.PlayPauseCommand.CanExecute(null))
            {
                _viewModel.PlayPauseCommand.Execute(null);
            }
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isFullScreen)
            {
                return;
            }

            ShowFullScreenControlsTemporarily();
        }

        private void OnFullScreenControlsTimerTick(object? sender, EventArgs e)
        {
            _fullScreenControlsTimer.Stop();

            if (_isFullScreen)
            {
                ControlBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                ExitFullScreen();
                return;
            }

            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResizeMode = ResizeMode;
            var restoreBounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            _previousLeft = restoreBounds.Left;
            _previousTop = restoreBounds.Top;
            _previousWidth = restoreBounds.Width;
            _previousHeight = restoreBounds.Height;
            _wasTopmost = Topmost;

            var monitorBounds = GetCurrentMonitorBounds();

            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Left = monitorBounds.Left;
            Top = monitorBounds.Top;
            Width = monitorBounds.Width;
            Height = monitorBounds.Height;
            Topmost = true;
            MainHeader.Visibility = Visibility.Collapsed;
            ControlBar.Visibility = Visibility.Collapsed;
            VideoSurface.BorderThickness = new Thickness(0);
            _isFullScreen = true;
            FullScreenButton.Content = "\uE73F";
            FullScreenButton.ToolTip = "Exit Full Screen";
        }

        private void ExitFullScreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = _previousWindowStyle;
            ResizeMode = _previousResizeMode;
            Left = _previousLeft;
            Top = _previousTop;
            Width = _previousWidth;
            Height = _previousHeight;
            Topmost = _wasTopmost;
            WindowState = _previousWindowState;
            MainHeader.Visibility = Visibility.Visible;
            ControlBar.Visibility = Visibility.Visible;
            VideoSurface.BorderThickness = new Thickness(1);
            _fullScreenControlsTimer.Stop();
            _isFullScreen = false;
            FullScreenButton.Content = "\uE740";
            FullScreenButton.ToolTip = "Full Screen";
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            UninstallVideoWindowHook();
            VideoView.Loaded -= OnVideoViewLoaded;
            _playbackTimer.Stop();
            _playbackTimer.Tick -= OnPlaybackTimerTick;
            _fullScreenControlsTimer.Stop();
            _fullScreenControlsTimer.Tick -= OnFullScreenControlsTimerTick;
            VideoView.MediaPlayer = null;
            _player.Dispose();
        }

        private void ShowFullScreenControlsTemporarily()
        {
            ControlBar.Visibility = Visibility.Visible;
            _fullScreenControlsTimer.Stop();
            _fullScreenControlsTimer.Start();
        }

        private void OnVideoViewLoaded(object sender, RoutedEventArgs e)
            => InstallVideoWindowHook();

        private void InstallVideoWindowHook()
        {
            if (_videoWindowHandle != IntPtr.Zero)
            {
                return;
            }

            var videoHost = FindVisualChild<HwndHost>(VideoView);
            _videoWindowHandle = videoHost?.Handle ?? _player.MediaPlayer.Hwnd;

            if (_videoWindowHandle == IntPtr.Zero)
            {
                return;
            }

            _previousVideoWindowProc = SetWindowLongPtr(_videoWindowHandle, GwlWndProc, Marshal.GetFunctionPointerForDelegate(_videoWindowProc));
        }

        private void UninstallVideoWindowHook()
        {
            if (_videoWindowHandle == IntPtr.Zero || _previousVideoWindowProc == IntPtr.Zero)
            {
                return;
            }

            SetWindowLongPtr(_videoWindowHandle, GwlWndProc, _previousVideoWindowProc);
            _videoWindowHandle = IntPtr.Zero;
            _previousVideoWindowProc = IntPtr.Zero;
        }

        private IntPtr VideoWindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam)
        {
            if (message == WmLButtonDown)
            {
                Dispatcher.Invoke(TogglePlayPauseFromVideoSurface);
                return IntPtr.Zero;
            }

            if (message == WmMouseMove)
            {
                Dispatcher.Invoke(() =>
                {
                    if (_isFullScreen)
                    {
                        ShowFullScreenControlsTemporarily();
                    }
                });
            }

            return CallWindowProc(_previousVideoWindowProc, hwnd, message, wParam, lParam);
        }

        private static T? FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    return typedChild;
                }

                var nestedChild = FindVisualChild<T>(child);
                if (nestedChild is not null)
                {
                    return nestedChild;
                }
            }

            return null;
        }

        private Rect GetCurrentMonitorBounds()
        {
            var windowHandle = new WindowInteropHelper(this).Handle;
            var monitorHandle = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);

            var monitorInfo = new MonitorInfo
            {
                Size = Marshal.SizeOf<MonitorInfo>()
            };

            if (!GetMonitorInfo(monitorHandle, ref monitorInfo))
            {
                return new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }

            var source = PresentationSource.FromVisual(this);
            var transform = source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
            var topLeft = transform.Transform(new Point(monitorInfo.Monitor.Left, monitorInfo.Monitor.Top));
            var bottomRight = transform.Transform(new Point(monitorInfo.Monitor.Right, monitorInfo.Monitor.Bottom));

            return new Rect(topLeft, bottomRight);
        }

        private const int MonitorDefaultToNearest = 2;
        private const int GwlWndProc = -4;
        private const int WmMouseMove = 0x0200;
        private const int WmLButtonDown = 0x0201;

        private delegate IntPtr WndProcDelegate(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo monitorInfo);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr newLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr previousWindowProc, IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor;
            public NativeRect WorkArea;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
