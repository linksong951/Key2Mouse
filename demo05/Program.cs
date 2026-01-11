using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace KeyboardControlMouse
{
    internal class Program
    {
        // ====================== Windows API 声明 ======================
        // 1. 键盘钩子相关API
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13; // 低级键盘钩子
        private const int WM_KEYDOWN = 0x0100; // 按键按下消息

        // 2. 模拟输入相关API (SendInput)
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            // 这里可以添加KEYBDINPUT等，用于模拟键盘
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        // 鼠标事件标志
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int INPUT_MOUSE = 0;

        // 3. 获取当前鼠标位置（可选，用于计算相对移动）
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // ====================== 程序核心逻辑 ======================
        private static IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc = HookCallback; // 必须长期持有委托，防止被GC回收

        static void Main(string[] args)
        {
            Console.WriteLine("=== 原始Windows API键盘控制鼠标示例 ===");
            Console.WriteLine("功能：");
            Console.WriteLine("  方向键 ↑ ↓ ← →  - 移动鼠标");
            Console.WriteLine("  F1 键           - 模拟鼠标左键单击");
            Console.WriteLine("  ESC 键          - 退出程序");
            Console.WriteLine("\n注意：程序需要以管理员权限运行！");
            Console.WriteLine("----------------------------------------");

            // 安装低级键盘钩子
            _hookID = SetHook(_proc);
            if (_hookID == IntPtr.Zero)
            {
                Console.WriteLine("设置钩子失败！请尝试以管理员身份重新运行。");
                return;
            }

            Console.WriteLine("钩子安装成功，开始监听键盘...");

            // 使用消息循环保持钩子激活
            Application.Run(); // 需要引用 System.Windows.Forms

            // 程序退出前清理
            UnhookWindowsHookEx(_hookID);
            Console.WriteLine("程序已退出。");
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curModule = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                // 获取当前模块句柄，并安装全局低级键盘钩子
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                // 从lParam中读取按键的虚拟键码
                int vkCode = Marshal.ReadInt32(lParam);

                // 根据按键执行相应鼠标操作
                switch (vkCode) 
                {
                    case 37: // 左箭头 Left Arrow
                        MoveMouse(-10, 0); // 向左移动10单位
                        break;
                    case 38: // 上箭头 Up Arrow
                        MoveMouse(0, -10); // 向上移动10单位
                        break;
                    case 39: // 右箭头 Right Arrow
                        MoveMouse(10, 0); // 向右移动10单位
                        break;
                    case 40: // 下箭头 Down Arrow
                        MoveMouse(0, 10); // 向下移动10单位
                        break;
                    case 112: // F1
                        SimulateMouseClick();
                        Console.WriteLine("鼠标左键单击");
                        break;
                    case 27: // ESC
                        Console.WriteLine("ESC键按下，退出程序...");
                        Application.Exit(); // 退出消息循环
                        break;
                    
                }
            }
            // 必须将消息传递给钩子链中的下一个钩子
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // 移动鼠标（相对移动）
        private static void MoveMouse(int deltaX, int deltaY)
        {
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.U.mi.dx = deltaX;
            input.U.mi.dy = deltaY;
            input.U.mi.dwFlags = MOUSEEVENTF_MOVE;
            input.U.mi.time = 0;
            input.U.mi.dwExtraInfo = IntPtr.Zero;

            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        // 模拟鼠标左键单击
        private static void SimulateMouseClick()
        {
            // 按下
            INPUT mouseDownInput = new INPUT();
            mouseDownInput.type = INPUT_MOUSE;
            mouseDownInput.U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            SendInput(1, ref mouseDownInput, Marshal.SizeOf(mouseDownInput));

            // 释放（可加入短暂延迟使点击更真实）
            System.Threading.Thread.Sleep(50);
            INPUT mouseUpInput = new INPUT();
            mouseUpInput.type = INPUT_MOUSE;
            mouseUpInput.U.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            SendInput(1, ref mouseUpInput, Marshal.SizeOf(mouseUpInput));
        }
    }
}