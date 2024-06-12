using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfMaiTouchEmulator;
public partial class RingButtonEmulator
{

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private static readonly IDictionary<TouchValue, byte> touchRingMapping = new Dictionary<TouchValue, byte>()
    {
        { TouchValue.A1, 0x57 }, // W
        { TouchValue.A2, 0x45 }, // E
        { TouchValue.A3, 0x44 }, // D
        { TouchValue.A4, 0x43 }, // C
        { TouchValue.A5, 0x58 }, // X
        { TouchValue.A6, 0x5A }, // Z
        { TouchValue.A7, 0x41 }, // A
        { TouchValue.A8, 0x51 }, // Q
    };
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static bool HasRingButtonMapping(TouchValue touchValue)
    {
        return touchRingMapping.ContainsKey(touchValue);
    }

    public static void PressButton(TouchValue touchValue)
    {
        if (touchRingMapping.TryGetValue(touchValue, out var vk))
        {
            keybd_event(vk, 0, 0, UIntPtr.Zero);
        }
    }

    public static void ReleaseButton(TouchValue touchValue)
    {
        if (touchRingMapping.TryGetValue(touchValue, out var vk))
        {
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    public static void ReleaseAllButtons()
    {
        foreach (var vk in touchRingMapping.Values)
        {
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
