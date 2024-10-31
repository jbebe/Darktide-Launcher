using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using Launcher.Properties;
using LauncherHelper;
using ResourceDictionary;
using ResourceDictionary.Properties;
using Stingray.Json;

namespace Launcher;

public class GameSettingsHolder : INotifyPropertyChanged
{
	public enum QualityType
	{
		None,
		low,
		medium,
		high,
		custom
	}

	public enum ScreenMode
	{
		fullscreen,
		window
	}

	public struct RaytracingQualitySettings
	{
		public RayTracingMode Reflections;

		public RayTracingMode Rtxgi;

		public RayTracingMode Preset;

		private static List<RaytracingQualitySettings> _finalQualityType = new List<RaytracingQualitySettings>
		{
			new RaytracingQualitySettings
			{
				Reflections = RayTracingMode.off,
				Rtxgi = RayTracingMode.off,
				Preset = RayTracingMode.off
			},
			new RaytracingQualitySettings
			{
				Reflections = RayTracingMode.low,
				Rtxgi = RayTracingMode.low,
				Preset = RayTracingMode.low
			},
			new RaytracingQualitySettings
			{
				Reflections = RayTracingMode.high,
				Rtxgi = RayTracingMode.medium,
				Preset = RayTracingMode.medium
			},
			new RaytracingQualitySettings
			{
				Reflections = RayTracingMode.high,
				Rtxgi = RayTracingMode.high,
				Preset = RayTracingMode.high
			}
		};

		public static void SetRaytracingSettingOption(Dictionary<string, object> dict, RayTracingMode mode)
		{
			string key = "rt_reflections_quality";
			string key2 = "rtxgi_quality";
			RaytracingQualitySettings raytracingQualitySettings = _finalQualityType.FirstOrDefault((RaytracingQualitySettings x) => x.Preset == mode);
			dict[key] = raytracingQualitySettings.Reflections.ToString();
			dict[key2] = raytracingQualitySettings.Rtxgi.ToString();
		}

		public static RayTracingMode GetRaytracingSettingOption(Dictionary<string, object> dict, bool forceDefault)
		{
			string key = "rt_reflections_quality";
			string key2 = "rtxgi_quality";
			RayTracingMode reflectionQuality = ParseEnumSetting(dict, key, RayTracingMode.off, forceDefault);
			RayTracingMode rtxgiQuality = ParseEnumSetting(dict, key2, RayTracingMode.off, forceDefault);
			RayTracingMode result = RayTracingMode.custom;
			IEnumerable<RaytracingQualitySettings> enumerable = _finalQualityType.Where((RaytracingQualitySettings x) => x.Reflections == reflectionQuality && x.Rtxgi == rtxgiQuality);
			if (!enumerable.IsEmpty())
			{
				result = enumerable.FirstOrDefault().Preset;
			}
			return result;
		}
	}

	public enum RayTracingMode
	{
		off,
		low,
		medium,
		high,
		custom
	}

	public enum DLSSMode
	{
		off,
		on
	}

	public enum SuperResolutionMode
	{
		off,
		auto,
		ultra_performance,
		max_performance,
		balanced,
		max_quality
	}

	public enum ReflexMode
	{
		disabled,
		enabled,
		boost
	}

	public enum FrameGenerationMode
	{
		off,
		on
	}

	public enum FSRMode
	{
		off,
		performance,
		balanced,
		quality,
		ultra_quality
	}

	public enum FSR2Mode
	{
		off,
		ultra_performance,
		performance,
		balanced,
		quality
	}

	public enum FFXFGMode
	{
		off,
		on
	}

	public enum XeSSMode
	{
		off,
		performance,
		balanced,
		quality,
		ultra_quality
	}

	public enum AntiAliasingMode
	{
		off,
		FXAA,
		TAA
	}

	public const bool DISABLE_LANGUAGES = false;

	private const int SETTINGS_VERSION = 3;

	public static QualityType DefaultQualityType = QualityType.medium;

	private int CurrentSettingsVersion;

	private Dictionary<string, object> _settings = new Dictionary<string, object>();

	private Dictionary<string, Func<object>> _setting_defaults = new Dictionary<string, Func<object>>();

	private SysInfo _system_info;

	private Window _ownerWindow;

	public IDictionary<QualityType, string> quality_types;

	private readonly CollectionView _qualities;

	protected QualityType _quality_type;

	protected SysInfo.Output _current_output;

	private readonly CollectionView _screen_modes;

	protected ScreenMode _screen_mode;

	private readonly CollectionView _on_off_feature_vsync;

	protected bool _vsync;

	private readonly IDictionary _rayTracingModes;

	protected RayTracingMode _rayTracingMode;

	private readonly IDictionary _DLSSModes;

	protected DLSSMode _dlssMode;

	private readonly IDictionary _SuperResolutionModes;

	protected SuperResolutionMode _CurrentSuperResolutionMode;

	private readonly IDictionary _ReflexModes;

	protected ReflexMode _CurrentReflexMode;

	private readonly IDictionary _FrameGenerationModes;

	protected FrameGenerationMode _CurrentFrameGenerationMode;

	private readonly IDictionary _FSRModes;

	protected FSRMode _FSRMode;

	private readonly IDictionary _FSR2Modes;

	protected FSR2Mode _FSR2Mode;

	private readonly IDictionary _FFXFGModes;

	protected FFXFGMode _FFXFGMode;

	private readonly IDictionary _XeSSModes;

	protected XeSSMode _XeSSMode;

