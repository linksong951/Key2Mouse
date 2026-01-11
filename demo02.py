from pynput import keyboard, mouse
from pynput.keyboard import GlobalHotKeys, Key
import threading
import time

class MouseControllerWithPause:
    """
    带暂停功能的鼠标控制器
    功能：
    1. 长按J键持续向左移动鼠标
    2. Ctrl+Shift+P 暂停/恢复程序
    3. Shift+ESC 安全退出程序
    """
    def __init__(self):
        self.mouse = mouse.Controller()
        
        # 程序状态控制
        self.is_running = True      # 程序是否运行
        self.is_paused = False      # 程序是否暂停
        self.is_j_pressed = False   # J键是否被按下
        
        # 线程控制
        self.move_thread = None
        self.stop_threads = False   # 线程停止标志
        
        # 移动参数
        self.move_step = -5         # 向左移动
        self.move_interval = 0.01   # 移动间隔
        
        # 监听器
        self.hotkey_listener = None
        self.key_listener = None
        
        print("=" * 60)
        print("鼠标控制器 V2.0")
        print("功能说明:")
        print("  1. 长按 J 键 -> 鼠标持续向左移动")
        print("  2. Ctrl+Shift+P -> 暂停/恢复程序")
        print("  3. Shift+ESC -> 安全退出程序")
        print("初始状态: 程序运行中")
        print("=" * 60)

    def move_mouse_loop(self):
        """鼠标移动的主循环"""
        while not self.stop_threads:
            if not self.is_paused and self.is_j_pressed:
                # 只有程序运行且未暂停，且J键被按下时才移动
                current_x, current_y = self.mouse.position
                new_x = current_x + self.move_step
                self.mouse.position = (new_x, current_y)
            time.sleep(self.move_interval)
        print("[线程] 鼠标移动线程已停止")

    def on_j_press(self, key):
        """处理J键按下"""
        if not self.is_running or self.is_paused:
            return  # 程序未运行或暂停时不处理
            
        try:
            if key.char.lower() == 'j' and not self.is_j_pressed:
                print(f"[操作] J键按下，开始向左移动鼠标")
                self.is_j_pressed = True
            if key.char.lower() == 'k' and not self.is_j_pressed:
                print(f"[操作] K键按下，开始向下移动鼠标")
                self.is_k_pressed = True
        except AttributeError:
            pass

    def on_j_release(self, key):
        """处理J键释放"""
        try:
            if key.char.lower() == 'j' and self.is_j_pressed:
                print(f"[操作] J键释放，停止鼠标移动")
                self.is_j_pressed = False
        except AttributeError:
            pass
        
        # ESC键退出（普通监听器中的退出方式）
        if key == Key.esc:
            self.exit_program()

    def toggle_pause(self):
        """切换暂停状态"""
        self.is_paused = not self.is_paused
        status = "已暂停" if self.is_paused else "运行中"
        print(f"\n[状态] 程序 {status}")
        
        if self.is_paused:
            # 暂停时立即停止鼠标移动
            self.is_j_pressed = False
            print("[提示] 程序已暂停，所有操作暂时无效")
        else:
            print("[提示] 程序恢复运行")

    def exit_program(self):
        """安全退出程序"""
        print("\n[系统] 正在退出程序...")
        self.is_running = False
        self.is_j_pressed = False
        self.stop_threads = True
        
        # 停止所有监听器
        if self.hotkey_listener:
            self.hotkey_listener.stop()
        if self.key_listener:
            self.key_listener.stop()
        
        # 等待线程结束
        if self.move_thread and self.move_thread.is_alive():
            self.move_thread.join(timeout=1.0)
        
        print("[系统] 程序已安全退出")

    def setup_hotkeys(self):
        """设置全局热键"""
        hotkey_map = {
            # 暂停/恢复热键
            '<ctrl>+<shift>+p': self.toggle_pause,
            # 退出热键
            '<shift>+<esc>': self.exit_program,
        }
        
        self.hotkey_listener = GlobalHotKeys(hotkey_map)
        return self.hotkey_listener

    def setup_key_listener(self):
        """设置J键监听器（用于长按检测）"""
        self.key_listener = keyboard.Listener(
            on_press=self.on_j_press,
            on_release=self.on_j_release
        )
        return self.key_listener

    def run(self):
        """主运行函数"""
        try:
            # 启动鼠标移动线程
            self.move_thread = threading.Thread(target=self.move_mouse_loop)
            self.move_thread.daemon = True  # 设置为守护线程
            self.move_thread.start()
            
            # 启动热键监听器
            hotkey_listener = self.setup_hotkeys()
            hotkey_listener.start()
            
            # 启动J键监听器
            key_listener = self.setup_key_listener()
            key_listener.start()
            
            print("[系统] 所有监听器已启动")
            print("[提示] 按 Ctrl+Shift+P 暂停/恢复程序")
            
            # 保持主线程运行
            while self.is_running:
                time.sleep(0.5)
                
        except KeyboardInterrupt:
            print("\n[系统] 收到中断信号")
        finally:
            self.exit_program()

# 运行程序
if __name__ == "__main__":
    controller = MouseControllerWithPause()
    controller.run()