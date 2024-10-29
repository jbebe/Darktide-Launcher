using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using LauncherHelper;
using ResourceDictionary;
using ResourceDictionary.Properties;

namespace Launcher;

public partial class MainWindow : Window, IComponentConnector
{
	private bool _developerGridDebugLanguageEnabled;

	public static readonly DependencyProperty ContentRevisionProperty = DependencyProperty.Register("ContentRevision", typeof(string), typeof(MainWindow), new PropertyMetadata("Data revision"));

	public static readonly DependencyProperty ExecutableDescriptionProperty = DependencyProperty.Register("ExecutableDescription", typeof(string), typeof(MainWindow), new PropertyMetadata("Executable description"));

	public static readonly DependencyProperty LauncherVersionProperty = DependencyProperty.Register("LauncherVersion", typeof(string), typeof(MainWindow), new PropertyMetadata("Launcher version"));

	public static readonly DependencyProperty CurrentDebugLanguageProperty = DependencyProperty.Register("CurrentDebugLanguage", typeof(LocalizationManager.Language_id), typeof(MainWindow), new PropertyMetadata(LocalizationManager.Language_id.english));

	public static readonly DependencyProperty DebugLanguagesProperty = DependencyProperty.Register("DebugLanguages", typeof(CollectionView), typeof(MainWindow), new PropertyMetadata(null));

	public string ContentRevision
	{
		get
		{
			return (string)GetValue(ContentRevisionProperty);
		}
		set
		{
			SetValue(ContentRevisionProperty, value);
		}
	}

	public string ExecutableDescription
	{
		get
		{
			return (string)GetValue(ExecutableDescriptionProperty);
		}
		set
		{
			SetValue(ExecutableDescriptionProperty, value);
		}
	}

	public string LauncherVersion
	{
		get
		{
			return (string)GetValue(LauncherVersionProperty);
		}
		set
		{
			SetValue(LauncherVersionProperty, value);
		}
	}

	public LocalizationManager.Language_id CurrentDebugLanguage
	{
		get
		{
			return (LocalizationManager.Language_id)GetValue(CurrentDebugLanguageProperty);
		}
		set
		{
			SetValue(CurrentDebugLanguageProperty, value);
		}
	}

	public CollectionView DebugLanguages
	{
		get
		{
			return (CollectionView)GetValue(DebugLanguagesProperty);
		}
		set
		{
			SetValue(DebugLanguagesProperty, value);
		}
	}

