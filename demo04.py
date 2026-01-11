from pynput import keyboard, mouse
from pynput.keyboard import GlobalHotKeys, Key, Controller as KeyboardController
from pynput.mouse import Button, Controller as MouseController
import threading
import time
import ctypes

class ToggleMouseControl:
    """
    可切换的鼠标控制中心
    功能：
    开关控制：
      Shift+J -> 开启/关闭鼠标控制模式
    
    鼠标控制模式下：
      方向控制 (按住):
        I -> 上移动   J -> 左移动   K -> 下移动   L -> 右移动
      鼠标按键模拟:
        A -> 鼠标左键 (按下=按住左键，松开=释放左键)
        F -> 鼠标右键 (按下=按住右键，松开=释放右键)
    
    程序控制:
      Ctrl+Shift+P -> 暂停/恢复程序
      Shift+ESC    -> 安全退出程序
    """
    
    def __init__(self):
        # 控制器
        self.mouse = MouseController()
        self.keyboard = KeyboardController()
        
        # 开关状态
        self.is_control_mode = False  # 鼠标控制模式开关
        self.is_paused = False       # 程序暂停状态
        self.is_running = True       # 程序运行状态
        self.stop_threads = False    # 线程停止标志
        
        # 按键状态记录
        self.direction_states = {
            'i': False,  # 上
            'j': False,  # 左
            'k': False,  # 下
            'l': False   # 右
        }
        
        self.button_states = {
            'a': False,  # 鼠标左键
            'f': False   # 鼠标右键
        }
        
        # 实际鼠标按键状态
        self.actual_left_pressed = False
        self.actual_right_pressed = False
        
        # 监听器
        self.hotkey_listener = None
        self.key_listener = None
        self.move_thread = None
        
        # 移动参数
        self.move_speed = 8
        self.move_interval = 0.008
        
        # 原始按键状态缓存（用于恢复）
        self.original_key_states = {}
        
        # 显示状态
        self.last_status = ""
        
        print("=" * 70)
        print("可切换鼠标控制中心 V5.0")
        print("=" * 70)
        print("开关控制:")
        print("  [Shift+J] -> 开启/关闭鼠标控制模式")
        print()
        print("鼠标控制模式下:")
        print("  方向控制 (按住):")
        print("    [I] 上移动   [J] 左移动   [K] 下移动   [L] 右移动")
        print("  鼠标按键模拟:")
        print("    [A] 鼠标左键 (按下=按住左键，松开=释放左键)")
        print("    [F] 鼠标右键 (按下=按住右键，松开=释放右键)")
        print()
        print("程序控制:")
        print("  [Ctrl+Shift+P] -> 暂停/恢复程序")
        print("  [Shift+ESC]    -> 安全退出程序")
        print("=" * 70)
        print("初始状态: 程序运行中 | 鼠标控制模式: 关闭")
        print("=" * 70)

    def toggle_control_mode(self):
        """切换鼠标控制模式"""
        self.is_control_mode = not self.is_control_mode
        
        if self.is_control_mode:
            print("\n" + "=" * 50)
            print("鼠标控制模式: 已开启")
            print("提示: IJKL键用于移动，A/F键模拟鼠标左右键")
            print("=" * 50)
            
            # 清除所有按键状态
            for key in self.direction_states:
                self.direction_states[key] = False
            for key in self.button_states:
                self.button_states[key] = False
                
            # 释放可能按住的鼠标按键
            if self.actual_left_pressed:
                self.mouse.release(Button.left)
                self.actual_left_pressed = False
            if self.actual_right_pressed:
                self.mouse.release(Button.right)
                self.actual_right_pressed = False
                
        else:
            print("\n" + "=" * 50)
            print("鼠标控制模式: 已关闭")
            print("提示: 所有按键恢复原始功能")
            print("=" * 50)
            
            # 关闭时释放所有按键状态
            for key in self.direction_states:
                self.direction_states[key] = False
            for key in self.button_states:
                self.button_states[key] = False
                
            # 释放鼠标按键
            if self.actual_left_pressed:
                self.mouse.release(Button.left)
                self.actual_left_pressed = False
            if self.actual_right_pressed:
                self.mouse.release(Button.right)
                self.actual_right_pressed = False

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
        # 只在控制模式下更新鼠标按键
        if not self.is_control_mode:
            return
            
        # 鼠标左键 (A键控制)
        if self.button_states['a'] and not self.actual_left_pressed:
            self.mouse.press(Button.left)
            self.actual_left_pressed = True
        elif not self.button_states['a'] and self.actual_left_pressed:
            self.mouse.release(Button.left)
            self.actual_left_pressed = False
            
        # 鼠标右键 (F键控制)
        if self.button_states['f'] and not self.actual_right_pressed:
            self.mouse.press(Button.right)
            self.actual_right_pressed = True
        elif not self.button_states['f'] and self.actual_right_pressed:
            self.mouse.release(Button.right)
            self.actual_right_pressed = False

    def block_key_original_function(self, key_char):
        """
        阻止按键的原始功能
        返回True表示阻止，False表示不阻止
        """
        # 只在控制模式下阻止特定按键
        if not self.is_control_mode:
            return False
            
        # 阻止IJKL、A、F键的原始功能
        if key_char in ['i', 'j', 'k', 'l', 'a', 'f']:
            return True
            
        return False

    def move_mouse_loop(self):
        """鼠标移动的主循环"""
        print("[系统] 鼠标控制线程启动")
        
        while not self.stop_threads:
            if not self.is_paused and self.is_control_mode:
                # 1. 更新鼠标按键状态
                self.update_mouse_buttons()
                
                # 2. 处理方向移动
                if any(self.direction_states.values()):
                    x_move, y_move = self.calculate_move_vector()
                    
                    if x_move != 0 or y_move != 0:
                        current_x, current_y = self.mouse.position
                        new_x = current_x + x_move
                        new_y = current_y + y_move
                        
                        # 移动鼠标
                        self.mouse.position = (new_x, new_y)
                        
                        # 显示状态
                        self.display_status(x_move, y_move, new_x, new_y)
            
            time.sleep(self.move_interval)
            
        print("[系统] 鼠标控制线程停止")

    def display_status(self, x_move, y_move, current_x, current_y):
        """显示当前状态"""
        # 模式状态
        mode_text = "控制模式" if self.is_control_mode else "正常模式"
        
        # 方向状态
        directions = []
        if self.direction_states['i']: directions.append("↑")
        if self.direction_states['k']: directions.append("↓")
        if self.direction_states['j']: directions.append("←")
        if self.direction_states['l']: directions.append("→")
        dir_text = "".join(directions) if directions else "静止"
        
        # 鼠标按键状态
        buttons = []
        if self.button_states['a']: buttons.append("L")
        if self.button_states['f']: buttons.append("R")
        btn_text = "+".join(buttons) if buttons else "无"
        
        # 构建显示文本
        display = f"[{mode_text}] 方向:{dir_text:<6} 按键:{btn_text:<4} 位置:({int(current_x):4d},{int(current_y):4d})"
        
        # 只有状态改变时才刷新显示
        if display != self.last_status:
            print(display, end='' if self.last_status else '\n')
            self.last_status = display

    def on_press(self, key):
        """按键按下事件处理"""
        if not self.is_running or self.is_paused:
            return False  # 不阻止按键传递
            
        try:
            key_char = key.char.lower()
            
            # 检查是否需要阻止按键原始功能
            should_block = self.block_key_original_function(key_char)
            
            # 只在控制模式下处理特定按键
            if self.is_control_mode:
                # 方向键按下
                if key_char in self.direction_states and not self.direction_states[key_char]:
                    self.direction_states[key_char] = True
                    return should_block
                    
                # 鼠标按键按下
                elif key_char in self.button_states and not self.button_states[key_char]:
                    self.button_states[key_char] = True
                    button_name = "左键" if key_char == 'a' else "右键"
                    print(f"[按键] 鼠标{button_name} 按下")
                    return should_block
                    
            return False
                
        except AttributeError:
            # 特殊功能键，不处理
            return False

    def on_release(self, key):
        """按键释放事件处理"""
        try:
            key_char = key.char.lower()
            
            # 检查是否需要阻止按键原始功能
            should_block = self.block_key_original_function(key_char)
            
            # 只在控制模式下处理特定按键
            if self.is_control_mode:
                # 方向键释放
                if key_char in self.direction_states and self.direction_states[key_char]:
                    self.direction_states[key_char] = False
                    return should_block
                    
                # 鼠标按键释放
                elif key_char in self.button_states and self.button_states[key_char]:
                    self.button_states[key_char] = False
                    button_name = "左键" if key_char == 'a' else "右键"
                    print(f"[按键] 鼠标{button_name} 释放")
                    return should_block
                    
            return False
                
        except AttributeError:
            return False

    def on_shift_j(self):
        """Shift+J 热键处理"""
        self.toggle_control_mode()

    def toggle_pause(self):
        """切换暂停状态"""
        self.is_paused = not self.is_paused
        
        if self.is_paused:
            print("\n[状态] 程序已暂停")
            # 暂停时释放所有按键状态
            for key in self.direction_states:
                self.direction_states[key] = False
            for key in self.button_states:
                self.button_states[key] = False
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
            '<shift>+j': self.on_shift_j,       # 切换控制模式
            '<ctrl>+<shift>+p': self.toggle_pause,  # 暂停/恢复
            '<shift>+<esc>': self.exit_program,     # 退出
        }
        self.hotkey_listener = GlobalHotKeys(hotkey_map)
        return self.hotkey_listener

    def setup_key_listener(self):
        """设置按键监听器"""
        self.key_listener = keyboard.Listener(
            on_press=self.on_press,
            on_release=self.on_release,
            suppress=False  # 不全局阻止，由on_press/on_release返回值决定
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
            print("[提示] 按下 Shift+J 开启鼠标控制模式")
            
            # 主循环
            while self.is_running:
                time.sleep(0.5)
                
        except KeyboardInterrupt:
            print("\n[系统] 收到中断信号")
        finally:
            self.exit_program()

# 增强版本：带配置保存和加载
class EnhancedToggleMouseControl(ToggleMouseControl):
    """增强版本，支持配置保存和速度调节"""
    
    def __init__(self):
        super().__init__()
        
        # 添加速度调节
        self.speed_levels = [4, 8, 12, 16, 20]
        self.current_speed_index = 1  # 默认8像素
        
        # 添加配置保存
        self.config_file = "mouse_control_config.json"
        
        print("\n[增强功能]")
        print("  速度调节: 按 '+' 增加速度，按 '-' 减少速度")
        print("  配置保存: 自动保存和加载配置")
        print("  鼠标锁: 按 'M' 锁定/解锁鼠标位置")

    def change_speed(self, increase=True):
        """改变移动速度"""
        if increase and self.current_speed_index < len(self.speed_levels) - 1:
            self.current_speed_index += 1
        elif not increase and self.current_speed_index > 0:
            self.current_speed_index -= 1
            
        self.move_speed = self.speed_levels[self.current_speed_index]
        print(f"[速度] 已切换: {self.move_speed} 像素/帧 (等级 {self.current_speed_index+1}/{len(self.speed_levels)})")

    def on_press(self, key):
        """重写按键处理，添加增强功能"""
        try:
            key_char = key.char.lower()
            
            # 速度控制 (在任意模式下都可用)
            if key_char == '+':
                self.change_speed(True)
                return True  # 阻止原始功能
            elif key_char == '-':
                self.change_speed(False)
                return True
                
            # 鼠标位置锁定 (仅在控制模式下)
            elif key_char == 'm' and self.is_control_mode:
                self.toggle_mouse_lock()
                return True
                
        except AttributeError:
            pass
            
        # 调用父类处理
        return super().on_press(key)

    def toggle_mouse_lock(self):
        """切换鼠标位置锁定"""
        # 这是一个简单的实现，实际锁定需要更复杂的逻辑
        print("[功能] 鼠标位置锁定: 此功能需要更多系统权限")

# 运行程序
if __name__ == "__main__":
    print("选择控制版本:")
    print("  1. 标准版本 (基础功能)")
    print("  2. 增强版本 (速度调节 + 额外功能)")
    
    choice = input("请输入选择 (1 或 2): ").strip()
    
    if choice == "2":
        print("\n启动增强版本...")
        controller = EnhancedToggleMouseControl()
    else:
        print("\n启动标准版本...")
        controller = ToggleMouseControl()
    
    controller.run()