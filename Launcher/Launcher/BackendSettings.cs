using System.Collections.Generic;

namespace Launcher;

public static class BackendSettings
{
	public enum SteamApp : uint
	{
		BishopInternal = 1075420u,
		DarktideExternal = 1033010u,
		DarktideMain = 1361210u,
		DarktideMainPlaytest = 2156210u
	}

	private class BranchBackend
	{
		public SteamApp AppId { get; set; }

		public string Branch { get; set; }

		public string Backend { get; set; }

		public BranchBackend(SteamApp appId, string branch, string backend)
		{
			AppId = appId;
			Branch = branch;
			Backend = backend;
		}
	}

	private static List<BranchBackend> branchBackends = new List<BranchBackend>
	{
		new BranchBackend(SteamApp.BishopInternal, "default", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "nightly_full", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "nightly_stripped", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "latest_playable", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "pwt", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "live_mirror", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "release1_dev", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "release2_dev", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "amd_testing", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "nvidia_testing", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "nvidia_testing2", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "intel_testing", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "wetest", "dev-classrew"),
		new BranchBackend(SteamApp.BishopInternal, "development", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "tools_builds_testing", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "backend_sandbox", "backend-sandbox"),
		new BranchBackend(SteamApp.BishopInternal, "recording", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "launcher_testing", "dev"),
		new BranchBackend(SteamApp.BishopInternal, "blue", "dev-blue"),
		new BranchBackend(SteamApp.BishopInternal, "red_ct", "dev-red"),
		new BranchBackend(SteamApp.BishopInternal, "dev_items", "dev-items"),
		new BranchBackend(SteamApp.BishopInternal, "wwise_2022", "dev"),
		new BranchBackend(SteamApp.DarktideExternal, "default", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "nightly_full", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "nightly_stripped", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "latest_playable", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "live_mirror", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "release1_staging", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "release1_staging_backup", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "release2_staging", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "release2_staging_backup", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "blue", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "red_ct", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "external_players", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "external_show", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "external_show_bckup", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "tcn_testing", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "external_partners", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "external_reviewone", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "external_reviewtwo", "staging-ptr"),
		new BranchBackend(SteamApp.DarktideExternal, "wetest", "staging-ratings"),
		new BranchBackend(SteamApp.DarktideExternal, "development", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "tools_builds_testing", "staging"),
		new BranchBackend(SteamApp.DarktideExternal, "launcher_testing", "staging"),
		new BranchBackend(SteamApp.DarktideMain, "default", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "release1_main", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "release1_main_backup", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "release2_main", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "release2_main_backup", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "experimental", "prod"),
		new BranchBackend(SteamApp.DarktideMain, "launcher_testing", "prod"),
		new BranchBackend(SteamApp.DarktideMainPlaytest, "default", "prod"),
		new BranchBackend(SteamApp.DarktideMainPlaytest, "release1_playtest", "prod"),
		new BranchBackend(SteamApp.DarktideMainPlaytest, "release2_playtest", "prod")
	};

	public const string DefaultBackend = "dev";

	private static readonly IDictionary<SteamApp, string> DefaultBackends = new Dictionary<SteamApp, string>
	{
		{
			SteamApp.BishopInternal,
			"dev"
		},
		{
			SteamApp.DarktideExternal,
			"staging"
		},
		{
			SteamApp.DarktideMain,
			"prod"
		},
		{
			SteamApp.DarktideMainPlaytest,
			"prod"
		}
	};

	public static string GetDefaultBackend(uint appId)
	{
		string value = "";
		if (!DefaultBackends.TryGetValue((SteamApp)appId, out value))
		{
			value = "dev";
		}
		return value;
	}

	public static string GetBackend(uint appId, string branch)
	{
		string result = GetDefaultBackend(appId);
		BranchBackend branchBackend = branchBackends.Find((BranchBackend b) => b.AppId == (SteamApp)appId && b.Branch == branch);
		if (branchBackend != null)
		{
			result = branchBackend.Backend;
		}
		return result;
	}

	private static string DetermineDomain(string backend)
	{
		if (!backend.StartsWith("prod"))
		{
			return "fatsharkgames.se";
		}
		return "atoma.cloud";
	}

	public static string BackendAuthenticationServiceUrl(string backend)
	{
		return "https://bsp-auth-" + backend + "." + DetermineDomain(backend);
	}

	public static string BackendTitleServiceUrl(string backend)
	{
		return "https://bsp-td-" + backend + "." + DetermineDomain(backend);
	}
}