	public MainWindow()
	{
		try
		{
			InitializeComponent();
			base.DataContext = this;
			bool flag = !SysInfo.IsWindowsVersionSupported();
			if (!SysInfo.IsWindowsMachine() || flag)
			{
				MainWindowHost.Children.Add(new FallbackUI(this));
				if (flag)
				{
					MessageBox.Show(ResourceDictionary.Properties.Resources.loc_operating_system_not_supported, ResourceDictionary.Properties.Resources.loc_warning);
				}
				SetupCustomCursor();
				return;
			}
			DeveloperGrid.Visibility = Visibility.Collapsed;
			MainWindowHost.IsEnabled = false;
			RegisterActions();
			MainWindowHost.Children.Add(new AirControl
			{
				Back = new WebpageControlHost(),
				Front = new UI(this)
			});
			if (FontManager.CurrentFont != null)
			{
				base.FontFamily = FontManager.CurrentFont;
			}
			SetupCustomCursor();
			SetupDebugLanguages();
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString(), "EXCEPTION OCCURRED!");
			throw;
		}
	}

	private void RegisterActions()
	{
		ActionManager.RegisterAction("EnableWindow", EnableMainWindowPanel);
		ActionManager.RegisterAction("EnableDeveloperGrid", EnableDeveloperGrid);
		ActionManager.RegisterAction("OnWebPageInitializationFailed", OnWebpageInitializationFailed);
	}

	private void SetupDebugLanguages()
	{
		try
		{
			LocalizationManager.LanguageFormat languageFromNameOrDefault = LocalizationManager.GetLanguageFromNameOrDefault(CultureInfo.CurrentCulture.Name);
			Dictionary<LocalizationManager.Language_id, string> collection = LocalizationManager.LanguageFormatMapping.Select((LocalizationManager.LanguageFormat x) => x).ToDictionary((LocalizationManager.LanguageFormat x) => x.Id, (LocalizationManager.LanguageFormat y) => y.Id.ToString());
			DebugLanguages = new CollectionView(collection);
			CurrentDebugLanguage = languageFromNameOrDefault.Id;
			FileLogger.Instance.CreateEntry($"Current language: {languageFromNameOrDefault.Id}");
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error while setting up debug languages: " + ex.Message);
		}
	}

	private void SetupCustomCursor()
	{
		try
		{
			Mouse.OverrideCursor = new Cursor(GetCursorIconStream(new Uri("pack://application:,,,/ResourceDictionary;component//assets/common/cursor_launcher.cur"), 9, 9));
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error while setting up custom cursor: " + ex.Message);
		}
	}

	public static Stream GetCursorIconStream(Uri uri, byte hotspotx, byte hotspoty)
	{
		Stream stream = Application.GetResourceStream(uri).Stream;
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, (int)stream.Length);
		MemoryStream memoryStream = new MemoryStream();
		array[10] = hotspotx;
		array[12] = hotspoty;
		memoryStream.Write(array, 0, (int)stream.Length);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private void OnWindowClosed(object sender, EventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void SwitchExeButton_OnClick(object sender, RoutedEventArgs e)
	{
		ActionManager.ExecuteAction("ExeSwitched");
	}

	public void SetVersionInfo(string contentRevision, string exeDescription, string launcherVersion)
	{
		ContentRevision = contentRevision;
		ExecutableDescription = exeDescription;
		LauncherVersion = launcherVersion;
	}

	private void EnableMainWindowPanel()
	{
		MainWindowHost.IsEnabled = true;
		FileLogger.Instance.CreateEntry("Main window enabled");
	}

	private void EnableDeveloperGrid()
	{
		DeveloperGrid.Visibility = Visibility.Visible;
	}

	private void OnWebpageInitializationFailed()
	{
		ActionManager.ExecuteAction("OnLauncherReady");
		FileLogger.Instance.CreateEntry("Executing OnWebpageInitializationFailed");
	}

	private void DeveloperGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			DragMove();
		}
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (DebugLanguageComboBox.IsLoaded && _developerGridDebugLanguageEnabled)
		{
			CultureInfo cultureInfo3 = (CultureInfo.DefaultThreadCurrentUICulture = (CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(LocalizationManager.GetLanguageFromNameOrDefault(((KeyValuePair<LocalizationManager.Language_id, string>)DebugLanguageComboBox.SelectedItem).Value).Localization)));
			Thread.CurrentThread.CurrentUICulture = cultureInfo3;
			Thread.CurrentThread.CurrentCulture = cultureInfo3;
			LocalizationManager.UsePlatformLanguage = false;
			ActionManager.ExecuteAction("OnDispose");
			ActionManager.Reset();
			RegisterActions();
			AirControl element = MainWindowHost.Children[0] as AirControl;
			MainWindowHost.Children.Remove(element);
			MainWindowHost.Children.Add(new AirControl
			{
				Back = new WebpageControlHost(),
				Front = new UI(this)
			});
			ActionManager.ExecuteAction("OnLauncherContentReady");
		}
	}

	private void DebugLanguageComboBox_OnMouseEnter(object sender, MouseEventArgs e)
	{
		_developerGridDebugLanguageEnabled = true;
	}

	private void Window_Activated(object sender, EventArgs e)
	{
		ActionManager.ExecuteAction("OnLauncherActivated");
	}
}
