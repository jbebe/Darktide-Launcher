using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using CrashReporter;
using Launcher.Properties;
using LauncherHelper;
using Microsoft.Win32;
using ResourceDictionary;
using ResourceDictionary.Properties;
using Steamworks;

namespace Launcher;

public partial class UI : System.Windows.Controls.UserControl, INotifyPropertyChanged, IDisposable, IComponentConnector
{
	private Window _ownerWindow;

	private ArgumentHolder arguments;

	private bool _steamInitialized;

	public static readonly DependencyProperty ShowPreorderProperty = DependencyProperty.Register("ShowPreorder", typeof(Visibility), typeof(MainWindow), new PropertyMetadata(Visibility.Visible));

	private bool _settingsEnabled;

	private bool _usingSteam;

	protected LauncherSettings _launcher_settings;

	protected GameSettingsHolder _game_settings_holder;

	protected GameSettings _game_settings_dialog;

	protected CrashMonitor _crash_handler;

	protected string _bundle_path;

	protected string _base_path;

	protected List<GameExecutable> _present_executables;

	protected List<string> _game_directories;

	protected GameExecutable _active_executable;

	private Dictionary<string, string> _argsList;

	private SysInfo _system_info;

	protected VersionInfo _local_version;

	protected bool able_to_log;

	protected string _log_exception_msg;

	private Dictionary<string, bool> _launch_disable_reasons = new Dictionary<string, bool>();

	private Brush _ScrollerOpacityMask;

	private long _peak_working_set;

	private long _peak_paged_mem;

	private long _peak_virtual_mem;

	private System.Timers.Timer _storeMemoryTimer;

	private Process _game_process;

	private bool _hasWebpageLoaded;

	private LocalizationManager.LanguageFormat _language;

	private string _content_revision;

	private double _triggered_share;

	private bool Disabled { get; set; }

	public Visibility ShowPreorder
	{
		get
		{
			return (Visibility)GetValue(ShowPreorderProperty);
		}
		set
		{
			SetValue(ShowPreorderProperty, value);
		}
	}

	public bool SettingsEnabled
	{
		get
		{
			return _settingsEnabled;
		}
		set
		{
			_settingsEnabled = value;
			OnPropertyChanged("SettingsEnabled");
		}
	}

	public bool UsingSteam
	{
		get
		{
			return _usingSteam;
		}
		set
		{
			_usingSteam = value;
			OnPropertyChanged("UsingSteam");
		}
	}

