using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LauncherHelper;

namespace Launcher;

public static class DirectXInterface
{
	public struct LUID
	{
		public uint LowPart;

		public uint HighPart;
	}

	internal struct DXGIAdapterDescription1
	{
		public unsafe fixed char Description[128];

		public uint VendorId;

		public uint DeviceId;

		public uint SubSysId;

		public uint Revision;

		public UIntPtr DedicatedVideoMemory;

		public UIntPtr DedicatedSystemMemory;

		public UIntPtr SharedSystemMemory;

		public LUID AdapterLuid;

		public uint Flags;
	}

	public struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	internal struct DXGIOutputDescription
	{
		public unsafe fixed char Description[32];

		public Rect Rect;

		[MarshalAs(UnmanagedType.Bool)]
		public bool AttachedToDesktop;

		public uint Rotation;

		public IntPtr Monitor;
	}

	internal struct DXGIRational
	{
		public uint Numerator;

		public uint Denominator;
	}

	public enum DXGIFormat : uint
	{
		DXGI_FORMAT_UNKNOWN = 0u,
		DXGI_FORMAT_R32G32B32A32_TYPELESS = 1u,
		DXGI_FORMAT_R32G32B32A32_FLOAT = 2u,
		DXGI_FORMAT_R32G32B32A32_UINT = 3u,
		DXGI_FORMAT_R32G32B32A32_SINT = 4u,
		DXGI_FORMAT_R32G32B32_TYPELESS = 5u,
		DXGI_FORMAT_R32G32B32_FLOAT = 6u,
		DXGI_FORMAT_R32G32B32_UINT = 7u,
		DXGI_FORMAT_R32G32B32_SINT = 8u,
		DXGI_FORMAT_R16G16B16A16_TYPELESS = 9u,
		DXGI_FORMAT_R16G16B16A16_FLOAT = 10u,
		DXGI_FORMAT_R16G16B16A16_UNORM = 11u,
		DXGI_FORMAT_R16G16B16A16_UINT = 12u,
		DXGI_FORMAT_R16G16B16A16_SNORM = 13u,
		DXGI_FORMAT_R16G16B16A16_SINT = 14u,
		DXGI_FORMAT_R32G32_TYPELESS = 15u,
		DXGI_FORMAT_R32G32_FLOAT = 16u,
		DXGI_FORMAT_R32G32_UINT = 17u,
		DXGI_FORMAT_R32G32_SINT = 18u,
		DXGI_FORMAT_R32G8X24_TYPELESS = 19u,
		DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20u,
		DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21u,
		DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22u,
		DXGI_FORMAT_R10G10B10A2_TYPELESS = 23u,
		DXGI_FORMAT_R10G10B10A2_UNORM = 24u,
		DXGI_FORMAT_R10G10B10A2_UINT = 25u,
		DXGI_FORMAT_R11G11B10_FLOAT = 26u,
		DXGI_FORMAT_R8G8B8A8_TYPELESS = 27u,
		DXGI_FORMAT_R8G8B8A8_UNORM = 28u,
		DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29u,
		DXGI_FORMAT_R8G8B8A8_UINT = 30u,
		DXGI_FORMAT_R8G8B8A8_SNORM = 31u,
		DXGI_FORMAT_R8G8B8A8_SINT = 32u,
		DXGI_FORMAT_R16G16_TYPELESS = 33u,
		DXGI_FORMAT_R16G16_FLOAT = 34u,
		DXGI_FORMAT_R16G16_UNORM = 35u,
		DXGI_FORMAT_R16G16_UINT = 36u,
		DXGI_FORMAT_R16G16_SNORM = 37u,
		DXGI_FORMAT_R16G16_SINT = 38u,
		DXGI_FORMAT_R32_TYPELESS = 39u,
		DXGI_FORMAT_D32_FLOAT = 40u,
		DXGI_FORMAT_R32_FLOAT = 41u,
		DXGI_FORMAT_R32_UINT = 42u,
		DXGI_FORMAT_R32_SINT = 43u,
		DXGI_FORMAT_R24G8_TYPELESS = 44u,
		DXGI_FORMAT_D24_UNORM_S8_UINT = 45u,
		DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46u,
		DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47u,
		DXGI_FORMAT_R8G8_TYPELESS = 48u,
		DXGI_FORMAT_R8G8_UNORM = 49u,
		DXGI_FORMAT_R8G8_UINT = 50u,
		DXGI_FORMAT_R8G8_SNORM = 51u,
		DXGI_FORMAT_R8G8_SINT = 52u,
		DXGI_FORMAT_R16_TYPELESS = 53u,
		DXGI_FORMAT_R16_FLOAT = 54u,
		DXGI_FORMAT_D16_UNORM = 55u,
		DXGI_FORMAT_R16_UNORM = 56u,
		DXGI_FORMAT_R16_UINT = 57u,
		DXGI_FORMAT_R16_SNORM = 58u,
		DXGI_FORMAT_R16_SINT = 59u,
		DXGI_FORMAT_R8_TYPELESS = 60u,
		DXGI_FORMAT_R8_UNORM = 61u,
		DXGI_FORMAT_R8_UINT = 62u,
		DXGI_FORMAT_R8_SNORM = 63u,
		DXGI_FORMAT_R8_SINT = 64u,
		DXGI_FORMAT_A8_UNORM = 65u,
		DXGI_FORMAT_R1_UNORM = 66u,
		DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67u,
		DXGI_FORMAT_R8G8_B8G8_UNORM = 68u,
		DXGI_FORMAT_G8R8_G8B8_UNORM = 69u,
		DXGI_FORMAT_BC1_TYPELESS = 70u,
		DXGI_FORMAT_BC1_UNORM = 71u,
		DXGI_FORMAT_BC1_UNORM_SRGB = 72u,
		DXGI_FORMAT_BC2_TYPELESS = 73u,
		DXGI_FORMAT_BC2_UNORM = 74u,
		DXGI_FORMAT_BC2_UNORM_SRGB = 75u,
		DXGI_FORMAT_BC3_TYPELESS = 76u,
		DXGI_FORMAT_BC3_UNORM = 77u,
		DXGI_FORMAT_BC3_UNORM_SRGB = 78u,
		DXGI_FORMAT_BC4_TYPELESS = 79u,
		DXGI_FORMAT_BC4_UNORM = 80u,
		DXGI_FORMAT_BC4_SNORM = 81u,
		DXGI_FORMAT_BC5_TYPELESS = 82u,
		DXGI_FORMAT_BC5_UNORM = 83u,
		DXGI_FORMAT_BC5_SNORM = 84u,
		DXGI_FORMAT_B5G6R5_UNORM = 85u,
		DXGI_FORMAT_B5G5R5A1_UNORM = 86u,
		DXGI_FORMAT_B8G8R8A8_UNORM = 87u,
		DXGI_FORMAT_B8G8R8X8_UNORM = 88u,
		DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89u,
		DXGI_FORMAT_B8G8R8A8_TYPELESS = 90u,
		DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91u,
		DXGI_FORMAT_B8G8R8X8_TYPELESS = 92u,
		DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93u,
		DXGI_FORMAT_BC6H_TYPELESS = 94u,
		DXGI_FORMAT_BC6H_UF16 = 95u,
		DXGI_FORMAT_BC6H_SF16 = 96u,
		DXGI_FORMAT_BC7_TYPELESS = 97u,
		DXGI_FORMAT_BC7_UNORM = 98u,
		DXGI_FORMAT_BC7_UNORM_SRGB = 99u,
		DXGI_FORMAT_AYUV = 100u,
		DXGI_FORMAT_Y410 = 101u,
		DXGI_FORMAT_Y416 = 102u,
		DXGI_FORMAT_NV12 = 103u,
		DXGI_FORMAT_P010 = 104u,
		DXGI_FORMAT_P016 = 105u,
		DXGI_FORMAT_420_OPAQUE = 106u,
		DXGI_FORMAT_YUY2 = 107u,
		DXGI_FORMAT_Y210 = 108u,
		DXGI_FORMAT_Y216 = 109u,
		DXGI_FORMAT_NV11 = 110u,
		DXGI_FORMAT_AI44 = 111u,
		DXGI_FORMAT_IA44 = 112u,
		DXGI_FORMAT_P8 = 113u,
		DXGI_FORMAT_A8P8 = 114u,
		DXGI_FORMAT_B4G4R4A4_UNORM = 115u,
		DXGI_FORMAT_P208 = 130u,
		DXGI_FORMAT_V208 = 131u,
		DXGI_FORMAT_V408 = 132u,
		DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE = 133u,
		DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE = 134u,
		DXGI_FORMAT_FORCE_UINT = uint.MaxValue
	}

