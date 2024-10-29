using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Launcher;

internal static class Animations
{
	public static DoubleAnimation Fade(double from, double to, double duration, bool autoReverse)
	{
		return new DoubleAnimation
		{
			From = from,
			To = to,
			Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
			AutoReverse = autoReverse
		};
	}
}
