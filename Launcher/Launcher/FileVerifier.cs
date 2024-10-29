using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Launcher.Properties;
using LauncherHelper;
using ResourceDictionary.Properties;

namespace Launcher;

public static class FileVerifier
{
	private class HashFilePaths
	{
		public string LocalFilePath { get; set; }

		public string BuildFilePath { get; set; }

		public HashFilePaths(string localFilePath = "", string buildFilePath = "")
		{
			LocalFilePath = localFilePath;
			BuildFilePath = buildFilePath;
		}
	}

	private static readonly Dictionary<string, string> _localHashFiles = new Dictionary<string, string>
	{
		{ "binaries", "local_binaries_manifest.txt" },
		{ "bundle", "local_bundle_manifest.txt" },
		{ "shader_cache", "local_shader_cache_manifest.txt" }
	};

	private static readonly Dictionary<string, string> _buildGeneratedHashFiles = new Dictionary<string, string>
	{
		{ "binaries", "binaries_manifest.txt" },
		{ "bundle", "bundle_manifest.txt" },
		{ "shader_cache", "shader_cache_manifest.txt" }
	};

	private static Dictionary<string, HashFilePaths> _dirToFilePaths = new Dictionary<string, HashFilePaths>();

	private static readonly string _hdbc = "HashDatabaseCreator.exe";

	private static readonly string _manifestFolderName = "manifests";

	public static async Task<VerifierResult> VerifyFiles(Window ownerWindow)
	{
		FileLogger.Instance.CreateEntry("Running file verification");
		_dirToFilePaths.Clear();
		VerifierResult verifierResult = await VerificationTest(ownerWindow);
		switch (verifierResult)
		{
		case VerifierResult.SUCCESS:
		{
			InfoBox infoBox2 = new InfoBox(Resources.loc_verify_files_success_body, Resources.loc_verify_files_completed_header);
			infoBox2.Owner = ownerWindow;
			infoBox2.ShowDialog();
			break;
		}
		case VerifierResult.FAIL:
		{
			InfoBox infoBox = new InfoBox(Resources.loc_verify_files_failed_body, Resources.loc_verify_files_completed_header, "Ok", 770.0, 560.0, 390.0, "/ResourceDictionary;component/assets/settings_window/settings_background.png");
			infoBox.Owner = ownerWindow;
			infoBox.ShowDialog();
			break;
		}
		}
		FileLogger.Instance.CreateEntry("Finished running file verification");
		FileLogger.Instance.CreateEntry("File verification result: " + verifierResult);
		return verifierResult;
	}

	private static async Task<VerifierResult> VerificationTest(Window ownerWindow)
	{
		string appDataGameDir = Directories.ProjectDirectory(Settings.Default.Project);
		string steamGameLauncherDir = Directory.GetCurrentDirectory();
		string steamGameDir = Directories.GetBaseDirectoryFromCurrentAppDirectory();
		foreach (KeyValuePair<string, string> buildGeneratedHashFile in _buildGeneratedHashFiles)
		{
			string text = Path.Combine(steamGameDir, _manifestFolderName, buildGeneratedHashFile.Value);
			if (!File.Exists(text))
			{
				InfoBox infoBox = new InfoBox(string.Format(Resources.loc_verify_files_missing_file, text));
				infoBox.Owner = ownerWindow;
				infoBox.ShowDialog();
				FileLogger.Instance.CreateEntry(string.Format($"Error: Could not find file '{0}' on disk.", text));
				return VerifierResult.FAIL;
			}
			_dirToFilePaths.Add(buildGeneratedHashFile.Key, new HashFilePaths("", text));
		}
		foreach (KeyValuePair<string, string> kvp in _localHashFiles)
		{
			string dirToHash = Path.Combine(steamGameDir, kvp.Key);
			string localHashFilePath = Path.Combine(appDataGameDir, kvp.Value);
			VerifierResult verifierResult = await GenerateFileHashes(ownerWindow, steamGameLauncherDir, dirToHash, localHashFilePath);
			if (verifierResult != 0)
			{
				FileLogger.Instance.CreateEntry("Error: The local hash file generation failed. See log for details.");
				return verifierResult;
			}
			if (_dirToFilePaths.ContainsKey(kvp.Key))
			{
				_dirToFilePaths[kvp.Key].LocalFilePath = localHashFilePath;
				continue;
			}
			FileLogger.Instance.CreateEntry("Error: The build generated has file corresponding to directory '" + kvp.Key + "' was not found. Mismatch between number of local hash files and build generates ones.");
			return VerifierResult.FAIL;
		}
		foreach (KeyValuePair<string, HashFilePaths> dirToFilePath in _dirToFilePaths)
		{
			if (string.IsNullOrEmpty(dirToFilePath.Value.BuildFilePath))
			{
				FileLogger.Instance.CreateEntry("Error: No path to the build file '" + _buildGeneratedHashFiles[dirToFilePath.Key] + "' for the '" + dirToFilePath.Key + "' directory could be found.");
				return VerifierResult.FAIL;
			}
			if (string.IsNullOrEmpty(dirToFilePath.Value.LocalFilePath))
			{
				FileLogger.Instance.CreateEntry("Error: No path to the local hash file '" + _localHashFiles[dirToFilePath.Key] + "' for the '" + dirToFilePath.Key + "' directory could be found.");
				return VerifierResult.FAIL;
			}
			if (!CompareHashFiles(dirToFilePath.Value.BuildFilePath, dirToFilePath.Value.LocalFilePath))
			{
				return VerifierResult.FAIL;
			}
		}
		return VerifierResult.SUCCESS;
	}

