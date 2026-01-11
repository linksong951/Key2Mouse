from pynput import keyboard
from pynput.keyboard import GlobalHotKeys

from pynput.mouse import Controller, Button
import threading

# 初始化鼠标控制器
mouse = Controller()

# 当前鼠标 x，y轴
print(mouse.position[0])
# def on_release(key):
#     """按键释放时触发"""jjjjjjj
#     print(f'键 {key} 被释放')f
#     # 通常用于判断组合键，例如只在释放时触发某些操作

# def on_press(key):
#     """按键按下时触发"""
#     try:
#         # 普通字符键
#         if key.char == 'a':
#             print('你按下了 A 键')
#             # 在这里触发鼠标操作，例如移动
#     except AttributeError:
#         # 特殊功能键
#         if key == keyboard.Key.esc:
#             print('按下了ESC，准备退出监听')
#             # 返回False会停止监听器
#             return False
#         elif key == keyboard.Key.f1:
#             print('按下了F1，模拟鼠标左键点击')
#             mouse.click(Button.left, 1)  # 左键单击一次

def on_a():
    mouse.click(Button.left,1)

def on_d():
    mouse.click(Button.middle,1)

def on_f():
    mouse.click(Button.right,1)

def on_j():
    # 循环开始

    # 监听循环结束
    x=mouse.position[0]-50
    y=mouse.position[1]
    mouse.position=(x,y)


def on_k():
    x=mouse.position[0]
    y=mouse.position[1]+50
    mouse.position=(x,y)

def on_l():
    x=mouse.position[0]+50
    y=mouse.position[1]
    mouse.position=(x,y)

def on_i():
    x=mouse.position[0]
    y=mouse.position[1]-50
    mouse.position=(x,y)

def on_shift_j():
    """Shift+J 被按下时执行"""
    print("检测到 Shift+J 组合键")
    # 执行你的操作，例如：
    # - 模拟鼠标点击
    # - 打开特定功能aaa
    # - 切换程序状态
    

def on_esc():
    """ESC 被按下时退出程序"""
    print("退出程序")
    # 停止监听器
    raise keyboard.Listener.StopException

# 创建热键映射
# 注意：键名要用尖括号<>包裹，使用小写字母
hotkeys = {
    '<shift>+j': on_shift_j,  # Shift+J
    'j': on_j,  # J键鼠标左移动
    'k': on_k, 
    'l': on_l, 
    'i': on_i, 
    'a': on_a, 
    'd': on_d, 
    'f': on_f, 
    '<esc>': on_esc,          # ESC键退出
}

# 创建并启动热键监听器
with GlobalHotKeys(hotkeys) as h:
    # 获取当前鼠标x，y轴
    print("正在使用 GlobalHotKeys 监听 Shift+J...")
    h.join()