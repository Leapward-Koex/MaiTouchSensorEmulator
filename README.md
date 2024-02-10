
# WpfMaiTouchEmulator
A WPF C# app meant to emulate the touch sensor hardware for SDEZ MaiMai using software on a touchscreen.

# How it works
The app uses com0com to install a virtual COM port pairing of COM3 - COM23. The app then connects to COM23 and emulates how the hardware touch sensor works, sending data over this port.

# Setup
COM3 needs to be free for this app to work as that is the port used by SDEZ. By default most people will find that COM3 is already in use by a USB Serial device. This will need to be uninstalled via Device Manager,
1. Open Device Manager
2. Show hidden devices. "View" -> "Show Hidden Devices" -> "Ports (COM & LPT)" -> Right click the COM3 device and uninstall.

Open the WpfMaiTouchEmulator.exe app, Install the virtual port via the "Install COM port" button, either tick the "Automatic port connecting" for the app to connect to the COM23 port on start up or press the "Connect to port" button.

Start SDEZ and use your touchscreen as normal.

# Why?
Most SDEZ don't work very will with the built in touchscreen and require either someone else to modify the SDEZ source code to get the inbuilt touch controls to work or for you to edit and recompile the .dll files. This app should work with all SDEZ versions (afaik) out of the box.

# Performance
Its ok, the app polls the touch sensor window at *up to* 1Khz but its running in C# so it may or may not actually be running at this speed, depending on the performance of the host machine.
As for accuracy. I'm not that good so I can't say how well it works at high levels of play but in my experience it works pretty well.

# Thanks
Thanks to [whowechina - mai_pico](https://github.com/whowechina/mai_pico) and [Sucareto - Mai2Touch](https://github.com/Sucareto/Mai2Touch)  for the basic serial data format and touch implementation research.