	public enum DXGIModeScanlineOrder : uint
	{
		DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED,
		DXGI_MODE_SCANLINE_ORDER_PROGRESSIVE,
		DXGI_MODE_SCANLINE_ORDER_UPPER_FIELD_FIRST,
		DXGI_MODE_SCANLINE_ORDER_LOWER_FIELD_FIRST
	}

	public enum DXGIModeScaling : uint
	{
		DXGI_MODE_SCALING_UNSPECIFIED,
		DXGI_MODE_SCALING_CENTERED,
		DXGI_MODE_SCALING_STRETCHED
	}

	public enum DXGIEnumModes : uint
	{
		DXGI_ENUM_MODES_INTERLACED = 1u,
		DXGI_ENUM_MODES_SCALING = 2u,
		DXGI_ENUM_MODES_STEREO = 4u,
		DXGI_ENUM_MODES_DISABLED_STEREO = 8u
	}

	internal struct DXGIModeDescription1
	{
		public uint Width;

		public uint Height;

		public DXGIRational RefreshRate;

		public DXGIFormat Format;

		public DXGIModeScanlineOrder ScanlineOrdering;

		public DXGIModeScaling Scaling;

		public int Stereo;
	}

	internal static class Factory
	{
		[ComMethodIdentifier(7u)]
		public delegate uint EnumAdaptersDelegate(IntPtr @this, uint index, out IntPtr r);

