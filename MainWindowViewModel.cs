using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfMaiTouchEmulator;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    private bool _isAutomaticPortConnectingEnabled;
    private bool _isDebugEnabled;
    private bool _isAutomaticPositioningEnabled;
    private bool _isExitWithSinmaiEnabled;

    public bool IsDebugEnabled
    {
        get => _isDebugEnabled;
        set
        {
            _isDebugEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsAutomaticPositioningEnabled
    {
        get => _isAutomaticPositioningEnabled;
        set
        {
            _isAutomaticPositioningEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsAutomaticPortConnectingEnabled
    {
        get => _isAutomaticPortConnectingEnabled;
        set
        {
            _isAutomaticPortConnectingEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsExitWithSinmaiEnabled
    {
        get => _isExitWithSinmaiEnabled;
        set
        {
            _isExitWithSinmaiEnabled = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
