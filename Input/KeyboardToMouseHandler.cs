using Key2Mouse.Mouse;
using WindowsInput;
using WindowsInput.Events;
using WindowsInput.Events.Sources;

namespace Key2Mouse.Input;

/// <summary>
/// 核心业务：键盘事件 → 鼠标动作
/// </summary>
public sealed class KeyboardToMouseHandler
{
    private readonly MouseController _mouse;
    private readonly KeyStateTracker _state = new();

    public KeyboardToMouseHandler(MouseController mouse)
    {
        _mouse = mouse;
    }

    // ---------------------------------------------------------------
    // 入口
    // ---------------------------------------------------------------
    public void Handle(object sender, EventSourceEventArgs<KeyboardEvent> e)
    {
        var dKey = e.Data?.KeyDown?.Key;
        var uKey = e.Data?.KeyUp?.Key;

        UpdateState(dKey, uKey);

        if (HandleToggles(dKey, uKey, e))
            return; // CapsLock 吞掉事件后不需要继续

        if (!_state.MonitorEnabled || !_state.SimulatorEnabled)
            return;

        HandleSpeed(dKey, uKey, e);
        HandleMovement(dKey, e);
        HandleDiagonal(dKey, e);
        HandleClicks(dKey, uKey, e);
        HandleScroll(dKey, e);
        HandleNavigation(dKey, uKey, e);
    }

    // ---------------------------------------------------------------
    // 状态更新
    // ---------------------------------------------------------------
    private void UpdateState(KeyCode? dKey, KeyCode? uKey)
    {
        if (dKey.HasValue) _state.OnKeyDown(dKey.Value);
        if (uKey.HasValue) _state.OnKeyUp(uKey.Value);
    }

    // ---------------------------------------------------------------
    // 开关：Alt+Q 总开关 / CapsLock 生效开关
    // ---------------------------------------------------------------
    /// <returns>true 表示 CapsLock 事件被吞掉</returns>
    private bool HandleToggles(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.Q && _state.LeftAlt)
        {
            _state.MonitorEnabled = !_state.MonitorEnabled;
            if (!_state.MonitorEnabled) _state.SimulatorEnabled = false;
            Console.WriteLine("alt+q组合键已按下");
        }

        if (dKey == KeyCode.CapsLock && _state.MonitorEnabled)
        {
            e.Next_Hook_Enabled = false;
            return true;
        }

        if (uKey == KeyCode.CapsLock && _state.MonitorEnabled)
        {
            _state.SimulatorEnabled = !_state.SimulatorEnabled;
            e.Next_Hook_Enabled = !_state.MonitorEnabled;
            Console.WriteLine(@$"键盘操作鼠标功能当前状态:{_state.SimulatorEnabled}");
            return true;
        }

        return false;
    }

    // ---------------------------------------------------------------
    // 光标基本移动（J/L/I/K）
    // ---------------------------------------------------------------
    private void HandleMovement(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        switch (dKey)
        {
            case KeyCode.J:
                e.Next_Hook_Enabled = false;
                _mouse.MoveBy(-1, 0);
                break;
            case KeyCode.L:
                e.Next_Hook_Enabled = false;
                _mouse.MoveBy( 1, 0);
                break;
            case KeyCode.I:
                e.Next_Hook_Enabled = false;
                _mouse.MoveBy(0, -1);
                break;
            case KeyCode.K:
                e.Next_Hook_Enabled = false;
                _mouse.MoveBy(0,  1);
                break;
        }
    }

    // ---------------------------------------------------------------
    // 斜向移动（两键同时按住时 x 分量 ×2 以补偿）
    // ---------------------------------------------------------------
    private void HandleDiagonal(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        // 左上
        if ((dKey == KeyCode.J && _state.I) || (dKey == KeyCode.I && _state.J))
        {
            e.Next_Hook_Enabled = false;
            _mouse.MoveBy(-2, -1);
        }
        // 左下
        else if ((dKey == KeyCode.J && _state.K) || (dKey == KeyCode.K && _state.J))
        {
            e.Next_Hook_Enabled = false;
            _mouse.MoveBy(-2,  1);
        }
        // 右下
        else if ((dKey == KeyCode.K && _state.L) || (dKey == KeyCode.L && _state.K))
        {
            e.Next_Hook_Enabled = false;
            _mouse.MoveBy( 2,  1);
        }
        // 右上
        else if ((dKey == KeyCode.I && _state.L) || (dKey == KeyCode.L && _state.I))
        {
            e.Next_Hook_Enabled = false;
            _mouse.MoveBy( 2, -1);
        }
    }

    // ---------------------------------------------------------------
    // 鼠标按钮（左 / 中 / 右，含 ; 与 ' 别名）
    // ---------------------------------------------------------------
    private void HandleClicks(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        // 左键（A / ;）
        if (dKey == KeyCode.A)      { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Left).Invoke(); }
        if (uKey == KeyCode.A)      { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Left).Invoke(); }
        if (dKey == KeyCode.Oem1)   { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Left).Invoke(); }
        if (uKey == KeyCode.Oem1)   { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Left).Invoke(); }

        // 中键（S）
        if (dKey == KeyCode.S)      { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Middle).Invoke(); }
        if (uKey == KeyCode.S)      { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Middle).Invoke(); }

        // 右键（F / '）
        if (dKey == KeyCode.F)      { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Right).Invoke(); }
        if (uKey == KeyCode.F)      { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Right).Invoke(); }
        if (dKey == KeyCode.Oem7)   { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Right).Invoke(); }
        if (uKey == KeyCode.Oem7)   { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Right).Invoke(); }
    }

    // ---------------------------------------------------------------
    // 滚轮（H = 下滑 / O = 上滑）
    // ---------------------------------------------------------------
    private void HandleScroll(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.H)
        {
            e.Next_Hook_Enabled = false;
            _mouse.VerticalScroll(-120);
            Console.WriteLine("当前动作:鼠标滚轮下滑");
        }
        if (dKey == KeyCode.O)
        {
            e.Next_Hook_Enabled = false;
            _mouse.VerticalScroll(120);
            Console.WriteLine("当前动作:鼠标滚轮上滑");
        }
    }

    // ---------------------------------------------------------------
    // 导航键（U→Home / P→End）与速度调节（Space +/-）
    // ---------------------------------------------------------------
    private void HandleNavigation(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.U) { e.Next_Hook_Enabled = false; }
        if (uKey == KeyCode.U) { Simulate.Events().Click(KeyCode.Home).Invoke(); Console.WriteLine("当前动作:Home键"); }

        if (dKey == KeyCode.P) { e.Next_Hook_Enabled = false; }
        if (uKey == KeyCode.P) { Simulate.Events().Click(KeyCode.End).Invoke(); Console.WriteLine("当前动作:End键"); }
    }

    private void HandleSpeed(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (_state.Space && dKey == KeyCode.OemMinus) e.Next_Hook_Enabled = false;
        if (_state.Space && uKey == KeyCode.OemMinus)
        {
            _mouse.ChangeSpeed(increase: false);
            Console.WriteLine("移动速度--");
        }

        if (_state.Space && dKey == KeyCode.Oemplus) e.Next_Hook_Enabled = false;
        if (_state.Space && uKey == KeyCode.Oemplus)
        {
            _mouse.ChangeSpeed(increase: true);
            Console.WriteLine("移动速度++");
        }
    }
}