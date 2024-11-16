using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WpfMaiTouchEmulator;

public class MainWindowViewModel : INotifyPropertyChanged
{
    // Language
    public string? LbAutoPortConnecting
    {
        get; set;
    }
    public string? LbAutoSensorPositioning
    {
        get; set;
    }
    public string? LbButtonState
    {
        get; set;
    }
    public string? LbConnectionState
    {
        get; set;
    }
    public string? LbConnectionStateNotConnected
    {
        get; set;
    }
    public string? LbConnectToPort
    {
        get; set;
    }
    public string? LbDebugMode
    {
        get; set;
    }
    public string? LbExitWithSinmai
    {
        get; set;
    }
    public string? LbEmulateRingButtons
    {
        get; set;
    }
    public string? LbOpenLogFolder
    {
        get; set;
    }
    public string? LbAppVersion
    {
        get; set;
    }
    public string? LbAbout
    {
        get; set;
    }
    public string? LbInstallComPort
    {
        get; set;
    }
    public string? LbLanguageDropdown
    {
        get; set;
    }
    public string? LbBorderSettings
    {
        get; set;
    }
    public string? LbSettings
    {
        get; set;
    }
    public string? LbComPort
    {
        get; set;
    }
    public string? LbBorderDisabled
    {
        get; set;
    }
    public string? LbBorderSolid
    {
        get; set;
    }
    public string? LbBorderRainbow
    {
        get; set;
    }
    public string? LbListComPorts
    {
        get; set;
    }
    public string? LbReceivedData
    {
        get; set;
    }
    public string? LbRecievedData
    {
        get;
        private set;
    }
    public string? LbSentData
    {
        get; set;
    }
    public string? LbUninstallComPort
    {
        get; set;
    }
    public string? LbMenuCategoryHelp
    {
        get;
        private set;
    }
    public string? LbMenuItemSetup
    {
        get;
        private set;
    }
    public string? TxtSetupInstructions
    {
        get;
        private set;
    }
    public string? TxtSetupInstructionsHeader
    {
        get;
        private set;
    }
    public string? TxtFailedToSetupSinmaiExitHeader
    {
        get;
        private set;
    }
    public string? TxtFailedToSetupSinmaiExit
    {
        get;
        private set;
    }
    public string? TxtCurrentlyInstalledPorts
    {
        get;
        private set;
    }
    public string? TxtErrorConnectingToPortHeader
    {
        get;
        private set;
    }
    public string? TxtComPortConnected
    {
        get;
        private set;
    }
    public string? TxtComPortConnecting
    {
        get;
        private set;
    }
    public string? TxtCom3AlreadyInstalled
    {
        get;
        private set;
    }
    public string? TxtCom3InstalledSuccessfully
    {
        get;
        private set;
    }
    public string? TxtCom3InstallFailed
    {
        get;
        private set;
    }
    public string? TxtCom3UninstallNotRequired
    {
        get;
        private set;
    }
    public string? TxtCom3UninstalledSuccessfully
    {
        get;
        private set;
    }
    public string? TxtCom3UninstallFailed
    {
        get;
        private set;
    }
    public string? LbTouchPanelResize
    {
        get;
        private set;
    }
    public string? LbTouchPanelDrag
    {
        get;
        private set;
    }
    public string? LbAutoPortConnectingTT
    {
        get;
        private set;
    }
    public string? LbAutoSensorPositioningTT
    {
        get;
        private set;
    }
    public string? LbExitWithSinmaiTT
    {
        get;
        private set;
    }
    public string? LbEmulateRingButtonsTT
    {
        get;
        private set;
    }

    private bool _isAutomaticPortConnectingEnabled;
    private bool _isDebugEnabled;
    private bool _isAutomaticPositioningEnabled;
    private bool _isExitWithSinmaiEnabled;
    private CultureInfo _selectedLanguage;
    private bool _isRingButtonEmulationEnabled;
    private string _borderColour;
    private readonly ResourceManager resourceManager;
    private readonly CultureInfo cultureInfo;

