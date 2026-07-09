# Repository Guidelines

## Project Shape

OpenFrame.Player is a Windows desktop media player built with .NET, WPF, and LibVLCSharp.

- `OpenFrame.Player.Core` owns domain contracts and simple shared types.
- `OpenFrame.Player.Infrastructure` owns concrete integrations such as VLC playback.
- `OpenFrame.Player.UI` owns WPF views, view models, UI services, and app startup.
- `OpenFrame.Player.Tests` owns automated tests.

Keep dependencies flowing inward: UI may reference Infrastructure and Core; Infrastructure may reference Core; Core should not reference UI or Infrastructure.

## Development Defaults

- Prefer Visual Studio for normal development because this is a WPF/XAML desktop app.
- Use `dotnet build OpenFrame.Player.slnx` as the basic verification command.
- Use `dotnet test OpenFrame.Player.slnx` once real tests exist.
- Do not hard-code local user paths, media files, machine-specific directories, or personal settings into app startup.
- Keep sample/demo behavior behind explicit user actions or configurable settings.
- Dispose unmanaged media resources carefully, especially `LibVLC`, `MediaPlayer`, and media handles.

## WPF and MVVM

- Keep view code-behind thin. Use it for WPF control glue that cannot reasonably live in a view model.
- Put user-facing player actions in view models or UI services: open file, play, pause, stop, seek, volume, fullscreen, and status updates.
- Use `CommunityToolkit.Mvvm` patterns already referenced by the project.
- Keep XAML readable and avoid large logic blocks in markup.
- Do not let UI projects leak LibVLC implementation details beyond the control binding boundary unless there is a practical WPF interop reason.

## Playback Behavior

- Treat `IPlayerService` as the main playback boundary.
- Update the Core contract when new player features need to be exposed across layers.
- Keep VLC-specific types in Infrastructure whenever possible.
- Validate file paths before opening media and surface friendly errors in the UI.
- Track playback state intentionally instead of relying only on instantaneous player flags.

## Documentation Expectations

When changes affect setup, runtime behavior, architecture, packaging, dependencies, or user-facing workflow, update or create documentation in the same change.

- Use `README.md` for prerequisites, setup, restore/build/run steps, troubleshooting, and user-facing workflows.
- Use `ARCHITECTURE.md` for app structure, project responsibilities, playback flow, dependency boundaries, and packaging/runtime notes.
- If either file does not exist yet and the change needs it, create it.
- Keep documentation beginner-friendly and explicit about required SDKs, Visual Studio workloads, VLC/LibVLC dependencies, and supported Windows assumptions.

## Git Workflow

- Preserve user changes. Do not revert unrelated files or cleanup work you did not make.
- Use short branch names like `feature/open-file-command` for published feature work.
- Treat a request like "push changes" as the full flow: create a feature branch, commit intentionally, and push the branch.
- Prefer separate PowerShell commands over chained shell commands.
- Before committing, check `git status --short --branch` and make sure the commit only includes intended files.

## Quality Bar

- Keep changes small and aligned with the existing project boundaries.
- Add tests when behavior moves out of WPF-only code and can be verified without launching the UI.
- Favor simple, explicit code over early abstractions.
- If package restore or build fails because network access is unavailable, say that clearly and include the exact command that was attempted.
