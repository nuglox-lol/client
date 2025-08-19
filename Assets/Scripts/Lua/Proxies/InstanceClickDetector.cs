using System;
using UnityEngine;
using MoonSharp.Interpreter;
using Mirror;

[MoonSharpUserData]
public class InstanceClickDetector : InstanceDatamodel
{
    private GameObject go;
    private LuaEvent clickEvent;

    public InstanceClickDetector(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

        go = gameObject;

        if (go.GetComponent<ClickDetector>() == null)
        {
            go.AddComponent<ClickDetector>();
        }
    }

    public LuaEvent Clicked
    {
        get
        {
            if (clickEvent == null)
            {
                ClickDetector handler = go.GetComponent<ClickDetector>();
                if (handler != null)
                {
                    clickEvent = handler.GetClickEvent(go.name);
                }
            }
            return clickEvent;
        }
    }
}