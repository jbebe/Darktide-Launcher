using System;
using System.Runtime.InteropServices;

namespace Launcher;

internal static class DXGIGuids
{
	public static readonly IntPtr Factory = Allocate("7b7166ec-21c7-44ae-b21a-c9ae321ae369");

	public static readonly IntPtr Output = Allocate("ae02eedb-c735-4690-8d52-5a8dc20213aa");

	public static readonly IntPtr Adapter = Allocate("29038f61-3839-4626-91fd-086879011a05");

	private static IntPtr Allocate(string guid)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(16);
		Marshal.StructureToPtr(new Guid(guid), intPtr, fDeleteOld: false);
		return intPtr;
	}
}
