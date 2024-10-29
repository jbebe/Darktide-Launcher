using System;

namespace Launcher;

[AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
public class ComMethodIdentifier : Attribute
{
	public uint Identifier { get; private set; }

	public ComMethodIdentifier(uint identifier)
	{
		Identifier = identifier;
	}
}
