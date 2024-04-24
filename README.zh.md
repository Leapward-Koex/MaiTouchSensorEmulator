# WpfMaiTouchEmulator
一个用于通过触摸屏软件模拟SDEZ MaiMai的触摸传感器硬件的WPF C# 应用程序。

# 工作原理
该应用程序使用com0com安装一个虚拟的COM端口配对：COM3 - COM23。然后，应用程序连接到COM23并模拟硬件触摸传感器的工作方式，通过此端口发送数据。

# 演示


https://github.com/Leapward-Koex/MaiTouchSensorEmulator/assets/30615050/eea21285-fec5-4393-a75d-74274b39789f



https://github.com/Leapward-Koex/MaiTouchSensorEmulator/assets/30615050/e45ca02f-8d82-4ad4-a610-ef08abcb5087



# 设置
在Windows中禁用三指和四指触摸手势（在W11设置 -> 蓝牙与设备 -> 触摸）

COM3需要空闲，因为这是SDEZ使用的端口。默认情况下，大多数人会发现COM3已被USB串行设备使用。需要通过设备管理器卸载，
1. 打开设备管理器
2. 显示隐藏设备。"视图" -> "显示隐藏设备" -> "端口（COM & LPT）" -> 右键点击COM3设备并卸载。

在您的maimai.ini文件的[AM]部分设置`DummyTouchPanel=0`。

打开WpfMaiTouchEmulator.exe应用程序，通过"安装COM端口"按钮安装虚拟端口，可以勾选"自动连接端口"使应用程序在启动时连接到COM23端口，或按"连接到端口"按钮。

如果在安装虚拟端口后应用程序无法绑定到COM23，我发现重新启动我的电脑通常可以解决问题。

启动SDEZ并正常使用您的触摸屏。

[可选] 为了避免每次都手动打开应用程序，您可以启用"自动连接端口"并在您的MaiMai的start.bat文件中`@echo off`之后添加这行代码。`start "" "<您的WpfMaiTouchEmulator.exe路径>"`

[可选] 如果您使用的是W11，并且发现左滑有时会打开“小部件”面板，您可以通过以管理员身份运行此命令`winget uninstall "windows web experience pack"`并按照屏幕上的指示操作来移除面板。

# 不使用随附的com0com程序安装虚拟COM端口
如果您不想使用应用程序中包含的安装/移除虚拟com操作，因为您不想给这个未知的应用程序管理员权限，那么您可以通过手动下载Null-modem com0com（需要版本2.2.2.0！）并运行此命令来达到同样的效果。
`cd "C:\Program Files (x86)\com0com" && setupc.exe install Portname=COM3 Portname=COM23`来安装虚拟端口和
`cd "C:\Program Files (x86)\com0com" && setupc.exe uninstall`来卸载虚拟端口。

# 为什么？
大多数SDEZ与内置触摸屏的兼容性不是很好，需要其他人修改SDEZ的源代码才能使内置触控功能正常工作，或者您需要编辑并重新编译.dll文件。这个应用程序应该可以开箱即用地与所有SDEZ版本兼容（据我所知）。

另一个原因是，获取用作触摸传感器的ITO蚀刻面板可能既困难又昂贵。

# 性能
还可以，应用程序以最高1Khz的频率轮询触摸传感器窗口，但它是用C#运行的，所以实际运行速度可能取决于主机的性能。

至于准确性。我不是很擅长，所以无法说它在高级别的游戏中表现如何，但根据我的经验，它运行得相当不错。

# 感谢
感谢 [whowechina - mai_pico](https://github.com/whowechina/mai_pico) 和 [Sucareto - Mai2Touch](https://github.com/Sucareto/Mai2Touch) 提供基本的串行数据格式和触摸实现研究。
