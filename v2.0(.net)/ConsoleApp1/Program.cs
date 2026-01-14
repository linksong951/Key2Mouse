using System.Windows.Input;
using WindowsInput.Events;
using WindowsInput.Events.Sources;
using System.Runtime.InteropServices;
using System.Drawing;
using WindowsInput;

namespace ConsoleApp1;

class Program
{

    #region 按键状态

    /// <summary>
    /// 键盘操作鼠标监听是否开启
    /// </summary>
    private static bool KeyMonitorState = false;

    /// <summary>
    /// 映射模式 true为模式1 false为模式2
    /// </summary>
    private static bool CurrentMode = true;

    /// <summary>
    /// 左Alt状态
    /// </summary>
    private static bool LeftAltState { get; set; }

    /// <summary>
    /// 空格状态
    /// </summary>
    private static bool SpaceState { get; set; }

    /// <summary>
    /// J键状态
    /// </summary>
    private static bool KeyJState { get; set; }

    /// <summary>
    /// K键状态
    /// </summary>
    private static bool KeyKState { get; set; }

    /// <summary>
    /// L键状态
    /// </summary>
    private static bool KeyLState { get; set; }

    /// <summary>
    /// I键状态
    /// </summary>
    private static bool KeyIState { get; set; }
    #endregion

    #region 鼠标相关变量
    /// <summary>
    /// 鼠标移动的基础倍率
    /// </summary>
    private static int BasicsSpeed = 5;

    /// <summary>
    /// 鼠标移动速度 正数
    /// </summary>
    private static int MouseMovementSpeed1 = 3;

    /// <summary>
    /// 鼠标移动速度 负数
    /// </summary>
    private static int MouseMovementSpeed2 = -3;

    /// <summary>
    /// 鼠标移动速度 (左上,左下,右上,右下优化) 正数
    /// </summary>
    private static int MouseMovementSpeed3 = 3;

    /// <summary>
    /// 鼠标移动速度 (左上,左下,右上,右下优化) 负数
    /// </summary>
    private static int MouseMovementSpeed4 = -3;
    #endregion

    // 1. 定义与C语言中POINT结构对应的结构体
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // 定义鼠标事件常量
    private const uint MOUSEEVENTF_WHEEL = 0x0800; // 垂直滚轮
    private const uint MOUSEEVENTF_HWHEEL = 0x01000; // 水平滚轮

    // 2. 导入 user32.dll 中的 GetCursorPos 函数[citation:9]
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    // 导入user32.dll中的mouse_event函数
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

    static async Task Main(string[] args)
    {
        // 创建监听
        using (var keyboard = WindowsInput.Capture.Global.KeyboardAsync())
        {
            keyboard.KeyEvent += Keyboard_keyEvent;
            Console.WriteLine("加载完毕!!");
            Console.ReadLine();
        }
    }

    #region windowinput库之外自己写或引入的方法
    /// <summary>
    /// 获取鼠标xy轴坐标并更新到定义的鼠标xy变量中
    /// </summary>
    private static POINT GetCursor_xy()
    {
        POINT cursorPos;
        // 3. 调用函数获取坐标[citation:6][citation:10]
        GetCursorPos(out cursorPos);
        Console.WriteLine($"鼠标坐标：({cursorPos.X}, {cursorPos.Y})");
        return cursorPos;
    }

