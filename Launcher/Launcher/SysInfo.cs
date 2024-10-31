using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows.Forms;
using Launcher.Properties;
using LauncherHelper;
using Microsoft.Win32;
using static Launcher.DirectXInterface;

namespace Launcher;

public class SysInfo
{
	public struct RangeQualitySettings
	{
		public GameSettingsHolder.QualityType Quality;

		public int From;

		public int To;
	}

	public struct ResolutionQualitySetting
	{
		public int From;

		public int To;

		public Resolution ResolutionInstance { get; set; }
	}

	public struct RaytracingQualitySetting
	{
		public int From;

		public int To;

		public GameSettingsHolder.RayTracingMode RaytracingMode { get; set; }
	}

	public struct FinalQualitySettings
	{
		public GameSettingsHolder.QualityType CPU;

		public GameSettingsHolder.QualityType GPU;

		public GameSettingsHolder.QualityType Preset;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private class MEMORYSTATUSEX
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	public class Resolution
	{
		public int Width { get; set; }

		public int Height { get; set; }

		public bool IsCustom { get; set; }

		public string DisplayName => string.Format("{0}x{1} {2}", Width, Height, IsCustom ? "(Custom)" : "");

		// Required by JSON parse
		public Resolution() {}

		public Resolution(int width, int height, bool is_custom = false)
		{
			Width = width;
			Height = height;
			IsCustom = is_custom;
		}
	}

	public class MakeshiftTuplePair
	{
		public int Item1;

		public int Item2;

		// Required by JSON parse
		public MakeshiftTuplePair() {}

		public MakeshiftTuplePair(int item1, int item2)
		{
			Item1 = item1;
			Item2 = item2;
		}
	}

	public class SerializableSupportedFeatures
	{
		public string Type;

		public bool SupportsRaytracing;

		// Nvidia props

		public bool SupportsDLSS;

		public bool SupportsDLSS_G;

		public bool SupportsReflex;

		public bool DisabledDLSS_G;

		public RayTracingCapabilities RayTracingCaps;

		public DLSSCapabilities DLSSCaps;

		public Nvidia_GPU_Architecture Architecture;

		public SerializableSupportedFeatures() {}

		public SerializableSupportedFeatures(SupportedFeatures supportedFeatures)
		{
			Type = supportedFeatures.GetType().Name;
			SupportsRaytracing = supportedFeatures.SupportsRaytracing;
			if (supportedFeatures is NvidiaSupportedFeatures nvidiaFeatures)
			{
				Architecture = nvidiaFeatures.Architecture;
				SupportsDLSS = nvidiaFeatures.SupportsDLSS;
				SupportsDLSS_G = nvidiaFeatures.SupportsDLSS_G;
				SupportsReflex = nvidiaFeatures.SupportsReflex;
				DisabledDLSS_G = nvidiaFeatures.DisabledDLSS_G;
				RayTracingCaps = nvidiaFeatures.RayTracingCaps;
				DLSSCaps = nvidiaFeatures.DLSSCaps;
			}
		}
	}

	public class Output : INotifyPropertyChanged
	{
		private Resolution _currentResolution;

		public MakeshiftTuplePair AspectRatio { get; set; }

		public int OutputId { get; set; }

		public int AdapterId { get; set; }

		public string AdapterName { get; set; }

		public List<Resolution> Resolutions { get; set; }

		public Resolution CurrentResolution
		{
			get
			{
				return _currentResolution;
			}
			set
			{
				_currentResolution = value;
				OnPropertyChanged("CurrentResolution");
			}
		}

		public SerializableSupportedFeatures SerializableFeatures { get; set; }

		private Lazy<SupportedFeatures> supportedFeatures;

		public SupportedFeatures Features => supportedFeatures.Value;

		public int OutputDisplayId => OutputId + 1;

