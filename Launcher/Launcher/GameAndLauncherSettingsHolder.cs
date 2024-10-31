namespace Launcher;

public class GameAndLauncherSettingsHolder: GameSettingsHolder
{
	private readonly LocalizationExtensions _localizationExtensions;

	private readonly LauncherSettings _launcherSettings;

	public string LocAutoRun => _localizationExtensions.Autorun;

	public string LocAutorunTooltip => _localizationExtensions.AutorunTooltip;

	private bool _autoRun;

	public bool AutoRun
	{
		get
		{
			return _autoRun;
		}
		set
		{
			if (_autoRun != value)
			{
				_autoRun = value;
				OnPropertyChanged("AutoRun");
			}
		}
	}

	public GameAndLauncherSettingsHolder(GameSettingsHolder gameSettingsHolder, LauncherSettings launcherSettings) : base(gameSettingsHolder) 
	{
		_localizationExtensions = new LocalizationExtensions(CurrentLanguage.Id);
		_launcherSettings = launcherSettings;
		AutoRun = _launcherSettings.AutoRun;
	}

	public new void SaveSettings()
	{
		_launcherSettings.AutoRun = AutoRun;
		_launcherSettings.Save();
		
		base.SaveSettings();
	}

	public new void LoadSettings(bool force_default = false)
	{
		_launcherSettings.Load();
		AutoRun = _launcherSettings.AutoRun;

		base.LoadSettings(force_default);
	}
}
