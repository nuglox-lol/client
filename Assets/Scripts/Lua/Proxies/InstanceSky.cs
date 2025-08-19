using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceSky : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceSky(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public int Id  
    {  
        get => go.GetComponent<Sky>().id;  
        set { go.GetComponent<Sky>().id = value; }  
    }  
}