using Key2Mouse.Native;
using Key2Mouse.Ui;

namespace Key2Mouse.Mouse;

/// <summary>鼠标动作统一封装：移动 / 滚动 / 调速</summary>
public sealed class MouseController
{
    private readonly MouseSpeedSettings _speed;
    private readonly Feedback _feedback;

    public MouseController(MouseSpeedSettings speed, Feedback feedback)
    {
        _speed    = speed;
        _feedback = feedback;
    }

    public int BasicsSpeed    => _speed.BasicsSpeed;
    public int CursorSpeedMode => _speed.CursorSpeedMode;

    public POINT GetPosition()
    {
        NativeMethods.GetCursorPos(out var pos);
        return pos;
    }

    public void MoveBy(int directionX, int directionY)
    {
        POINT current = GetPosition();

        int x = current.X + directionX * _speed.BasicsSpeed;
        int y = current.Y + directionY * _speed.BasicsSpeed;

        int minX = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        int minY = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        int maxX = minX + NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN) - 1;
        int maxY = minY + NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN) - 1;

        if (x < minX) x = minX;
        if (y < minY) y = minY;
        if (x > maxX) x = maxX;
        if (y > maxY) y = maxY;

        NativeMethods.SetCursorPos(x, y);
    }

    public void VerticalScroll(int delta)
        => NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);

    public void HorizontalScroll(int delta)
        => NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_HWHEEL, 0, 0, (uint)delta, UIntPtr.Zero);

    public void ChangeSpeed(bool increase)
    {
        bool ok = increase ? _speed.Increase() : _speed.Decrease();

        if (!ok)
        {
            _feedback.Action(increase ? "已到最高挡位" : "已到最低挡位");
            return;
        }

        _feedback.PlayBeep(_speed.CursorSpeedMode.ToString());
        _feedback.SpeedChanged(_speed.CursorSpeedMode, _speed.BasicsSpeed);
        _feedback.Action(increase ? "速度++" : "速度--");
    }
}