		public static EnumAdaptersDelegate EnumAdapters = CalliILBytecodeGenerator.GetCalliDelegate<EnumAdaptersDelegate>();
	}

	internal static class Adapter
	{
		[ComMethodIdentifier(7u)]
		public delegate uint EnumOutputsDelegate(IntPtr @this, uint index, out IntPtr r);

		[ComMethodIdentifier(8u)]
		public unsafe delegate uint GetDescDelegate(IntPtr @this, void* desc);

		[ComMethodIdentifier(10u)]
		public unsafe delegate int GetDesc1Delegate(IntPtr thisPtr, void* adapterDescription1);

		public static EnumOutputsDelegate EnumOutputs = CalliILBytecodeGenerator.GetCalliDelegate<EnumOutputsDelegate>();

		public static GetDescDelegate GetDesc = CalliILBytecodeGenerator.GetCalliDelegate<GetDescDelegate>();

		public static GetDesc1Delegate GetDesc1 = CalliILBytecodeGenerator.GetCalliDelegate<GetDesc1Delegate>();
	}

	internal static class Output
	{
		[ComMethodIdentifier(7u)]
		public unsafe delegate int GetMonitorDelegate(IntPtr thisPtr, void* monitorDescription);

		[ComMethodIdentifier(19u)]
		public unsafe delegate int GetDisplayModeList1Delegate(IntPtr thisPtr, DXGIFormat format, DXGIEnumModes flags, ref uint numModes, DXGIModeDescription1* modesDescriptions);

		public static GetMonitorDelegate GetMonitor = CalliILBytecodeGenerator.GetCalliDelegate<GetMonitorDelegate>();

		public static GetDisplayModeList1Delegate GetDisplayModeList1 = CalliILBytecodeGenerator.GetCalliDelegate<GetDisplayModeList1Delegate>();
	}

	public abstract class SupportedFeatures
	{
		public bool SupportsRaytracing;
	}

	public class DefaultSupportedFeatures : SupportedFeatures
	{
	}

	public class IntelSupportedFeatures : SupportedFeatures
	{
		public IntelSupportedFeatures()
		{
			SupportsRaytracing = false;
		}
	}

	public class AmdSupportedFeatures : SupportedFeatures
	{
		public AmdSupportedFeatures()
		{
			SupportsRaytracing = false;
		}
	}

	public class NvidiaSupportedFeatures : SupportedFeatures
	{
		public bool SupportsDLSS;

		public bool SupportsDLSS_G;

		public bool SupportsReflex;

		public bool DisabledDLSS_G;

		public RayTracingCapabilities RayTracingCaps;

