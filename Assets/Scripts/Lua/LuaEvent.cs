using System;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaEvent
{
	private GameObject go;

	private Script luaScript;

	private event Action<DynValue> touchedHandlers;

	public LuaEvent(GameObject gameObject, Script lua)
	{
		go = gameObject;
		luaScript = lua;
	}

	public void Connect(DynValue function)
	{
		if (function.Type == DataType.Function)
		{
			touchedHandlers += delegate(DynValue hitObj)
			{
				luaScript.Call(function, hitObj);
			};
		}
	}

	public void Fire(DynValue hitInstanceDatamodel)
	{
		this.touchedHandlers?.Invoke(hitInstanceDatamodel);
	}
}
