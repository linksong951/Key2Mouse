namespace demo02;

using InputSimulatorEx;
using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
 // 导入Windows API函数，用于获取按键的异步状态
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    
    // J键的虚拟键码 (Virtual-Key Code)
    private const int VK_J = 0x4A;
    // Q键的虚拟键码，用于退出
    private const int VK_Q = 0x51;



    static void Main(string[] args)
    {
        var inpt =new InputSimulator();

         double Cursor_x=0;
         double Cursor_y=0;


        bool running = true;
        while(running)
        {
            // 以10ms为间隔 减少cpu占用
            Thread.Sleep(10);
            // 检查j键是否被按下 并判断最高位是否为1
            if ((GetAsyncKeyState(VK_J) & 0x8000) != 0)
            {
                Cursor_x=Cursor_x+ 1500;
                inpt.Mouse.MoveMouseTo(Cursor_x,Cursor_y);
                System.Console.WriteLine("j键被按下");
                Thread.Sleep(200);
            }
            if ((GetAsyncKeyState(VK_Q) & 0x8000) != 0)
            {
                System.Console.WriteLine("退出程序");
                running=false;
            }
        }
    }
}
