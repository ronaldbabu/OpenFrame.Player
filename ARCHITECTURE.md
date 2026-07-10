# OpenFrame.Player Architecture

OpenFrame.Player is organized as a layered WPF application with a small playback boundary around LibVLCSharp.

## Goals

- Provide a Windows desktop media player UI.
- Use LibVLCSharp for playback.
- Keep playback contracts separate from VLC-specific implementation details.
- Keep WPF UI code focused on presentation and control hosting.
- Keep commands, validation, and user-facing state in the view model.

## Solution Structure

```text
OpenFrame.Player.slnx

OpenFrame.Player.Core
OpenFrame.Player.Infrastructure
OpenFrame.Player.UI
OpenFrame.Player.Tests
```

## Project Responsibilities

### OpenFrame.Player.Core

Core owns contracts and shared domain types that should not depend on WPF or VLC.

`IPlayerService` is the playback boundary. It exposes open, play, pause, stop, seek, volume, current media path, position, duration, and `PlaybackState`.

`PlaybackState` represents the user-visible playback state:

- `Stopped`
- `Playing`
- `Paused`

### OpenFrame.Player.Infrastructure

Infrastructure owns concrete integrations.

`VlcPlayerService` implements `IPlayerService` using LibVLCSharp. It:

- initializes LibVLC;
- creates the VLC `MediaPlayer`;
- validates media paths before opening;
- maps playback commands to VLC operations;
- tracks `PlaybackState`;
- exposes position, duration, volume, and current media path;
- implements `IDisposable` for `MediaPlayer` and `LibVLC`.

The WPF `VideoView` still needs the concrete VLC `MediaPlayer` for rendering, so `VlcPlayerService` exposes that object for the main window glue code.

### OpenFrame.Player.UI

UI owns WPF application startup, windows, view models, controls, resources, and UI services.

`MainWindow.xaml` contains:

- custom dark header bar with Open and Full Screen icons;
- VLC `VideoView`;
- dark media-player style bottom playback bar using `#d97757` as the accent color;
- icon buttons for Open, Play/Pause, Stop, Full Screen, seek, volume, status, and error bindings.
- video-surface click handling for Play/Pause toggle.

`MainWindow.xaml.cs` is intentionally thin. It creates the VLC player, creates `MainWindowViewModel`, assigns the VLC media player to the `VideoView`, refreshes playback state on a timer, handles fullscreen window chrome, handles video-surface click-to-play-pause behavior, handles click-to-seek timeline behavior, and disposes resources when the window closes.

`MainWindowViewModel` owns:

- open/play-pause/stop/seek commands;
- selected media path and display name;
- playback state;
- seek position and duration;
- volume;
- friendly error text.

`IFileDialogService` isolates file picker behavior. `WpfFileDialogService` implements it with `OpenFileDialog`.

`App.xaml.cs` configures Serilog file logging under `%LOCALAPPDATA%\OpenFrame.Player\Logs`.

### OpenFrame.Player.Tests

The test project uses xUnit.

Tests focus on view model behavior without launching WPF:

- open canceled;
- open valid file;
- open missing file;
- play/pause toggle;
- stop behavior;
- volume clamping;
- seek behavior.
- fullscreen and timeline click behavior remain WPF interaction concerns and should be smoke-tested manually unless a UI automation layer is added.

## Dependency Direction

The intended dependency direction is:

```text
UI -> Infrastructure -> Core
UI -> Core
Tests -> Core/Infrastructure/UI as needed
```

Core should remain independent:

```text
Core -/-> Infrastructure
Core -/-> UI
Infrastructure -/-> UI
```

## Runtime Flow

1. WPF starts from `App.xaml`.
2. `App.xaml.cs` configures Serilog.
3. `MainWindow` is created from `StartupUri`.
4. `MainWindow` creates `VlcPlayerService`.
5. `MainWindow` creates `MainWindowViewModel` with the player service and WPF file dialog service.
6. `MainWindow` assigns the VLC `MediaPlayer` to the WPF `VideoView`.
7. The user chooses a media file through the header or transport-bar Open icon.
8. The view model validates the selected file and calls the playback boundary.
9. Infrastructure opens the media through VLC.
10. UI state updates as playback starts, pauses, stops, seeks, or fails.

Fullscreen is a view-level concern. The main window stores the previous window state/style/placement, enters borderless fullscreen using the current monitor bounds, hides the custom header bar, hides the control bar, removes the video border, and exits with `Esc` or the fullscreen button.

Fullscreen controls are intentionally transient. Mouse movement reveals the control bar for a short period, then a timer hides it again.

Click-to-seek is also handled at the WPF layer. The seek slider converts the click position into a timeline percentage and writes the target value back to the bound view model position.

Click-to-play-pause is handled by the video surface and delegates to the same `PlayPauseCommand` used by the transport button. VLC mouse input is disabled on the media player, and the native video child window is subclassed so mouse clicks and movement are still captured when the WPF `VideoView` hosts VLC in an `HwndHost`.

## Resource Lifetime

LibVLC uses unmanaged resources.

`VlcPlayerService` disposes:

- `MediaPlayer`
- `LibVLC`

`MainWindow` stops its refresh timer and disposes the player service when the window closes.

## Error Handling and Logging

Path validation happens before media open. Missing or invalid media selections are shown as friendly messages in the UI.

Serilog records:

- startup failures;
- media open failures;
- playback command failures.

Logging should stay focused on actionable failures rather than routine playback events.

## Packaging Notes

The app is Windows-only because it uses WPF and targets `net10.0-windows`.

Packaging work should confirm:

- LibVLC native assets are included.
- The app starts on a clean Windows machine.
- Media playback works without a developer environment.
- Any installer or packaged build includes runtime prerequisites or documents them clearly.

## Documentation Rules

Update this file when project structure, runtime flow, dependency boundaries, packaging, or major playback behavior changes.

Update `README.md` when prerequisites, setup, build, run, troubleshooting, or user-facing workflows change.