	private readonly IDictionary _AntiAliasingModes;

	protected AntiAliasingMode _AntiAliasingMode;

	private readonly CollectionView _on_off_force_flush;

	protected bool _force_flush;

	private readonly CollectionView _on_off_debugger;

	private bool _wait_for_debugger;

	protected double _current_worker_threads = 1.0;

	protected double _max_worker_threads = 1.0;

	protected LocalizationManager.LanguageFormat _language;

	public readonly CollectionView _featureStatus;

	private float _gamma;

	private int _framerateCap;

	private string _gpuId = "";

	private bool _GPUCrashDumpsOverride;

	private bool _IPv6NetworkEnabled;

	public bool IsInitialized { get; set; }

	public bool RayTracingOptionVisible
	{
		get
		{
			if (_current_output.Features != null)
			{
				return _current_output.Features.SupportsRaytracing;
			}
			return false;
		}
	}

	public bool DLSSOptionVisible
	{
		get
		{
			if (_current_output.Features is DirectXInterface.NvidiaSupportedFeatures nvidiaSupportedFeatures)
			{
				return nvidiaSupportedFeatures.SupportsDLSS;
			}
			return false;
		}
	}

	public bool FrameGenerationOptionVisible
	{
		get
		{
			if (_current_output.Features is DirectXInterface.NvidiaSupportedFeatures nvidiaSupportedFeatures)
			{
				return nvidiaSupportedFeatures.SupportsDLSS_G;
			}
			return false;
		}
	}

	public bool FrameGenerationDisabledVisible
	{
		get
		{
			if (_current_output.Features is DirectXInterface.NvidiaSupportedFeatures nvidiaSupportedFeatures)
			{
				return nvidiaSupportedFeatures.DisabledDLSS_G;
			}
			return false;
		}
	}

	public bool ReflexOptionVisible
	{
		get
		{
			if (_current_output.Features is DirectXInterface.NvidiaSupportedFeatures nvidiaSupportedFeatures)
			{
				return nvidiaSupportedFeatures.SupportsReflex;
			}
			return false;
		}
	}

	public bool AntiAliasingOptionEnabled
	{
		get
		{
			if (CurrentDLSSMode == DLSSMode.off && CurrentFSR2Mode == FSR2Mode.off)
			{
				return CurrentXeSSMode == XeSSMode.off;
			}
			return false;
		}
	}

	public bool ReflexOptionEnabled => CurrentFrameGenerationMode == FrameGenerationMode.off;

	public bool GPUCrashDumpsOptionVisible => _current_output.Features is DirectXInterface.NvidiaSupportedFeatures;

	public bool ResetKeyboardsBindings { get; set; }

	public bool FileVerificationPassed { get; set; }

	private bool HasChangedGPUHardware
	{
		get
		{
			if (!string.IsNullOrEmpty(_gpuId))
			{
				return _current_output.AdapterName != _gpuId;
			}
			return false;
		}
	}

	public FontFamily CurrentFontFamily => FontManager.CurrentFont;

	public CollectionView Qualities => _qualities;

	public QualityType CurrentQualityType
	{
		get
		{
			return _quality_type;
		}
		set
		{
			if (_quality_type != value)
			{
				_quality_type = value;
				OnPropertyChanged("CurrentQualityType");
			}
		}
	}

	public List<SysInfo.Output> Outputs => _system_info._outputs;

	public SysInfo.Output CurrentOutput
	{
		get
		{
			return _current_output;
		}
		set
		{
			if (value == _current_output)
			{
				return;
			}
			if (_current_output != null && value != null)
			{
				SysInfo.Resolution previous_resolution = _current_output.CurrentResolution;
				SysInfo.Resolution resolution = value.Resolutions.Find((SysInfo.Resolution i) => i.Width == previous_resolution.Width && i.Height == previous_resolution.Height);
				if (resolution != null)
				{
					value.CurrentResolution = resolution;
				}
			}
			_current_output = value;
			OnPropertyChanged("CurrentOutput");
		}
	}

	public CollectionView ScreenModes => _screen_modes;

	public ScreenMode CurrentScreenMode
	{
		get
		{
			return _screen_mode;
		}
		set
		{
			if (_screen_mode != value)
			{
				_screen_mode = value;
				OnPropertyChanged("CurrentScreenMode");
			}
		}
	}

	public CollectionView VsyncSettings => _on_off_feature_vsync;

	public bool Vsync
	{
		get
		{
			return _vsync;
		}
		set
		{
			if (_vsync != value)
			{
				_vsync = value;
				OnPropertyChanged("Vsync");
			}
		}
	}

	public IDictionary RayTracingModes => _rayTracingModes;

	public RayTracingMode CurrentRayTracingMode
	{
		get
		{
			return _rayTracingMode;
		}
		set
		{
			if (_rayTracingMode != value && (_current_output == null || RayTracingOptionVisible))
			{
				_rayTracingMode = value;
				OnPropertyChanged("CurrentRayTracingMode");
			}
		}
	}

	public IDictionary DLSSModes => _DLSSModes;

	public DLSSMode CurrentDLSSMode
	{
		get
		{
			return _dlssMode;
		}
		set
		{
			if (_dlssMode != value && (_current_output == null || DLSSOptionVisible))
			{
				_dlssMode = value;
				OnPropertyChanged("CurrentDLSSMode");
				OnPropertyChanged("AntiAliasingOptionEnabled");
			}
		}
	}

	public IDictionary SuperResolutionModes => _SuperResolutionModes;

