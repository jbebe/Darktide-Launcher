using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Launcher;

internal class CalliILBytecodeGenerator
{
	public unsafe static TDelegate GetCalliDelegate<TDelegate>() where TDelegate : class
	{
		Type typeFromHandle = typeof(TDelegate);
		MethodInfo method = typeFromHandle.GetMethod("Invoke");
		uint identifier = (typeFromHandle.GetCustomAttribute<ComMethodIdentifier>() ?? throw new Exception($"{typeFromHandle} isn't defined correctly!")).Identifier;
		ScanParameter(method.GetParameters(), out var invokeTypes, out var calliTypes, out var lastRefIntPtr);
		DynamicMethod dynamicMethod = new DynamicMethod("CalliInvoke", method.ReturnType, invokeTypes, typeof(CalliILBytecodeGenerator).Module, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.DeclareLocal(typeof(IntPtr*), pinned: true);
		if (lastRefIntPtr != -1)
		{
			iLGenerator.Emit(OpCodes.Ldarg, lastRefIntPtr);
			iLGenerator.Emit(OpCodes.Stloc_0);
		}
		GeneratePushArguments(iLGenerator, calliTypes.Length, lastRefIntPtr, -1);
		GenerateGetVTable(iLGenerator, identifier);
		iLGenerator.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, method.ReturnType, calliTypes);
		iLGenerator.Emit(OpCodes.Ret);
		return (TDelegate)(object)dynamicMethod.CreateDelegate(typeFromHandle);
	}

	private static void ScanParameter(ParameterInfo[] parameters, out Type[] invokeTypes, out Type[] calliTypes, out int lastRefIntPtr)
	{
		invokeTypes = new Type[parameters.Length];
		calliTypes = new Type[parameters.Length];
		lastRefIntPtr = -1;
		for (int i = 0; i < parameters.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			invokeTypes[i] = parameterType;
			calliTypes[i] = GetPointerTypeIfReference(parameterType);
			if (parameterType.IsByRef && parameterType.GetElementType() == typeof(IntPtr))
			{
				lastRefIntPtr = i;
			}
		}
	}

	private static Type GetPointerTypeIfReference(Type type)
	{
		if (type.IsByRef)
		{
			return type.GetElementType().MakePointerType();
		}
		return type;
	}

	private static void GeneratePushArguments(ILGenerator generator, int count, int pinnedRef, int pinnedRef2)
	{
		for (int i = 0; i < count; i++)
		{
			if (i == pinnedRef)
			{
				generator.Emit(OpCodes.Ldloc_0);
				continue;
			}
			if (i == pinnedRef2)
			{
				generator.Emit(OpCodes.Ldloc_1);
				continue;
			}
			switch (i)
			{
			case 0:
				generator.Emit(OpCodes.Ldarg_0);
				break;
			case 1:
				generator.Emit(OpCodes.Ldarg_1);
				break;
			case 2:
				generator.Emit(OpCodes.Ldarg_2);
				break;
			case 3:
				generator.Emit(OpCodes.Ldarg_3);
				break;
			default:
				generator.Emit(OpCodes.Ldarg, i);
				break;
			}
		}
	}

	private unsafe static void GenerateGetVTable(ILGenerator generator, uint offset)
	{
		generator.Emit(OpCodes.Ldarg_0);
		generator.Emit(OpCodes.Ldind_I);
		generator.Emit(OpCodes.Ldc_I4, offset);
		generator.Emit(OpCodes.Conv_I);
		generator.Emit(OpCodes.Sizeof, typeof(void*));
		generator.Emit(OpCodes.Mul);
		generator.Emit(OpCodes.Add);
		generator.Emit(OpCodes.Ldind_I);
	}
}
