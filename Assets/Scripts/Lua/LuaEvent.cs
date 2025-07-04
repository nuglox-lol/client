using System;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaEvent
{
    private GameObject go;

    private Script initialScript;

    private event Action<DynValue> touchedHandlers;

    public LuaEvent(GameObject gameObject, Script luaScript)
    {
        go = gameObject;
        initialScript = luaScript;
    }

    public void Connect(Script luaScript, DynValue function)
    {
        if (function.Type == DataType.Function)
        {
            touchedHandlers += (DynValue hitObj) =>
            {
                luaScript.Call(function, hitObj);
            };
        }
    }

    public void Fire(DynValue hitInstanceDatamodel)
    {
        touchedHandlers?.Invoke(hitInstanceDatamodel);
    }
}