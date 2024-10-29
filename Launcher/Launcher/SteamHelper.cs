using System;
using System.Windows;
using LauncherHelper;
using ResourceDictionary.Properties;
using Steamworks;

namespace Launcher;

internal static class SteamHelper
{
	public static bool InitializeSteam(uint wantedAppId, out bool steamInitialized)
	{
		try
		{
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(wantedAppId)))
			{
				FileLogger.Instance.CreateEntry("Launcher was run from explorer. Restarting launcher through Steam...");
				FileLogger.Instance.Close();
				Application.Current.Shutdown();
				steamInitialized = false;
				return false;
			}
			steamInitialized = SteamAPI.Init();
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("SteamAPI initialization error: " + ex.ToString());
			ex.Message.ToString();
			new DebugDialog($"SteamAPI initialization error: {ex.ToString()}").ShowDialog();
			FileLogger.Instance.Close();
			Application.Current.Shutdown();
			steamInitialized = false;
			return false;
		}
		try
		{
			if (SteamAPI.IsSteamRunning() & steamInitialized)
			{
				string personaName = SteamFriends.GetPersonaName();
				if (!string.IsNullOrEmpty(personaName))
				{
					FileLogger.Instance.CreateEntry("Steam name: " + personaName);
				}
				CSteamID steamID = SteamUser.GetSteamID();
				byte[] array = new byte[1024];
				SteamUser.GetAuthSessionTicket(array, 1024, out var pcbTicket);
				SteamUser.BeginAuthSession(array, (int)pcbTicket, steamID);
				try
				{
					AppId_t appID = SteamUtils.GetAppID();
					if (wantedAppId != appID.m_AppId)
					{
						FileLogger.Instance.CreateEntry("!! Expected " + wantedAppId + ", got " + appID.m_AppId);
						if (MessageBox.Show(string.Format(Resources.ResourceManager.GetString("LOC_SteamAppIdMismatch_Message"), appID.m_AppId.ToString(), wantedAppId.ToString()), Resources.ResourceManager.GetString("LOC_SteamAppIdMismatch_Title"), MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
						{
							FileLogger.Instance.CreateEntry("User aborted.");
							Application.Current.Shutdown();
						}
						else
						{
							FileLogger.Instance.CreateEntry("User continued.");
						}
					}
				}
				catch (Exception ex2)
				{
					FileLogger.Instance.CreateEntry("Steam App Info Error: " + ex2.ToString());
				}
			}
		}
		catch (Exception ex3)
		{
			FileLogger.Instance.CreateEntry("Steam get user failed with error: " + ex3.ToString());
			MessageBox.Show(Resources.ResourceManager.GetString("LOC_SteamErrorMessage"), Resources.ResourceManager.GetString("LOC_ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Hand);
			FileLogger.Instance.Close();
			Application.Current.Shutdown();
			steamInitialized = false;
			return false;
		}
		return true;
	}
}