		public DLSSCapabilities DLSSCaps;

		public Nvidia_GPU_Architecture Architecture;

		public bool Is4000Series()
		{
			return Architecture == Nvidia_GPU_Architecture.NV_GPU_ARCHITECTURE_AD100;
		}

		public NvidiaSupportedFeatures()
		{
			SupportsDLSS = false;
			SupportsDLSS_G = false;
			SupportsReflex = false;
			DisabledDLSS_G = false;
			RayTracingCaps = RayTracingCapabilities.NO_RAY_TRACING;
			DLSSCaps = DLSSCapabilities.NO_DLSS;
			SupportsRaytracing = false;
			Architecture = Nvidia_GPU_Architecture.NV_GPU_ARCHITECTURE_FAIL;
		}
	}

	public enum RayTracingCapabilities : uint
	{
		NO_RAY_TRACING,
		RAY_TRACING_LEVEL_1,
		RAY_TRACING_LEVEL_2,
		RAY_TRACING_LEVEL_3
	}

	public enum DLSSCapabilities : uint
	{
		NO_DLSS,
		DLSS_2,
		DLSS_3
	}

	private enum DlssFeature : uint
	{
		eFeatureDLSS = 0u,
		eFeatureNRD = 1u,
		eFeatureNIS = 2u,
		eFeatureReflex = 3u,
		eFeatureDebug = 5u,
		eFeatureDLSS_G = 1000u,
		eFeatureCommon = uint.MaxValue
	}

	public enum Nvidia_GPU_Architecture : uint
	{
		NV_GPU_ARCHITECTURE_FAIL = 0u,
		NV_GPU_ARCHITECTURE_T2X = 3758096416u,
		NV_GPU_ARCHITECTURE_T3X = 3758096432u,
		NV_GPU_ARCHITECTURE_T4X = 3758096448u,
		NV_GPU_ARCHITECTURE_T12X = 3758096448u,
		NV_GPU_ARCHITECTURE_NV40 = 64u,
		NV_GPU_ARCHITECTURE_NV50 = 80u,
		NV_GPU_ARCHITECTURE_G78 = 96u,
		NV_GPU_ARCHITECTURE_G80 = 128u,
		NV_GPU_ARCHITECTURE_G90 = 144u,
		NV_GPU_ARCHITECTURE_GT200 = 160u,
		NV_GPU_ARCHITECTURE_GF100 = 192u,
		NV_GPU_ARCHITECTURE_GF110 = 208u,
		NV_GPU_ARCHITECTURE_GK100 = 224u,
		NV_GPU_ARCHITECTURE_GK110 = 240u,
		NV_GPU_ARCHITECTURE_GK200 = 256u,
		NV_GPU_ARCHITECTURE_GM000 = 272u,
		NV_GPU_ARCHITECTURE_GM200 = 288u,
		NV_GPU_ARCHITECTURE_GP100 = 304u,
		NV_GPU_ARCHITECTURE_GV100 = 320u,
		NV_GPU_ARCHITECTURE_GV110 = 336u,
		NV_GPU_ARCHITECTURE_TU100 = 352u,
		NV_GPU_ARCHITECTURE_GA100 = 368u,
		NV_GPU_ARCHITECTURE_AD100 = 400u
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct PHYSICAL_MONITOR
	{
		public IntPtr hPhysicalMonitor;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string szPhysicalMonitorDescription;
	}

	private static uint DXGI_ERROR_NOT_FOUND = 2289696770u;

	private static Dictionary<uint, DXGIAdapterDescription1> foundAdapters = new Dictionary<uint, DXGIAdapterDescription1>();

	private static Dictionary<uint, List<MonitorDescription>> foundMonitorsPerAdapter = new Dictionary<uint, List<MonitorDescription>>();

	[DllImport("DXGI.dll")]
	private static extern uint CreateDXGIFactory(IntPtr guid, out IntPtr r);

	private unsafe static bool GetAdapterDesc1(IntPtr adapter, out DXGIAdapterDescription1 desc)
	{
		fixed (DXGIAdapterDescription1* adapterDescription = &desc)
		{
			return Adapter.GetDesc1(adapter, adapterDescription) == 0;
		}
	}

