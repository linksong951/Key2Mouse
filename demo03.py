from pynput import keyboard, mouse
from pynput.keyboard import GlobalHotKeys, Key
from pynput.mouse import Button
import threading
import time

class MouseControlCenter:
    """
    鼠标控制中心lkjkja
    功能：
    方向控制：
      I 键 -> 鼠标持续向上移动
      J 键 -> 鼠标持续向左移动
      K 键 -> 鼠标持续向下移动
      L 键 -> 鼠标持续向右移动
    
    鼠标按键模拟：
      A 键 -> 模拟鼠标左键（按下/松开）
      F 键 -> 模拟鼠标右键（按下/松开）
    
    程序控制：
      Ctrl+Shift+P -> 暂停/恢复程序
      Shift+ESC    -> 安全退出程序
    """
    
    def __init__(self, move_speed=8, move_interval=0.008):
        # 鼠标控制器
        self.mouse = mouse.Controller()
        
        # 方向键状态 (I=上, J=左, K=下, L=右)
        self.direction_states = {
            'i': False,  # 上
            'j': False,  # 左
            'k': False,  # 下
            'l': False   # 右
        }
        
        # 鼠标按键状态
        self.mouse_button_states = {
            'a': False,  # 鼠标左键状态
            'f': False   # 鼠标右键状态
        }
        
        # 实际的鼠标按键状态（用于防止重复按下）
        self.actual_left_pressed = False
        self.actual_right_pressed = False
        
        # 程序控制状态
        self.is_running = True
        self.is_paused = False
        self.stop_threads = False
        
        # 移动参数
        self.move_speed = move_speed
        self.move_interval = move_interval
        
        # 监听器
        self.hotkey_listener = None
        self.key_listener = None
        self.move_thread = None
        
        # 显示配置
        self.last_display = ""
        
        print("=" * 70)
        print("鼠标控制中心 V4.0")
        print("=" * 70)
        print("方向控制 (按住):")
        print("  [I] 上移动   [J] 左移动   [K] 下移动   [L] 右移动")
        print("")
        print("鼠标按键模拟 (按下=按住对应鼠标键，松开=释放鼠标键):")
        print("  [A] 鼠标左键 (按住A=按住鼠标左键，松开A=释放左键)")
        print("  [F] 鼠标右键 (按住F=按住鼠标右键，松开F=释放右键)")
        print("")
        print("程序控制:")
        print("  [Ctrl+Shift+P] 暂停/恢复程序")
        print("  [Shift+ESC]    安全退出程序")
        print("")
        print("提示: 可以同时按住方向键和鼠标键进行组合操作")
        print("例如: 按住A(左键) + I/J/K/L = 拖动鼠标")
        print("=" * 70)
        print("初始状态: 程序运行中")
        print("=" * 70)

    def calculate_move_vector(self):
        """计算移动向量"""
        x_move = 0
        y_move = 0
        
        if self.direction_states['i']: y_move -= self.move_speed  # 上
        if self.direction_states['k']: y_move += self.move_speed  # 下
        if self.direction_states['j']: x_move -= self.move_speed  # 左
        if self.direction_states['l']: x_move += self.move_speed  # 右
            
        return x_move, y_move

    def update_mouse_buttons(self):
        """更新鼠标按键状态"""
        # 鼠标左键 (A键控制)
        if self.mouse_button_states['a'] and not self.actual_left_pressed:
            self.mouse.press(Button.left)
            self.actual_left_pressed = True
        elif not self.mouse_button_states['a'] and self.actual_left_pressed:
            self.mouse.release(Button.left)
            self.actual_left_pressed = False
            
        # 鼠标右键 (F键控制)
        if self.mouse_button_states['f'] and not self.actual_right_pressed:
            self.mouse.press(Button.right)
            self.actual_right_pressed = True
        elif not self.mouse_button_states['f'] and self.actual_right_pressed:
            self.mouse.release(Button.right)
            self.actual_right_pressed = False

    def move_mouse_loop(self):
        """鼠标控制的主循环"""
        print("[系统] 鼠标控制线程启动")
        
        while not self.stop_threads:
            if not self.is_paused:
                # 1. 更新鼠标按键状态
                self.update_mouse_buttons()
                
                # 2. 处理方向移动
                if any(self.direction_states.values()):
                    x_move, y_move = self.calculate_move_vector()
                    
                    if x_move != 0 or y_move != 0:
                        current_x, current_y = self.mouse.position
                        new_x = current_x + x_move
                        new_y = current_y + y_move
                        self.mouse.position = (new_x, new_y)
                        
                        # 显示状态
                        self.display_status(x_move, y_move, current_x, current_y)
            
            time.sleep(self.move_interval)
            
        print("[系统] 鼠标控制线程停止")

    def display_status(self, x_move, y_move, current_x, current_y):
        """显示当前状态"""
        # 方向状态
        directions = []
        if self.direction_states['i']: directions.append("↑")
        if self.direction_states['k']: directions.append("↓")
        if self.direction_states['j']: directions.append("←")
        if self.direction_states['l']: directions.append("→")
        dir_text = "".join(directions) if directions else "静止"
        
        # 鼠标按键状态
        buttons = []
        if self.mouse_button_states['a']: buttons.append("L")
        if self.mouse_button_states['f']: buttons.append("R")
        btn_text = "+".join(buttons) if buttons else "无"
        
        # 构建显示文本
        display = f"[控制] 方向: {dir_text:<6} 按键: {btn_text:<4} 位置: ({int(current_x):4d}, {int(current_y):4d})"
        
        # 只有状态改变时才刷新显示
        if display != self.last_display:
            print(display, end='' if self.last_display else '\n')
            self.last_display = display

    def on_press(self, key):
        """按键按下事件"""
        if not self.is_running or self.is_paused:
            return
            
        try:
            key_char = key.char.lower()
            
            # 方向键按下
            if key_char in self.direction_states and not self.direction_states[key_char]:
                self.direction_states[key_char] = True
                
            # 鼠标按键按下
            elif key_char in self.mouse_button_states and not self.mouse_button_states[key_char]:
                self.mouse_button_states[key_char] = True
                button_name = "左键" if key_char == 'a' else "右键"
                print(f"[按键] 鼠标{button_name} 按下")
                
        except AttributeError:
            pass

    def on_release(self, key):
        """按键释放事件"""
        try:
            key_char = key.char.lower()
            
            # 方向键释放
            if key_char in self.direction_states and self.direction_states[key_char]:
                self.direction_states[key_char] = False
                
            # 鼠标按键释放
            elif key_char in self.mouse_button_states and self.mouse_button_states[key_char]:
                self.mouse_button_states[key_char] = False
                button_name = "左键" if key_char == 'a' else "右键"
                print(f"[按键] 鼠标{button_name} 释放")
                
        except AttributeError:
            # ESC键退出
            if key == Key.esc:
                self.exit_program()

    def toggle_pause(self):
        """切换暂停状态"""
        self.is_paused = not self.is_paused
        
        if self.is_paused:
            print("\n[状态] 程序已暂停")
            # 暂停时释放所有按键状态
            for key in self.direction_states:
                self.direction_states[key] = False
            for key in self.mouse_button_states:
                self.mouse_button_states[key] = False
            # 释放实际鼠标按键
            if self.actual_left_pressed:
                self.mouse.release(Button.left)
                self.actual_left_pressed = False
            if self.actual_right_pressed:
                self.mouse.release(Button.right)
                self.actual_right_pressed = False
        else:
            print("\n[状态] 程序恢复运行")

    def exit_program(self):
        """安全退出程序"""
        print("\n[系统] 正在退出程序...")
        self.is_running = False
        self.stop_threads = True
        
        # 释放所有鼠标按键
        if self.actual_left_pressed:
            self.mouse.release(Button.left)
        if self.actual_right_pressed:
            self.mouse.release(Button.right)
        
        # 停止监听器
        if self.hotkey_listener:
            self.hotkey_listener.stop()
        if self.key_listener:
            self.key_listener.stop()
        
        # 等待线程
        if self.move_thread and self.move_thread.is_alive():
            self.move_thread.join(timeout=1.0)
        
        print("[系统] 程序已安全退出")

    def setup_hotkeys(self):
        """设置全局热键"""
        hotkey_map = {
            '<ctrl>+<shift>+p': self.toggle_pause,
            '<shift>+<esc>': self.exit_program,
        }
        self.hotkey_listener = GlobalHotKeys(hotkey_map)
        return self.hotkey_listener

    def setup_key_listener(self):
        """设置按键监听器"""
        self.key_listener = keyboard.Listener(
            on_press=self.on_press,
            on_release=self.on_release
        )
        return self.key_listener

    def run(self):
        """主运行函数"""
        try:
            # 启动鼠标控制线程
            self.move_thread = threading.Thread(target=self.move_mouse_loop)
            self.move_thread.daemon = True
            self.move_thread.start()
            
            # 启动热键监听器
            self.setup_hotkeys().start()
            
            # 启动按键监听器
            self.setup_key_listener().start()
            
            print("[系统] 所有监听器已启动")
            print("[提示] 开始使用鼠标控制功能")
            
            # 主循环
            while self.is_running:
                time.sleep(0.5)
                
        except KeyboardInterrupt:
            print("\n[系统] 收到中断信号")
        finally:
            self.exit_program()

