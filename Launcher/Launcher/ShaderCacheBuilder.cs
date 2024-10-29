using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Launcher.Properties;
using LauncherHelper;
using ResourceDictionary.Properties;

namespace Launcher;

internal class ShaderCacheBuilder
{
	public static async Task BuildShaderCacheAsync(Window ownerWindow, string basePath, int adapterId)
	{
		FileLogger.Instance.CreateEntry("Running shader cache builder");
		LoadingBar loadingBar = new LoadingBar();
		loadingBar.Owner = ownerWindow;
		try
		{
			FileLogger.Instance.PushScope("ShaderCacheBuilder");
			string text = Path.Combine(basePath, "shader_cache");
			string path = Directories.ProjectDirectory(Settings.Default.Project);
			Directory.SetCurrentDirectory(text);
			Process process = new Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WorkingDirectory = text;
			processStartInfo.FileName = "darktide_shader_cache_builder.exe";
			processStartInfo.Arguments = "-i \"" + Path.Combine(text, "bundled_shader_cache.hans") + "\" -o \"" + Path.Combine(path, "shader_cache.hans") + "\" " + $"-device-index {adapterId}";
			FileLogger.Instance.CreateEntry("Executing " + processStartInfo.FileName + " with args: " + processStartInfo.Arguments);
			string outputData = "";
			process.StartInfo = processStartInfo;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			string @string = Resources.ResourceManager.GetString("LOC_ShaderCache");
			loadingBar.Label = @string;
			bool hasThrownExceptions = false;
			process.OutputDataReceived += delegate(object send, DataReceivedEventArgs args)
			{
				string data = args.Data;
				if (!string.IsNullOrEmpty(data))
				{
					outputData = outputData + "\n" + data;
				}
				if (!string.IsNullOrEmpty(data) && (data.Contains("Shader is valid") || data.Contains("Compiling shader")))
				{
					try
					{
						loadingBar.Show();
						int num = data.IndexOf("(") + 1;
						int num2 = data.IndexOf(")");
						if (num != -1 && num2 != -1)
						{
							string[] array = data.Substring(num, num2 - num).Split('/');
							double num3 = Convert.ToDouble(array[0]) / Convert.ToDouble(array[1]);
							loadingBar.Value = num3;
							loadingBar.ProgressText = $"{num3:P0}";
						}
						return;
					}
					catch (Exception ex2)
					{
						FileLogger.Instance.CreateEntry("Exception was thrown while reading data from shader cache builder process. \nData received: " + data + "\nError: " + ex2.Message);
						hasThrownExceptions = true;
						return;
					}
				}
				if (!string.IsNullOrEmpty(data))
				{
					FileLogger.Instance.CreateEntry("Data from Shader Cache Process: " + data);
				}
			};
			process.Start();
			process.BeginOutputReadLine();
			await Task.Run(delegate
			{
				process.WaitForExit();
			});
			if (process.ExitCode != 0 || hasThrownExceptions)
			{
				FileLogger.Instance.CreateEntry("Something went wrong in the Shader Cache Builder process. Exit Code = 0x" + process.ExitCode.ToString("X") + ".\nData from output: " + outputData);
				string obj = Resources.loc_shader_cache_builder_failed ?? "";
				string text2 = string.Format(Resources.loc_exit_code, "0x" + process.ExitCode.ToString("X")) ?? "";
				MessageBox.Show(obj + "\n" + text2);
			}
			FileLogger.Instance.PopScope();
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Exception was thrown when attempting to run DarktideShaderCacheBuilder.exe. Cache will not be validated. Error: " + ex.Message);
			FileLogger.Instance.PopScope();
		}
		loadingBar.Close();
		Directory.SetCurrentDirectory(Path.Combine(basePath, "launcher"));
		FileLogger.Instance.CreateEntry("Shader cache builder completed.");
	}
}