	public SuperResolutionMode CurrentSuperResolutionMode
	{
		get
		{
			return _CurrentSuperResolutionMode;
		}
		set
		{
			if (_CurrentSuperResolutionMode != value && (_current_output == null || DLSSOptionVisible))
			{
				_CurrentSuperResolutionMode = value;
				OnPropertyChanged("CurrentSuperResolutionMode");
			}
		}
	}

	public IDictionary ReflexModes => _ReflexModes;

	public ReflexMode CurrentReflexMode
	{
		get
		{
			return _CurrentReflexMode;
		}
		set
		{
			if (_CurrentReflexMode != value && (_current_output == null || ReflexOptionVisible))
			{
				_CurrentReflexMode = value;
				OnPropertyChanged("CurrentReflexMode");
			}
		}
	}

	public IDictionary FrameGenerationModes => _FrameGenerationModes;

	public FrameGenerationMode CurrentFrameGenerationMode
	{
		get
		{
			return _CurrentFrameGenerationMode;
		}
		set
		{
			if (_CurrentFrameGenerationMode != value && (_current_output == null || FrameGenerationOptionVisible))
			{
				_CurrentFrameGenerationMode = value;
				OnPropertyChanged("ReflexOptionEnabled");
				OnPropertyChanged("CurrentFrameGenerationMode");
			}
		}
	}

	public IDictionary FSRModes => _FSRModes;

	public FSRMode CurrentFSRMode
	{
		get
		{
			return _FSRMode;
		}
		set
		{
			if (_FSRMode != value)
			{
				_FSRMode = value;
				OnPropertyChanged("CurrentFSRMode");
				OnPropertyChanged("AntiAliasingOptionEnabled");
			}
		}
	}

	public IDictionary FSR2Modes => _FSR2Modes;

	public FSR2Mode CurrentFSR2Mode
	{
		get
		{
			return _FSR2Mode;
		}
		set
		{
			if (_FSR2Mode != value)
			{
				_FSR2Mode = value;
				OnPropertyChanged("CurrentFSR2Mode");
				OnPropertyChanged("AntiAliasingOptionEnabled");
			}
		}
	}

	public IDictionary FFXFGModes => _FFXFGModes;

	public FFXFGMode CurrentFFXFGMode
	{
		get
		{
			return _FFXFGMode;
		}
		set
		{
			if (_FFXFGMode != value)
			{
				_FFXFGMode = value;
				OnPropertyChanged("CurrentFFXFGMode");
			}
		}
	}

	public IDictionary XeSSModes => _XeSSModes;

	public XeSSMode CurrentXeSSMode
	{
		get
		{
			return _XeSSMode;
		}
		set
		{
			if (_XeSSMode != value)
			{
				_XeSSMode = value;
				OnPropertyChanged("CurrentXeSSMode");
				OnPropertyChanged("AntiAliasingOptionEnabled");
			}
		}
	}

	public IDictionary AntiAliasingModes => _AntiAliasingModes;

	public AntiAliasingMode CurrentAntiAliasingMode
	{
		get
		{
			return _AntiAliasingMode;
		}
		set
		{
			if (_AntiAliasingMode != value)
			{
				_AntiAliasingMode = value;
				OnPropertyChanged("CurrentAntiAliasingMode");
			}
		}
	}

	public CollectionView ForceFlushSettings => _on_off_force_flush;

