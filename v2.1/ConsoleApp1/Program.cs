using WindowsInput;
using WindowsInput.Events;
using WindowsInput.Events.Sources;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Media;
using System.Reflection;

namespace ConsoleApp1;

class Program
{


    #region 按键状态

    /// <summary>
    /// 键盘操作鼠标监听是否开启
    /// </summary>
    private static bool KeyMonitorState = false;

    /// <summary>
    /// 键盘操作鼠标监听是否开启2
    /// </summary>
    private static bool KeyMonitorState2 = false;

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
    private static int BasicsSpeed = 12;

    /// <summary>                                
    /// 鼠标移动速度模式挡位 
    /// </summary>
    private static int CursorSpeedMode = 2;
    /// <summary>
    /// 鼠标移动速度 正数
    /// </summary>
    private static int MouseMovementSpeed1 = 1;

    /// <summary>
    /// 鼠标移动速度 负数
    /// </summary>
    private static int MouseMovementSpeed2 = -1;

    /// <summary>
    /// 鼠标移动速度 (左上,左下,右上,右下优化) 正数
    /// </summary>
    private static int MouseMovementSpeed3 = 1;

    /// <summary>
    /// 鼠标移动速度 (左上,左下,右上,右下优化) 负数
    /// </summary>
    private static int MouseMovementSpeed4 = -1;
    #endregion 

