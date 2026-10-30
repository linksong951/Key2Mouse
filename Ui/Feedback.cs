using Key2Mouse.Audio;

namespace Key2Mouse.Ui;

/// <summary>
/// 用户反馈统一入口：控制台日志（保留可滚动历史）+ 音效 + 托盘状态。
/// 业务层只关心"发生了什么"，由 Feedback 决定"怎么告诉用户"。
/// </summary>
public sealed class Feedback
{
    private readonly EmbeddedBeepPlayer _beep;
    private readonly TrayIcon _tray;

    public Feedback(EmbeddedBeepPlayer beep, TrayIcon tray)
    {
        _beep = beep;
        _tray = tray;
    }

    public void PlayBeep(string name) => _beep.Play(name);

    public void MonitorStateChanged(bool monitorEnabled, bool simulatorEnabled)
    {
        string text = !monitorEnabled
            ? "监听:OFF"
            : simulatorEnabled ? "监听:ON" : "监听:PAUSED";

        Console.WriteLine($"[状态] {text}");
        _tray.SetListening(monitorEnabled && simulatorEnabled);
    }

    public void SpeedChanged(int cursorMode, int basicsSpeed)
        => Console.WriteLine($"[速度] 挡位 {cursorMode}，基础速率 {basicsSpeed}");

    public void Action(string action)
        => Console.WriteLine($"[动作] {action}");
}