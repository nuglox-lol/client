using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceFloatValue : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceFloatValue(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public float Value  
    {  
        get => go.GetComponent<FloatValue>().Value;  
        set { go.GetComponent<FloatValue>().Value = value; }  
    }  
}