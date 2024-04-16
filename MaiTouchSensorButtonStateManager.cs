using System.Windows;
using System.Windows.Controls;

namespace WpfMaiTouchEmulator;

enum TouchValue: long
{
    A1 = 1 << 0,  // 2^0
    A2 = 1 << 1,  // 2^1
    A3 = 1 << 2,  // 2^2
    A4 = 1 << 3,  // 2^3
    A5 = 1 << 4,  // 2^4
    A6 = 1 << 5,  // 2^5
    A7 = 1 << 6,  // 2^6
    A8 = 1 << 7,  // 2^7
    B1 = 1 << 8,  // 2^8
    B2 = 1 << 9,  // 2^9
    B3 = 1 << 10, // 2^10
    B4 = 1 << 11, // 2^11
    B5 = 1 << 12, // 2^12
    B6 = 1 << 13, // 2^13
    B7 = 1 << 14, // 2^14
    B8 = 1 << 15, // 2^15
    C1 = 1 << 16, // 2^16
    C2 = 1 << 17, // 2^17
    C3 = C1 | C2, // A special sensor used because center notes are hard to press using a windows touchscreen
    D1 = 1 << 18, // 2^18
    D2 = 1 << 19, // 2^19
    D3 = 1 << 20, // 2^20
    D4 = 1 << 21, // 2^21
    D5 = 1 << 22, // 2^22
    D6 = 1 << 23, // 2^23
    D7 = 1 << 24, // 2^24
    D8 = 1 << 25, // 2^25
    E1 = 1 << 26, // 2^26
    E2 = 1 << 27, // 2^27
    E3 = 1 << 28, // 2^28
    E4 = 1 << 29, // 2^29
    E5 = 1 << 30, // 2^30
    E6 = 1L << 31, // Note: Use 1L for long literals, as this and subsequent values exceed Int32.MaxValue
    E7 = 1L << 32,
    E8 = 1L << 33,
}

internal class MaiTouchSensorButtonStateManager
{
    static long buttonState = 0L;
    private readonly Label buttonStateValue;

    public MaiTouchSensorButtonStateManager(Label buttonStateValue)
    {
        this.buttonStateValue = buttonStateValue;
        SetupUpdateLoop();
    }

    private async Task SetupUpdateLoop()
    {
        string? lastButtonState = null;
        while (true)
        {
            if (lastButtonState != buttonState.ToString())
            {
                lastButtonState = buttonState.ToString();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    buttonStateValue.Content = lastButtonState;
                });
            }
            await Task.Delay(16);
        }
    }

    public void Reset()
    {
        buttonState = 0L;
    }

    public void PressButton(TouchValue button)
    {
        buttonState |= ((long)button);
    }

    public void ReleaseButton(TouchValue button)
    {
        buttonState &= ~((long)button);
    }

    public byte[] GetCurrentState()
    {
        return
        [
            0x28,
            (byte)(buttonState & 0b11111),
            (byte)(buttonState >> 5 & 0b11111),
            (byte)(buttonState >> 10 & 0b11111),
            (byte)(buttonState >> 15 & 0b11111),
            (byte)(buttonState >> 20 & 0b11111),
            (byte)(buttonState >> 25 & 0b11111),
            (byte)(buttonState >> 30 & 0b11111),
            0x29
        ];
    }
}
