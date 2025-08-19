using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceIntValue : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceIntValue(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public int Value  
    {  
        get => go.GetComponent<IntValue>().Value;  
        set { go.GetComponent<IntValue>().Value = value; }  
    }  
}