using System.IO;
using System.Windows;
using Serilog;

namespace OpenFrame.Player.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenFrame.Player",
            "Logs");

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(logDirectory, "openframe-player.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            base.OnStartup(e);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "OpenFrame.Player failed to start");
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
