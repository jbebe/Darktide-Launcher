using System;
using System.Collections.Generic;
using System.IO;
using Launcher.Properties;
using LauncherHelper;
using Steamworks;

namespace Launcher;

public class ArgumentHolder
{
	private Dictionary<string, string> ArgsList { get; set; } = new Dictionary<string, string>();


	private Dictionary<string, string> GameArgs { get; set; } = new Dictionary<string, string>();


	public string CommonSettingsIniPath { get; set; }

	public string Win32SettingsIniPath { get; set; }

	public string UseEAC { get; set; }

	private string BasePath { get; set; }

	public ArgumentHolder(string base_path, bool steam_initialized, string releasePlatform)
	{
		using (new FileLogger.ScopeHolder("Parse Arguments"))
		{
			BasePath = base_path;
			parseArguments(Settings.Default.ExeArgs);
			SetBackend(steam_initialized, releasePlatform);
			parseArguments(Environment.CommandLine, command_line: true);
			SetIniPaths();
		}
	}

	public string CreateGameParameters(bool forceFlush, bool waitForDebugger, bool use_eac, string eac_config, bool resetKeyBindings, bool fileVerificationSucces)
	{
		if (resetKeyBindings)
		{
			GameArgs["-reset_keybind_on_start"] = "";
		}
		GameArgs["-launcher_verification_passed_crashify_property"] = fileVerificationSucces.ToString().ToLowerInvariant();
		string text = "";
		if (use_eac)
		{
			if (!string.IsNullOrEmpty(eac_config))
			{
				text = text + " -anticheat_settings=" + eac_config;
			}
			foreach (KeyValuePair<string, string> args in ArgsList)
			{
				string text2 = ((!string.IsNullOrEmpty(text)) ? " " : "");
				text = text + text2 + args.Key;
				if (args.Value != "")
				{
					text = text + " " + args.Value;
				}
			}
		}
		else
		{
			text = "-eac-untrusted";
			foreach (KeyValuePair<string, string> args2 in ArgsList)
			{
				string text3 = ((!string.IsNullOrEmpty(text)) ? " " : "");
				text = text + text3 + args2.Key;
				if (args2.Value != "")
				{
					text = text + " " + args2.Value;
				}
			}
			if (forceFlush)
			{
				text += " --force-flush-logfile";
			}
			if (waitForDebugger)
			{
				text += " --wait-for-native-debugger";
			}
		}
		if (GameArgs.Count != 0)
		{
			text += " -game";
			foreach (KeyValuePair<string, string> gameArg in GameArgs)
			{
				string text4 = ((!string.IsNullOrEmpty(text)) ? " " : "");
				text = text + text4 + gameArg.Key;
				if (gameArg.Value != "")
				{
					text = text + " " + gameArg.Value;
				}
			}
		}
		return text;
	}

	private void parseArguments(string arguments, bool command_line = false)
	{
		char[] separator = new char[1] { ' ' };
		List<string> list = new List<string>(arguments.Split(separator));
		if (list.Count == 0 || list[0] == "")
		{
			return;
		}
		if (command_line)
		{
			list.RemoveAt(0);
		}
		string text = "";
		bool flag = false;
		foreach (string item in list)
		{
			if (item.StartsWith("-game"))
			{
				flag = true;
				text = "";
			}
			else if (item.StartsWith("-"))
			{
				if (flag)
				{
					GameArgs[item] = "";
				}
				else
				{
					ArgsList[item] = "";
				}
				text = item;
			}
			else if (text != "")
			{
				if (flag)
				{
					GameArgs[text] = item;
				}
				else
				{
					ArgsList[text] = item;
				}
				text = "";
			}
		}
	}

	private void SetIniPaths()
	{
		string path = "settings_common.ini";
		string path2 = "win32_settings.ini";
		string contentPath = getContentPath("--data-dir", ArgsList);
		if (contentPath != null)
		{
			CommonSettingsIniPath = Path.Combine(contentPath, "application_settings", path);
			Win32SettingsIniPath = Path.Combine(contentPath, "application_settings", path2);
			FileLogger.Instance.CreateEntry("Data dir: " + contentPath);
		}
		else
		{
			string contentPath2 = getContentPath("--bundle-dir", ArgsList, "bundle");
			CommonSettingsIniPath = Path.Combine(contentPath2, "application_settings", path);
			Win32SettingsIniPath = Path.Combine(contentPath2, "application_settings", path2);
			FileLogger.Instance.CreateEntry("Bundle dir: " + contentPath2);
		}
		FileLogger.Instance.CreateEntry("Common Ini-file: " + CommonSettingsIniPath);
		FileLogger.Instance.CreateEntry("Win32 Ini-file: " + Win32SettingsIniPath);
	}

	private string getContentPath(string parameter, Dictionary<string, string> argsList, string default_base_path = null)
	{
		string text;
		if (argsList.ContainsKey(parameter))
		{
			text = ArgsList[parameter];
		}
		else
		{
			if (default_base_path == null)
			{
				return null;
			}
			text = default_base_path;
		}
		string text2 = Path.Combine(BasePath, text);
		if (!Directory.Exists(text2) && text.StartsWith(".."))
		{
			text = text.Replace('\\', '/').Replace("../", "");
			return Path.Combine(BasePath, text);
		}
		return text2;
	}

	public void SetBackend(bool steamInitialized, string releasePlatform)
	{
		string text = "dev";
		if (steamInitialized && releasePlatform == ReleasePlatform.steam.ToString())
		{
			FileLogger.Instance.CreateEntry("Steam is initialized.");
			uint appId = SteamUtils.GetAppID().m_AppId;
			string pchName = new string(' ', 32);
			if (!SteamApps.GetCurrentBetaName(out pchName, 32))
			{
				pchName = "default";
			}
			FileLogger.Instance.CreateEntry($"Retrieving backend settings for Steam application '{appId}' (branch='{pchName}')");
			text = BackendSettings.GetBackend(appId, pchName);
		}
		if (releasePlatform == ReleasePlatform.ms_store.ToString())
		{
			text = Settings.Default.Backend.ToString().Replace("_", "-");
		}
		if (releasePlatform == ReleasePlatform.steam.ToString())
		{
			FileLogger.Instance.CreateEntry($"Using backend '{text}'");
			ArgsList["--backend-auth-service-url"] = BackendSettings.BackendAuthenticationServiceUrl(text);
			ArgsList["--backend-title-service-url"] = BackendSettings.BackendTitleServiceUrl(text);
		}
	}
}
