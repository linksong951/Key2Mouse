using WindowsInput.Events;

namespace Key2Mouse.Input;

/// <summary>
/// 键盘状态跟踪器：只负责记录 & 暴露状态，不做任何业务决策。
/// </summary>
public sealed class KeyStateTracker
{
    public bool LeftAlt { get; private set; }
    public bool Space   { get; private set; }
    public bool J       { get; private set; }
    public bool K       { get; private set; }
    public bool L       { get; private set; }
    public bool I       { get; private set; }

    /// <summary>键盘操作鼠标总开关（原 KeyMonitorState）</summary>
    public bool MonitorEnabled { get; set; }

    /// <summary>当前是否处于"键盘模拟鼠标"生效态（原 KeyMonitorState2）</summary>
    public bool SimulatorEnabled { get; set; }

    public void OnKeyDown(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LAlt:  LeftAlt = true; break;
            case KeyCode.Space: Space   = true; break;
            case KeyCode.J:     J       = true; break;
            case KeyCode.K:     K       = true; break;
            case KeyCode.L:     L       = true; break;
            case KeyCode.I:     I       = true; break;
        }
    }

    public void OnKeyUp(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LAlt:  LeftAlt = false; break;
            case KeyCode.Space: Space   = false; break;
            case KeyCode.J:     J       = false; break;
            case KeyCode.K:     K       = false; break;
            case KeyCode.L:     L       = false; break;
            case KeyCode.I:     I       = false; break;
        }
    }
}