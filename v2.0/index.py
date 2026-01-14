
# 更改计划
# 1.改为多线程支持

from pynput import keyboard
from pynput.keyboard import GlobalHotKeys,Key, Controller as KeyboardController
from pynput.mouse import Controller, Button·

import time
import re
# 键盘模拟鼠标监听开启状态a
listeningDevice=True
# 模式
pattern=1
# # 鼠标x轴点位
# mouseX=0
# # 鼠标y轴点位
# mouseY=0

# 移速倍率
speed=5
# 历史按键
historicalButton=[]
# 是否首次进入函数
initialEntry_1=True

# 判断用时间
intervalTimeMs_1=int(time.time()* 1000)
# 组合键
# shift+alt+q
shift_1_alt_1_q=["shift_1","alt_l","q"]
mouse=Controller()
kb=KeyboardController()
key=keyboard.Key

# 添加历史快捷键方法
def add_HistoricalButton(data):
    data1=data
    global historicalButton,intervalTimeMs_1,initialEntry_1
    # def replace_match(match):
    #     # 只保留 Key. 后面的部分
    #     return match.group(1)
    # pattern = r'[Kk][Ee][Yy]\.([A-Za-z0-9_])'
    # re.sub(pattern, replace_match, data1)

    if len(historicalButton)==0:
        pass
    else :
        # 判断按键是否和已经储存的数据匹配
        if data1 in historicalButton:
            return
        pass

    # 判断是否首次进入函数
    if initialEntry_1:
        initialEntry_1=False
        print("首次进入该函数")
        intervalTimeMs_1=int(time.time())
        historicalButton.append(data1)
    else:
        # 间隔没有超过1秒保存并且不清空
        i=intervalTimeMs_1+1
        j=int(time.time())
        # 注意好奇怪的问题只有功能键会触发超过一秒清空的判定 算是误打误撞用bug跑起来了
        if i==j:
            intervalTimeMs_1=int(time.time())
            historicalButton.append(data1)
        # 间隔超过1秒不保存直接清空
        elif i<j:
            intervalTimeMs_1=int(time.time())
            historicalButton=[]
            print(f"超过1秒直接清空:{time.asctime( time.localtime(time.time()) )}")
            print("------------------------------------")

    # 长度超过3就会进行裁剪
    if len(historicalButton)>3:
        historicalButton.pop(0)
    print(f"添加后:{historicalButton}")

# 模式1
def pattern_1(key):
    x=mouse.position()
    mouse.position=(x,y)

def on_press(key):
    """按键按下时的回调函数"""
    try:
        add_HistoricalButton(key.char)
        # 监听shift+alt+q
        # if historicalButton=
        # 监听alt+q
        # 尝试获取字符（普通键）
        # print(f'按下字符键: {key.char}')
        pass
    except AttributeError:
        add_HistoricalButton(key.name)
        # 功能键没有 char 属性
        # print(f'按下功能键: {key}')
        # pass

def on_release(key):
    """按键释放时的回调函数"""
    # print(f'释放按键: {key}')
    # 按 ESC 键停止监听
    if key == keyboard.Key.esc:
        print('停止监听')
        return False  # 返回 False 停止监听器

# 使用 with 语句自动管理监听器资源
with keyboard.Listener(
    on_press=on_press,
    on_release=on_release,
    suppress=True
) as listener:
    listener.join()  # 阻塞直到监听器停止