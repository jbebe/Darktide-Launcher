using System;
using LauncherHelper;

namespace Launcher;

public class VersionInfo
{
	private const short INVALID_VERSION = -1;

	protected short major_version;

	protected short minor_version;

	protected short patch_version;

	protected short hotfix_version;

	public VersionInfo()
	{
		major_version = -1;
		minor_version = -1;
		patch_version = -1;
		hotfix_version = 0;
	}

	public override string ToString()
	{
		string text;
		if (major_version < 0 || minor_version < 0 || patch_version < 0)
		{
			text = "Unknown Version";
		}
		text = major_version + "." + minor_version + "." + patch_version;
		if (hotfix_version > 0)
		{
			text = text + "." + hotfix_version;
		}
		return text;
	}

	public void SetVersion(string versionInfo)
	{
		try
		{
			string[] array = versionInfo.Split('.');
			if (array.Length != 0)
			{
				major_version = short.Parse(array[0]);
			}
			if (array.Length > 1)
			{
				minor_version = short.Parse(array[1]);
			}
			if (array.Length > 2)
			{
				patch_version = short.Parse(array[2]);
			}
			if (array.Length > 3)
			{
				hotfix_version = short.Parse(array[3]);
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error while setting version: " + ex.Message);
			major_version = (minor_version = (patch_version = -1));
		}
	}

	public bool Equals(VersionInfo other)
	{
		if (major_version == other.major_version && minor_version == other.minor_version && patch_version == other.patch_version)
		{
			return hotfix_version == other.hotfix_version;
		}
		return false;
	}

	public static bool operator <(VersionInfo v1, VersionInfo v2)
	{
		if (v1.major_version == v2.major_version)
		{
			if (v1.minor_version == v2.minor_version)
			{
				if (v1.patch_version == v2.patch_version)
				{
					return v1.hotfix_version < v2.hotfix_version;
				}
				return v1.patch_version < v2.patch_version;
			}
			return v1.minor_version < v2.minor_version;
		}
		return v1.major_version < v2.major_version;
	}

	public static bool operator >(VersionInfo v1, VersionInfo v2)
	{
		return v2 < v1;
	}
}
