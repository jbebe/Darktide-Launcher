using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Launcher.Properties;
using LauncherHelper;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ResourceDictionary;

namespace Launcher;

public class WebpageControl : UserControl
{
	private enum WebView2VersionResult
	{
		Ok,
		NotFound,
		Outdated,
		Other
	}

	private Timer _timeoutTimer = new Timer();

	private bool _navigationCompleted;

	private IContainer components;

	private WebView2 webpageHost;

	public WebpageControl()
	{
		InitializeComponent();
		ActionManager.RegisterAction("OnPlayButtonPressed", Dispose);
		ActionManager.RegisterAction("OnDispose", Dispose);
		base.Resize += Form_Resize;
	}

	private async void Loaded(object sender, EventArgs e)
	{
		try
		{
			if (await AssureWebView2RuntimeInstalledAndInitialize())
			{
				string text = "-" + Settings.Default.ReleasePlatform.Replace("_", "-");
				string text2 = ((!string.IsNullOrEmpty(Settings.Default.Release)) ? ("-" + Settings.Default.Release) : "");
				string languageIdentifierForWebpage = LocalizationManager.GetLanguageIdentifierForWebpage(CultureInfo.CurrentCulture.Name);
				webpageHost.Source = new Uri(Settings.Default.MarketingURL + languageIdentifierForWebpage + text + text2 + "-d56f3915-759b-4fb5-8425-e6ee9bafd2a3");
				StartTimeoutTimer();
			}
			else
			{
				FileLogger.Instance.CreateEntry("Couldn't initialize WebView2 control. Fallback UI used instead.");
				ActionManager.ExecuteAction("OnLauncherReady");
			}
		}
		catch (Exception ex)
		{
			ActionManager.ExecuteAction("OnWebPageInitializationFailed");
			FileLogger.Instance.CreateEntry("Error while initializing WebView2 control in Loaded: " + ex.Message);
		}
	}

	private void StartTimeoutTimer()
	{
		_timeoutTimer.Interval = 3000;
		_timeoutTimer.Tick += TimeoutTimer_Tick;
		_timeoutTimer.Start();
	}

	private void TimeoutTimer_Tick(object sender, EventArgs e)
	{
		if (_navigationCompleted)
		{
			_timeoutTimer.Stop();
			return;
		}
		FileLogger.Instance.CreateEntry("Webpage request timed out");
		_timeoutTimer.Stop();
		FileLogger.Instance.CreateEntry("OnLauncherReady");
		ActionManager.ExecuteAction("OnLauncherReady");
	}

	private async Task<bool> AssureWebView2RuntimeInstalledAndInitialize()
	{
		if (await CheckWebView2RuntimeVersion() == WebView2VersionResult.Ok)
		{
			return true;
		}
		return false;
	}

	private async Task<WebView2VersionResult> CheckWebView2RuntimeVersion()
	{
		_ = 1;
		try
		{
			FileLogger.Instance.CreateEntry("Hooking up Fixed Version WebView2 Environment");
			CoreWebView2Environment coreWebView2Environment = await CoreWebView2Environment.CreateAsync(Path.Combine("./WebView2/Microsoft.WebView2.FixedVersionRuntime.116.0.1938.76.x64"), Directories.ProjectDirectory(Settings.Default.Project));
			string browserVersionString = coreWebView2Environment.BrowserVersionString;
			Version version = new Version(browserVersionString);
			FileLogger.Instance.CreateEntry("MS Edge WebView2 Runtime Version: " + browserVersionString);
			if (version.Build == 1938)
			{
				FileLogger.Instance.CreateEntry("Initializing WebView2");
				webpageHost.NavigationCompleted += WebpageHost_NavigationCompleted;
				webpageHost.CoreWebView2InitializationCompleted += WebpageHost_CoreWebView2InitializationCompleted;
				await webpageHost.EnsureCoreWebView2Async(coreWebView2Environment);
				FileLogger.Instance.CreateEntry("WebView2 Initialized");
				return WebView2VersionResult.Ok;
			}
			FileLogger.Instance.CreateEntry("Wrong Fixed WebView 2 Runtime?");
			return WebView2VersionResult.Outdated;
		}
		catch (WebView2RuntimeNotFoundException)
		{
			FileLogger.Instance.CreateEntry("Couldn't find Fixed WebView2 Runtime.");
			return WebView2VersionResult.NotFound;
		}
		catch (Exception ex2)
		{
			FileLogger.Instance.CreateEntry("Something went wrong while checking or initializing WebView2 runtime version: " + ex2.StackTrace + Environment.NewLine + ex2.Message);
			return WebView2VersionResult.Other;
		}
	}

	private void WebpageHost_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
	{
		if (webpageHost.CoreWebView2 != null)
		{
			FileLogger.Instance.CreateEntry("WebView2 setting up settings");
			webpageHost.CoreWebView2.Settings.IsStatusBarEnabled = false;
			webpageHost.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
			webpageHost.CoreWebView2.Settings.AreDevToolsEnabled = false;
			webpageHost.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
			webpageHost.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
			webpageHost.CoreWebView2.Settings.IsZoomControlEnabled = false;
			webpageHost.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
			webpageHost.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
			FileLogger.Instance.CreateEntry("WebView2 settings setup completed");
		}
		else
		{
			FileLogger.Instance.CreateEntry("WebView2 CoreWebView2 was null");
			ActionManager.ExecuteAction("OnWebPageInitializationFailed");
		}
	}

	private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
	{
		e.Cancel = true;
	}

	private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
	{
		Process.Start(e.Uri);
		e.Handled = true;
	}

	private void WebpageHost_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		_navigationCompleted = true;
		FileLogger.Instance.CreateEntry("WebView2 Navigation Completed");
		if (e.IsSuccess)
		{
			webpageHost.Visible = true;
			FileLogger.Instance.CreateEntry("Running OnWebpageLoaded");
			ActionManager.ExecuteAction("OnWebpageLoaded");
		}
		FileLogger.Instance.CreateEntry("OnLauncherReady");
		ActionManager.ExecuteAction("OnLauncherReady");
	}

	private void Form_Resize(object sender, EventArgs e)
	{
		webpageHost.Size = base.ClientSize - new Size(webpageHost.Location);
	}

	public new void Dispose()
	{
		webpageHost.Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher.WebpageControl));
		this.webpageHost = new Microsoft.Web.WebView2.WinForms.WebView2();
		((System.ComponentModel.ISupportInitialize)this.webpageHost).BeginInit();
		base.SuspendLayout();
		this.webpageHost.BackgroundImage = (System.Drawing.Image)resources.GetObject("webpageHost.BackgroundImage");
		this.webpageHost.CreationProperties = null;
		this.webpageHost.Location = new System.Drawing.Point(0, 0);
		this.webpageHost.Margin = new System.Windows.Forms.Padding(0);
		this.webpageHost.Name = "webpageHost";
		this.webpageHost.Size = new System.Drawing.Size(1280, 800);
		this.webpageHost.TabIndex = 0;
		this.webpageHost.Visible = false;
		this.webpageHost.ZoomFactor = 1.0;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("webpageHost.BackgroundImage");
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.Controls.Add(this.webpageHost);
		base.Name = "WebpageControl";
		base.Size = new System.Drawing.Size(1280, 800);
		base.Load += new System.EventHandler(Loaded);
		((System.ComponentModel.ISupportInitialize)this.webpageHost).EndInit();
		base.ResumeLayout(false);
	}
}