	private unsafe static bool GetMonitorDesc(IntPtr monitor, out DXGIOutputDescription desc)
	{
		fixed (DXGIOutputDescription* monitorDescription = &desc)
		{
			return Output.GetMonitor(monitor, monitorDescription) == 0;
		}
	}

	private unsafe static bool GetMonitorDisplayModeList(IntPtr monitor, DXGIFormat format, DXGIEnumModes flags, ref uint numModes, out DXGIModeDescription1[] desc)
	{
		int num = 0;
		uint numModes2 = 0u;
		DXGIModeDescription1* modesDescriptions = null;
		num = Output.GetDisplayModeList1(monitor, format, flags, ref numModes2, modesDescriptions);
		desc = new DXGIModeDescription1[numModes2];
		if (num == 0)
		{
			fixed (DXGIModeDescription1* modesDescriptions2 = &desc[0])
			{
				numModes = numModes2;
				num = Output.GetDisplayModeList1(monitor, format, flags, ref numModes, modesDescriptions2);
				return num == 0;
			}
		}
		return num == 0;
	}

	private unsafe static string GetString(char* chars, int size)
	{
		byte[] array = new byte[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = ((byte*)chars)[i];
		}
		return Encoding.Unicode.GetString(array).Trim(default(char));
	}

	public static async Task<LUID> GetPreferredAdapter(bool debugLog)
	{
		FileLogger.Instance.CreateEntry("Getting preferred adapter");
		LUID result = default(LUID);
		result.LowPart = 0u;
		result.HighPart = 0u;
		try
		{
			Process process = new Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Minimized,
				CreateNoWindow = true,
				FileName = "GPUDetection.exe",
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			string outputData = "";
			FileLogger.Instance.CreateEntry("Executing " + processStartInfo.FileName);
			process.StartInfo = processStartInfo;
			process.OutputDataReceived += delegate(object send, DataReceivedEventArgs args)
			{
				if (string.IsNullOrEmpty(args.Data))
				{
					return;
				}
				string data = args.Data;
				outputData = outputData + "\n" + data;
				try
				{
					if (data.StartsWith("<") && data.Contains(">"))
					{
						string[] array = data.Replace("<", "").Split('>');
						if (array.Length == 2)
						{
							string text = array[0];
							if (!(text == "LUIDLow"))
							{
								if (text == "LUIDHigh")
								{
									result.HighPart = Convert.ToUInt32(array[1]);
								}
							}
							else
							{
								result.LowPart = Convert.ToUInt32(array[1]);
							}
						}
					}
				}
				catch (Exception arg2)
				{
					FileLogger.Instance.CreateEntry($"Exception while getting adapter:\n{arg2}");
				}
			};
			if (debugLog)
			{
				FileLogger.Instance.CreateEntry("[DEBUG] Data from output: " + outputData);
			}
			process.Start();
			process.BeginOutputReadLine();
			await Task.Run(delegate
			{
				process.WaitForExit();
			});
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"Exception while trying to run detection:\n{arg}");
		}
		return result;
	}

