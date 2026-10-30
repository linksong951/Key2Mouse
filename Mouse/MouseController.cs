using Key2Mouse.Audio;
using Key2Mouse.Native;

namespace Key2Mouse.Mouse;

/// <summary>
/// 鼠标动作统一封装：移动 / 滚动 / 调速
/// </summary>
public sealed class MouseController
{
    private readonly MouseSpeedSettings _speed;
    private readonly EmbeddedBeepPlayer _beep;

    public MouseController(MouseSpeedSettings speed, EmbeddedBeepPlayer beep)
    {
        _speed  = speed;
        _beep   = beep;
    }

    public int BasicsSpeed => _speed.BasicsSpeed;
    public int CursorSpeedMode => _speed.CursorSpeedMode;

    public POINT GetPosition()
    {
        NativeMethods.GetCursorPos(out var pos);
        return pos;
    }

    /// <summary>
    /// 按方向位移量移动鼠标（内部会乘以 BasicsSpeed，并钳制到虚拟桌面边界）。
    /// </summary>
    /// <param name="directionX">方向单位（例如 -1/0/1/2）</param>
    /// <param name="directionY">方向单位（例如 -1/0/1/2）</param>
    public void MoveBy(int directionX, int directionY)
    {
        POINT current = GetPosition();

        int x = current.X + directionX * _speed.BasicsSpeed;
        int y = current.Y + directionY * _speed.BasicsSpeed;

        // 用虚拟屏幕（所有显示器合并后的整个桌面）边界钳制
        int minX = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        int minY = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        int maxX = minX + NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN) - 1;
        int maxY = minY + NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN) - 1;

        if (x < minX) x = minX;
        if (y < minY) y = minY;
        if (x > maxX) x = maxX;
        if (y > maxY) y = maxY;

        // 关键：SetCursorPos 直接接收虚拟桌面物理坐标，跨屏可靠
        // （不要用 WindowsInput 的 MoveTo，它内部按 SM_CXSCREEN 归一化，跨屏会溢出）
        NativeMethods.SetCursorPos(x, y);
    }

    public void VerticalScroll(int delta)
        => NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);

    public void HorizontalScroll(int delta)
        => NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_HWHEEL, 0, 0, (uint)delta, UIntPtr.Zero);

    /// <summary>调整挡位并播放反馈音</summary>
    public void ChangeSpeed(bool increase)
    {
        bool ok = increase ? _speed.Increase() : _speed.Decrease();

        if (!ok)
        {
            Console.WriteLine(increase ? "已经是最高挡位了" : "已经是最低挡位了");
            return;
        }

        _beep.Play(_speed.CursorSpeedMode.ToString());
        Console.WriteLine($"当前挡位:{_speed.CursorSpeedMode}");
        Console.WriteLine($"基础速率:{_speed.BasicsSpeed}");
    }
}