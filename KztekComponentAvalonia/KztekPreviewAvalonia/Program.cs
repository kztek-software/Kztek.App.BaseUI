using Avalonia;
using System;

namespace KztekPreviewAvalonia;

internal static class Program
{
    public static int StartPage { get; private set; } = 0;

    [STAThread]
    public static void Main(string[] args)
    {
        // --page N (0-based) to open on a specific page
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--page" && int.TryParse(args[i + 1], out int page))
                StartPage = Math.Clamp(page, 0, 8);
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .WithInterFont()
                     .LogToTrace();
}