		public string DisplayName => $"Display {OutputDisplayId}";

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string prop)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}
		}

		public Output()
		{
			Resolutions = new List<Resolution>();
			CurrentResolution = null;
			SerializableFeatures = new SerializableSupportedFeatures();
			supportedFeatures = new Lazy<SupportedFeatures>(() =>
			{
				return SerializableFeatures.Type switch
				{
					nameof(DefaultSupportedFeatures) => new DefaultSupportedFeatures(),
					nameof(IntelSupportedFeatures) => new IntelSupportedFeatures(),
					nameof(AmdSupportedFeatures) => new AmdSupportedFeatures(),
					nameof(NvidiaSupportedFeatures) => new NvidiaSupportedFeatures
					{
						Architecture = SerializableFeatures.Architecture,
						SupportsDLSS = SerializableFeatures.SupportsDLSS,
						SupportsDLSS_G = SerializableFeatures.SupportsDLSS_G,
						SupportsReflex = SerializableFeatures.SupportsReflex,
						DisabledDLSS_G = SerializableFeatures.DisabledDLSS_G,
						RayTracingCaps = SerializableFeatures.RayTracingCaps,
						DLSSCaps = SerializableFeatures.DLSSCaps,
						SupportsRaytracing = SerializableFeatures.SupportsRaytracing,
					},
					_ => throw new ArgumentOutOfRangeException(),
				};
			});
		}

		public Output(int output_id, int adapter_id, string adapter_name): this()
		{
			OutputId = output_id;
			AdapterId = adapter_id;
			AdapterName = adapter_name;
		}

		public void MatchResolution(GameSettingsHolder.ScreenMode screenmode, int wanted_width, int wanted_height)
		{
			Resolution resolution = Resolutions.Find((Resolution i) => i.Width == wanted_width && i.Height == wanted_height);
			if (resolution == null)
			{
				FileLogger.Instance.CreateEntry($"Wanted resolution {wanted_width}x{wanted_height} was not found in the resolution list");
				if (screenmode != GameSettingsHolder.ScreenMode.window)
				{
					string[] array = Settings.Default.DefaultResolutions.Split(';');
					foreach (string text in array)
					{
						string[] lres = text.Split('x');
						resolution = Resolutions.Find((Resolution i) => i.Width == int.Parse(lres[0]) && i.Height == int.Parse(lres[1]));
						if (resolution != null)
						{
							FileLogger.Instance.CreateEntry($"Using resolution {resolution.Width}x{resolution.Height}");
							break;
						}
					}
					if (resolution == null)
					{
						resolution = Resolutions[0];
						FileLogger.Instance.CreateEntry($"Using resolution {resolution.Width}x{resolution.Height}");
					}
				}
				else
				{
					resolution = new Resolution(wanted_width, wanted_height, is_custom: true);
					Resolutions.Add(resolution);
				}
			}
			CurrentResolution = resolution;
		}

		public void SetOptimalResolution(int GPUMark)
		{
			Resolution optimalResolution = (from x in _screenResolutionRanges
				where x.From <= GPUMark && GPUMark <= x.To
				select x.ResolutionInstance).FirstOrDefault();
			if (optimalResolution == null)
			{
				return;
			}
			int widthByAspectRatio = optimalResolution.Width;
			int heightByAspectRatio = optimalResolution.Width * AspectRatio.Item2 / AspectRatio.Item1;
			Resolution resolution = Resolutions.Find((Resolution x) => x.Width == widthByAspectRatio && x.Height == heightByAspectRatio);
			if (resolution == null)
			{
				resolution = Resolutions.LastOrDefault((Resolution x) => x.Height == optimalResolution.Height);
			}
			if (resolution != null)
			{
				CurrentResolution = resolution;
			}
		}
	}

	public class Processor
	{
		public string _manufacturer;

		public string _name;

		public string _caption;

		public uint _maxclock;

		public override string ToString()
		{
			return string.Concat(string.Concat(string.Concat(string.Concat("Processor" + Environment.NewLine, "Manufacturer: ", _manufacturer, Environment.NewLine), "Name: ", _name, Environment.NewLine), "Caption: ", _caption, Environment.NewLine), "Maxclock: ", _maxclock.ToString());
		}
	}

	public class Memory
	{
		public ulong _total;

		public ulong _avail;

		public override string ToString()
		{
			return "Total Memory: " + _total + Environment.NewLine + "Available Memory:" + _avail;
		}
	}

	public struct DetectQualityResult
	{
		public string GPU;

		public string CPU;

		public int cpu_mark;

		public int gpu_mark;

		public GameSettingsHolder.QualityType preferred_quality;

		public GameSettingsHolder.QualityType gpu_quality;

		public GameSettingsHolder.QualityType cpu_quality;

		public GameSettingsHolder.DLSSMode DLSSMode;

		public GameSettingsHolder.ReflexMode ReflexMode;

		public GameSettingsHolder.FrameGenerationMode FrameGenerationMode;

		public GameSettingsHolder.SuperResolutionMode SuperResolutionMode;

		public GameSettingsHolder.FSRMode FSRMode;

		public GameSettingsHolder.FSR2Mode FSR2Mode;

		public GameSettingsHolder.FFXFGMode FFXFGMode;

		public GameSettingsHolder.XeSSMode XeSSMode;

		public GameSettingsHolder.AntiAliasingMode AntiAliasingMode;

		public GameSettingsHolder.RayTracingMode RayTracingMode;
	}

	private const string HARDWARE_HOST = "http://cdn.fatsharkgames.se";

	public static readonly int MIN_REQ_CPU_MARK = 6000;

	public static readonly int MIN_REQ_GPU_MARK = 8000;

	private static readonly Resolution DEFAULT_RESOLUTION_FULL_HD = new Resolution(1920, 1080);

	private static readonly Resolution DEFAULT_RESOLUTION_2K = new Resolution(2560, 1440);

	private static readonly Resolution DEFAULT_RESOLUTION_4K = new Resolution(3840, 2160);

	private List<RangeQualitySettings> _cpuMarkRanges = new List<RangeQualitySettings>
	{
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.low,
			From = 0,
			To = 8000
		},
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.medium,
			From = 8001,
			To = 9500
		},
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.high,
			From = 9501,
			To = int.MaxValue
		}
	};

	private List<RangeQualitySettings> _gpuMarkRanges = new List<RangeQualitySettings>
	{
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.low,
			From = 0,
			To = 13500
		},
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.medium,
			From = 13501,
			To = 20000
		},
		new RangeQualitySettings
		{
			Quality = GameSettingsHolder.QualityType.high,
			From = 20001,
			To = int.MaxValue
		}
	};

	private static List<ResolutionQualitySetting> _screenResolutionRanges = new List<ResolutionQualitySetting>
	{
		new ResolutionQualitySetting
		{
			ResolutionInstance = DEFAULT_RESOLUTION_FULL_HD,
			From = 0,
			To = 24000
		},
		new ResolutionQualitySetting
		{
			ResolutionInstance = DEFAULT_RESOLUTION_2K,
			From = 24001,
			To = 30000
		},
		new ResolutionQualitySetting
		{
			ResolutionInstance = DEFAULT_RESOLUTION_4K,
			From = 30001,
			To = int.MaxValue
		}
	};

	private static List<RaytracingQualitySetting> _raytracingQualityRanges = new List<RaytracingQualitySetting>
	{
		new RaytracingQualitySetting
		{
			RaytracingMode = GameSettingsHolder.RayTracingMode.off,
			From = 0,
			To = 20000
		},
		new RaytracingQualitySetting
		{
			RaytracingMode = GameSettingsHolder.RayTracingMode.low,
			From = 20001,
			To = 24000
		},
		new RaytracingQualitySetting
		{
			RaytracingMode = GameSettingsHolder.RayTracingMode.medium,
			From = 24001,
			To = 30000
		},
		new RaytracingQualitySetting
		{
			RaytracingMode = GameSettingsHolder.RayTracingMode.high,
			From = 30001,
			To = int.MaxValue
		}
	};

	private List<FinalQualitySettings> _finalQualityType = new List<FinalQualitySettings>
	{
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.high,
			GPU = GameSettingsHolder.QualityType.high,
			Preset = GameSettingsHolder.QualityType.high
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.high,
			GPU = GameSettingsHolder.QualityType.medium,
			Preset = GameSettingsHolder.QualityType.medium
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.high,
			GPU = GameSettingsHolder.QualityType.low,
			Preset = GameSettingsHolder.QualityType.low
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.medium,
			GPU = GameSettingsHolder.QualityType.high,
			Preset = GameSettingsHolder.QualityType.high
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.medium,
			GPU = GameSettingsHolder.QualityType.medium,
			Preset = GameSettingsHolder.QualityType.medium
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.medium,
			GPU = GameSettingsHolder.QualityType.low,
			Preset = GameSettingsHolder.QualityType.low
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.low,
			GPU = GameSettingsHolder.QualityType.high,
			Preset = GameSettingsHolder.QualityType.medium
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.low,
			GPU = GameSettingsHolder.QualityType.medium,
			Preset = GameSettingsHolder.QualityType.medium
		},
		new FinalQualitySettings
		{
			CPU = GameSettingsHolder.QualityType.low,
			GPU = GameSettingsHolder.QualityType.low,
			Preset = GameSettingsHolder.QualityType.low
		}
	};

	public List<Output> _outputs;

	public List<Processor> _processors;

	public Memory _memory;

	public Output _primary_output;

	private bool _debugLog;

	public static bool IsWindowsMachine()
	{
		return RuntimeInformation.FrameworkDescription.Contains(".NET Framework");
	}

	public SysInfo(bool debugLog)
	{
		_outputs = new List<Output>();
		_processors = new List<Processor>();
		_memory = new Memory();
		_debugLog = debugLog;
	}

	public override string ToString()
	{
		string text = "";
		foreach (Output output in _outputs)
		{
			text = text + "AdapterId: " + output.AdapterId + " (" + output.AdapterName + ") OutputId: " + output.OutputId + " CurrentResolution: " + output.CurrentResolution.DisplayName + Environment.NewLine;
		}
		foreach (Processor processor in _processors)
		{
			text = text + processor.ToString() + Environment.NewLine;
		}
		return text + _memory;
	}

	private void GenerateFallbackOutput()
	{
		FileLogger.Instance.CreateMultiLineEntry("Generating fallback output");
		string unknownAdapter = "Unknown Adapter";
		Output output = new Output(0, 0, unknownAdapter);
		int height = SystemInformation.PrimaryMonitorSize.Height;
		int width = SystemInformation.PrimaryMonitorSize.Width;
		output.Resolutions.Add(new Resolution(width, height));
		output.CurrentResolution = output.Resolutions[0];
		_primary_output = output;
		if (!_outputs.Where((Output x) => x.AdapterName == unknownAdapter).Any())
		{
			_outputs.Add(output);
		}
	}

	public bool fetch_outputs(LauncherSettings launcherSettings = null)
	{
		int preferredAdapter = -1;
		try
		{
			int adapters = DirectXInterface.GetAdapters();
			var adapterDescriptions = Enumerable.Range(0, adapters).Select(idx => ((uint)idx, DirectXInterface.GetAdapterDescription((uint)idx))).ToArray();

			// Create cache for _outputs because DirectXInterface.GetSupportedFeatures is extremely heavy
			string outputVersion = null;
			 if (launcherSettings != null)
			{
				var outputVersionRaw = adapterDescriptions.Aggregate("", (acc, curr) =>
				{
					var (idx, desc) = curr;
					return $"{acc}[{idx}][{desc}]";
				});
				using (MD5 md5 = MD5.Create())
				{
					md5.Initialize();
					md5.ComputeHash(Encoding.UTF8.GetBytes(outputVersionRaw));
					outputVersion = BitConverter.ToString(md5.Hash).Replace("-", "");
				}
				if (launcherSettings.OutputVersion == outputVersion)
				{
					_outputs = launcherSettings.Outputs;
					_primary_output = _outputs[launcherSettings.PrimaryOutputId];
					return true;
				}
			}

			preferredAdapter = Task.Run(() => DirectXInterface.GetSystemPreferredAdapter(_debugLog)).Result;
			if (preferredAdapter != -1)
			{
				FileLogger.Instance.CreateEntry($"Preferred performance adapter is adapter #{preferredAdapter} ({DirectXInterface.GetAdapterDescription((uint)preferredAdapter)})");
			}
			else
			{
				preferredAdapter = 0;
				FileLogger.Instance.CreateEntry("!! Couldn't find preferred adapter! Defaulting to 0 (" + DirectXInterface.GetAdapterDescription((uint)preferredAdapter) + ") !!");
			}
			foreach (var (num, adapterDescription) in adapterDescriptions)
			{
				List<MonitorDescription> adapterMonitors = DirectXInterface.GetAdapterMonitors(num);
				if (adapterMonitors.Count > 0)
				{
					FileLogger.Instance.CreateEntry("Checking Supported Graphics Features...");
					DirectXInterface.SupportedFeatures supportedFeatures = DirectXInterface.GetSupportedFeatures(_debugLog);
					for (int i = 0; i < adapterMonitors.Count; i++)
					{
						Output output = new Output(i, (int)num, adapterDescription);
						for (int j = 0; j < adapterMonitors[i].DisplayModes.Count; j++)
						{
							output.Resolutions.Add(new Resolution((int)adapterMonitors[i].DisplayModes[j].Width, (int)adapterMonitors[i].DisplayModes[j].Height));
						}
						if (output.Resolutions.Count > 0)
						{
							int desktopWidth = (int)adapterMonitors[i].CurrentResolution.Width;
							int desktopHeight = (int)adapterMonitors[i].CurrentResolution.Height;
							output.CurrentResolution = output.Resolutions.Find((Resolution x) => x.Width == desktopWidth && x.Height == desktopHeight);
							if (output.CurrentResolution == null)
							{
								output.CurrentResolution = output.Resolutions[output.Resolutions.Count - 1];
							}
							output.AspectRatio = new MakeshiftTuplePair(16, desktopHeight * 16 / desktopWidth);
							output.SerializableFeatures = new SerializableSupportedFeatures(supportedFeatures);
							if (num == preferredAdapter && (i == 0 || _primary_output == null))
							{
								_primary_output = output;
							}
							_outputs.Add(output);
						}
						else
						{
							FileLogger.Instance.CreateEntry($"!! DirectX returns no valid display modes for monitor {i} on adapter {num} ({adapterDescription})");
						}
					}
				}
				else
				{
					FileLogger.Instance.CreateEntry($"!! DirectX returns no valid monitors attached for adapter {num} ({adapterDescription})");
				}
			}

			if (_outputs.Count > 0 && _primary_output != null && launcherSettings != null)
			{
				launcherSettings.OutputVersion = outputVersion;
				launcherSettings.Outputs = _outputs;
				launcherSettings.PrimaryOutputId = _primary_output.AdapterId;
				launcherSettings.Save();
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateMultiLineEntry("Error detecting display outputs: " + ex.ToString());
			if (_outputs.Count > 0)
			{
				_primary_output = _outputs.FirstOrDefault();
				if (_primary_output == null)
				{
					GenerateFallbackOutput();
				}
				return true;
			}
			GenerateFallbackOutput();
		}
		if (_primary_output == null)
		{
			if (preferredAdapter != -1)
			{
				string adapterDescription2 = DirectXInterface.GetAdapterDescription((uint)preferredAdapter);
				FileLogger.Instance.CreateMultiLineEntry($"?? Didn't find a valid display for adapter {preferredAdapter} ({adapterDescription2})?\nPotential strange dual-GPU laptop situation?\nDefaulting to current desktop resolution on preferred adapter.");
				Resolution resolution = new Resolution(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
				Output output2 = new Output(0, preferredAdapter, adapterDescription2)
				{
					CurrentResolution = resolution
				};
				output2.Resolutions.Add(resolution);
				_primary_output = output2;
				FileLogger.Instance.CreateEntry("Re-Checking Supported Graphics Features...");
				_primary_output.SerializableFeatures = new SerializableSupportedFeatures(DirectXInterface.GetSupportedFeatures(_debugLog));
				if (!_outputs.Where((Output x) => x.AdapterId == preferredAdapter).Any())
				{
					_outputs.Add(output2);
				}
			}
			else
			{
				GenerateFallbackOutput();
			}
		}
		return true;
	}

	public bool fetch_cpu_info()
	{
		// WMI doesn't work on Linux anyway so we can simply use Registry
		var cpuName = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0")?.GetValue("ProcessorNameString") as string;
		var processor = new Processor
		{
			_caption = "",
			_manufacturer = "",
			_maxclock = 0u,
			_name = cpuName ?? "UNKNOWN"
		};
		_processors.Add(processor);
		return true;
#if NEVER
		try
		{
			ManagementScope scope = new ManagementScope();
			ObjectQuery query = new ObjectQuery("SELECT * FROM WIN32_Processor");
			foreach (ManagementBaseObject item in new ManagementObjectSearcher(scope, query).Get())
			{
				Processor processor = new Processor();
				processor._caption = item["Caption"].ToString();
				processor._manufacturer = item["Manufacturer"].ToString();
				processor._maxclock = uint.Parse(item["MaxClockSpeed"].ToString());
				processor._name = item["Name"].ToString();
				_processors.Add(processor);
			}
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateMultiLineEntry("Error detecting CPU: " + ex.ToString());
			Processor processor2 = new Processor();
			processor2._caption = "";
			processor2._manufacturer = "";
			processor2._maxclock = 0u;
			processor2._name = "UNKNOWN";
			_processors.Add(processor2);
		}
		return true;
#endif
	}

	public bool fetch_memory_info()
	{
		MEMORYSTATUSEX mEMORYSTATUSEX = new MEMORYSTATUSEX();
		mEMORYSTATUSEX.dwLength = (uint)Marshal.SizeOf(mEMORYSTATUSEX);
		return false;
	}

	public async Task<DetectQualityResult> detect_quality()
	{
		DetectQualityResult result = default(DetectQualityResult);
		result.gpu_quality = GameSettingsHolder.QualityType.None;
		result.cpu_quality = GameSettingsHolder.QualityType.None;
		result.cpu_mark = 0;
		result.cpu_quality = GameSettingsHolder.QualityType.None;
		result.preferred_quality = GameSettingsHolder.DefaultQualityType;
		result.DLSSMode = GameSettingsHolder.DLSSMode.off;
		result.ReflexMode = GameSettingsHolder.ReflexMode.disabled;
		result.FrameGenerationMode = GameSettingsHolder.FrameGenerationMode.off;
		result.SuperResolutionMode = GameSettingsHolder.SuperResolutionMode.off;
		result.FSRMode = GameSettingsHolder.FSRMode.off;
		result.FSR2Mode = GameSettingsHolder.FSR2Mode.off;
		result.FFXFGMode = GameSettingsHolder.FFXFGMode.off;
		result.XeSSMode = GameSettingsHolder.XeSSMode.off;
		result.AntiAliasingMode = GameSettingsHolder.AntiAliasingMode.off;
		result.RayTracingMode = GameSettingsHolder.RayTracingMode.off;
		string text = _primary_output?.AdapterName;
		string name = _processors[0]._name;
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name))
		{
			return result;
		}
		result.GPU = text.Trim();
		result.CPU = name.Trim();
		try
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			int gpu_mark = await GetMarkFromHardwareDatabase("http://cdn.fatsharkgames.se/GPUHardwares.json", result.GPU);
			result.gpu_mark = gpu_mark;
			gpu_mark = await GetMarkFromHardwareDatabase("http://cdn.fatsharkgames.se/CPUHardwares.json", result.CPU);
			result.cpu_mark = gpu_mark;
			result.gpu_quality = GetHardwareQuality(_gpuMarkRanges, result.gpu_mark);
			result.cpu_quality = GetHardwareQuality(_cpuMarkRanges, result.cpu_mark);
			if (_primary_output.Features is DirectXInterface.NvidiaSupportedFeatures nvidiaSupportedFeatures)
			{
				if (nvidiaSupportedFeatures.Is4000Series())
				{
					result.gpu_quality = GameSettingsHolder.QualityType.high;
				}
				if (nvidiaSupportedFeatures.SupportsDLSS)
				{
					result.DLSSMode = GameSettingsHolder.DLSSMode.on;
					result.SuperResolutionMode = GameSettingsHolder.SuperResolutionMode.auto;
				}
				else
				{
					SetDefaultFSR();
				}
				if (nvidiaSupportedFeatures.SupportsReflex)
				{
					result.ReflexMode = GameSettingsHolder.ReflexMode.enabled;
				}
				if (nvidiaSupportedFeatures.SupportsDLSS_G)
				{
					result.FrameGenerationMode = GameSettingsHolder.FrameGenerationMode.on;
				}
				if (nvidiaSupportedFeatures.SupportsRaytracing)
				{
					if (nvidiaSupportedFeatures.Is4000Series())
					{
						result.RayTracingMode = GameSettingsHolder.RayTracingMode.high;
					}
					else
					{
						result.RayTracingMode = GetRaytracingQuality(result.gpu_mark);
					}
				}
			}
			else if (_primary_output.Features is DirectXInterface.AmdSupportedFeatures amdSupportedFeatures)
			{
				SetDefaultFSR();
				if (amdSupportedFeatures.SupportsRaytracing)
				{
					result.RayTracingMode = GetRaytracingQuality(result.gpu_mark);
				}
			}
			else if (_primary_output.Features is DirectXInterface.IntelSupportedFeatures)
			{
				SetDefaultXeSS();
			}
			else
			{
				SetDefaultFSR();
			}
			foreach (Output output in _outputs)
			{
				output.SetOptimalResolution(result.gpu_mark);
			}
			result.preferred_quality = GetPreferredQuality(result);
			FileLogger.Instance.CreateEntry($"Current output resolution: {_primary_output.CurrentResolution}");
			FileLogger.Instance.CreateEntry($"DLSS: {result.DLSSMode}");
			FileLogger.Instance.CreateEntry($"Super Resolution Mode: {result.SuperResolutionMode}");
			FileLogger.Instance.CreateEntry($"Frame Generation Mode: {result.FrameGenerationMode}");
			FileLogger.Instance.CreateEntry($"Reflex Mode: {result.ReflexMode}");
			FileLogger.Instance.CreateEntry($"FSR Mode: {result.FSRMode}");
			FileLogger.Instance.CreateEntry($"FSR 2 Mode: {result.FSR2Mode}");
			FileLogger.Instance.CreateEntry($"FFXFG Mode: {result.FFXFGMode}");
			FileLogger.Instance.CreateEntry($"Antialiasing: {result.AntiAliasingMode}");
			FileLogger.Instance.CreateEntry($"Raytracing Quality: {result.RayTracingMode}");
			FileLogger.Instance.CreateEntry($"GPU Quality: {result.gpu_quality}");
			FileLogger.Instance.CreateEntry($"CPU Quality: {result.cpu_quality}");
			FileLogger.Instance.CreateEntry($"GPU Mark: {result.gpu_mark}");
			FileLogger.Instance.CreateEntry($"CPU Mark: {result.cpu_mark}");
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"Error while detecting hardware quality : {arg}");
		}
		return result;
		void SetDefaultFSR()
		{
			if (result.gpu_mark < MIN_REQ_GPU_MARK)
			{
				result.FSRMode = GameSettingsHolder.FSRMode.quality;
				result.AntiAliasingMode = GameSettingsHolder.AntiAliasingMode.TAA;
			}
			else
			{
				result.FSR2Mode = GameSettingsHolder.FSR2Mode.balanced;
			}
		}
		void SetDefaultXeSS()
		{
			if (result.gpu_mark < MIN_REQ_GPU_MARK)
			{
				result.XeSSMode = GameSettingsHolder.XeSSMode.performance;
			}
			else
			{
				result.XeSSMode = GameSettingsHolder.XeSSMode.balanced;
			}
		}
	}

	private GameSettingsHolder.QualityType GetPreferredQuality(DetectQualityResult result)
	{
		if (result.cpu_quality == GameSettingsHolder.QualityType.None || result.gpu_quality == GameSettingsHolder.QualityType.None)
		{
			return GameSettingsHolder.DefaultQualityType;
		}
		return _finalQualityType.FirstOrDefault((FinalQualitySettings x) => x.CPU == result.cpu_quality && x.GPU == result.gpu_quality).Preset;
	}

	public async Task<int> GetMarkFromHardwareDatabase(string link, string hardwareName)
	{
		Dictionary<string, int?> partial;
		Dictionary<string, int?> full;
		try
		{
			FileLogger.Instance.CreateEntry("Getting hardware performance settings for hardware: " + hardwareName);
			WebClientWithAsyncTimeout client = new WebClientWithAsyncTimeout();
			FileLogger.Instance.CreateEntry("Fetching hardware information for " + hardwareName);
			Uri address = new Uri(link);
			string value = await client.DownloadStringTaskAsync(address, Settings.Default.ConnectionTimeoutSeconds * 1000);
			if (client.TimeoutOccurred)
			{
				FileLogger.Instance.CreateEntry($"Couldn't fetch correct hardware information from CDN within {Settings.Default.ConnectionTimeoutSeconds} seconds!");
				return -1;
			}
			List<Hardware> list = Json.Decode<List<Hardware>>(value);
			if (list == null)
			{
				FileLogger.Instance.CreateEntry("Hardware list for " + hardwareName + " was not found!");
				return -1;
			}
			partial = list.ToDictionary((Hardware x) => x.ID, (Hardware y) => y.Mark);
			full = new Dictionary<string, int?>();
			foreach (Hardware item in list)
			{
				foreach (string name in item.Names)
				{
					if (!full.ContainsKey(name))
					{
						full.Add(name, item.Mark);
					}
				}
			}
			int? num = GetMark();
			if (!num.HasValue)
			{
				FileLogger.Instance.CreateEntry("Could not detect hardware marks for " + hardwareName);
				return -1;
			}
			FileLogger.Instance.CreateEntry($"{hardwareName} hardware mark: {num}");
			return num.Value;
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Error while getting latest hardware list: " + ex);
			return -1;
		}
		int? GetMark()
		{
			if (partial.ContainsKey(hardwareName))
			{
				return partial[hardwareName];
			}
			if (full.ContainsKey(hardwareName))
			{
				return full[hardwareName];
			}
			return full.FirstOrDefault((KeyValuePair<string, int?> x) => x.Key.Contains(hardwareName)).Value;
		}
	}

	private GameSettingsHolder.RayTracingMode GetRaytracingQuality(int mark)
	{
		if (mark < 0)
		{
			return GameSettingsHolder.RayTracingMode.off;
		}
		return (from x in _raytracingQualityRanges
			where x.From <= mark && mark <= x.To
			select x.RaytracingMode).FirstOrDefault();
	}

	public GameSettingsHolder.QualityType GetHardwareQuality(List<RangeQualitySettings> qualityRange, int mark)
	{
		if (mark < 0 || qualityRange == null)
		{
			return GameSettingsHolder.QualityType.medium;
		}
		return (from x in qualityRange
			where x.From <= mark && mark <= x.To
			select x.Quality).FirstOrDefault();
	}

	private static float GetOSVersion()
	{
		try
		{
			object value = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("CurrentMajorVersionNumber");
			object value2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion").GetValue("CurrentVersion");
			return (value != null) ? float.Parse(value.ToString(), CultureInfo.InvariantCulture) : float.Parse(value2.ToString(), CultureInfo.InvariantCulture);
		}
		catch (Exception)
		{
			return 10f;
		}
	}

	public static bool IsWindowsVersionSupported()
	{
		return GetOSVersion() >= 10f;
	}
}
