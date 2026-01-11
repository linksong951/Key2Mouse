import time

from pynput import keyboard
from pynput.keyboard import Key, Controller as KeyboardController

from pynput.mouse import Controller, Button
def warn(number):
    print(f"播放数字：{number}")

# 键盘模拟鼠标监听开启状态a
listeningDevice=True
# 模式
pattern=1
# 鼠标x轴点位
mouseX=0
# 鼠标y轴点位
mouseY=0

# 移速倍率
speed=5

# 创建键盘控制器
mouse=Controller()
kb=KeyboardController()
key=keyboard.Key
def on_j():
    # 判定监听开关状态
    if listeningDevice:
        print(f"mouse--x:{mouseX},y:{mouseY}")
        if pattern==1:
            pattern_1_j()
        elif pattern==2:
            pattern_2_j()
def on_():
    # 判定监听开关状态
    if listeningDevice:
        print(f"mouse--x:{mouseX},y:{mouseY}")
        if pattern==1:
            pattern_1_j()
        elif pattern==2:
            pattern_2_j()

def on_alt_q():
    global listeningDevice
    listeningDevice=False if listeningDevice else True
    print(f"监听状态：{listeningDevice}")

# 模式1
def pattern_1_j():
    current_position = mouse.position
    mouseX=current_position[0]
    mouseY=current_position[1]
    mouseX-=5*speed
    mouse.position=(mouseX,mouseY)
def pattern_1_i():
    current_position = mouse.position
    mouseX=current_position[0]
    mouseY=current_position[1]
    mouseY-=5*speed
    mouse.position=(mouseX,mouseY)
    # m=;号
def pattern_1_m():
    current_position = mouse.position
    mouseX=current_position[0]
    mouseY=current_position[1]
    mouseY+=5*speed
    mouse.position=(mouseX,mouseY)
def pattern_1_l():
    current_position = mouse.position
    mouseX=current_position[0]
    mouseY=current_position[1]
    mouseX+=5*speed
    mouse.position=(mouseX,mouseY)
# 模式2
def pattern_2_j():
    kb.press(key.left)
def pattern_2_i():
    kb.press(key.up)
def pattern_2_m():
    kb.press(key.down)
def pattern_2_l():
    kb.press(key.right)
# # 模式3
# def pattern_3_j():
    

# 创建热键映射
# 注意：键名要用尖括号<>包裹，使用小写字母
hotkeys = {
    '<alt>+q': on_alt_q, 
    'j': on_j,  # J键鼠标左移动
    ';': on_m, 
    'l': on_l, 
    'i': on_i, 
    # 'a': on_a, 
    # 'd': on_d, 
    # 'f': on_f, 
    # '<esc>': on_esc,          # ESC键退出
}

def main():
    # 同时监听鼠标和键盘
    with GlobalHotKeys(hotkeys) as h:
        # 获取当前鼠标x，y轴
        print("正在使用 GlobalHotKeys 监听")
        h.join()
if __name__ == "__main__":
    main()