	private static async Task<SupportedFeatures> GetGPUFeatures(bool debugLog)
	{
		SupportedFeatures supportedFeatures = new DefaultSupportedFeatures();
		try
		{
			FileLogger.Instance.CreateEntry("Running gpu detection");
			FileLogger.Instance.PushScope("GPU Detection");
			Process process = new Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Minimized,
				CreateNoWindow = true,
				FileName = "GPUDetection.exe",
				Arguments = "get_gpu_features",
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			FileLogger.Instance.CreateEntry("Executing " + processStartInfo.FileName);
			string outputData = "";
			process.StartInfo = processStartInfo;
			process.OutputDataReceived += delegate(object send, DataReceivedEventArgs args)
			{
				if (string.IsNullOrEmpty(args.Data))
				{
					return;
				}
				string data = args.Data;
				outputData = outputData + "\n" + data;
				try
				{
					if (data.StartsWith("<") && data.Contains('>'))
					{
						string[] array = data.Replace("<", "").Split('>');
						if (array.Length == 2)
						{
							string text = array[0];
							string text2 = array[1];
							if (text == "gpu_vendor")
							{
								switch (text2)
								{
								case "NVIDIA":
									supportedFeatures = new NvidiaSupportedFeatures();
									break;
								case "AMD":
									supportedFeatures = new AmdSupportedFeatures();
									break;
								case "INTEL":
									supportedFeatures = new IntelSupportedFeatures();
									break;
								default:
									supportedFeatures = new DefaultSupportedFeatures();
									break;
								}
							}
							if (supportedFeatures is NvidiaSupportedFeatures nvidiaSupportedFeatures)
							{
								switch (text)
								{
								case "Architecture":
								{
									if (Enum.TryParse<Nvidia_GPU_Architecture>(text2, out var result2))
									{
										nvidiaSupportedFeatures.Architecture = result2;
									}
									break;
								}
								case "SupportsDLSS":
									nvidiaSupportedFeatures.SupportsDLSS = Convert.ToBoolean(Convert.ToInt32(text2));
									break;
								case "SupportsDLSS_G":
									nvidiaSupportedFeatures.SupportsDLSS_G = Convert.ToBoolean(Convert.ToInt32(text2));
									if (nvidiaSupportedFeatures.Is4000Series())
									{
										nvidiaSupportedFeatures.DisabledDLSS_G = true;
									}
									break;
								case "SupportsReflex":
									nvidiaSupportedFeatures.SupportsReflex = Convert.ToBoolean(Convert.ToInt16(text2));
									break;
								case "DLSSVersion":
								{
									if (Enum.TryParse<DLSSCapabilities>(text2, out var result3))
									{
										nvidiaSupportedFeatures.DLSSCaps = result3;
									}
									break;
								}
								case "RaytracingCaps":
								{
									if (Enum.TryParse<RayTracingCapabilities>(text2, out var result))
									{
										nvidiaSupportedFeatures.RayTracingCaps = result;
									}
									break;
								}
								case "DisabledDLSS_G":
									nvidiaSupportedFeatures.DisabledDLSS_G = Convert.ToBoolean(Convert.ToInt32(text2));
									break;
								}
							}
							if (text == "SupportsRaytracing")
							{
								supportedFeatures.SupportsRaytracing = Convert.ToBoolean(Convert.ToInt16(text2));
							}
						}
					}
				}
				catch (Exception arg)
				{
					FileLogger.Instance.CreateEntry($"Exception while parsing data:\n{arg}");
				}
			};
			process.Start();
			process.BeginOutputReadLine();
			await Task.Run(delegate
			{
				process.WaitForExit();
			});
			if (process.ExitCode != 0)
			{
				FileLogger.Instance.CreateEntry($"Something went wrong in the GPU Detection process. Exit Code = {process.ExitCode}.\nData from output: {outputData}");
			}
			if (debugLog)
			{
				FileLogger.Instance.CreateEntry("[DEBUG] Data from output: " + outputData);
			}
			FileLogger.Instance.PopScope();
		}
		catch (Exception ex)
		{
			FileLogger.Instance.CreateEntry("Exception was thrown when attempting to run GPUDetection.exe. Error: " + ex.Message);
			FileLogger.Instance.PopScope();
			return supportedFeatures;
		}
		FileLogger.Instance.CreateEntry("GPU detection completed.");
		return supportedFeatures;
	}

	public static async Task<int> GetSystemPreferredAdapter(bool debugLog)
	{
		int result = -1;
		try
		{
			LUID lUID = default(LUID);
			lUID.HighPart = 0u;
			lUID.LowPart = 0u;
			result = GetPreferredAdapterIndex(await GetPreferredAdapter(debugLog));
		}
		catch
		{
		}
		return result;
	}

