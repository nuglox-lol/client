using System;  
using UnityEngine;  
using MoonSharp.Interpreter;  

[MoonSharpUserData]  
public class InstanceDecal : InstanceDatamodel  
{  
    private GameObject go;  

    public InstanceDecal(GameObject gameObject) : base(gameObject)  
    {  
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));  
        go = gameObject;  
    }  

    public string Face  
    {  
        get => go.GetComponent<Decal>().Face;  
        set { go.GetComponent<Decal>().Face = value; }  
    } 

    public int DecalId
    {
        get => go.GetComponent<Decal>().DecalId;  
        set { go.GetComponent<Decal>().DecalId = value; }  
    }
}