    /// <summary>
    /// 模拟鼠标滚轮垂直滚动
    /// </summary>
    /// <param name="delta">滚动量：正数向上，负数向下。120为一个标准刻度。</param>
    public static void VerticalScroll(int delta)
    {
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);
    }

    /// <summary>
    /// 模拟鼠标滚轮水平滚动
    /// </summary>
    /// <param name="delta">滚动量：正数向右，负数向左。</param>
    public static void HorizontalScroll(int delta)
    {
        mouse_event(MOUSEEVENTF_HWHEEL, 0, 0, (uint)delta, UIntPtr.Zero);
    }

    /// <summary>
    /// 更新鼠标坐标
    /// </summary>
    /// <param name="increatex">要增加或修改的x轴方向</param>
    /// <param name="increateY">要增加或修改的y轴方向</param>
    private static void PutCursor_xy(int increatex, int increateY)
    {
        POINT cursorPos = GetCursor_xy();
        int x = cursorPos.X;
        int y = cursorPos.Y;
        x += increatex * BasicsSpeed;
        y += increateY * BasicsSpeed;
        // 判断x和y最后位置任意一个是负的就默认为0
        if (x < 0)
        {
            x = 0;
        }
        if (y < 0)
        {
            y = 0;
        }
        WindowsInput.Simulate.Events()
            .MoveTo(x, y)
            .Invoke();
    }
    #endregion

    private static void Keyboard_keyEvent(object sender, EventSourceEventArgs<KeyboardEvent> e)
    {
        e.Next_Hook_Enabled = false;
        var Dkey = e.Data?.KeyDown?.Key;
        var Ukey = e.Data?.KeyUp?.Key;

        // 防止测试时候老是乱写东西出来
        // 使用状态变更实现组合键功能

        #region 按键状态更新
        // LAlt按下
        if (Dkey == KeyCode.LAlt)
        {
            LeftAltState = true;
            System.Console.WriteLine("LAlt按下");
        }
        // LAlt松开
        if (Ukey == KeyCode.LAlt)
        {
            LeftAltState = false;
            System.Console.WriteLine("LAlt松开");
        }
        // 按键J按下
        if (Dkey == KeyCode.J)
        {
            KeyJState = true;
            System.Console.WriteLine("按键J按下");
        }
        // 按键J松开
        if (Ukey == KeyCode.J)
        {
            KeyJState = false;
            System.Console.WriteLine("按键J松开");
        }
        // 按键K按下
        if (Dkey == KeyCode.K)
        {
            KeyKState = true;
            System.Console.WriteLine("按键K按下");
        }
        // 按键K松开
        if (Ukey == KeyCode.K)
        {
            KeyKState = false;
            System.Console.WriteLine("按键K松开");
        }
        // 按键I按下
        if (Dkey == KeyCode.I)
        {
            KeyIState = true;
            System.Console.WriteLine("按键I按下");
        }
        // 按键I松开
        if (Ukey == KeyCode.I)
        {
            KeyIState = false;
            System.Console.WriteLine("按键I松开");
        }
        // 按键L按下
        if (Dkey == KeyCode.L)
        {
            KeyLState = true;
            System.Console.WriteLine("按键L按下");
        }
        // 按键L松开
        if (Ukey == KeyCode.L)
        {
            KeyLState = false;
            System.Console.WriteLine("按键L松开");
        }
        // 按键Space按下
        if (Dkey == KeyCode.Space)
        {
            BasicsSpeed = 9;
            SpaceState = true;
            System.Console.WriteLine("按键Space按下");
        }
        // 按键Space松开
        if (Ukey == KeyCode.Space)
        {
            BasicsSpeed = 3;
            SpaceState = false;
            System.Console.WriteLine("按键Space松开");
        }
        #endregion

        #region 组合键处理
        // 监听alt+q组合键
        if (Dkey == KeyCode.Q && LeftAltState)
        {
            // 取反
            KeyMonitorState = !KeyMonitorState;
            System.Console.WriteLine(@$"键盘操作鼠标功能当前状态:{KeyMonitorState}");
            System.Console.WriteLine("alt+q组合键已按下");
        }
        #endregion
        // 切换模式
        if (Dkey == KeyCode.CapsLock && KeyMonitorState)
        {
            CurrentMode = !CurrentMode;
            string r = CurrentMode == true ? "模式1" : "模式2";
            System.Console.WriteLine($"--切换为模式:{r}");
        }

        #region 模式1 
        // 键盘监听状态变量为true并且映射模式为true(1)
        if (KeyMonitorState && CurrentMode)
        {
            // 鼠标左移动
            if (Dkey == KeyCode.J)
            {
                int x = MouseMovementSpeed2;
                PutCursor_xy(x, 0);
                System.Console.WriteLine($"--模式1--鼠标左移动:{x * BasicsSpeed} 坐标");
            }
            // 鼠标右移动
            if (Dkey == KeyCode.L)
            {
                int x = MouseMovementSpeed1;
                PutCursor_xy(x, 0);
                System.Console.WriteLine($"--模式1--鼠标右移动:{x * BasicsSpeed} 坐标");
            }
            // 鼠标上移动
            if (Dkey == KeyCode.I)
            {
                int y = MouseMovementSpeed2;
                PutCursor_xy(0, y);
                System.Console.WriteLine($"--模式1--鼠标上移动:{y * BasicsSpeed} 坐标");
            }
            // 鼠标下移动
            if (Dkey == KeyCode.K)
            {
                int y = MouseMovementSpeed1;

                PutCursor_xy(0, y);
                System.Console.WriteLine($"--模式1--鼠标下移动:{y * BasicsSpeed} 坐标");
            }
            // 鼠标左键按下
            if (Dkey == KeyCode.A)
            {
                WindowsInput.Simulate.Events()
                        .Hold(ButtonCode.Left)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标左键按下");
            }
            // 鼠标左键松开
            if (Ukey == KeyCode.A)
            {
                WindowsInput.Simulate.Events()
                        .Release(ButtonCode.Left)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标左键松开");
            }
            // 鼠标中键按下
            if (Dkey == KeyCode.S)
            {
                WindowsInput.Simulate.Events()
                        .Hold(ButtonCode.Middle)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标中键按下");
            }
            // 鼠标中键松开
            if (Ukey == KeyCode.S)
            {
                WindowsInput.Simulate.Events()
                        .Release(ButtonCode.Middle)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标中键松开");
            }
            // 鼠标右键按下
            if (Dkey == KeyCode.F)
            {
                WindowsInput.Simulate.Events()
                        .Hold(ButtonCode.Right)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标右键按下");
            }
            // 鼠标右键松开
            if (Ukey == KeyCode.F)
            {
                WindowsInput.Simulate.Events()
                        .Release(ButtonCode.Right)
                        .Invoke();
                System.Console.WriteLine("当前动作:鼠标右键松开");
            }
            // 鼠标滚轮上滑
            if (Dkey == KeyCode.H)
            {
                VerticalScroll(-120);
                System.Console.WriteLine("当前动作:鼠标滚轮上滑");
            }
            // 鼠标滚轮下滑
            if (Dkey == KeyCode.O)
            {
                VerticalScroll(120);
                System.Console.WriteLine("当前动作:鼠标滚轮下滑");
            }

            #region 优化操作体验
            // 左上角移动
            if (Dkey == KeyCode.J && KeyIState || Dkey == KeyCode.I && KeyJState)
            {
                int x = MouseMovementSpeed4;
                int y = MouseMovementSpeed4;
                PutCursor_xy(x, y);
                System.Console.WriteLine($"--模式1--左上角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 左下角移动
            if (Dkey == KeyCode.J && KeyKState || Dkey == KeyCode.K && KeyJState)
            {
                int x = MouseMovementSpeed4;
                int y = MouseMovementSpeed3;
                PutCursor_xy(x, y);
                System.Console.WriteLine($"--模式1--左下角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 右下角移动
            if (Dkey == KeyCode.K && KeyLState || Dkey == KeyCode.L && KeyKState)
            {
                int x = MouseMovementSpeed3;
                int y = MouseMovementSpeed3;
                PutCursor_xy(x, y);
                System.Console.WriteLine($"--模式1--右下角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 右上角移动
            if (Dkey == KeyCode.I && KeyLState || Dkey == KeyCode.L && KeyIState)
            {
                int x = MouseMovementSpeed3;
                int y = MouseMovementSpeed4;
                PutCursor_xy(x, y);
                System.Console.WriteLine($"--模式1--右上角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            #endregion
        }
        #endregion
        #region 模式2

        #endregion
    }


}