	private static async Task<VerifierResult> GenerateFileHashes(Window ownerWindow, string workingDir, string dirToHash, string outputFileName)
	{
		FileLogger.Instance.CreateEntry("Generating hashes for game files in '" + dirToHash + "'");
		LoadingBar loadingBar = new LoadingBar();
		loadingBar.Topmost = false;
		loadingBar.Owner = ownerWindow;
		loadingBar.Label = Resources.loc_verify_files_in_progress;
		FileLogger.Instance.PushScope("HashDatabaseCreator");
		try
		{
			Process process = new Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = workingDir;
			processStartInfo.FileName = _hdbc;
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.Arguments = "\"" + dirToHash + "\"" + " " + "\"" + outputFileName + "\"" + " " + "\"" + "BASE" + "\"" + " " + "\"" + "SKIP_SPINNER" + "\"";
			loadingBar.Closing += delegate
			{
				if (!process.HasExited)
				{
					process.Kill();
				}
			};
			process.StartInfo = processStartInfo;
			string outputData = "";
			process.OutputDataReceived += delegate(object send, DataReceivedEventArgs args)
			{
				string data = args.Data;
				if (string.IsNullOrEmpty(data))
				{
					FileLogger.Instance.CreateEntry("Error: Empty or null data received from HashDatabaseCreator.");
				}
				else
				{
					outputData = outputData + "\n" + data;
					if (data.Contains("/"))
					{
						try
						{
							loadingBar.Show();
							string[] array = data.Split('/');
							if (array.Length != 2)
							{
								FileLogger.Instance.CreateEntry("Error: " + _hdbc + " output data was not correct format. Data:" + data);
							}
							else
							{
								double num = Convert.ToDouble(array[0]) / Convert.ToDouble(array[1]);
								loadingBar.Value = num;
								loadingBar.ProgressText = $"{num:P0}";
							}
							return;
						}
						catch (Exception ex2)
						{
							FileLogger.Instance.CreateEntry("Exception was thrown while reading data from " + _hdbc + ".\nError: " + ex2.Message);
							return;
						}
					}
					FileLogger.Instance.CreateEntry(data ?? "");
				}
			};
			FileLogger.Instance.CreateEntry("Executing " + _hdbc + " in " + processStartInfo.WorkingDirectory + " with args: " + processStartInfo.Arguments);
			process.Start();
			process.BeginOutputReadLine();
			await Task.Run(delegate
			{
				process.WaitForExit();
			});
			if (process.ExitCode != 0)
			{
				FileLogger.Instance.CreateEntry($"Error: Something went wrong in the HashDatabaseCreator process. Exit Code = '{process.ExitCode}'. Data from output:\n{outputData}");
				loadingBar?.Close();
				return VerifierResult.ABORTED;
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Exception was thrown when attempting to run " + _hdbc + ". Error:\n" + ex.Message);
		}
		loadingBar.Close();
		FileLogger.Instance.PopScope();
		return VerifierResult.SUCCESS;
	}

	private static bool CompareHashFiles(string buildFilesPath, string localFilesPath)
	{
		try
		{
			Dictionary<string, string> dictionary = File.ReadLines(buildFilesPath)?.Select((string line) => line.Split(','))?.ToDictionary((string[] line) => line[0], (string[] line) => line[3]);
			Dictionary<string, string> dictionary2 = File.ReadLines(localFilesPath)?.Select((string line) => line.Split(','))?.ToDictionary((string[] line) => line[0], (string[] line) => line[3]);
			if (dictionary2 == null || dictionary == null)
			{
				FileLogger.Instance.CreateEntry("Error: Failed to create dictionaries from files " + buildFilesPath + " and " + localFilesPath + ".");
				return false;
			}
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (!dictionary2.ContainsKey(item.Key))
				{
					return false;
				}
				if (dictionary2[item.Key] != item.Value)
				{
					return false;
				}
			}
		}
		catch (IndexOutOfRangeException ex)
		{
			FileLogger.Instance.CreateEntry("Index out of bounds exception when creating dictionaries from hash files " + buildFilesPath + " and " + localFilesPath + ".\n" + ex.Message);
			return false;
		}
		catch (Exception ex2)
		{
			FileLogger.Instance.CreateEntry("Unknown error when converting .csv files " + buildFilesPath + " and " + localFilesPath + " to dictionaries.\n" + ex2.Message);
			return false;
		}
		return true;
	}
}