	public static SupportedFeatures GetSupportedFeatures(bool debugLog)
	{
		SupportedFeatures supportedFeatures = new DefaultSupportedFeatures();
		try
		{
			supportedFeatures = Task.Run(() => GetGPUFeatures(debugLog)).Result;
		}
		catch (Exception arg)
		{
			FileLogger.Instance.CreateEntry($"Exception while checking if hardware supports NVIDIA features:\n{arg}");
		}
		if (supportedFeatures is NvidiaSupportedFeatures nvidiaSupportedFeatures)
		{
			FileLogger.Instance.CreateEntry("NVIDIA GPU");
			FileLogger.Instance.CreateEntry($"DLSS Capability: {nvidiaSupportedFeatures.DLSSCaps}");
			FileLogger.Instance.CreateEntry($"RayTracing Capability: {nvidiaSupportedFeatures.RayTracingCaps}");
			FileLogger.Instance.CreateEntry($"Supports DLSS: {nvidiaSupportedFeatures.SupportsDLSS}");
			FileLogger.Instance.CreateEntry($"Supports Frame Generation: {nvidiaSupportedFeatures.SupportsDLSS_G}");
			FileLogger.Instance.CreateEntry($"Supports Reflex: {nvidiaSupportedFeatures.SupportsReflex}");
			if (nvidiaSupportedFeatures.DisabledDLSS_G)
			{
				FileLogger.Instance.CreateEntry("Hardware accelerated GPU Sheduling not available!");
			}
		}
		else if (supportedFeatures is AmdSupportedFeatures amdSupportedFeatures)
		{
			FileLogger.Instance.CreateEntry("AMD GPU");
			FileLogger.Instance.CreateEntry($"Supports Raytracing: {amdSupportedFeatures.SupportsRaytracing}");
		}
		else if (supportedFeatures is IntelSupportedFeatures intelSupportedFeatures)
		{
			FileLogger.Instance.CreateEntry("INTEL GPU");
			FileLogger.Instance.CreateEntry($"Supports Raytracing: {intelSupportedFeatures.SupportsRaytracing}");
		}
		else
		{
			FileLogger.Instance.CreateEntry("Other GPU");
		}
		return supportedFeatures;
	}

