// 模拟鼠标
// 左键、右键
// 点击 ，长按拖动效果
// 上下左右移动

// 分配
// 大小写键切换监听映射开关

// 组合键ctrl shift+k 作为开关

// j键左移动
// l键右移动
// i键上移动
// k键下移动
// a键左键点击
// 注意按键按下代表鼠标对应按键按下松开代表对应按键松开
// s键左键点击
// d键 开启中键滚轮 j上 l下
// f键右键点击

// 实现流程
// 1.监听按键
//     设置开关触发按键
//     设置鼠标按键
// 尽可能不被杀毒软件杀掉
using InputSimulatorEx;

// 创建模拟器实例
var simu01=new InputSimulator();


using (var Keyboard = WindowsInput.Capture.Global.KeyboardAsync()) {
        //Capture all events from the keyboard
        Keyboard.KeyEvent += Keyboard_KeyEvent;
        Console.ReadLine();
    }


// 监听按键按下



