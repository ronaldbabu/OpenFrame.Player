using Microsoft.Win32;

namespace OpenFrame.Player.UI.Services;

public sealed class WpfFileDialogService : IFileDialogService
{
    public string? OpenMediaFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open media file",
            Filter = "Media files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.mp3;*.wav;*.flac;*.m4a|Video files|*.mp4;*.mkv;*.avi;*.mov;*.wmv|Audio files|*.mp3;*.wav;*.flac;*.m4a|All files|*.*",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true
            ? dialog.FileName
            : null;
    }
}
