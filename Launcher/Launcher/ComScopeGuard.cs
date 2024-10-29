using System;
using System.Runtime.InteropServices;

namespace Launcher;

internal class ComScopeGuard : IDisposable
{
	public IntPtr Ptr;

	public ComScopeGuard()
	{
	}

	public ComScopeGuard(IntPtr ptr)
	{
		Ptr = ptr;
	}

	public void Dispose()
	{
		if (Ptr != IntPtr.Zero)
		{
			Marshal.Release(Ptr);
			Ptr = IntPtr.Zero;
		}
	}

	public IntPtr Move()
	{
		IntPtr ptr = Ptr;
		Ptr = IntPtr.Zero;
		return ptr;
	}
}
