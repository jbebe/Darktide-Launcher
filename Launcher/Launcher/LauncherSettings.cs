using System;
using System.Collections.Generic;
using System.IO;
using Launcher.Properties;
using LauncherHelper;
using Stingray.Json;

namespace Launcher;

public class LauncherSettings
{
	public bool SendCrashReports = true;

	public bool DebugLog;

	public int LastScreenIndex = -1;

	public double WindowLeftPos = 40.0;

	public double WindowTopPos = 40.0;

	public string LastContentRevision = "";

	public string OutputVersion;

	public int PrimaryOutputId;

	public List<SysInfo.Output> Outputs;
	
	public bool AutoRun;

	public bool Load()
	{
		try
		{
			using (new FileLogger.ScopeHolder("Load Settings"))
			{
				string text = Directories.LauncherConfigFileName(Settings.Default.Project);
				if (!Directories.HasWriteAccessToFolder(Directories.ProjectDirectory(Settings.Default.Project)))
				{
					return false;
				}
				FileLogger.Instance.CreateEntry("Load settings from: " + text);
				if (File.Exists(text))
				{
					JsonSerializer.Load(this, SJSON.LoadGeneric(text));
					FileLogger.Instance.CreateEntry("Load settings successful");
				}
				return true;
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error loading launcher settings: " + ex.Message);
			return true;
		}
	}

	public void Save()
	{
		try
		{
			SJSON.Save(JsonSerializer.Save(this), Directories.LauncherConfigFileName(Settings.Default.Project));
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error saving launcher settings: " + ex.Message);
		}
	}
}
