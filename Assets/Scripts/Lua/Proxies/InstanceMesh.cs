using System;
using UnityEngine;
using MoonSharp.Interpreter;
using TMPro;

[MoonSharpUserData]
public class InstanceMesh : InstanceDatamodel
{
    private GameObject go;

    public InstanceMesh(GameObject gameObject) : base(gameObject)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
        go = gameObject;
    }

    public int MeshID
    {
        get => go.GetComponent<MeshComponent>().meshID;
        set
        {	
            if (go.GetComponent<MeshComponent>() != null)
                go.GetComponent<MeshComponent>().meshID = value;
        }
    }
}
