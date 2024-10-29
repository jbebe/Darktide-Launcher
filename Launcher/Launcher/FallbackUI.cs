using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Launcher.Properties;
using LauncherHelper;
using ResourceDictionary;
using Steamworks;

namespace Launcher;

public partial class FallbackUI : UserControl, IComponentConnector
{
	private Window _ownerWindow;

	private bool _steamInitialized;

	private string _base_path;

	private ArgumentHolder _arguments;

	private List<GameExecutable> _present_executables;

	private List<string> _game_directories;

	private GameExecutable _active_executable;

	private SysInfo _system_info;

	private bool _disabled;

	public FallbackUI(Window ownerWindow)
	{
		_ownerWindow = ownerWindow;
		OpenLog();
		Setup();
	}

	private void Setup()
	{
		try
		{
			FileLogger.Instance.CreateEntry("Local time: " + DateTime.Now.ToString());
			FileLogger.Instance.CreateEntry("Running machine different than Windows 10");
			bool flag = Settings.Default.ReleasePlatform == ReleasePlatform.steam.ToString();
			if (flag)
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
				_base_path = Directories.GetBaseDirectoryFromCurrentAppDirectory();
				FileLogger.Instance.CreateEntry("Base Directory: " + _base_path);
				FileLogger.Instance.CreateEntry("OS Version: " + RuntimeInformation.OSDescription);
				FileLogger.Instance.CreateEntry("OS Architecture: " + RuntimeInformation.OSArchitecture);
				FileLogger.Instance.CreateEntry("Framework Version: " + RuntimeInformation.FrameworkDescription);
				_arguments = new ArgumentHolder(_base_path, _steamInitialized, Settings.Default.ReleasePlatform);
				_present_executables = new List<GameExecutable>();
				_game_directories = new List<string>();
				using (new FileLogger.ScopeHolder("SysInfo"))
				{
					_system_info = new SysInfo(debugLog: false);
					_system_info.fetch_outputs();
					_system_info.fetch_cpu_info();
					_system_info.fetch_memory_info();
					FileLogger.Instance.CreateMultiLineEntry(_system_info.ToString());
				}
				string languageName = CultureInfo.CurrentCulture.Name;
				if (LocalizationManager.UsePlatformLanguage)
				{
					if (flag && _steamInitialized)
					{
						languageName = SteamApps.GetCurrentGameLanguage();
					}
					else if (Settings.Default.ReleasePlatform == ReleasePlatform.ms_store.ToString())
					{
						languageName = CultureInfo.CurrentCulture.Name;
					}
				}
				LocalizationManager.LanguageFormat languageFromNameOrDefault = LocalizationManager.GetLanguageFromNameOrDefault(languageName);
				CultureInfo cultureInfo3 = (CultureInfo.DefaultThreadCurrentUICulture = (CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(languageFromNameOrDefault.Localization)));
				Thread.CurrentThread.CurrentUICulture = cultureInfo3;
				Thread.CurrentThread.CurrentCulture = cultureInfo3;
				base.FontFamily = FontManager.SetFont(languageFromNameOrDefault.Id);
				InitializeComponent();
				CheckForExes(Settings.Default.ExeName);
				SelectExecutable();
				if (!_disabled)
				{
					new GameSettingsHolder(_ownerWindow, _system_info, languageFromNameOrDefault);
					PlayButton.Click += OnLaunchButtonClick;
					if (Directory.Exists(Path.Combine(_base_path, "dev")))
					{
						ActionManager.RegisterAction("ExeSwitched", OnSwitchExe);
					}
					SetVersionInfo();
					base.DataContext = this;
				}
				else
				{
					FileLogger.Instance.CreateEntry("Launch game disabled, missing exe");
					PlayButton.IsEnabled = false;
				}
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry($"MainWindow invocation error: {ex}");
			MessageBox.Show(ex.Message ?? "", "Error");
			Application.Current.Shutdown();
		}
	}

	protected void SetVersionInfo()
	{
		string exeDescription = "Executable: " + _active_executable.VersionString().Replace("_", "__");
		string text = "Launcher: " + typeof(MainWindow).Assembly.GetName().Version;
		FileLogger.Instance.CreateEntry("Launcher version: " + text);
		(_ownerWindow as MainWindow).SetVersionInfo("", exeDescription, text);
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

	public void SelectExecutable()
	{
		if (_present_executables.Count == 0)
		{
			_disabled = true;
			return;
		}
		_active_executable = _present_executables[0];
		FileLogger.Instance.CreateEntry($"Setting active executable to: {_active_executable.Name} in directory: {_active_executable.Path}");
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

	public void OpenLog()
	{
		string path = Directories.ProjectDirectory(Settings.Default.Project);
		FileLogger.Instance.OpenLog(path, "Launcher");
	}

	private void LoadLauncherPlacement()
	{
		if (!ScreenHandler.IsInsideCurrentScreen(_ownerWindow))
		{
			ScreenHandler.DoCenterPrimaryScreen(_ownerWindow);
		}
	}

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		LoadLauncherPlacement();
	}

	private void OnWindowContentRendered(object sender, EventArgs e)
	{
		_ownerWindow.Focus();
	}

	private void UI_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			_ownerWindow.DragMove();
		}
	}

	private void ButtonClose_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
		FileLogger.Instance.Close();
	}

	private async void OnLaunchButtonClick(object sender, RoutedEventArgs e)
	{
		using (new FileLogger.ScopeHolder("Launch Game"))
		{
			bool use_eac = SettingsHelper.UseEAC(_active_executable.Name);
			_arguments.SetBackend(_steamInitialized, Settings.Default.ReleasePlatform);
			string base_path = _base_path;
			Directory.SetCurrentDirectory(base_path);
			ProcessStartInfo sinfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = false,
				FileName = (use_eac ? Path.Combine(base_path, "start_protected_game.exe") : _active_executable.Path),
				WorkingDirectory = base_path,
				Arguments = _arguments.CreateGameParameters(forceFlush: false, waitForDebugger: false, use_eac, _active_executable.EACConfig, resetKeyBindings: false, fileVerificationSucces: false),
				UseShellExecute = true
			};
			FileLogger.Instance.CreateEntry("Launching Game");
			FileLogger.Instance.CreateEntry("Executable: " + sinfo.FileName);
			FileLogger.Instance.CreateEntry("Arguments: " + sinfo.Arguments);
			_ownerWindow.WindowState = WindowState.Minimized;
			_ownerWindow.ShowInTaskbar = false;
			_ownerWindow.Hide();
			await ShaderCacheBuilder.BuildShaderCacheAsync(_ownerWindow, _base_path, _system_info._primary_output.AdapterId);
			FileLogger.Instance.CreateEntry("Using EAC: " + use_eac);
			using Process process = Process.Start(sinfo);
			try
			{
				process.WaitForExit();
				uint exitCode = (uint)process.ExitCode;
				using (new FileLogger.ScopeHolder("Process Exit"))
				{
					FileLogger.Instance.CreateEntry("Exit code: 0x" + exitCode.ToString("X8"));
					FileLogger.Instance.CreateEntry("Start time: " + process.StartTime.ToUniversalTime().ToString());
					FileLogger.Instance.CreateEntry("Exit time: " + process.ExitTime.ToUniversalTime().ToString());
				}
			}
			catch (Exception arg)
			{
				FileLogger.Instance.CreateEntry($"Error starting process: {arg}");
			}
			FileLogger.Instance.Close();
			Application.Current.Shutdown();
		}
	}
}