	public double TriggeredShare
	{
		get
		{
			return _triggered_share;
		}
		set
		{
			_triggered_share = value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void SetVersionInfo()
	{
		_content_revision = _crash_handler.ContentRevision;
		string contentRevision = "Data revision: " + _crash_handler.ContentRevision;
		string exeDescription = "Executable: " + _active_executable.VersionString().Replace("_", "__");
		string launcherVersion = "Launcher: " + typeof(MainWindow).Assembly.GetName().Version;
		(_ownerWindow as MainWindow).SetVersionInfo(contentRevision, exeDescription, launcherVersion);
	}

	protected void GetParentProcess()
	{
		try
		{
			int id = Process.GetCurrentProcess().Id;
			string queryString = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {id}";
			ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\CIMV2", queryString).Get().GetEnumerator();
			enumerator.MoveNext();
			uint processId = (uint)enumerator.Current["ParentProcessId"];
			Process processById = Process.GetProcessById((int)processId);
			FileLogger.Instance.CreateEntry("Launcher was started by: " + processById.ProcessName + "(" + processId + ")");
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error when checking parent process: " + ex.ToString());
		}
	}

	public void CloseAndReopenLog()
	{
		FileLogger.Instance.Close();
		OpenLog();
	}

	public void OpenLog()
	{
		string path = Directories.ProjectDirectory(Settings.Default.Project);
		FileLogger.Instance.OpenLog(path, "Launcher");
	}

	private void SetButtonState(System.Windows.Controls.Button button, bool allowed = true)
	{
		if (allowed)
		{
			button.Opacity = 1.0;
			SettingsEnabled = true;
		}
		else
		{
			button.Opacity = 0.5;
			SettingsEnabled = false;
		}
	}

	public void SelectExecutable()
	{
		if (_present_executables.Count == 0)
		{
			Disabled = true;
			return;
		}
		_active_executable = _present_executables[0];
		FileLogger.Instance.CreateEntry($"Setting active executable to: {_active_executable.Name} in directory: {_active_executable.Path}");
	}

	private void EnableUI()
	{
		FileLogger.Instance.CreateEntry("UI Enabled");
		UIPanel.IsEnabled = true;
	}

	public UI(Window ownerWindow)
	{
		_ownerWindow = ownerWindow;
		ActionManager.RegisterAction("EnableWindow", EnableUI);
		ActionManager.RegisterAction("OnWebpageLoaded", OnWebpageLoaded);
		ActionManager.RegisterAction("OnLauncherReady", OnLauncherReady);
		ActionManager.RegisterAction("OnLauncherActivated", OnLauncherActivated);
		ActionManager.RegisterAction("OnDispose", Dispose);
		ActionManager.RegisterAction("BlurryBackground", BlurryBackground);
		ActionManager.RegisterAction("UnBlurryBackground", UnBlurryBackground);
		ActionManager.RegisterAction("OnLauncherContentReady", OnLauncherContentReady);
		OpenLog();
		string text = "Launcher: " + typeof(MainWindow).Assembly.GetName().Version;
		FileLogger.Instance.CreateEntry("Launcher version: " + text);
		FileLogger.Instance.CreateEntry("Local time: " + DateTime.Now.ToString());
		TriggeredShare = 1.0;
		UsingSteam = Settings.Default.ReleasePlatform == ReleasePlatform.steam.ToString();
		if (UsingSteam)
		{
			uint.TryParse(Settings.Default.AppId, out var result);
			if (!SteamHelper.InitializeSteam(result, out _steamInitialized))
			{
				InitializeComponent();
				return;
			}
		}
		_ownerWindow.Loaded += OnWindowLoaded;
		_ownerWindow.ContentRendered += OnWindowContentRendered;
		using (new FileLogger.ScopeHolder("Main Window"))
		{
			try
			{
				_base_path = Directories.GetBaseDirectoryFromCurrentAppDirectory();
				_bundle_path = Path.Combine(_base_path, "bundle");
				FileLogger.Instance.CreateEntry("Base Directory: " + _base_path);
				arguments = new ArgumentHolder(_base_path, _steamInitialized, Settings.Default.ReleasePlatform);
				FileLogger.Instance.CreateEntry("OS Version: " + RuntimeInformation.OSDescription);
				FileLogger.Instance.CreateEntry("OS Architecture: " + RuntimeInformation.OSArchitecture);
				FileLogger.Instance.CreateEntry("Framework Version: " + RuntimeInformation.FrameworkDescription);
				GetParentProcess();
				_present_executables = new List<GameExecutable>();
				_game_directories = new List<string>();
				_launcher_settings = new LauncherSettings();
				if (!_launcher_settings.Load())
				{
					throw new Exception("skip_dialogue");
				}
				using (new FileLogger.ScopeHolder("SysInfo"))
				{
					_system_info = new SysInfo(_launcher_settings.DebugLog);
					_system_info.fetch_outputs();
					_system_info.fetch_cpu_info();
					_system_info.fetch_memory_info();
					FileLogger.Instance.CreateMultiLineEntry(_system_info.ToString());
				}
				string languageName = CultureInfo.CurrentCulture.Name;
				if (LocalizationManager.UsePlatformLanguage)
				{
					if (UsingSteam && _steamInitialized)
					{
						languageName = SteamApps.GetCurrentGameLanguage();
					}
					else if (Settings.Default.ReleasePlatform == ReleasePlatform.ms_store.ToString())
					{
						languageName = CultureInfo.CurrentCulture.Name;
					}
				}
				LocalizationManager.LanguageFormat languageFormat = (_language = LocalizationManager.GetLanguageFromNameOrDefault(languageName));
				CultureInfo cultureInfo3 = (CultureInfo.DefaultThreadCurrentUICulture = (CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(languageFormat.Localization)));
				Thread.CurrentThread.CurrentUICulture = cultureInfo3;
				Thread.CurrentThread.CurrentCulture = cultureInfo3;
				base.FontFamily = FontManager.SetFont(languageFormat.Id);
				InitializeComponent();
				TestWindowsDebug();
				PlayButtonPanel.Visibility = Visibility.Hidden;
				ButtonClose.Visibility = Visibility.Hidden;
				UploadPreviouslyFailedCrashReport(Directories.ProjectDirectory(Settings.Default.Project));
				UIPanel.IsEnabled = false;
				FetchLocalVersionInfo();
				CheckForExes(Settings.Default.ExeName);
				SelectExecutable();
				if (!Disabled)
				{
					_crash_handler = new CrashMonitor(Settings.Default.Project, FileLogger.Instance.FilePath, _base_path);
					_crash_handler.ReadSettings(arguments.CommonSettingsIniPath, arguments.Win32SettingsIniPath);
					_crash_handler.CleanupOldLogs();
					_crash_handler.SetupWatchers();
					ButtonSettings.Click += OnSettingsButtonClick;
					ButtonVerifyFiles.Click += OnVerifyFilesButtonClick;
					PlayButton.Click += OnLaunchButtonClick;
					if (Directory.Exists(Path.Combine(_base_path, "dev")))
					{
						ActionManager.ExecuteAction("EnableDeveloperGrid");
						ActionManager.RegisterAction("ExeSwitched", OnSwitchExe);
					}
					SetVersionInfo();
					base.DataContext = this;
					SetButtonState(ButtonSettings);
					SetButtonState(ButtonVerifyFiles);
				}
				else
				{
					FileLogger.Instance.CreateEntry("Launch game disabled, missing exe");
					SetButtonState(ButtonSettings, allowed: false);
					SetButtonState(ButtonVerifyFiles, allowed: false);
					SetLaunchDisableReason(disable: true, "Can't start: Missing executables");
				}
			}
			catch (Exception ex)
			{
				FileLogger.Instance.CreateEntry("MainWindow invocation error: " + ex.ToString());
				if (ex.Message.ToString() != "skip_dialogue")
				{
					new DebugDialog($"MainWindow invocation error: {ex.ToString()}").ShowDialog();
				}
				System.Windows.Application.Current.Shutdown();
			}
			CheckGraphicsCardSoftware();
			UpdatePlayButtonContent(clear: true);
		}
	}

	private void TestWindowsDebug()
	{
	}

	~UI()
	{
		_launcher_settings?.Save();
	}

	protected string GetProperty(string key)
	{
		string text = Environment.GetEnvironmentVariable(key);
		if (text == null)
		{
			SettingsProperty settingsProperty = Settings.Default.Properties[key];
			if (settingsProperty != null)
			{
				text = settingsProperty.DefaultValue.ToString();
			}
		}
		return text;
	}

	private void FetchLocalVersionInfo()
	{
		string text = "";
		try
		{
			_local_version = new VersionInfo();
			text = Path.Combine(_base_path, "versioninfo");
			if (!Directory.Exists(text))
			{
				return;
			}
			string path = Path.Combine(text, "version.json");
			if (File.Exists(path))
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					dynamic val = Json.Decode(streamReader.ReadToEnd());
					dynamic val2 = val.version;
					string version = Convert.ToString(val2);
					_local_version.SetVersion(version);
					FileLogger.Instance.CreateEntry("Local version is: " + _local_version.ToString());
					return;
				}
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Could not read local version info from: " + text + " Error message: " + ex.Message);
		}
	}

	private void checkDirTree(string path)
	{
		if (Directory.Exists(path))
		{
			_game_directories.Add(path);
			string[] directories = Directory.GetDirectories(path);
			foreach (string path2 in directories)
			{
				checkDirTree(path2);
			}
		}
	}

	private int CheckForExes(string exe_list)
	{
		using (new FileLogger.ScopeHolder("Check Exe"))
		{
			char[] separator = new char[1] { ' ' };
			string[] array = exe_list.Split(separator);
			string[] array2 = new string[2]
			{
				Path.Combine(_base_path, "dev"),
				Path.Combine(_base_path, "binaries")
			};
			string[] array3 = array;
			foreach (string text in array3)
			{
				string[] array4 = array2;
				foreach (string path in array4)
				{
					GameExecutable gameExecutable = new GameExecutable
					{
						Name = text
					};
					string path2 = Path.Combine(path, text);
					if (File.Exists(path2))
					{
						gameExecutable.Path = path2;
						gameExecutable.CheckForEACConfig(_base_path);
						_present_executables.Add(gameExecutable);
					}
				}
			}
			return _present_executables.Count;
		}
	}

	private void loadLauncherPlacement()
	{
		_ownerWindow.Top = _launcher_settings.WindowTopPos;
		_ownerWindow.Left = _launcher_settings.WindowLeftPos;
		if (!ScreenHandler.IsInsideCurrentScreen(_ownerWindow))
		{
			ScreenHandler.DoCenterPrimaryScreen(_ownerWindow);
		}
		saveLauncherPlacement();
	}

	private void saveLauncherPlacement()
	{
		_launcher_settings.WindowTopPos = _ownerWindow.Top;
		_launcher_settings.WindowLeftPos = _ownerWindow.Left;
		Screen currentScreen = ScreenHandler.GetCurrentScreen(_ownerWindow);
		_launcher_settings.LastScreenIndex = ScreenHandler.GetScreenIdx(currentScreen);
	}

	private string makeRelativePath(string workingDirectory, string fullPath)
	{
		string text = string.Empty;
		if (fullPath.StartsWith(workingDirectory))
		{
			return fullPath.Substring(workingDirectory.Length + 1);
		}
		string[] array = workingDirectory.Split(':', '\\', '/');
		string[] array2 = fullPath.Split(':', '\\', '/');
		if (array.Length == 0 || array2.Length == 0 || array[0] != array2[0])
		{
			return fullPath;
		}
		int i;
		for (i = 1; i < array.Length && !(array[i] != array2[i]); i++)
		{
		}
		for (int j = 0; j < array.Length - i; j++)
		{
			text += "..\\";
		}
		for (int k = i; k < array2.Length - 1; k++)
		{
			text = text + array2[i] + "\\";
		}
		return text + array2[array2.Length - 1];
	}

	private void launchBrowser(string url)
	{
		Process.Start(url);
	}

	protected bool SetLaunchDisableReason(bool disable, string reason)
	{
		_launch_disable_reasons[reason] = disable;
		bool flag = false;
		foreach (KeyValuePair<string, bool> launch_disable_reason in _launch_disable_reasons)
		{
			flag = flag || launch_disable_reason.Value;
		}
		PlayButton.IsEnabled = !flag;
		UpdateLaunchButtonTooltip(reason);
		return flag;
	}

	private void UpdateLaunchButtonTooltip(string disableReason)
	{
		TooltipStack.Children.Clear();
		if (!PlayButton.IsEnabled)
		{
			PlayButtonTooltip.Visibility = Visibility.Visible;
			{
				foreach (KeyValuePair<string, bool> launch_disable_reason in _launch_disable_reasons)
				{
					if (launch_disable_reason.Value)
					{
						TextBlock element = new TextBlock
						{
							Text = launch_disable_reason.Key,
							Foreground = new SolidColorBrush(new Color
							{
								R = 226,
								G = 226,
								B = 226,
								A = byte.MaxValue
							}),
							Background = new SolidColorBrush(new Color
							{
								R = 100,
								G = 100,
								B = 100,
								A = byte.MaxValue
							})
						};
						TooltipStack.Children.Add(element);
					}
				}
				return;
			}
		}
		PlayButtonTooltip.Visibility = Visibility.Hidden;
	}

	private void OnPropertyChanged(string info)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
	}

