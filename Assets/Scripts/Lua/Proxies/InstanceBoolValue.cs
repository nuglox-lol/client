using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceBoolValue : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceBoolValue(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public bool Value
    {  
        get => go.GetComponent<BoolValue>().Value;  
        set { go.GetComponent<BoolValue>().Value = value; }  
    }  
}