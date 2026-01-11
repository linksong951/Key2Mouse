using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Input;


namespace demo03;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // 添加一个变量来记录当前工具是否在运行
    private bool _isRunning = false;

    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    // 定义鼠标动作的常量
    private const int MOUSEEVENTF_MOVE = 0x0001;
    private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const int MOUSEEVENTF_LEFTUP = 0x0004;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void StartButton_Click()
    {

    }



    // 这是按钮点击后触发的方法
    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {

        _isRunning = !_isRunning; // 切换状态

        if (_isRunning)
        {
            StatusText.Text = "状态: 监听中...";
            ToggleButton.Content = "停止监听 (快捷键: F2)";
            InfoBox.Text += "\n工具已启动。";
            // 这里将来可以写入开始监听键盘的代码
        }
        else
        {
            StatusText.Text = "状态: 已停止";
            ToggleButton.Content = "启动监听 (快捷键: F2)";
            InfoBox.Text += "\n工具已停止。";
            // 这里将来可以写入停止监听键盘的代码
        }
    }

}