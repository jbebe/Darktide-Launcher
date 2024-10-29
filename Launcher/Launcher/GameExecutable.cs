using System;
using System.Diagnostics;
using System.IO;
using LauncherHelper;

namespace Launcher;

public class GameExecutable
{
	protected string _name;

	protected string _path;

	protected FileVersionInfo _version;

	protected bool _present;

	protected bool _active;

	protected string _eac_config_name;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
			if (value)
			{
				FileLogger.Instance.CreateEntry("Executable " + _path + " set to active");
			}
		}
	}

	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
			if (File.Exists(_path))
			{
				_present = true;
				FetchVersionInfo();
			}
		}
	}

	public bool Present => _present;

	public string EACConfig => _eac_config_name;

	public bool EACPresent => true;

	public void FetchVersionInfo()
	{
		_version = FileVersionInfo.GetVersionInfo(_path);
		string text = _version.ToString();
		FileLogger.Instance.CreateMultiLineEntry("Version info for excutable " + _path + Environment.NewLine + text);
	}

	public string get_directory()
	{
		if (_present)
		{
			return new FileInfo(_path).DirectoryName;
		}
		return "";
	}

	public string VersionString()
	{
		return _name + " {" + _version.Comments + "}";
	}

	public bool CheckForEACConfig(string base_dir)
	{
		_eac_config_name = null;
		if (_present)
		{
			if (_path.EndsWith("Darktide.exe"))
			{
				return true;
			}
			if (_path.Contains("stingray"))
			{
				_eac_config_name = "../dev/eac_settings.json";
				return true;
			}
		}
		return false;
	}
}
