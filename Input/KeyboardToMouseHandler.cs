using Key2Mouse.Mouse;
using Key2Mouse.Ui;
using WindowsInput;
using WindowsInput.Events;
using WindowsInput.Events.Sources;

namespace Key2Mouse.Input;

/// <summary>核心业务：键盘事件 → 鼠标动作</summary>
public sealed class KeyboardToMouseHandler
{
    private readonly MouseController _mouse;
    private readonly Feedback _feedback;
    private readonly KeyStateTracker _state = new();

    public KeyboardToMouseHandler(MouseController mouse, Feedback feedback)
    {
        _mouse    = mouse;
        _feedback = feedback;
    }

    public void Handle(object sender, EventSourceEventArgs<KeyboardEvent> e)
    {
        var dKey = e.Data?.KeyDown?.Key;
        var uKey = e.Data?.KeyUp?.Key;

        UpdateState(dKey, uKey);

        if (HandleToggles(dKey, uKey, e)) return;

        if (!_state.MonitorEnabled || !_state.SimulatorEnabled) return;

        HandleSpeed(dKey, uKey, e);
        HandleMovement(dKey, e);
        HandleDiagonal(dKey, e);
        HandleClicks(dKey, uKey, e);
        HandleScroll(dKey, e);
        HandleNavigation(dKey, uKey, e);
    }

    private void UpdateState(KeyCode? dKey, KeyCode? uKey)
    {
        if (dKey.HasValue) _state.OnKeyDown(dKey.Value);
        if (uKey.HasValue) _state.OnKeyUp(uKey.Value);
    }

    /// <returns>true 表示 CapsLock 事件被吞掉</returns>
    private bool HandleToggles(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        // Alt+Q：总开关
        if (dKey == KeyCode.Q && _state.LeftAlt)
        {
            _state.MonitorEnabled = !_state.MonitorEnabled;
            if (!_state.MonitorEnabled) _state.SimulatorEnabled = false;

            // _feedback.PlayBeep(_state.MonitorEnabled ? "开启键盘模拟鼠标" : "关闭键盘模拟鼠标");
            _feedback.MonitorStateChanged(_state.MonitorEnabled, _state.SimulatorEnabled);
            return true;
        }

        // CapsLock：按下吞掉，松开切换监听
        if (dKey == KeyCode.CapsLock && _state.MonitorEnabled)
        {
            e.Next_Hook_Enabled = false;
            return true;
        }
        if (uKey == KeyCode.CapsLock && _state.MonitorEnabled)
        {
            _state.SimulatorEnabled = !_state.SimulatorEnabled;
            e.Next_Hook_Enabled = !_state.MonitorEnabled;

            // _feedback.PlayBeep(_state.SimulatorEnabled ? "开启键盘监听" : "关闭键盘监听");
            _feedback.MonitorStateChanged(_state.MonitorEnabled, _state.SimulatorEnabled);
            return true;
        }

        return false;
    }

    private void HandleMovement(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        switch (dKey)
        {
            case KeyCode.J: e.Next_Hook_Enabled = false; _mouse.MoveBy(-1, 0); _feedback.Action("鼠标左移"); break;
            case KeyCode.L: e.Next_Hook_Enabled = false; _mouse.MoveBy( 1, 0); _feedback.Action("鼠标右移"); break;
            case KeyCode.I: e.Next_Hook_Enabled = false; _mouse.MoveBy(0, -1); _feedback.Action("鼠标上移"); break;
            case KeyCode.K: e.Next_Hook_Enabled = false; _mouse.MoveBy(0,  1); _feedback.Action("鼠标下移"); break;
        }
    }

    private void HandleDiagonal(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if ((dKey == KeyCode.J && _state.I) || (dKey == KeyCode.I && _state.J))
        { e.Next_Hook_Enabled = false; _mouse.MoveBy(-2, -1); _feedback.Action("左上斜移"); }
        else if ((dKey == KeyCode.J && _state.K) || (dKey == KeyCode.K && _state.J))
        { e.Next_Hook_Enabled = false; _mouse.MoveBy(-2,  1); _feedback.Action("左下斜移"); }
        else if ((dKey == KeyCode.K && _state.L) || (dKey == KeyCode.L && _state.K))
        { e.Next_Hook_Enabled = false; _mouse.MoveBy( 2,  1); _feedback.Action("右下斜移"); }
        else if ((dKey == KeyCode.I && _state.L) || (dKey == KeyCode.L && _state.I))
        { e.Next_Hook_Enabled = false; _mouse.MoveBy( 2, -1); _feedback.Action("右上斜移"); }
    }

    private void HandleClicks(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.A)    { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Left).Invoke();    _feedback.Action("左键按下"); }
        if (uKey == KeyCode.A)    { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Left).Invoke(); _feedback.Action("左键松开"); }
        if (dKey == KeyCode.Oem1) { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Left).Invoke();    _feedback.Action("左键按下"); }
        if (uKey == KeyCode.Oem1) { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Left).Invoke(); _feedback.Action("左键松开"); }

        if (dKey == KeyCode.S)    { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Middle).Invoke();   _feedback.Action("中键按下"); }
        if (uKey == KeyCode.S)    { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Middle).Invoke(); _feedback.Action("中键松开"); }

        if (dKey == KeyCode.F)    { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Right).Invoke();    _feedback.Action("右键按下"); }
        if (uKey == KeyCode.F)    { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Right).Invoke(); _feedback.Action("右键松开"); }
        if (dKey == KeyCode.Oem7) { e.Next_Hook_Enabled = false; Simulate.Events().Hold(ButtonCode.Right).Invoke();    _feedback.Action("右键按下"); }
        if (uKey == KeyCode.Oem7) { e.Next_Hook_Enabled = false; Simulate.Events().Release(ButtonCode.Right).Invoke(); _feedback.Action("右键松开"); }
    }

    private void HandleScroll(KeyCode? dKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.H)
        {
            e.Next_Hook_Enabled = false;
            _mouse.VerticalScroll(-120);
            _feedback.Action("滚轮下滑");
        }
        if (dKey == KeyCode.O)
        {
            e.Next_Hook_Enabled = false;
            _mouse.VerticalScroll(120);
            _feedback.Action("滚轮上滑");
        }
    }

    private void HandleNavigation(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (dKey == KeyCode.U) { e.Next_Hook_Enabled = false; }
        if (uKey == KeyCode.U) { Simulate.Events().Click(KeyCode.Home).Invoke(); _feedback.Action("Home 键"); }

        if (dKey == KeyCode.P) { e.Next_Hook_Enabled = false; }
        if (uKey == KeyCode.P) { Simulate.Events().Click(KeyCode.End).Invoke(); _feedback.Action("End 键"); }
    }

    private void HandleSpeed(KeyCode? dKey, KeyCode? uKey, EventSourceEventArgs<KeyboardEvent> e)
    {
        if (_state.Space && dKey == KeyCode.OemMinus) e.Next_Hook_Enabled = false;
        if (_state.Space && uKey == KeyCode.OemMinus) _mouse.ChangeSpeed(increase: false);

        if (_state.Space && dKey == KeyCode.Oemplus) e.Next_Hook_Enabled = false;
        if (_state.Space && uKey == KeyCode.Oemplus) _mouse.ChangeSpeed(increase: true);
    }
}