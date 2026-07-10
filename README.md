# OpenFrame.Player

OpenFrame.Player is a Windows desktop media player built with .NET, WPF, and LibVLCSharp.

The app has a dark media-player style UI with a `#d97757` accent theme, video surface, custom header bar, bottom transport bar, icon playback controls, seek timeline, volume control, fullscreen mode, and user-facing status/error text.

## Current Features

- Header Open icon media picker.
- Bottom transport bar with icon controls for Open, Play/Pause, Stop, Full Screen, seek, and volume.
- Fullscreen mode from the fullscreen icon or `F11`.
- Fullscreen hides the header, controls, and video border for a video-only view.
- Moving the mouse in fullscreen temporarily shows the controls.
- Exit fullscreen with `Esc` or the `Exit Full Screen` button when controls are visible.
- Click anywhere on the timeline to jump directly to that point.
- Drag the timeline thumb to seek.
- Click the video area to toggle Play/Pause.
- LibVLCSharp playback inside a WPF `VideoView`.
- MVVM playback state through `MainWindowViewModel`.
- File picker behavior isolated behind `IFileDialogService`.
- Minimal Serilog logging for startup/open/playback failures.
- xUnit tests for view model command behavior.

## Prerequisites

- Windows 10 or newer.
- Visual Studio with the `.NET desktop development` workload.
- .NET SDK that supports the target framework used by the project.
- NuGet access for package restore.

The UI project currently targets `net10.0-windows`, so use a matching .NET SDK and Visual Studio version that supports .NET 10 and WPF.

## Recommended Editor

Use Visual Studio for day-to-day development. This project uses WPF and XAML, so Visual Studio gives better support for building, debugging, designer tooling, NuGet package management, and Windows desktop workflows.

VS Code is fine for quick edits and Git work, but Visual Studio is the better main environment for this app.

## Restore, Build, and Test

From the repository root:

```powershell
dotnet restore OpenFrame.Player.slnx
dotnet build OpenFrame.Player.slnx
dotnet test OpenFrame.Player.slnx
```

If restore fails with a NuGet network error, confirm that the machine can reach `https://api.nuget.org/v3/index.json`.

## Run

From Visual Studio:

1. Open `OpenFrame.Player.slnx`.
2. Set `OpenFrame.Player.UI` as the startup project.
3. Restore NuGet packages if Visual Studio prompts you.
4. Build the solution.
5. Start debugging.
6. Use the header or transport-bar Open icon to choose a media file.

From the command line:

```powershell
dotnet run --project OpenFrame.Player.UI\OpenFrame.Player.UI.csproj
```

## Controls

- Open icon: choose a media file.
- Play/Pause icon: toggle playback for the selected media.
- Video area: click to toggle Play/Pause.
- Stop icon: stop playback and reset the seek position.
- Timeline: click any point to jump there, or drag the thumb to seek.
- Volume: adjust player volume from 0 to 100.
- Fullscreen icon or `F11`: enter fullscreen.
- In fullscreen, move the mouse to reveal controls.
- `Esc`: exit fullscreen.

## Logging

Logs are written under the current user's local application data folder:

```text
%LOCALAPPDATA%\OpenFrame.Player\Logs\openframe-player.log
```

Logging is intentionally minimal and focuses on startup failures, media open failures, and playback command failures.

## Main Dependencies

- `CommunityToolkit.Mvvm` for MVVM support.
- `LibVLCSharp` for playback.
- `LibVLCSharp.WPF` for WPF video rendering.
- `VideoLAN.LibVLC.Windows` for the Windows LibVLC runtime package.
- `Serilog` and `Serilog.Sinks.File` for file logging.
- `xUnit` for tests.

## Repository Notes

See `AGENTS.md` for contributor and agent workflow rules. Keep `README.md` and `ARCHITECTURE.md` updated when setup, runtime behavior, architecture, packaging, dependencies, or user-facing workflows change.
