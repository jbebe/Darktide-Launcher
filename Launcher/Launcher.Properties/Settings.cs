using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Launcher.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("user_settings.config")]
	public string SettingsFile => (string)this["SettingsFile"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Medium")]
	public string DefaultQuality => (string)this["DefaultQuality"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1280x768;1024x768;800x600")]
	public string DefaultResolutions => (string)this["DefaultResolutions"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("--bundle-dir ../bundle --ini settings --silent-mode")]
	public string ExeArgs => (string)this["ExeArgs"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1.0.0")]
	public string LauncherVersion => (string)this["LauncherVersion"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("binaries dev")]
	public string ExeDirs => (string)this["ExeDirs"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("stingray_win64_dev_x64.exe Darktide.exe")]
	public string ExeName => (string)this["ExeName"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("1075420")]
	public string AppId => (string)this["AppId"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("BishopInternal")]
	public string Project => (string)this["Project"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("steam")]
	public string ReleasePlatform => (string)this["ReleasePlatform"];

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("https://www.2f8a830db8-2d0e-4a55-aced-ef6d0279b1442f.org/")]
	public string MarketingURL
	{
		get
		{
			return (string)this["MarketingURL"];
		}
		set
		{
			this["MarketingURL"] = value;
		}
	}

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("dev")]
	public string Backend => (string)this["Backend"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string Release => (string)this["Release"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("10")]
	public int ConnectionTimeoutSeconds => (int)this["ConnectionTimeoutSeconds"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