# 配置优化版本
class OptimizedMouseControl(MouseControlCenter):
    """优化版本的鼠标控制器"""
    
    def __init__(self):
        super().__init__()
        
        # 更平滑的移动参数
        self.move_speed = 6
        self.move_interval = 0.006
        
        # 添加速度等级
        self.speed_levels = [3, 6, 12, 24]
        self.current_speed_index = 1  # 默认为6像素
        
        # 添加轨迹记录
        self.trajectory = []
        self.record_trajectory = False
        
        print("[优化] 使用平滑移动模式")
        print("[优化] 速度等级: 慢速(3) 中速(6) 快速(12) 极速(24)")
        print("使用 '+' 增加速度，'-' 减少速度")

    def change_speed(self, increase=True):
        """改变移动速度"""
        if increase and self.current_speed_index < len(self.speed_levels) - 1:
            self.current_speed_index += 1
        elif not increase and self.current_speed_index > 0:
            self.current_speed_index -= 1
            
        self.move_speed = self.speed_levels[self.current_speed_index]
        print(f"[速度] 已切换: {self.move_speed} 像素/帧")

    def on_press(self, key):
        """重写按键处理，添加速度控制"""
        try:
            key_char = key.char.lower()
            
            # 速度控制
            if key_char == '+':
                self.change_speed(True)
                return
            elif key_char == '-':
                self.change_speed(False)
                return
                
            # 轨迹记录
            elif key_char == 'r':
                self.record_trajectory = not self.record_trajectory
                status = "开启" if self.record_trajectory else "关闭"
                print(f"[轨迹] 记录{status}")
                return
                
        except AttributeError:
            pass
            
        # 调用父类处理
        super().on_press(key)

# 运行程序
if __name__ == "__main__":
    print("选择控制模式:")
    print("  1. 基础模式 (标准功能)")
    print("  2. 优化模式 (平滑移动 + 速度控制)")
    
    choice = input("请输入选择 (1 或 2): ").strip()
    
    if choice == "2":
        print("\n启动优化模式...")
        controller = OptimizedMouseControl()
    else:
        print("\n启动基础模式...")
        controller = MouseControlCenter()
    
    controller.run()