	public bool ForceFlush
	{
		get
		{
			return _force_flush;
		}
		set
		{
			if (_force_flush != value)
			{
				if (value && value != _force_flush)
				{
					System.Windows.MessageBox.Show(Resources.LOC_FORCE_FLUSH_WARNING, Resources.LOC_FORCE_FLUSH_WARNING_TITLE, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				_force_flush = value;
				OnPropertyChanged("ForceFlush");
			}
		}
	}

	public CollectionView WaitForDebuggerSettings => _on_off_debugger;

	public bool WaitForDebugger
	{
		get
		{
			return _wait_for_debugger;
		}
		set
		{
			if (_wait_for_debugger != value)
			{
				if (value && value != _force_flush)
				{
					System.Windows.MessageBox.Show(Resources.LOC_WAIT_DEBUGGER_MESSAGE, Resources.LOC_WAIT_DEBUGGER_TITLE, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				_wait_for_debugger = value;
				OnPropertyChanged("WaitForDebugger");
			}
		}
	}

	public double CurrentWorkerThreads
	{
		get
		{
			return _current_worker_threads;
		}
		set
		{
			if (_current_worker_threads != value)
			{
				_current_worker_threads = value;
				OnPropertyChanged("CurrentWorkerThreads");
				OnPropertyChanged("WorkerThreadForegroundBrush");
			}
		}
	}

	public double MaxWorkerThreads
	{
		get
		{
			return _max_worker_threads;
		}
		set
		{
			_max_worker_threads = value;
			OnPropertyChanged("MaxWorkerThreads");
		}
	}

	public Brush WorkerThreadForegroundBrush
	{
		get
		{
			if (_current_worker_threads == _max_worker_threads)
			{
				return new SolidColorBrush(Colors.Red);
			}
			return new SolidColorBrush(Color.FromArgb(byte.MaxValue, 172, 168, 134));
		}
	}

	public LocalizationManager.LanguageFormat CurrentLanguage
	{
		get
		{
			return _language;
		}
		set
		{
			if (_language != value)
			{
				_language = value;
				OnPropertyChanged("CurrentLanguage");
			}
		}
	}

	public CollectionView FeatureStatus => _featureStatus;

	public bool GPUCrashDumpsOverride
	{
		get
		{
			return _GPUCrashDumpsOverride;
		}
		set
		{
			if (_GPUCrashDumpsOverride != value)
			{
				_GPUCrashDumpsOverride = value;
				OnPropertyChanged("GPUCrashDumpsOverride");
			}
		}
	}

	public bool IPv6NetworkEnabled
	{
		get
		{
			return _IPv6NetworkEnabled;
		}
		set
		{
			if (_IPv6NetworkEnabled != value)
			{
				_IPv6NetworkEnabled = value;
				OnPropertyChanged("IPv6NetworkEnabled");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public GameSettingsHolder(GameSettingsHolder other) : this(other._ownerWindow, other._system_info, other.CurrentLanguage) {}

	public GameSettingsHolder(Window ownerWindow, SysInfo si, LocalizationManager.LanguageFormat currentLanguage)
	{
		_ownerWindow = ownerWindow;
		_system_info = si;
		CurrentLanguage = currentLanguage;
		quality_types = new Dictionary<QualityType, string>();
		quality_types.Add(QualityType.low, Resources.LOC_Low);
		quality_types.Add(QualityType.medium, Resources.LOC_Medium);
		quality_types.Add(QualityType.high, Resources.LOC_High);
		quality_types.Add(QualityType.custom, Resources.LOC_Custom);
		_qualities = new CollectionView(quality_types);
		_screen_modes = new CollectionView(new Dictionary<ScreenMode, string>
		{
			{
				ScreenMode.fullscreen,
				Resources.LOC_FULLSCREEN
			},
			{
				ScreenMode.window,
				Resources.LOC_WINDOW
			}
		});
		IDictionary<bool, string> collection = new Dictionary<bool, string>
		{
			{
				true,
				Resources.LOC_FEATURE_ON
			},
			{
				false,
				Resources.LOC_FEATURE_OFF
			}
		};
		_on_off_feature_vsync = new CollectionView(collection);
		_on_off_debugger = new CollectionView(collection);
		_on_off_force_flush = new CollectionView(collection);
		_DLSSModes = new Dictionary<DLSSMode, string>();
		_DLSSModes.Add(DLSSMode.off, Resources.LOC_setting_dlss_quality_off);
		_DLSSModes.Add(DLSSMode.on, Resources.LOC_setting_dlss_quality_on);
		_SuperResolutionModes = new Dictionary<SuperResolutionMode, string>();
		_SuperResolutionModes.Add(SuperResolutionMode.off, Resources.LOC_setting_dlss_quality_off);
		_SuperResolutionModes.Add(SuperResolutionMode.auto, Resources.LOC_setting_dlss_quality_auto);
		_SuperResolutionModes.Add(SuperResolutionMode.ultra_performance, Resources.LOC_setting_dlss_quality_ultra_performance);
		_SuperResolutionModes.Add(SuperResolutionMode.max_performance, Resources.LOC_setting_dlss_quality_max_performance);
		_SuperResolutionModes.Add(SuperResolutionMode.balanced, Resources.LOC_setting_dlss_quality_balanced);
		_SuperResolutionModes.Add(SuperResolutionMode.max_quality, Resources.LOC_setting_dlss_quality_max_quality);
		_FrameGenerationModes = new Dictionary<FrameGenerationMode, string>();
		_FrameGenerationModes.Add(FrameGenerationMode.off, Resources.LOC_FEATURE_OFF);
		_FrameGenerationModes.Add(FrameGenerationMode.on, Resources.LOC_FEATURE_ON);
		_ReflexModes = new Dictionary<ReflexMode, string>();
		_ReflexModes.Add(ReflexMode.disabled, Resources.loc_setting_nv_reflex_disabled);
		_ReflexModes.Add(ReflexMode.enabled, Resources.loc_setting_nv_reflex_low_latency_enabled);
		_ReflexModes.Add(ReflexMode.boost, Resources.loc_setting_nv_reflex_low_latency_boost);
		_FSRModes = new Dictionary<FSRMode, string>();
		_FSRModes.Add(FSRMode.off, Resources.LOC_setting_fsr_quality_off);
		_FSRModes.Add(FSRMode.performance, Resources.LOC_setting_fsr_quality_performance);
		_FSRModes.Add(FSRMode.balanced, Resources.LOC_setting_fsr_quality_balanced);
		_FSRModes.Add(FSRMode.quality, Resources.LOC_setting_fsr_quality_quality);
		_FSRModes.Add(FSRMode.ultra_quality, Resources.LOC_setting_fsr_quality_ultra_quality);
		_FSR2Modes = new Dictionary<FSR2Mode, string>();
		_FSR2Modes.Add(FSR2Mode.off, Resources.LOC_setting_fsr_quality_off);
		_FSR2Modes.Add(FSR2Mode.ultra_performance, Resources.LOC_setting_dlss_quality_ultra_performance);
		_FSR2Modes.Add(FSR2Mode.performance, Resources.LOC_setting_dlss_quality_max_performance);
		_FSR2Modes.Add(FSR2Mode.balanced, Resources.LOC_setting_dlss_quality_balanced);
		_FSR2Modes.Add(FSR2Mode.quality, Resources.LOC_setting_dlss_quality_max_quality);
		_FFXFGModes = new Dictionary<FFXFGMode, string>();
		_FFXFGModes.Add(FFXFGMode.off, Resources.LOC_FEATURE_OFF);
		_FFXFGModes.Add(FFXFGMode.on, Resources.LOC_FEATURE_ON);
		_XeSSModes = new Dictionary<XeSSMode, string>();
		_XeSSModes.Add(XeSSMode.off, Resources.LOC_setting_fsr_quality_off);
		_XeSSModes.Add(XeSSMode.performance, Resources.LOC_setting_fsr_quality_performance);
		_XeSSModes.Add(XeSSMode.balanced, Resources.LOC_setting_fsr_quality_balanced);
		_XeSSModes.Add(XeSSMode.quality, Resources.LOC_setting_fsr_quality_quality);
		_XeSSModes.Add(XeSSMode.ultra_quality, Resources.LOC_setting_fsr_quality_ultra_quality);
		_AntiAliasingModes = new Dictionary<AntiAliasingMode, string>();
		_AntiAliasingModes.Add(AntiAliasingMode.off, Resources.LOC_setting_anti_ailiasing_off);
		_AntiAliasingModes.Add(AntiAliasingMode.FXAA, Resources.LOC_setting_anti_ailiasing_FXAA);
		_AntiAliasingModes.Add(AntiAliasingMode.TAA, Resources.LOC_setting_anti_ailiasing_TAA);
		_rayTracingModes = new Dictionary<RayTracingMode, string>();
		_rayTracingModes.Add(RayTracingMode.off, Resources.LOC_FEATURE_OFF);
		_rayTracingModes.Add(RayTracingMode.low, Resources.LOC_Low);
		_rayTracingModes.Add(RayTracingMode.medium, Resources.LOC_Medium);
		_rayTracingModes.Add(RayTracingMode.high, Resources.LOC_High);
		_rayTracingModes.Add(RayTracingMode.custom, Resources.LOC_Custom);
		Dictionary<string, object> master_render_settings = new Dictionary<string, object>();
		master_render_settings.Add("graphics_quality", QualityType.None);
		master_render_settings.Add("fsr", 0);
		master_render_settings.Add("fsr2", 0);
		master_render_settings.Add("ffx_frame_gen", 0);
		master_render_settings.Add("xess", 0);
		master_render_settings.Add("dlss_master", DLSSMode.off.ToString());
		master_render_settings.Add("dlss", 0);
		master_render_settings.Add("dlss_g", 0);
		master_render_settings.Add("nv_reflex_low_latency", 0);
		master_render_settings.Add("anti_aliasing_solution", AntiAliasingMode.off);
		RaytracingQualitySettings.SetRaytracingSettingOption(master_render_settings, RayTracingMode.off);
		master_render_settings.Add("nv_reflex_framerate_cap", 0);
		Dictionary<string, object> launcher_overrides = new Dictionary<string, object>();
		launcher_overrides.Add("gpu_crash_dumps", false);
		Dictionary<string, object> networkSettings = new Dictionary<string, object>();
		networkSettings.Add("try_nat64", false);
		_setting_defaults.Add("launcher_overrides", () => launcher_overrides);
		_setting_defaults.Add("master_render_settings", () => master_render_settings);
		_setting_defaults.Add("network_settings", () => networkSettings);
		_setting_defaults.Add("adapter_index", () => 0);
		_setting_defaults.Add("gamma", () => 0);
		_setting_defaults.Add("fullscreen_output", () => 0);
		_setting_defaults.Add("fullscreen", () => true);
		_setting_defaults.Add("screen_mode", () => ScreenMode.fullscreen.ToString());
		_setting_defaults.Add("screen_resolution", () => (CurrentOutput == null) ? _system_info._primary_output?.CurrentResolution : CurrentOutput.CurrentResolution);
		_setting_defaults.Add("last_fullscreen_resolution", () => (CurrentOutput == null) ? new List<object>
		{
			_system_info._primary_output?.CurrentResolution.Width,
			_system_info._primary_output?.CurrentResolution.Height
		} : new List<object>
		{
			CurrentOutput.CurrentResolution.Width,
			CurrentOutput.CurrentResolution
		});
		_setting_defaults.Add("vsync", () => false);
		_setting_defaults.Add("language_id", () => LocalizationManager.GetLanguageFromNameOrDefault(LocalizationManager.Language_id.english.ToString()));
		_setting_defaults.Add("max_worker_threads", () => (double)Math.Max(1, Environment.ProcessorCount - 3));
		_setting_defaults.Add("version", () => 3);
		_setting_defaults.Add("launcher_verification_passed", () => FileVerificationPassed);
		LoadSettings();
	}

	protected void ShowDetectDialog(SysInfo.DetectQualityResult result)
	{
		QualityDialog obj = new QualityDialog(this, result)
		{
			Owner = _ownerWindow
		};
		ScreenHandler.DoCenterTop(obj, _ownerWindow);
		obj.ShowDialog();
	}

	protected void ShowDisableRaytracingDialog(RayTracingMode rayTracingMode)
	{
		if (rayTracingMode != 0)
		{
			DisableRaytracingMessageWindow obj = new DisableRaytracingMessageWindow
			{
				Owner = _ownerWindow
			};
			ScreenHandler.DoCenterTop(obj, _ownerWindow);
			obj.ShowDialog();
			if (obj.Result == DialogResult.Yes)
			{
				CurrentRayTracingMode = RayTracingMode.off;
			}
		}
	}

	private string settingsFilePath()
	{
		string project = Settings.Default.Project;
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "fatshark", project, Settings.Default.SettingsFile);
	}

	public async Task VerifyQualitySetting()
	{
		try
		{
			FileLogger.Instance.PushScope("Quality Settings");
			FileLogger.Instance.CreateEntry("Verifying Quality Settings");
			if (CurrentQualityType == QualityType.None)
			{
				LoadingDialog loadingDialog = new LoadingDialog
				{
					Message = Resources.LOC_FetchingHardwareConfig,
					Owner = _ownerWindow
				};
				ScreenHandler.DoCenter(loadingDialog, _ownerWindow);
				loadingDialog.Show();
				SysInfo.DetectQualityResult result = await _system_info.detect_quality();
				loadingDialog.Close();
				IsInitialized = false;
				CurrentQualityType = result.preferred_quality;
				CurrentRayTracingMode = result.RayTracingMode;
				CurrentDLSSMode = result.DLSSMode;
				CurrentFSRMode = result.FSRMode;
				CurrentFSR2Mode = result.FSR2Mode;
				CurrentFFXFGMode = result.FFXFGMode;
				CurrentXeSSMode = result.XeSSMode;
				CurrentAntiAliasingMode = result.AntiAliasingMode;
				CurrentFrameGenerationMode = result.FrameGenerationMode;
				CurrentReflexMode = result.ReflexMode;
				CurrentSuperResolutionMode = result.SuperResolutionMode;
				SetDefaultDetectedSettings();
				IsInitialized = true;
				FileLogger.Instance.CreateEntry($"Quality: {result.preferred_quality}");
				ActionManager.ExecuteAction("BlurryBackground");
				ShowDetectDialog(result);
				ShowDisableRaytracingDialog(result.RayTracingMode);
				ActionManager.ExecuteAction("UnBlurryBackground");
			}
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"Exception while verifying quality settings:\n{arg}");
			IsInitialized = true;
		}
		finally
		{
			ActionManager.ExecuteAction("EnableWindow");
			FileLogger.Instance.CreateEntry("Quality Settings Completed");
			SaveSettings();
			FileLogger.Instance.CreateEntry("Quality Settings Saved");
			FileLogger.Instance.PopScope();
		}
	}

	private Dictionary<string, object> ReadFromSettingsFile(string path)
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		try
		{
			if (File.Exists(path))
			{
				result = SJSON.LoadGeneric(path);
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error loading game settings: " + ex.Message);
		}
		return result;
	}

	private void writeToSettingsFile(Dictionary<string, object> settings, string path)
	{
		try
		{
			SJSON.Save(settings, path);
		}
		catch (Exception ex)
		{
			Console.Out.WriteLine("ERROR SAVING SETTINGS: " + ex.ToString());
		}
	}

	private T parseSetting<T>(string key, Func<object, T> parse_func, bool force_default)
	{
		if (!_setting_defaults.ContainsKey(key) || _setting_defaults[key] == null)
		{
			FileLogger.Instance.CreateEntry("!! Error parsing settings for '" + key + "'! No default function! Returning generic!");
			return default(T);
		}
		object obj = null;
		try
		{
			obj = _setting_defaults[key]();
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"!! Failed getting defaults for '{key}'!\n{arg}");
		}
		T val = default(T);
		if (obj is T val2)
		{
			val = val2;
		}
		else
		{
			FileLogger.Instance.CreateEntry("!! Error parsing settings for '" + key + "'! Default value invalid! Returning generic!");
		}
		if (force_default)
		{
			return val;
		}
		try
		{
			if (_settings.ContainsKey(key))
			{
				object obj2 = parse_func(_settings[key]);
				if (obj2 is T)
				{
					return (T)obj2;
				}
				FileLogger.Instance.CreateEntry("!! Something went wrong parsing setting for '" + key + "'! Returning generic!");
			}
			else
			{
				FileLogger.Instance.CreateEntry($"Could not find setting '{key}', using default value '{val}'");
			}
		}
		catch
		{
			FileLogger.Instance.CreateEntry("!! Malformed setting '" + key + "', using default value '" + (val.ToString() ?? "NULL") + "'");
		}
		return val;
	}

	public void VerifySettingsVersion()
	{
		try
		{
			if (CurrentSettingsVersion != 3 || HasChangedGPUHardware)
			{
				_settings["master_render_settings"] = new Dictionary<string, object>();
				_settings["render_settings"] = new Dictionary<string, object>();
				_settings["launcher_overrides"] = new Dictionary<string, object>();
				_settings.Remove("adapter_index");
				_settings.Remove("fullscreen_output");
				_settings.Remove("screen_mode");
				_settings.Remove("fullscreen");
				_settings.Remove("screen_resolution");
				_settings.Remove("last_fullscreen_resolution");
				_settings.Remove("vsync");
				_settings.Remove("language_id");
				_settings.Remove("max_worker_threads");
				_settings["version"] = 3;
				CurrentQualityType = QualityType.None;
			}
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"Failed to verifying settings version:\n{arg}");
		}
	}

	public void LoadSettings(bool force_default = false)
	{
		IsInitialized = false;
		_settings = ReadFromSettingsFile(settingsFilePath());
		if (_settings.ContainsKey("borderless_fullscreen"))
		{
			bool flag = (bool)_settings["borderless_fullscreen"] || (_settings.ContainsKey("fullscreen") && (bool)_settings["fullscreen"]);
			_settings["fullscreen"] = flag;
			_settings.Remove("borderless_fullscreen");
			_settings["screen_mode"] = (flag ? ScreenMode.fullscreen.ToString() : ScreenMode.window.ToString());
		}
		if (!_settings.ContainsKey("last_fullscreen_resolution"))
		{
			_settings.Add("last_fullscreen_resolution", _setting_defaults["last_fullscreen_resolution"]());
		}
		if (force_default && _settings.ContainsKey("render_settings"))
		{
			_settings["render_settings"] = new Dictionary<string, object>();
		}
		Dictionary<string, object> dictionary = parseSetting("launcher_overrides", (object x) => (x == null) ? new Dictionary<string, object>() : (x as Dictionary<string, object>), force_default);
		Dictionary<string, object> dictionary2 = parseSetting("network_settings", (object x) => (x == null) ? new Dictionary<string, object>() : (x as Dictionary<string, object>), force_default);
		GPUCrashDumpsOverride = dictionary.ContainsKey("gpu_crash_dumps") && Convert.ToBoolean(dictionary["gpu_crash_dumps"]);
		IPv6NetworkEnabled = dictionary2.ContainsKey("try_nat64") && Convert.ToBoolean(dictionary2["try_nat64"]);
		Dictionary<string, object> dictionary3 = parseSetting("master_render_settings", (object x) => (x == null) ? new Dictionary<string, object>() : (x as Dictionary<string, object>), force_default);
		CurrentQualityType = ParseEnumSetting(dictionary3, "graphics_quality", QualityType.None, force_default);
		CurrentDLSSMode = ParseEnumSetting(dictionary3, "dlss_master", DLSSMode.off, force_default);
		CurrentSuperResolutionMode = ParseEnumSetting(dictionary3, "dlss", SuperResolutionMode.off, force_default);
		CurrentFrameGenerationMode = ParseEnumSetting(dictionary3, "dlss_g", FrameGenerationMode.off, force_default);
		CurrentReflexMode = ParseEnumSetting(dictionary3, "nv_reflex_low_latency", ReflexMode.disabled, force_default);
		CurrentFSRMode = ParseEnumSetting(dictionary3, "fsr", FSRMode.off, force_default);
		CurrentFSR2Mode = ParseEnumSetting(dictionary3, "fsr2", FSR2Mode.off, force_default);
		CurrentFFXFGMode = ParseEnumSetting(dictionary3, "ffx_frame_gen", FFXFGMode.on, force_default);
		CurrentXeSSMode = ParseEnumSetting(dictionary3, "xess", XeSSMode.off, force_default);
		CurrentAntiAliasingMode = ParseEnumSetting(dictionary3, "anti_aliasing_solution", AntiAliasingMode.off, force_default);
		CurrentRayTracingMode = RaytracingQualitySettings.GetRaytracingSettingOption(dictionary3, force_default);
		if (force_default)
		{
			_framerateCap = 0;
		}
		else
		{
			_framerateCap = (dictionary3.ContainsKey("nv_reflex_framerate_cap") ? Convert.ToInt32(dictionary3["nv_reflex_framerate_cap"]) : 0);
		}
		_gamma = parseSetting("gamma", (object x) => Convert.ToSingle(x), force_default);
		int adapter_id = parseSetting("adapter_index", (object x) => Convert.ToInt32(x), force_default);
		int output_id = parseSetting("fullscreen_output", delegate(object x)
		{
			int num = Convert.ToInt32(x);
			if (num > _system_info._outputs.Count - 1 || num < 0)
			{
				num = _system_info._primary_output.OutputId;
			}
			return num;
		}, force_default);
		CurrentOutput = _system_info._outputs.Find((SysInfo.Output x) => x.AdapterId == adapter_id && x.OutputId == output_id);
		CurrentOutput = CurrentOutput ?? _system_info._primary_output;
		SysInfo.Resolution resolution = parseSetting("screen_resolution", delegate(object x)
		{
			if (x == null)
			{
				return new SysInfo.Resolution(1280, 720);
			}
			List<object> obj2 = x as List<object>;
			int width = int.Parse(obj2[0].ToString());
			int height = int.Parse(obj2[1].ToString());
			return new SysInfo.Resolution(width, height);
		}, force_default);
		SysInfo.Resolution currentResolution = CurrentOutput.CurrentResolution;
		CurrentOutput.MatchResolution(CurrentScreenMode, resolution.Width, resolution.Height);
		if (currentResolution != CurrentOutput.CurrentResolution)
		{
			OnPropertyChanged("CurrentOutput");
		}
		CurrentScreenMode = ParseEnumSetting(_settings, "screen_mode", ScreenMode.fullscreen, force_default);
		Vsync = parseSetting("vsync", (object x) => Convert.ToBoolean(x), force_default);
		ForceFlush = false;
		WaitForDebugger = false;
		int processorCount = Environment.ProcessorCount;
		MaxWorkerThreads = Math.Max(1, processorCount - 2);
		CurrentWorkerThreads = parseSetting("max_worker_threads", (object x) => Math.Max(1.0, int.Parse(x.ToString())), force_default);
		if (_settings.ContainsKey("version"))
		{
			try
			{
				CurrentSettingsVersion = Convert.ToInt32(_settings["version"]);
			}
			catch
			{
			}
		}
		FileVerificationPassed = parseSetting("launcher_verification_passed", (object x) => Convert.ToBoolean(x), force_default: false);
		_gpuId = _current_output.AdapterName;
		if (_settings.ContainsKey("gpu_id") && !string.IsNullOrEmpty(_settings["gpu_id"].ToString()))
		{
			_gpuId = _settings["gpu_id"].ToString();
		}
		VerifySettingsVersion();
		IsInitialized = true;
	}

	private static T ParseEnumSetting<T>(Dictionary<string, object> container, string key, T defaultValue, bool forceDefault) where T : struct
	{
		if (forceDefault)
		{
			return defaultValue;
		}
		if (container.ContainsKey(key) && Enum.TryParse<T>(container[key].ToString(), out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public void SetDefaultDetectedSettings()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["graphics_quality"] = CurrentQualityType.ToString();
		dictionary["fsr"] = (int)CurrentFSRMode;
		dictionary["fsr2"] = (int)CurrentFSR2Mode;
		dictionary["ffx_frame_gen"] = (int)CurrentFFXFGMode;
		dictionary["xess"] = (int)CurrentXeSSMode;
		dictionary["dlss_master"] = CurrentDLSSMode.ToString();
		dictionary["dlss"] = (int)CurrentSuperResolutionMode;
		dictionary["dlss_g"] = (int)CurrentFrameGenerationMode;
		dictionary["nv_reflex_low_latency"] = (int)CurrentReflexMode;
		dictionary["anti_aliasing_solution"] = (int)CurrentAntiAliasingMode;
		dictionary["nv_reflex_framerate_cap"] = 0;
		RaytracingQualitySettings.SetRaytracingSettingOption(dictionary, CurrentRayTracingMode);
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["adapter_index"] = CurrentOutput.AdapterId;
		dictionary2["fullscreen_output"] = CurrentOutput.OutputId;
		dictionary2["screen_mode"] = CurrentScreenMode.ToString();
		dictionary2["fullscreen"] = CurrentScreenMode == ScreenMode.fullscreen;
		dictionary2["screen_resolution"] = new List<object>
		{
			CurrentOutput.CurrentResolution.Width,
			CurrentOutput.CurrentResolution.Height
		};
		dictionary2["last_fullscreen_resolution"] = new List<object>
		{
			CurrentOutput.CurrentResolution.Width,
			CurrentOutput.CurrentResolution.Height
		};
		dictionary2["vsync"] = _vsync;
		dictionary2["language_id"] = CurrentLanguage.Extension;
		dictionary2["max_worker_threads"] = (int)CurrentWorkerThreads;
		dictionary["gamma"] = 0;
		dictionary2["master_render_settings"] = dictionary;
		_settings["detected_user_settings"] = dictionary2;
	}

	public void SaveSettings()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (_settings.ContainsKey("launcher_overrides"))
		{
			dictionary = _settings["launcher_overrides"] as Dictionary<string, object>;
		}
		else
		{
			_settings["launcher_overrides"] = dictionary;
		}
		dictionary["gpu_crash_dumps"] = GPUCrashDumpsOverride;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (_settings.ContainsKey("network_settings"))
		{
			dictionary2 = _settings["network_settings"] as Dictionary<string, object>;
		}
		else
		{
			_settings["network_settings"] = dictionary2;
		}
		dictionary2["try_nat64"] = IPv6NetworkEnabled;
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		if (_settings.ContainsKey("master_render_settings"))
		{
			dictionary3 = _settings["master_render_settings"] as Dictionary<string, object>;
		}
		else
		{
			_settings["master_render_settings"] = dictionary3;
		}
		dictionary3["graphics_quality"] = ((CurrentQualityType == QualityType.None) ? DefaultQualityType.ToString() : CurrentQualityType.ToString());
		dictionary3["fsr"] = (int)CurrentFSRMode;
		dictionary3["fsr2"] = (int)CurrentFSR2Mode;
		dictionary3["ffx_frame_gen"] = (int)CurrentFFXFGMode;
		dictionary3["xess"] = (int)CurrentXeSSMode;
		dictionary3["dlss_master"] = CurrentDLSSMode.ToString();
		dictionary3["dlss"] = (int)CurrentSuperResolutionMode;
		dictionary3["dlss_g"] = (int)CurrentFrameGenerationMode;
		dictionary3["nv_reflex_low_latency"] = (int)CurrentReflexMode;
		RaytracingQualitySettings.SetRaytracingSettingOption(dictionary3, CurrentRayTracingMode);
		dictionary3["anti_aliasing_solution"] = (int)CurrentAntiAliasingMode;
		dictionary3["nv_reflex_framerate_cap"] = _framerateCap;
		try
		{
			_settings["adapter_index"] = CurrentOutput.AdapterId;
			_settings["fullscreen_output"] = CurrentOutput.OutputId;
		}
		catch (Exception)
		{
		}
		_settings["gamma"] = _gamma;
		_settings["screen_mode"] = CurrentScreenMode.ToString();
		_settings["fullscreen"] = CurrentScreenMode == ScreenMode.fullscreen;
		_settings["screen_resolution"] = new List<object>
		{
			CurrentOutput.CurrentResolution.Width,
			CurrentOutput.CurrentResolution.Height
		};
		_settings["vsync"] = _vsync;
		_settings["language_id"] = CurrentLanguage.Extension;
		_settings["max_worker_threads"] = (int)CurrentWorkerThreads;
		_settings["gpu_id"] = CurrentOutput.AdapterName;
		_settings["launcher_verification_passed"] = FileVerificationPassed;
		Settings.Default.Save();
		writeToSettingsFile(_settings, settingsFilePath());
	}

	public string GetSettingsFileLogString()
	{
		return SJSON.Encode(_settings).Replace("\n", "\r\n");
	}

	protected void OnPropertyChanged(string prop)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
