using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct VersionInfo
{
	public const string Title = "Launcher for 'Warhammer 40'000: DARKTIDE'";

	public const string Company = "Fatshark AB";

	public const string Copyright = "(c) 2018 Games Workshop Limited and Fatshark AB. All rights reserved";

	public const string Description = "Launcher for 'Warhammer 40'000: DARKTIDE'";

	public const string Product = "Warhammer 40'000: DARKTIDE";

	public const string Trademark = "(c) 2018 Games Workshop Limited and Fatshark AB. All rights reserved";

	public const string Configuration = "BETA DEBUG";

	public const string Version = "1.0";

	public const string FileVersion = "1.0.380";

	public const string FileInformationalVersion = "1.0.380.0";
}
