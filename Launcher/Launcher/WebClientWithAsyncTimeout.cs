using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Launcher;

public class WebClientWithAsyncTimeout : WebClient
{
	private bool timeoutOccurred;

	private Timer timeoutTimer;

	public bool TimeoutOccurred
	{
		get
		{
			return timeoutOccurred;
		}
		private set
		{
			timeoutOccurred = value;
			if (timeoutTimer != null)
			{
				timeoutTimer.Stop();
				timeoutTimer.Dispose();
			}
		}
	}

	public Task<string> DownloadStringTaskAsync(Uri address, int timeout)
	{
		Task<string> result = DownloadStringTaskAsync(address);
		if (timeout > 0)
		{
			timeoutTimer = new Timer(timeout);
			timeoutTimer.Elapsed += TimeoutTimer_Elapsed;
			timeoutTimer.Start();
		}
		return result;
	}

	private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (sender is WebClientWithAsyncTimeout webClientWithAsyncTimeout)
		{
			webClientWithAsyncTimeout.TimeoutOccurred = true;
			webClientWithAsyncTimeout.CancelAsync();
		}
	}
}
