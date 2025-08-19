using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceStringValue : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceStringValue(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public string Value  
    {  
        get => go.GetComponent<StringValue>().Value;  
        set { go.GetComponent<StringValue>().Value = value; }  
    }  
}