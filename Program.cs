using Key2Mouse.Audio;
using Key2Mouse.Input;
using Key2Mouse.Mouse;
using Key2Mouse.Native;
using System.Text;
using WindowsInput;
using WindowsInput.Events;
using WindowsInput.Events.Sources;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Media;
using System.Reflection;
using System.Text.Unicode;
using System.Text;

namespace Key2Mouse;

internal static class Program
{
    private static void Main(string[] args)
    {
        // DPI 感知：避免多屏不同缩放下坐标错位
        NativeMethods.SetProcessDPIAware();

        // 依赖装配（手写 DI，够用就好）
        var speedSettings = new MouseSpeedSettings();
        var beepPlayer    = new EmbeddedBeepPlayer();
        var mouse         = new MouseController(speedSettings, beepPlayer);
        var handler       = new KeyboardToMouseHandler(mouse);

        PrintManual();

        using var keyboard = Capture.Global.KeyboardAsync();
        keyboard.KeyEvent += handler.Handle;

        Console.ReadLine();
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