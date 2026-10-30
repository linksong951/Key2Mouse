using Key2Mouse.Audio;
using Key2Mouse.Input;
using Key2Mouse.Mouse;
using Key2Mouse.Native;
using Key2Mouse.Ui;
using System.Text;
using WindowsInput;

namespace Key2Mouse;

internal static class Program
{
    private static void Main(string[] args)
    {
        NativeMethods.SetProcessDPIAware();

        // 依赖装配
        var speedSettings = new MouseSpeedSettings();
        var beepPlayer    = new EmbeddedBeepPlayer();
        var trayIcon      = new TrayIcon();
        var feedback      = new Feedback(beepPlayer, trayIcon);
        var mouse         = new MouseController(speedSettings, feedback);
        var handler       = new KeyboardToMouseHandler(mouse, feedback);

        PrintManual();

        trayIcon.Start();

        using var keyboard = Capture.Global.KeyboardAsync();
        keyboard.KeyEvent += handler.Handle;

        Console.ReadLine();
        trayIcon.Dispose();
    }

    private static void PrintManual()
    {
        string filepath = Path.Combine(AppContext.BaseDirectory, "功能手册.txt");
        string content  = File.ReadAllText(filepath, Encoding.UTF8);

        Console.WriteLine("加载完毕!!");
        Console.WriteLine("===========================================");
        Console.WriteLine(content);
        Console.WriteLine("===========================================");
    }
}