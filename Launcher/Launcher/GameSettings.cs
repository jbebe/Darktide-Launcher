using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Effects;
using ResourceDictionary;

namespace Launcher;

public partial class GameSettings : Window, IComponentConnector
{
	private GameAndLauncherSettingsHolder _settings_holder;

	private bool _isUpdating;

	public GameSettings(GameSettingsHolder game_settings_holder, LauncherSettings launcher_settings)
	{
		_settings_holder = new GameAndLauncherSettingsHolder(game_settings_holder, launcher_settings);
		InitializeComponent();
		base.FontFamily = FontManager.CurrentFont;
		base.DataContext = _settings_holder;
		ResolutionCombo.SelectionChanged += on_resolution_changed;
		DLSSComboMode.SelectionChanged += OnDLSSSelectionChanged;
		FrameGenerationComboMode.SelectionChanged += OnFrameGenerationSelectionChanged;
		SuperResolutionComboMode.SelectionChanged += OnSuperResolutionSelectionChanged;
		FSRComboMode.SelectionChanged += OnFSRSelectionChanged;
		FSR2ComboMode.SelectionChanged += OnFSR2SelectionChanged;
		XeSSComboMode.SelectionChanged += OnXeSSSelectionChanged;
		AntiAliasingComboMode.SelectionChanged += OnAntiAliasingSelectionChanged;
		base.Loaded += GameSettings_Loaded;
		base.Closing += GameSettings_Closing;
	}

	private void GameSettings_Loaded(object sender, RoutedEventArgs e)
	{
		BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
	}

	private void GameSettings_Closing(object sender, CancelEventArgs e)
	{
	}

	protected void on_resolution_changed(object sender, SelectionChangedEventArgs e)
	{
		SysInfo.Resolution resolution = (SysInfo.Resolution)ResolutionCombo.SelectedItem;
		if (resolution != null)
		{
			_settings_holder.CurrentOutput.MatchResolution(_settings_holder.CurrentScreenMode, resolution.Width, resolution.Height);
		}
	}

	private void DisableDlssMainSwitch()
	{
		_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.off;
		_settings_holder.CurrentFrameGenerationMode = GameSettingsHolder.FrameGenerationMode.off;
		_settings_holder.CurrentSuperResolutionMode = GameSettingsHolder.SuperResolutionMode.off;
	}

	private void EnableDlssMainSwitch()
	{
		_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.on;
		_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
		_settings_holder.CurrentFSR2Mode = GameSettingsHolder.FSR2Mode.off;
		_settings_holder.CurrentFFXFGMode = GameSettingsHolder.FFXFGMode.off;
		_settings_holder.CurrentXeSSMode = GameSettingsHolder.XeSSMode.off;
		_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
		_settings_holder.CurrentFrameGenerationMode = GameSettingsHolder.FrameGenerationMode.on;
		_settings_holder.CurrentSuperResolutionMode = GameSettingsHolder.SuperResolutionMode.auto;
		if (_settings_holder.CurrentReflexMode == GameSettingsHolder.ReflexMode.disabled)
		{
			_settings_holder.CurrentReflexMode = GameSettingsHolder.ReflexMode.enabled;
		}
	}

	private bool CanUpdate()
	{
		if (base.IsActive && !_isUpdating)
		{
			return _settings_holder.IsInitialized;
		}
		return false;
	}