	private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
	{
		_game_settings_dialog = new GameSettings(_game_settings_holder);
		_game_settings_dialog.IsVisibleChanged += OnGameSettingsIsVisibleChanged;
		_game_settings_dialog.Owner = _ownerWindow;
		ScreenHandler.DoCenterTop(_game_settings_dialog, _ownerWindow);
		_game_settings_dialog.ShowDialog();
		_game_settings_dialog.Close();
	}

	private async void OnVerifyFilesButtonClick(object sender, RoutedEventArgs e)
	{
		_autorun_timer?.Stop();
		ConfirmationBox obj = new ConfirmationBox(ResourceDictionary.Properties.Resources.loc_run_verify_files_dialog, "", ResourceDictionary.Properties.Resources.loc_btn_yes, ResourceDictionary.Properties.Resources.loc_btn_no, DialogResult.Yes, DialogResult.No, 770.0, 560.0, 390.0, "/ResourceDictionary;component/assets/settings_window/settings_background.png")
		{
			Owner = _ownerWindow
		};
		BlurryBackground();
		obj.ShowDialog();
		if (obj.Result == DialogResult.Yes)
		{
			FileLogger.Instance.PushScope("FileVerification");
			FileLogger.Instance.CreateEntry("File verification force started by user.");
			VerifierResult verifierResult = await FileVerifier.VerifyFiles(_ownerWindow);
			_game_settings_holder.FileVerificationPassed = verifierResult == VerifierResult.SUCCESS;
			FileLogger.Instance.PopScope();
		}
		UnBlurryBackground();
	}

