using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WpfMaiTouchEmulator;

public class MainWindowViewModel : INotifyPropertyChanged
{
    // Language
    public string LbAutoPortConnecting
    {
        get; set;
    }
    public string LbAutoSensorPositioning
    {
        get; set;
    }
    public string LbButtonState
    {
        get; set;
    }
    public string LbConnectionState
    {
        get; set;
    }
    public string LbConnectionStateNotConnected
    {
        get; set;
    }
    public string LbConnectToPort
    {
        get; set;
    }
    public string LbDebugMode
    {
        get; set;
    }
    public string LbExitWithSinmai
    {
        get; set;
    }
    public string LbInstallComPort
    {
        get; set;
    }
    public string LbLanguageDropdown
    {
        get; set;
    }
    public string LbListComPorts
    {
        get; set;
    }
    public string LbReceivedData
    {
        get; set;
    }
    public string LbRecievedData
    {
        get;
        private set;
    }
    public string LbSentData
    {
        get; set;
    }
    public string LbUninstallComPort
    {
        get; set;
    }


    private bool _isAutomaticPortConnectingEnabled;
    private bool _isDebugEnabled;
    private bool _isAutomaticPositioningEnabled;
    private bool _isExitWithSinmaiEnabled;
    private CultureInfo _selectedLanguage;
    private readonly ResourceManager resourceManager;
    private readonly CultureInfo cultureInfo;

    public List<CultureInfo> SupportedLanguages
    {
        get;
    } =
    [
        new CultureInfo("en-US"),  // English
        new CultureInfo("zh-CN")   // Chinese (Simplified)
    ];


    public MainWindowViewModel()
    {
        resourceManager = new ResourceManager("WpfMaiTouchEmulator.Properties.Resources", typeof(MainWindowViewModel).Assembly);
        LoadLanguageSettings();
    }

    private void LoadLanguageSettings()
    {
        var savedLang = Properties.Settings.Default.UserLanguage;
        var culture = string.IsNullOrEmpty(savedLang) ? CultureInfo.CurrentCulture : new CultureInfo(savedLang);

        SelectedLanguage = SupportedLanguages.Contains(culture) ? culture : SupportedLanguages[0];
    }

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

    public CultureInfo SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedLanguage)));
                ChangeLanguage(value);
            }
        }
    }

    private void ChangeLanguage(CultureInfo culture)
    {
        Properties.Settings.Default.UserLanguage = culture.Name;
        Properties.Settings.Default.Save();

        Thread.CurrentThread.CurrentUICulture = culture;
        ResourceManager rm = new ResourceManager(typeof(Resources.Strings));
        UpdateLocalizedResources(rm);
    }

    private void UpdateLocalizedResources(ResourceManager resourceManager)
    {
        LbAutoPortConnecting = resourceManager.GetString("lbAutoPortConnecting");
        LbAutoSensorPositioning = resourceManager.GetString("lbAutoSensorPositioning");
        LbButtonState = resourceManager.GetString("lbButtonState");
        LbConnectionState = resourceManager.GetString("lbConnectionState");
        LbConnectionStateNotConnected = resourceManager.GetString("lbConnectionStateNotConnected");
        LbConnectToPort = resourceManager.GetString("lbConnectToPort");
        LbDebugMode = resourceManager.GetString("lbDebugMode");
        LbExitWithSinmai = resourceManager.GetString("lbExitWithSinmai");
        LbInstallComPort = resourceManager.GetString("lbInstallComPort");
        LbLanguageDropdown = resourceManager.GetString("lbLanguageDropdown");
        LbListComPorts = resourceManager.GetString("lbListComPorts");
        LbReceivedData = resourceManager.GetString("lbReceivedData");
        LbRecievedData = resourceManager.GetString("lbRecievedData");
        LbSentData = resourceManager.GetString("lbSentData");
        LbUninstallComPort = resourceManager.GetString("lbUninstallComPort");

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
