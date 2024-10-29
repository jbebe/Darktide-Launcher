using System.Collections.Generic;
using System.Linq;

namespace Launcher;

public class MonitorDescription
{
	public string MonitorName = "unknown";

	public MonitorResolutionDescription CurrentResolution;

	public List<MonitorResolutionDescription> DisplayModes;

	public MonitorDescription()
	{
		DisplayModes = new List<MonitorResolutionDescription>();
		CurrentResolution = new MonitorResolutionDescription(0u, 0u);
	}

	public void AddResolution(uint width, uint height)
	{
		if (!DisplayModes.Any((MonitorResolutionDescription x) => x.Width == width && x.Height == height))
		{
			DisplayModes.Add(new MonitorResolutionDescription(width, height));
		}
	}
}
