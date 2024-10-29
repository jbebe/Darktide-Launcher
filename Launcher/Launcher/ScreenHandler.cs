using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Launcher;

public static class ScreenHandler
{
	public enum SnapMode
	{
		Centre,
		UpperLeft
	}

	private static SnapMode DEFAULT_SCREEN_SNAPMODE;

	public static Screen GetCurrentScreen(Window window)
	{
		return Screen.FromHandle(new WindowInteropHelper(window).Handle);
	}

	public static int SendToScreen(Window window, Screen target)
	{
		_ = GetCurrentScreen(window).DeviceName == target.DeviceName;
		placeOnScreen(window, target);
		return GetScreenIdx(target);
	}

	public static int SendToScreen(Window window, int id)
	{
		Screen screen = getScreen(id);
		return SendToScreen(window, screen);
	}

	public static int SendToPrimary(Window window)
	{
		Screen primaryScreen = Screen.PrimaryScreen;
		return SendToScreen(window, primaryScreen);
	}

	public static void SendToNext(Window window, int offset)
	{
		int id = (GetScreenIdx(GetCurrentScreen(window)) + offset) % Screen.AllScreens.Length;
		SendToScreen(window, getScreen(id));
	}

	private static void placeOnScreen(Window window, Screen target)
	{
		Rectangle workingArea = target.WorkingArea;
		window.WindowState = WindowState.Normal;
		switch (DEFAULT_SCREEN_SNAPMODE)
		{
		case SnapMode.Centre:
			DoCenter(window, workingArea);
			break;
		case SnapMode.UpperLeft:
			window.Left = workingArea.Left;
			window.Top = workingArea.Top;
			break;
		}
	}

	public static void DoCenterTop(Window window, Rectangle dest)
	{
		window.Left = (double)dest.Left + (double)dest.Width / 2.0 - window.Width / 2.0;
		window.Top = dest.Top + 50;
	}

	public static void DoCenterTop(Window window, Rectangle windowSize, Rectangle dest)
	{
		window.Left = (double)dest.Left + (double)dest.Width / 2.0 - (double)windowSize.Width / 2.0;
		window.Top = dest.Top + 50;
	}

	public static void DoCenter(Window window, Rectangle dest)
	{
		window.Left = (double)dest.Left + (double)dest.Width / 2.0 - window.Width / 2.0;
		window.Top = (double)dest.Top + (double)dest.Height / 2.0 - window.Height / 2.0;
	}

	public static void DoCenter(Window window, Window dest)
	{
		Rectangle dest2 = new Rectangle((int)dest.Left, (int)dest.Top, (int)dest.ActualWidth, (int)dest.ActualHeight);
		DoCenter(window, dest2);
	}

	public static void DoCenterTop(Window window, Window dest)
	{
		Rectangle dest2 = new Rectangle((int)dest.Left, (int)dest.Top, (int)dest.ActualWidth, (int)dest.ActualHeight);
		DoCenterTop(window, dest2);
	}

	public static void DoCenterTop(Window window, Rectangle windowSize, Window dest)
	{
		Rectangle dest2 = new Rectangle((int)dest.Left, (int)dest.Top, (int)dest.ActualWidth, (int)dest.ActualHeight);
		DoCenterTop(window, windowSize, dest2);
	}

	public static void DoCenterPrimaryScreen(Window window)
	{
		double dpiScaling = GetDpiScaling(window);
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		int num = (workingArea.Right - workingArea.Left) / 2;
		int num2 = (workingArea.Bottom - workingArea.Top) / 2;
		window.Left = (double)num * dpiScaling - window.Width / 2.0;
		window.Top = (double)num2 * dpiScaling - window.Height / 2.0;
	}

	public static bool IsInsideCurrentScreen(Window window)
	{
		Rectangle workingArea = GetCurrentScreen(window).WorkingArea;
		double dpiScaling = GetDpiScaling(window);
		if (window.Top / dpiScaling <= (double)workingArea.Top)
		{
			return false;
		}
		if (window.Top / dpiScaling + window.Height / dpiScaling >= (double)workingArea.Bottom)
		{
			return false;
		}
		if (window.Left / dpiScaling <= (double)workingArea.Left)
		{
			return false;
		}
		if (window.Left / dpiScaling + window.Width / dpiScaling >= (double)workingArea.Right)
		{
			return false;
		}
		return true;
	}

	public static int GetScreenIdx(Screen screen)
	{
		return Screen.AllScreens.ToList().FindIndex((Screen x) => x.DeviceName == screen.DeviceName);
	}

	private static Screen getScreen(int id)
	{
		Screen[] allScreens = Screen.AllScreens;
		if (id >= 0 && id < allScreens.Length)
		{
			return allScreens[id];
		}
		return Screen.PrimaryScreen;
	}

	private static double GetDpiScaling(Window window)
	{
		PresentationSource presentationSource = PresentationSource.FromVisual(window);
		if (presentationSource != null && presentationSource.CompositionTarget != null)
		{
			return presentationSource.CompositionTarget.TransformFromDevice.M11;
		}
		return 1.0;
	}
}