    /// <summary>
    /// 提示音
    /// </summary>
    private static void PlayEmbeddedBeep(string name)
    {
        // 注意：假设文件已转换为 1.wav
        string resourceName = $"ConsoleApp1.Audio.{name}.wav";
        var assembly = Assembly.GetExecutingAssembly();

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                using (var player = new SoundPlayer(stream))
                {
                    player.Play(); // 异步播放
                }
            }
            else
            {
                Console.WriteLine($"未找到嵌入式资源: {resourceName}");
                // 可列出所有资源名以调试
                // foreach (var name in assembly.GetManifestResourceNames()) Console.WriteLine(name);
            }
        }
    }
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
    /// 鼠标移动速度变更
    /// </summary>
    /// <param name="i">true为增加false为减少</param>
    private static void PutCursorSpeed(bool i)
    {
        // 加挡位
        if (i)
        {
            if (CursorSpeedMode == 6)
            {
                System.Console.WriteLine("已经是最高挡位了");
            }
            else
            {
                CursorSpeedMode++;
                BasicsSpeed *= 2;
            }
        }
        // 减挡位
        else
        {
            if (CursorSpeedMode == 1)
            {
                System.Console.WriteLine("已经是最低挡位了");
            }
            else
            {
                CursorSpeedMode--;
                BasicsSpeed /= 2;
                if (BasicsSpeed == 0)
                {
                    BasicsSpeed = 1;
                }
            }
        }
        string d = CursorSpeedMode.ToString();
        PlayEmbeddedBeep(d);
        System.Console.WriteLine($"当前挡位:{CursorSpeedMode}");
        System.Console.WriteLine($"基础速率:{BasicsSpeed}");
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
        Simulate.Events()
            .MoveTo(x, y)
            .Invoke();
    }
    #endregion

    private static void Keyboard_keyEvent(object sender, EventSourceEventArgs<KeyboardEvent> e)
    {
        var Dkey = e.Data?.KeyDown?.Key;
        var Ukey = e.Data?.KeyUp?.Key;

        // Console.WriteLine($"按下:{Dkey}");
        // Console.WriteLine($"按下:{Ukey}");

        #region 按键状态更新
        // LAlt按下
        if (Dkey == KeyCode.LAlt)
        {
            LeftAltState = true;
            Console.WriteLine($"LeftAltState:{LeftAltState}");
        }
        // LAlt松开
        if (Ukey == KeyCode.LAlt)
        {
            LeftAltState = false;
            Console.WriteLine($"LeftAltState:{LeftAltState}");
        }
        // 按键J按下
        if (Dkey == KeyCode.J)
        {
            KeyJState = true;
            Console.WriteLine($"KeyJState:{KeyJState}");
        }
        // 按键J松开
        if (Ukey == KeyCode.J)
        {
            KeyJState = false;
            Console.WriteLine($"KeyJState:{KeyJState}");
        }
        // 按键K按下
        if (Dkey == KeyCode.K)
        {
            KeyKState = true;
            Console.WriteLine($"KeyKState:{KeyKState}");
        }
        // 按键K松开
        if (Ukey == KeyCode.K)
        {
            KeyKState = false;
            Console.WriteLine($"KeyKState:{KeyKState}");
        }
        // 按键I按下
        if (Dkey == KeyCode.I)
        {
            KeyIState = true;
            Console.WriteLine($"KeyIState:{KeyIState}");
        }
        // 按键I松开
        if (Ukey == KeyCode.I)
        {
            KeyIState = false;
            Console.WriteLine($"KeyIState:{KeyIState}");
        }
        // 按键L按下
        if (Dkey == KeyCode.L)
        {
            KeyLState = true;
            Console.WriteLine($"KeyLState:{KeyLState}");
        }
        // 按键L松开
        if (Ukey == KeyCode.L)
        {
            KeyLState = false;
            Console.WriteLine($"KeyLState:{KeyLState}");
        }
        // 按键Space按下
        if (Dkey == KeyCode.Space)
        {
            SpaceState = true;
            Console.WriteLine($"SpaceState:{SpaceState}");
        }
        // 按键Space松开
        if (Ukey == KeyCode.Space)
        {
            SpaceState = false;
            Console.WriteLine($"SpaceState:{SpaceState}");
        }
        #endregion

        #region 组合键处U理 
        // 监听alt+q组合键
        if (Dkey == KeyCode.Q && LeftAltState)
        {
            // 取反
            KeyMonitorState = !KeyMonitorState;
            if (!KeyMonitorState)
            {  
                KeyMonitorState2 = false;
                // 播报关闭键盘模拟鼠标
                PlayEmbeddedBeep("关闭键盘模拟鼠标");
            }
            else
            {
                // 播报开启键盘模拟鼠标
                PlayEmbeddedBeep("开启键盘模拟鼠标"); 
            }
            Console.WriteLine("alt+q组合键已按 下");
        }
        // 监听CapsLock键
        if (Dkey == KeyCode.CapsLock && KeyMonitorState)
        {
            e.Next_Hook_Enabled = false;
        }
        if (Ukey == KeyCode.CapsLock && KeyMonitorState)
        {
            KeyMonitorState2 = !KeyMonitorState2;
            e.Next_Hook_Enabled = !KeyMonitorState;
            if (KeyMonitorState2)
            {
                PlayEmbeddedBeep("开启键盘监听");
            }
            else
            {
                PlayEmbeddedBeep("关闭键盘监听");
            }
            Console.WriteLine(@$"键盘操作鼠标功能当前状态:{KeyMonitorState2}");
        }
        #endregion

        // 键盘监听状态变量为true
        if (KeyMonitorState && KeyMonitorState2)
        {
            // 鼠标左移动
            if (Dkey == KeyCode.J)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed2;
                PutCursor_xy(x, 0);
                Console.WriteLine($"--模式1--鼠标左移动:{x * BasicsSpeed} 坐标");
            }
            // 鼠标右移动
            if (Dkey == KeyCode.L)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed1;
                PutCursor_xy(x, 0);
                Console.WriteLine($"--模式1--鼠标右移动:{x * BasicsSpeed} 坐标");
            }
            // 鼠标上移动
            if (Dkey == KeyCode.I)
            {
                e.Next_Hook_Enabled = false;
                int y = MouseMovementSpeed2;
                PutCursor_xy(0, y);
                Console.WriteLine($"--模式1--鼠标上移动:{y * BasicsSpeed} 坐标");
            }
            // 鼠标下移动
            if (Dkey == KeyCode.K)
            {
                e.Next_Hook_Enabled = false;
                int y = MouseMovementSpeed1;
                PutCursor_xy(0, y);
                Console.WriteLine($"--模式1--鼠标下移动:{y * BasicsSpeed} 坐标");
            }
            // 鼠标左键按下
            if (Dkey == KeyCode.A)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Hold(ButtonCode.Left)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标左键按下");
            }
            // 鼠标左键松开
            if (Ukey == KeyCode.A)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Release(ButtonCode.Left)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标左键松开");
            }
            // 鼠标中键按下
            if (Dkey == KeyCode.S)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Hold(ButtonCode.Middle)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标中键按下");
            }
            // 鼠标中键松开
            if (Ukey == KeyCode.S)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Release(ButtonCode.Middle)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标中键松开");
            }
            // 鼠标右键按下
            if (Dkey == KeyCode.F)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Hold(ButtonCode.Right)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标右键按下");
            }
            // 鼠标右键松开
            if (Ukey == KeyCode.F)
            {
                e.Next_Hook_Enabled = false;
                Simulate.Events()
                        .Release(ButtonCode.Right)
                        .Invoke();
                Console.WriteLine("当前动作:鼠标右键松开");
            }
            // 鼠标滚轮上滑
            if (Dkey == KeyCode.H)
            {
                e.Next_Hook_Enabled = false;
                VerticalScroll(-120);
                Console.WriteLine("当前动作:鼠标滚轮上滑");
            }
            // 鼠标滚轮下滑
            if (Dkey == KeyCode.O)
            {
                e.Next_Hook_Enabled = false;
                VerticalScroll(120);
                Console.WriteLine("当前动作:鼠标滚轮下滑");
            }
            // Home键
            if (Dkey == KeyCode.U)
            {
                e.Next_Hook_Enabled = false;
            }
            if (Ukey == KeyCode.U)
            {
                Simulate.Events()
                    .Click(KeyCode.Home)
                    .Invoke();
                Console.WriteLine("当前动作:Home键");
            }
            // End键
            if (Dkey == KeyCode.P)
            {
                e.Next_Hook_Enabled = false;
            }
            if (Ukey == KeyCode.P)
            {
                Simulate.Events()
                    .Click(KeyCode.End)
                    .Invoke();
                Console.WriteLine("当前动作:End键");
            }
            #region 按下空格键+其他键

            #region 鼠标移动速度--
            if (SpaceState && Dkey == KeyCode.OemMinus)
            {
                e.Next_Hook_Enabled = false;
            }
            if (SpaceState && Ukey == KeyCode.OemMinus)
            {
                PutCursorSpeed(false);
                System.Console.WriteLine("移动速度--");
            }
            #endregion  

            #region 鼠标移动速度+
            if (SpaceState && Dkey == KeyCode.Oemplus)
            {
                e.Next_Hook_Enabled = false;
            }
            if (SpaceState && Ukey == KeyCode.Oemplus)
            {
                PutCursorSpeed(true);
                System.Console.WriteLine("移动速度++");
            }
            #endregion


            #endregion
            #region 优化操作体验
            // 左上角移动
            if (Dkey == KeyCode.J && KeyIState || Dkey == KeyCode.I && KeyJState)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed4 * 2;
                int y = MouseMovementSpeed4;
                PutCursor_xy(x, y);
                Console.WriteLine($"--模式1--左上角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 左下角移动
            if (Dkey == KeyCode.J && KeyKState || Dkey == KeyCode.K && KeyJState)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed4 * 2;
                int y = MouseMovementSpeed3;
                PutCursor_xy(x, y);
                Console.WriteLine($"--模式1--左下角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 右下角移动
            if (Dkey == KeyCode.K && KeyLState || Dkey == KeyCode.L && KeyKState)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed3 * 2;
                int y = MouseMovementSpeed3;
                PutCursor_xy(x, y);
                Console.WriteLine($"--模式1--右下角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            // 右上角移动
            if (Dkey == KeyCode.I && KeyLState || Dkey == KeyCode.L && KeyIState)
            {
                e.Next_Hook_Enabled = false;
                int x = MouseMovementSpeed3 * 2;
                int y = MouseMovementSpeed4;
                PutCursor_xy(x, y);
                Console.WriteLine($"--模式1--右上角移动=== x:{x * BasicsSpeed} === y:{y * BasicsSpeed}");
            }
            #endregion
        }
    }
}