	[DllImport("dxva2.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

	[DllImport("dxva2.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

	public unsafe static int GetAdapters()
	{
		foundAdapters.Clear();
		using (ComScopeGuard comScopeGuard = new ComScopeGuard())
		{
			uint num = CreateDXGIFactory(DXGIGuids.Factory, out comScopeGuard.Ptr);
			if (num != 0)
			{
				throw new Exception("Failed to initialize DirectX Graphical Interface: error code " + num.ToString("X"));
			}
			uint num2 = 0u;
			while (num != DXGI_ERROR_NOT_FOUND)
			{
				using ComScopeGuard comScopeGuard2 = new ComScopeGuard();
				num = Factory.EnumAdapters(comScopeGuard.Ptr, num2, out comScopeGuard2.Ptr);
				if (num == 0 && GetAdapterDesc1(comScopeGuard2.Ptr, out var desc))
				{
					if ((desc.Flags & 1) == 0 && (desc.Flags & 2) == 0)
					{
						foundAdapters.Add(num2, desc);
						uint num3 = 0u;
						num = 0u;
						while (num != DXGI_ERROR_NOT_FOUND)
						{
							using (ComScopeGuard comScopeGuard3 = new ComScopeGuard())
							{
								num = Adapter.EnumOutputs(comScopeGuard2.Ptr, num3, out comScopeGuard3.Ptr);
								if (num == 0 && GetMonitorDesc(comScopeGuard3.Ptr, out var desc2))
								{
									uint pdwNumberOfPhysicalMonitors = 0u;
									if (GetNumberOfPhysicalMonitorsFromHMONITOR(desc2.Monitor, ref pdwNumberOfPhysicalMonitors) && pdwNumberOfPhysicalMonitors != 0)
									{
										PHYSICAL_MONITOR[] array = new PHYSICAL_MONITOR[pdwNumberOfPhysicalMonitors];
										if (GetPhysicalMonitorsFromHMONITOR(desc2.Monitor, pdwNumberOfPhysicalMonitors, array))
										{
											if (!foundMonitorsPerAdapter.ContainsKey(num2))
											{
												foundMonitorsPerAdapter.Add(num2, new List<MonitorDescription>());
											}
											MonitorDescription monitorDescription = new MonitorDescription
											{
												MonitorName = array[0].szPhysicalMonitorDescription,
												CurrentResolution = new MonitorResolutionDescription((uint)Math.Abs(desc2.Rect.Right - desc2.Rect.Left), (uint)Math.Abs(desc2.Rect.Top - desc2.Rect.Bottom))
											};
											uint numModes = 0u;
											if (GetMonitorDisplayModeList(comScopeGuard3.Ptr, DXGIFormat.DXGI_FORMAT_R8G8B8A8_UNORM, DXGIEnumModes.DXGI_ENUM_MODES_INTERLACED, ref numModes, out var desc3))
											{
												for (int i = 0; i < numModes; i++)
												{
													monitorDescription.AddResolution(desc3[i].Width, desc3[i].Height);
												}
											}
											if (monitorDescription.DisplayModes.Count == 0)
											{
												FileLogger.Instance.CreateEntry($"!! Inserting fallback resolution for adapter {num2} output {num3}!");
												monitorDescription.AddResolution(monitorDescription.CurrentResolution.Width, monitorDescription.CurrentResolution.Height);
											}
											FileLogger.Instance.CreateEntry($"Found Physical HMONITOR Monitor {num3} ({monitorDescription.MonitorName}) supporting {monitorDescription.DisplayModes.Count} resolutions");
											foundMonitorsPerAdapter[num2].Add(monitorDescription);
										}
										else if (desc2.AttachedToDesktop)
										{
											if (!foundMonitorsPerAdapter.ContainsKey(num2))
											{
												foundMonitorsPerAdapter.Add(num2, new List<MonitorDescription>());
											}
											MonitorDescription monitorDescription2 = new MonitorDescription
											{
												MonitorName = GetString(desc2.Description, 64),
												CurrentResolution = new MonitorResolutionDescription((uint)Math.Abs(desc2.Rect.Right - desc2.Rect.Left), (uint)Math.Abs(desc2.Rect.Top - desc2.Rect.Bottom))
											};
											uint numModes2 = 0u;
											if (GetMonitorDisplayModeList(comScopeGuard3.Ptr, DXGIFormat.DXGI_FORMAT_R8G8B8A8_UNORM, DXGIEnumModes.DXGI_ENUM_MODES_INTERLACED, ref numModes2, out var desc4))
											{
												for (int j = 0; j < numModes2; j++)
												{
													monitorDescription2.AddResolution(desc4[j].Width, desc4[j].Height);
												}
											}
											if (monitorDescription2.DisplayModes.Count == 0)
											{
												FileLogger.Instance.CreateEntry($"!! Inserting fallback resolution for adapter {num2} output {num3}!");
												monitorDescription2.AddResolution(monitorDescription2.CurrentResolution.Width, monitorDescription2.CurrentResolution.Height);
											}
											FileLogger.Instance.CreateEntry($"Found Physical DXGI Monitor {num3} ({monitorDescription2.MonitorName}) supporting {monitorDescription2.DisplayModes.Count} resolutions");
											foundMonitorsPerAdapter[num2].Add(monitorDescription2);
										}
										else
										{
											FileLogger.Instance.CreateEntry($"!! There was a problem getting information for the physical monitor for adapter {num2} output {num3}!");
										}
									}
									else
									{
										FileLogger.Instance.CreateEntry($"!! Windows returned no physical monitors for adapter {num2} output {num3}!");
									}
								}
							}
							num3++;
						}
						num = 0u;
					}
					else
					{
						string text = "";
						if ((desc.Flags & (true ? 1u : 0u)) != 0)
						{
							text += "[Reserved Remote]";
						}
						if ((desc.Flags & 2u) != 0)
						{
							text += "[Software Adapter]";
						}
						FileLogger.Instance.CreateEntry($"!! Skipping adapter {num2}: {text}!");
					}
				}
				num2++;
			}
		}
		return foundAdapters.Count;
	}

	public unsafe static string GetAdapterDescription(uint index)
	{
		if (foundAdapters.ContainsKey(index))
		{
			DXGIAdapterDescription1 dXGIAdapterDescription = foundAdapters[index];
			return GetString(dXGIAdapterDescription.Description, 256);
		}
		return "UNKNOWN ADAPTER";
	}

	public static List<MonitorDescription> GetAdapterMonitors(uint index)
	{
		if (foundMonitorsPerAdapter.ContainsKey(index))
		{
			return foundMonitorsPerAdapter[index];
		}
		return new List<MonitorDescription>();
	}

	public static int GetPreferredAdapterIndex(LUID wantedLUID)
	{
		int result = -1;
		for (uint num = 0u; num < foundAdapters.Count; num++)
		{
			if (foundAdapters[num].AdapterLuid.LowPart == wantedLUID.LowPart && foundAdapters[num].AdapterLuid.HighPart == wantedLUID.HighPart)
			{
				result = (int)num;
			}
		}
		return result;
	}
}