	protected void OnDLSSSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentDLSSMode == GameSettingsHolder.DLSSMode.on)
			{
				EnableDlssMainSwitch();
			}
			else
			{
				DisableDlssMainSwitch();
			}
			_isUpdating = false;
		}
	}

	protected void OnFrameGenerationSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!CanUpdate())
		{
			return;
		}
		_isUpdating = true;
		if (_settings_holder.CurrentFrameGenerationMode == GameSettingsHolder.FrameGenerationMode.on)
		{
			_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.on;
			_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
			_settings_holder.CurrentFSR2Mode = GameSettingsHolder.FSR2Mode.off;
			_settings_holder.CurrentFFXFGMode = GameSettingsHolder.FFXFGMode.off;
			_settings_holder.CurrentXeSSMode = GameSettingsHolder.XeSSMode.off;
			_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
			if (_settings_holder.CurrentReflexMode == GameSettingsHolder.ReflexMode.disabled)
			{
				_settings_holder.CurrentReflexMode = GameSettingsHolder.ReflexMode.enabled;
			}
		}
		else if (_settings_holder.CurrentSuperResolutionMode == GameSettingsHolder.SuperResolutionMode.off)
		{
			_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.off;
		}
		_isUpdating = false;
	}

	protected void OnSuperResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!CanUpdate())
		{
			return;
		}
		_isUpdating = true;
		if (_settings_holder.CurrentSuperResolutionMode != 0)
		{
			_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.on;
			_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
			_settings_holder.CurrentFSR2Mode = GameSettingsHolder.FSR2Mode.off;
			_settings_holder.CurrentFFXFGMode = GameSettingsHolder.FFXFGMode.off;
			_settings_holder.CurrentXeSSMode = GameSettingsHolder.XeSSMode.off;
			_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
			if (_settings_holder.CurrentReflexMode == GameSettingsHolder.ReflexMode.disabled)
			{
				_settings_holder.CurrentReflexMode = GameSettingsHolder.ReflexMode.enabled;
			}
		}
		else if (_settings_holder.CurrentFrameGenerationMode == GameSettingsHolder.FrameGenerationMode.off)
		{
			_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.off;
		}
		_isUpdating = false;
	}

	protected void OnFSRSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentFSRMode != 0)
			{
				_settings_holder.CurrentFSR2Mode = GameSettingsHolder.FSR2Mode.off;
				_settings_holder.CurrentXeSSMode = GameSettingsHolder.XeSSMode.off;
				_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.TAA;
				DisableDlssMainSwitch();
			}
			_isUpdating = false;
		}
	}

	protected void OnFSR2SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentFSR2Mode != 0)
			{
				_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
				_settings_holder.CurrentXeSSMode = GameSettingsHolder.XeSSMode.off;
				_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
				DisableDlssMainSwitch();
			}
			_isUpdating = false;
		}
	}

	protected void OnFFXFGSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentFFXFGMode != GameSettingsHolder.FFXFGMode.on)
			{
				_settings_holder.CurrentFrameGenerationMode = GameSettingsHolder.FrameGenerationMode.off;
				_settings_holder.CurrentReflexMode = GameSettingsHolder.ReflexMode.disabled;
			}
			_isUpdating = false;
		}
	}

	protected void OnXeSSSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentXeSSMode != 0)
			{
				_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
				_settings_holder.CurrentFSR2Mode = GameSettingsHolder.FSR2Mode.off;
				_settings_holder.CurrentAntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
				DisableDlssMainSwitch();
			}
			_isUpdating = false;
		}
	}

	protected void OnAntiAliasingSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CanUpdate())
		{
			_isUpdating = true;
			if (_settings_holder.CurrentAntiAliasingMode == GameSettingsHolder.AntiAliasingMode.off)
			{
				_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
			}
			else if (_settings_holder.CurrentAntiAliasingMode == GameSettingsHolder.AntiAliasingMode.FXAA)
			{
				_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.off;
				_settings_holder.CurrentFSRMode = GameSettingsHolder.FSRMode.off;
			}
			else if (_settings_holder.CurrentAntiAliasingMode == GameSettingsHolder.AntiAliasingMode.TAA)
			{
				_settings_holder.CurrentDLSSMode = GameSettingsHolder.DLSSMode.off;
			}
			_isUpdating = false;
		}
	}

	protected void on_click_cancel_settings(object sender, RoutedEventArgs e)
	{
		_settings_holder.LoadSettings();
		Hide();
	}

	protected void on_click_accept_settings(object sender, RoutedEventArgs e)
	{
		_settings_holder.SaveSettings();
		Hide();
	}

	private async void on_click_reset_settings(object sender, RoutedEventArgs e)
	{
		_settings_holder.LoadSettings(force_default: true);
		BlurEffect blurEffect = new BlurEffect();
		blurEffect.Radius = 9.0;
		base.Effect = blurEffect;
		base.IsEnabled = false;
		await _settings_holder.VerifyQualitySetting();
		base.Effect = null;
		base.IsEnabled = true;
	}
}
