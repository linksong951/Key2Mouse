namespace Key2Mouse.Mouse;

/// <summary>
/// 鼠标移动速度设置（纯状态 + 档位迁移规则）
/// </summary>
public sealed class MouseSpeedSettings
{
    public const int MinMode = 1;
    public const int MaxMode = 6;

    /// <summary>鼠标移动的基础倍率</summary>
    public int BasicsSpeed { get; private set; } = 12;

    /// <summary>鼠标移动速度挡位（1~6）</summary>
    public int CursorSpeedMode { get; private set; } = 2;

    /// <summary>提高一档；已达上限返回 false</summary>
    public bool Increase()
    {
        if (CursorSpeedMode >= MaxMode) return false;
        CursorSpeedMode++;
        BasicsSpeed *= 2;
        return true;
    }

    /// <summary>降低一档；已达下限返回 false</summary>
    public bool Decrease()
    {
        if (CursorSpeedMode <= MinMode) return false;
        CursorSpeedMode--;
        BasicsSpeed /= 2;
        if (BasicsSpeed == 0) BasicsSpeed = 1;
        return true;
    }
}