    public List<CultureInfo> SupportedLanguages
    {
        get;
    } =
    [
        new CultureInfo("en-US"),  // English
        new CultureInfo("zh-CN"),   // Chinese (Simplified)
        new CultureInfo("ja-JP")   // Japanese
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

    public bool IsRingButtonEmulationEnabled
    {
        get => _isRingButtonEmulationEnabled;
        set
        {
            _isRingButtonEmulationEnabled = value;
            OnPropertyChanged();
        }
    }

    public string BorderColour
    {
        get => _borderColour;
        set
        {
            _borderColour = value;
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
        LbEmulateRingButtons = resourceManager.GetString("lbEmulateRingButtons");
        LbOpenLogFolder = resourceManager.GetString("LbOpenLogFolder");
        LbAbout = resourceManager.GetString("LbAbout");
        
        LbInstallComPort = resourceManager.GetString("lbInstallComPort");
        LbLanguageDropdown = resourceManager.GetString("lbLanguageDropdown");

        LbComPort = resourceManager.GetString("LbComPort");
        LbBorderSettings = resourceManager.GetString("LbBorderSettings");
        LbSettings = resourceManager.GetString("LbSettings");
        LbBorderDisabled = resourceManager.GetString("LbBorderDisabled");
        LbBorderSolid = resourceManager.GetString("LbBorderSolid");
        LbBorderRainbow = resourceManager.GetString("LbBorderRainbow");

        LbListComPorts = resourceManager.GetString("lbListComPorts");
        LbReceivedData = resourceManager.GetString("lbReceivedData");
        LbRecievedData = resourceManager.GetString("lbRecievedData");
        LbSentData = resourceManager.GetString("lbSentData");
        LbUninstallComPort = resourceManager.GetString("lbUninstallComPort");
        LbMenuCategoryHelp = resourceManager.GetString("lbMenuCategoryHelp");
        LbMenuItemSetup = resourceManager.GetString("lbMenuItemSetup");
        LbAutoPortConnectingTT = resourceManager.GetString("lbAutoPortConnectingTT");
        LbAutoSensorPositioningTT = resourceManager.GetString("lbAutoSensorPositioningTT");
        LbExitWithSinmaiTT = resourceManager.GetString("lbExitWithSinmaiTT");
        LbEmulateRingButtonsTT = resourceManager.GetString("lbEmulateRingButtonsTT");
        LbMenuCategoryHelp = resourceManager.GetString("lbMenuCategoryHelp");
        LbMenuItemSetup = resourceManager.GetString("lbMenuItemSetup");

        TxtSetupInstructions = resourceManager.GetString("TxtSetupInstructions");
        TxtSetupInstructionsHeader = resourceManager.GetString("TxtSetupInstructionsHeader");
        TxtFailedToSetupSinmaiExitHeader = resourceManager.GetString("TxtFailedToSetupSinmaiExitHeader");
        TxtFailedToSetupSinmaiExit = resourceManager.GetString("TxtFailedToSetupSinmaiExit");
        TxtCurrentlyInstalledPorts = resourceManager.GetString("TxtCurrentlyInstalledPorts");
        TxtErrorConnectingToPortHeader = resourceManager.GetString("TxtErrorConnectingToPortHeader");
        TxtComPortConnected = resourceManager.GetString("TxtComPortConnected");
        TxtComPortConnecting = resourceManager.GetString("TxtComPortConnecting");

        TxtCom3AlreadyInstalled = resourceManager.GetString("TxtCom3AlreadyInstalled");
        TxtCom3InstalledSuccessfully = resourceManager.GetString("TxtCom3InstalledSuccessfully");
        TxtCom3InstallFailed = resourceManager.GetString("TxtCom3InstallFailed");
        TxtCom3UninstallNotRequired = resourceManager.GetString("TxtCom3UninstallNotRequired");
        TxtCom3UninstalledSuccessfully = resourceManager.GetString("TxtCom3UninstalledSuccessfully");
        TxtCom3UninstallFailed = resourceManager.GetString("TxtCom3UninstallFailed");

        LbTouchPanelResize = resourceManager.GetString("LbTouchPanelResize");
        LbTouchPanelDrag = resourceManager.GetString("LbTouchPanelDrag");

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
