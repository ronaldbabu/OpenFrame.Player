# OpenFrame.Player

OpenFrame.Player is a Windows desktop media player built with .NET, WPF, and LibVLCSharp.

The app now has the first usable playback slice: users can open a media file from the UI, play or pause it, stop playback, seek, adjust volume, and see basic status or error text.

## Current Status

- WPF desktop shell with a `File > Open` menu.
- Bottom playback bar with Open, Play/Pause, Stop, seek, and volume controls.
- LibVLCSharp is wired to a WPF `VideoView`.
- Playback commands and state live in `MainWindowViewModel`.
- File selection is isolated behind a small file dialog service.
- VLC resources are disposed when the main window closes.
- Startup and playback/open failures are logged with Serilog.
- xUnit tests cover view model command behavior.

## Prerequisites

- Windows 10 or newer.
- Visual Studio with the `.NET desktop development` workload.
- .NET SDK that supports the target framework used by the project.
- NuGet access for package restore.

The UI project currently targets `net10.0-windows`, so use a matching .NET SDK and Visual Studio version that supports .NET 10 and WPF.

## Recommended Editor

Use Visual Studio for day-to-day development. This project uses WPF and XAML, so Visual Studio gives better support for building, debugging, designer tooling, NuGet package management, and Windows desktop workflows.

VS Code is fine for quick text edits, Git work, or reviewing files, but Visual Studio is the better main environment for this app.

## Restore, Build, and Test

From the repository root:

```powershell
dotnet restore OpenFrame.Player.slnx
dotnet build OpenFrame.Player.slnx
dotnet test OpenFrame.Player.slnx
```

If restore fails with a NuGet network error, confirm that the machine can reach `https://api.nuget.org/v3/index.json`.

## Run

The easiest way to run the app during development is from Visual Studio:

1. Open `OpenFrame.Player.slnx`.
2. Set `OpenFrame.Player.UI` as the startup project.
3. Restore NuGet packages if Visual Studio prompts you.
4. Build the solution.
5. Start debugging.
6. Use `File > Open` or the `Open` button to choose a media file.

You can also run from the command line:

```powershell
dotnet run --project OpenFrame.Player.UI\OpenFrame.Player.UI.csproj
```

## Controls

- `File > Open` opens a media file picker.
- `Open` opens a media file picker.
- `Play/Pause` toggles playback for the selected media.
- `Stop` stops playback and resets the seek position.
- Seek slider changes the playback position.
- Volume slider adjusts the player volume from 0 to 100.

Supported picker filters include common video/audio extensions and all files.

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

## Project Layout

```text
OpenFrame.Player.Core
  Interfaces
  Enums

OpenFrame.Player.Infrastructure
  Players

OpenFrame.Player.UI
  Views
  ViewModels
  Services
  Controls
  Converters
  Resources

OpenFrame.Player.Tests
```

## Development Roadmap

Good next steps:

- Improve seek updates with richer VLC event handling.
- Add formatted elapsed/duration time labels.
- Add keyboard shortcuts.
- Add fullscreen support.
- Add recent files and last-opened-folder settings.
- Add user-facing error presentation beyond plain status text.
- Add packaging validation for clean Windows machines.

## Troubleshooting

### NuGet restore fails

Make sure the machine has internet access and can reach NuGet. Then run:

```powershell
dotnet restore OpenFrame.Player.slnx
```

### App starts but video does not play

Open a known-good media file through `File > Open`. If playback still fails, check the log file under `%LOCALAPPDATA%\OpenFrame.Player\Logs`.

### Visual Studio cannot load the project

Install or update Visual Studio with the `.NET desktop development` workload and a .NET SDK that supports `net10.0-windows`.

## Repository Notes

See `AGENTS.md` for contributor and agent workflow rules. Keep `README.md` and `ARCHITECTURE.md` updated when setup, runtime behavior, architecture, packaging, dependencies, or user-facing workflows change.
