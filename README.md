
# WpfMaiTouchEmulator
A WPF C# app meant to emulate the touch sensor hardware for SDEZ MaiMai using software on a touchscreen.

## README in Other Languages

- [简体中文](README.zh.md)
- [日本語](README.jp.md)

# How it works
The app uses com0com to install a virtual COM port pairing of COM3 - COM23. The app then connects to COM23 and emulates how the hardware touch sensor works, sending data over this port.

# Demo


https://github.com/Leapward-Koex/MaiTouchSensorEmulator/assets/30615050/eea21285-fec5-4393-a75d-74274b39789f



https://github.com/Leapward-Koex/MaiTouchSensorEmulator/assets/30615050/e45ca02f-8d82-4ad4-a610-ef08abcb5087



# Setup
Disable Three- and four-finger touch gestures in windows (on W11 Settings -> Bluetooth & devices -> Touch)

COM3 needs to be free for this app to work as that is the port used by SDEZ. By default most people will find that COM3 is already in use by a USB Serial device. This will need to be uninstalled via Device Manager,
1. Open Device Manager
2. Show hidden devices. "View" -> "Show Hidden Devices" -> "Ports (COM & LPT)" -> Right click the COM3 device and uninstall.

Set `DummyTouchPanel=0` in the [AM] section of your maimai .ini file.

Open the WpfMaiTouchEmulator.exe app, Install the virtual port via the "Install COM port" button, either tick the "Automatic port connecting" for the app to connect to the COM23 port on start up or press the "Connect to port" button.

If you have issues where the app can't bind to COM23 after installing the virtual port I find that restarting my PC normally fixes it.

Start SDEZ and use your touchscreen as normal.

[Optional] To avoid having to manually open the application each time, you can enable "Automatic port connecting" and add this line after `@echo off` in your MaiMai's start.bat file. `start "" "<your path to WpfMaiTouchEmulator.exe>"`

[Optional] If you're on W11 and find that left swipes sometimes open the "widgets" pane you can remove the pane by running this command as administrator `winget uninstall "windows web experience pack"` and following the onscreen instructions.

# Installing the virtual COM ports without using the included com0com program
If you don't want to use the included install/remove virtual com actions in the app because you don't want to give admin access to this unknown app then you can achieve the same outcome by manually downloading Null-modem com0com online (version 2.2.2.0 required!) and then running this command.
`cd "C:\Program Files (x86)\com0com" && setupc.exe install Portname=COM3 Portname=COM23` to install the virtual port and
`cd "C:\Program Files (x86)\com0com" && setupc.exe uninstall` to uninstall the virtual ports.

# Why?
Most SDEZ don't work very will with the built in touchscreen and require either someone else to modify the SDEZ source code to get the inbuilt touch controls to work or for you to edit and recompile the .dll files. This app should work with all SDEZ versions (afaik) out of the box.

Another reason is sourcing a ITO etched panel for use as a touch sensor can be difficult and reasonable expensive.

# Performance
Its ok, the app polls the touch sensor window at *up to* 1Khz but its running in C# so it may or may not actually be running at this speed, depending on the performance of the host machine.

As for accuracy. I'm not that good so I can't say how well it works at high levels of play but in my experience it works pretty well.

# Thanks
Thanks to [whowechina - mai_pico](https://github.com/whowechina/mai_pico) and [Sucareto - Mai2Touch](https://github.com/Sucareto/Mai2Touch)  for the basic serial data format and touch implementation research.