	private void OnSwitchExe()
	{
		int num = _present_executables.Count();
		int num2 = _present_executables.IndexOf(_active_executable);
		num2 = (num2 + 1) % num;
		_active_executable.Active = false;
		_active_executable = _present_executables[num2];
		SetVersionInfo();
	}

	private static UploadPolicy policyFromBitmask(uint errorCode)
	{
		return (errorCode & 0x30000) switch
		{
			65536u => UploadPolicy.Show, 
			131072u => UploadPolicy.Silent, 
			196608u => UploadPolicy.SilentLimited, 
			_ => UploadPolicy.None, 
		};
	}

	private async void OnLaunchButtonClick(object sender, RoutedEventArgs e)
	{
		using (new FileLogger.ScopeHolder("Launch Game"))
		{
			bool use_eac = SettingsHelper.UseEAC(_active_executable.Name);
			_launcher_settings.Save();
			_game_settings_holder.SaveSettings();
			arguments.SetBackend(_steamInitialized, Settings.Default.ReleasePlatform);
			string base_path = _base_path;
			Directory.SetCurrentDirectory(base_path);
			ProcessStartInfo sinfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = false,
				FileName = (use_eac ? Path.Combine(base_path, "start_protected_game.exe") : _active_executable.Path),
				WorkingDirectory = base_path,
				Arguments = arguments.CreateGameParameters(_game_settings_holder.ForceFlush, _game_settings_holder.WaitForDebugger, use_eac, _active_executable.EACConfig, _game_settings_holder.ResetKeyboardsBindings, _game_settings_holder.FileVerificationPassed),
				UseShellExecute = !_launcher_settings.SendCrashReports
			};
			FileLogger.Instance.CreateEntry("Launching Game");
			FileLogger.Instance.CreateEntry("Executable: " + sinfo.FileName);
			FileLogger.Instance.CreateEntry("Arguments: " + sinfo.Arguments);
			FileLogger.Instance.CreateMultiLineEntry("Game Settings At Startup: " + _game_settings_holder.GetSettingsFileLogString());
			if (_launcher_settings.SendCrashReports)
			{
				_ownerWindow.WindowState = WindowState.Minimized;
				_ownerWindow.ShowInTaskbar = false;
				_ownerWindow.Hide();
				await ShaderCacheBuilder.BuildShaderCacheAsync(_ownerWindow, _base_path, _game_settings_holder.CurrentOutput.AdapterId);
				try
				{
					uint num = 0u;
					FileLogger.Instance.CreateEntry("Using EAC: " + use_eac);
					using (Process process = Process.Start(sinfo))
					{
						ActionManager.ExecuteAction("OnPlayButtonPressed");
						_storeMemoryTimer = new System.Timers.Timer();
						_storeMemoryTimer.Interval = 2000.0;
						_storeMemoryTimer.Elapsed += StoreMemory_Timer_Elapsed;
						_storeMemoryTimer.Start();
						FileLogger.Instance.CreateEntry("Game starting...");
						process.WaitForExit();
						_storeMemoryTimer.Stop();
						num = (uint)process.ExitCode;
						using (new FileLogger.ScopeHolder("Process Exit"))
						{
							FileLogger.Instance.CreateEntry("Exit code: 0x" + num.ToString("X8"));
							FileLogger.Instance.CreateEntry("Start time: " + process.StartTime.ToUniversalTime().ToString());
							FileLogger.Instance.CreateEntry("Exit time: " + process.ExitTime.ToUniversalTime().ToString());
							FileLogger.Instance.CreateEntry("Peak physical memory: " + _peak_working_set);
							FileLogger.Instance.CreateEntry("Peak paged memory: " + _peak_paged_mem);
							FileLogger.Instance.CreateEntry("Peak virtual memory: " + _peak_virtual_mem);
						}
					}
					bool flag = true;
					bool showPlayer = true;
					if ((num & 0xFFF00000u) == 4201644032u)
					{
						switch (policyFromBitmask(num))
						{
						case UploadPolicy.Show:
							flag = true;
							showPlayer = true;
							break;
						case UploadPolicy.Silent:
							flag = true;
							showPlayer = false;
							break;
						case UploadPolicy.SilentLimited:
						{
							Random random = new Random();
							flag = TriggeredShare > random.NextDouble();
							showPlayer = false;
							break;
						}
						}
					}
					if (num == 3221225477u)
					{
						FileLogger.Instance.CreateEntry("EAC crash occurred");
						flag = true;
						showPlayer = false;
					}
					if (num != 0 && flag)
					{
						string text = Directories.ProjectDirectory(Settings.Default.Project);
						CreateCrashReport(text, showPlayer);
						FileLogger.Instance.CreateEntry("Starting CrashReporter.exe");
						string args = string.Format("{0} \"{1}\"", "-crash_report_path", text);
						FileLogger.Instance.Close();
						CrashCacheReport.StartProcess(args);
					}
					FileLogger.Instance.Close();
					System.Windows.Application.Current.Shutdown();
				}
				catch (Exception ex)
				{
					FileLogger.Instance.CreateEntry("Error starting process: " + ex.Message);
					CloseAndReopenLog();
				}
			}
			else
			{
				FileLogger.Instance.Close();
				Process.Start(sinfo);
				System.Windows.Application.Current.Shutdown();
			}
		}
	}

	private void StoreMemory_Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (_game_process == null || _game_process.HasExited)
		{
			Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_active_executable.Path));
			if (processesByName.Length == 0)
			{
				return;
			}
			_game_process = processesByName[0];
		}
		_game_process.Refresh();
		_peak_working_set = _game_process.PeakWorkingSet64;
		_peak_paged_mem = _game_process.PeakPagedMemorySize64;
		_peak_virtual_mem = _game_process.PeakVirtualMemorySize64;
	}

	private SolidColorBrush GetNeutralMask()
	{
		return new SolidColorBrush
		{
			Color = Colors.White
		};
	}

	private Brush GetOpacityMask(ScrollViewer scrollViewer)
	{
		if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
		{
			return GetNeutralMask();
		}
		return _ScrollerOpacityMask;
	}

	private void OnGameSettingsIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		base.IsEnabled = !_game_settings_dialog.IsVisible;
		if (_game_settings_dialog.IsVisible)
		{
			_autorun_timer?.Stop();
			BlurryBackground();
		}
		else
		{
			UpdatePlayButtonContent(clear: true);
			UnBlurryBackground();
		}
	}

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		loadLauncherPlacement();
	}

	private async void OnWindowContentRendered(object sender, EventArgs e)
	{
		_ownerWindow.Focus();
		ActionManager.ExecuteAction("OnLauncherContentReady");
	}

	private async void OnLauncherContentReady()
	{
		if (_game_settings_holder == null)
		{
			_game_settings_holder = new GameSettingsHolder(_ownerWindow, _system_info, _language);
			if (_game_settings_holder.AutoRun) StartAutorunTimer();
		}
		await _game_settings_holder.VerifyQualitySetting();
		if (FileVerificationConditionsMet())
		{
			FileLogger.Instance.PushScope("FileVerification");
			FileLogger.Instance.CreateEntry("File verification conditions met with content revision: " + _content_revision + ", last content revision: " + _launcher_settings.LastContentRevision + ".");
			ConfirmationBox obj = new ConfirmationBox(ResourceDictionary.Properties.Resources.loc_run_verify_files_dialog, "", ResourceDictionary.Properties.Resources.loc_btn_yes, ResourceDictionary.Properties.Resources.loc_btn_no, DialogResult.Yes, DialogResult.No, 770.0, 560.0, 390.0, "/ResourceDictionary;component/assets/settings_window/settings_background.png")
			{
				Owner = _ownerWindow
			};
			BlurryBackground();
			obj.ShowDialog();
			VerifierResult verifierResult = VerifierResult.FAIL;
			if (obj.Result == DialogResult.Yes)
			{
				FileLogger.Instance.CreateEntry("Suggested file verification ACCEPTED by user.");
				verifierResult = await FileVerifier.VerifyFiles(_ownerWindow);
			}
			else
			{
				FileLogger.Instance.CreateEntry("Suggested file verification DECLINED by user.");
			}
			_game_settings_holder.FileVerificationPassed = verifierResult == VerifierResult.SUCCESS;
			FileLogger.Instance.PopScope();
			UnBlurryBackground();
		}
		_launcher_settings.LastContentRevision = _content_revision;
	}

	private void CheckGraphicsCardSoftware()
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NVIDIA Corporation\\Global\\GFExperience", writable: false);
		if (registryKey == null)
		{
			return;
		}
		string text = (string)registryKey.GetValue("Version", "orphaned GFExperience key?");
		registryKey.Close();
		string[] array = text.Split('.');
		int[] array2 = new int[array.Length];
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			flag = flag && int.TryParse(array[i], out array2[i]);
		}
		if (flag && array.Length == 4)
		{
			if (array2[0] < 3 || (array2[0] == 3 && array2[1] < 11) || (array2[0] == 3 && array2[1] == 11 && array2[2] == 0 && array2[3] < 73))
			{
				FileLogger.Instance.CreateEntry("!! Outdated Geforce Experience version found: " + text);
				if (System.Windows.MessageBox.Show(ResourceDictionary.Properties.Resources.ResourceManager.GetString("LOC_OutdatedGFXP_Message"), ResourceDictionary.Properties.Resources.ResourceManager.GetString("LOC_OutdatedGFXP_Title"), MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
				{
					launchBrowser("https://www.geforce.com/geforce-experience/download");
					FileLogger.Instance.CreateEntry("Taking user to the NVIDIA site.");
					System.Windows.Application.Current.Shutdown();
				}
				else
				{
					FileLogger.Instance.CreateEntry("!! User declined to go to the NVIDIA site to download a new version.");
				}
			}
		}
		else
		{
			FileLogger.Instance.CreateEntry("?? Unknown Geforce Experience version found: " + text);
		}
	}

	private void ButtonClose_Click(object sender, RoutedEventArgs e)
	{
		_autorun_timer?.Stop();
		saveLauncherPlacement();
		_launcher_settings.Save();
		_game_settings_holder.SaveSettings();
		System.Windows.Application.Current.Shutdown();
		FileLogger.Instance.Close();
	}

	private void UploadPreviouslyFailedCrashReport(string crashReportPath)
	{
		string text = crashReportPath + "\\crash_report.zip";
		if (File.Exists(text))
		{
			new CrashUploadFailedWindow().ShowDialog();
			global::CrashReporter.CrashReporter.Execute(crashReportPath, text, deleteZipFile: true);
		}
	}

	private void CreateCrashReport(string crashReportPath, bool showPlayer)
	{
		FileLogger.Instance.CreateEntry("Creating crash report");
		_game_settings_holder.LoadSettings();
		FileLogger.Instance.CreateMultiLineEntry("Game Settings After Crash: " + _game_settings_holder.GetSettingsFileLogString());
		string text = crashReportPath + "\\crash_report_cache.cache";
		string text2 = crashReportPath + "\\crash_report.zip";
		CrashCacheReport crashCacheReport = new CrashCacheReport();
		crashCacheReport.Project = Settings.Default.Project;
		crashCacheReport.ShowPlayer = showPlayer;
		crashCacheReport.LaunchLog = _crash_handler.LaunchLog;
		crashCacheReport.ConsLog = _crash_handler.ConsLog;
		crashCacheReport.CrashDump = _crash_handler.CrashDump;
		crashCacheReport.GpuCrashDump = _crash_handler.GpuCrashDump;
		crashCacheReport.EarlyCrashDump = _crash_handler.EarlyCrashDump;
		crashCacheReport.LuaDumps = FilterLuaDumps(_crash_handler.LuaDumps);
		crashCacheReport.Language = CultureInfo.CurrentCulture.Name;
		crashCacheReport.Save(crashReportPath);
		try
		{
			ZipUtility.CreateZipFromFile(text, text2, "tq'zCFa&[DV4EHG8");
			FileLogger.Instance.CreateEntry("Crash report created in " + crashReportPath);
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error while creating zip " + text2 + " : " + ex.Message);
		}
		File.Delete(text);
	}

	private List<string> FilterLuaDumps(List<string> dumpFiles)
	{
		if (dumpFiles.Count <= 0)
		{
			return dumpFiles.ToList();
		}
		string text = "REGISTRY";
		string text2 = "GLOBAL";
		string text3 = dumpFiles[dumpFiles.Count() - 1];
		(string, FileInfo) tuple = (text3, new FileInfo(text3));
		foreach (string dumpFile in dumpFiles)
		{
			FileInfo fileInfo = new FileInfo(dumpFile);
			if (fileInfo.Length > tuple.Item2.Length)
			{
				tuple.Item1 = dumpFile;
				tuple.Item2 = fileInfo;
			}
		}
		string largestFileIdentifier = "";
		if (tuple.Item1.Contains(text))
		{
			largestFileIdentifier = text;
		}
		else if (tuple.Item1.Contains(text2))
		{
			largestFileIdentifier = text2;
		}
		else
		{
			FileLogger.Instance.CreateEntry("Error parsing lua dump files: The largest file found, '" + tuple.Item1 + "', didn't contain REGISTRY or GLOBAL identifier. Has lua dump files changed name?");
		}
		string text4 = dumpFiles.Find((string x) => x.Contains(largestFileIdentifier));
		(string, FileInfo) tuple2 = (text4, new FileInfo(text4));
		foreach (string dumpFile2 in dumpFiles)
		{
			FileInfo fileInfo2 = new FileInfo(dumpFile2);
			if (fileInfo2.Length < tuple2.Item2.Length && dumpFile2.Contains(largestFileIdentifier))
			{
				tuple2.Item1 = dumpFile2;
				tuple2.Item2 = fileInfo2;
			}
		}
		if (tuple.Item1 == tuple2.Item1)
		{
			return new List<string> { tuple.Item1 };
		}
		return new List<string> { tuple.Item1, tuple2.Item1 };
	}

	private void UI_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			_ownerWindow.DragMove();
			_launcher_settings.WindowTopPos = _ownerWindow.Top;
			_launcher_settings.WindowLeftPos = _ownerWindow.Left;
		}
	}

	public void OnWebpageLoaded()
	{
		Background.BeginAnimation(UIElement.OpacityProperty, Animations.Fade(1.0, 0.0, 1000.0, autoReverse: false));
		FileLogger.Instance.CreateEntry("OnWebpageLoaded completed");
	}

	public void OnLauncherReady()
	{
		if (_game_settings_holder == null)
		{
			_game_settings_holder = new GameSettingsHolder(_ownerWindow, _system_info, _language);
		}
		PlayButtonPanel.Visibility = Visibility.Visible;
		ButtonClose.Visibility = Visibility.Visible;
		PlayButtonPanel.BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 2000.0, autoReverse: false));
		ButtonClose.BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 1000.0, autoReverse: false));
	}

	public void OnLauncherActivated()
	{
		if (_game_settings_dialog != null && _game_settings_dialog.Visibility == Visibility.Visible)
		{
			_game_settings_dialog.Activate();
		}
	}

	public void BlurryBackground()
	{
		DisabledOverlay.Visibility = Visibility.Visible;
		BlurEffect blurEffect = new BlurEffect();
		blurEffect.Radius = 9.0;
		base.Effect = blurEffect;
	}

	public void UnBlurryBackground()
	{
		base.Effect = null;
		DisabledOverlay.Visibility = Visibility.Hidden;
	}

	public void Dispose()
	{
		FileLogger.Instance.Close();
	}

	private bool FileVerificationConditionsMet()
	{
		if (UsingSteam && !string.IsNullOrEmpty(_content_revision) && _launcher_settings != null && !string.IsNullOrEmpty(_launcher_settings.LastContentRevision))
		{
			return _launcher_settings.LastContentRevision != _content_revision;
		}
		return false;
	}

	private const int AUTORUN_TIME = 3;

	private DispatcherTimer _autorun_timer;

	private int _autorun_time_left = AUTORUN_TIME;

	public string _play_button_content { get; set; }

	public string PlayButtonContent
	{
		get
		{
			return _play_button_content;
		}
		set
		{
			_play_button_content = value;
			OnPropertyChanged("PlayButtonContent");
		}
	}

	public void UpdatePlayButtonContent(bool clear = false)
	{
		var buttonText = ResourceDictionary.Properties.Resources.LOC_BTN_LAUNCH;
		PlayButtonContent = clear ? buttonText : $"{buttonText} ({_autorun_time_left})";
	}

	private void StartAutorunTimer()
	{
		UpdatePlayButtonContent();
		_autorun_time_left = AUTORUN_TIME;
		_autorun_timer = new DispatcherTimer();
		_autorun_timer.Interval = TimeSpan.FromSeconds(1);
		_autorun_timer.Tick += AutoRunTimer_Tick;
		_autorun_timer.Start();
	}

	private void AutoRunTimer_Tick(object sender, EventArgs e)
	{
		_autorun_time_left -= 1;
		if (_autorun_time_left == 0)
		{
			_autorun_timer.Stop();
			PlayButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
			return;
		}

		UpdatePlayButtonContent();
	}
}
