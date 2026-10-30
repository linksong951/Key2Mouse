using System.Drawing;
using System.Windows.Forms;

namespace Key2Mouse.Ui;

/// <summary>
/// 系统托盘图标：ON 显示 Info 图标、OFF 显示默认图标，Tooltip 同步状态，右键退出。
/// 独立 STA 线程跑 WinForms 消息循环，不阻塞控制台主线程。
/// </summary>
public sealed class TrayIcon : IDisposable
{
    private volatile NotifyIcon? _icon;
    private volatile bool _listening;
    private Thread? _thread;

    public void Start()
    {
        _thread = new Thread(Run) { IsBackground = true, Name = "TrayIcon" };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    /// <summary>主线程调用；若托盘尚未就绪，会在 Run() 启动时补应用。</summary>
    public void SetListening(bool listening)
    {
        _listening = listening;
        Apply();
    }

    private void Apply()
    {
        var icon = _icon;
        if (icon is null) return;
        icon.Icon = _listening ? SystemIcons.Information : SystemIcons.Application;
        icon.Text = _listening ? "Key2Mouse - 监听中" : "Key2Mouse - 已停止";
    }

    private void Run()
    {
        _icon = new NotifyIcon
        {
            Icon    = SystemIcons.Application,
            Text    = "Key2Mouse - 已停止",
            Visible = true,
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("退出", null, (_, _) =>
        {
            var icon = _icon;
            if (icon is not null) { icon.Visible = false; icon.Dispose(); }
            Environment.Exit(0);
        });
        _icon.ContextMenuStrip = menu;

        Apply(); // 应用启动期间 SetListening 传入的状态
        Application.Run();
    }

    public void Dispose()
    {
        var icon = _icon;
        if (icon is not null)
        {
            icon.Visible = false;
            icon.Dispose();
        }
        _icon = null;
    }
}