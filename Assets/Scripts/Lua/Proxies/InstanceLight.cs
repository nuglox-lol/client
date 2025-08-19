using System;
using UnityEngine;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class InstanceLight : InstanceDatamodel
{
    private GameObject go;
    private LightComponent lightComp;

    public InstanceLight(GameObject gameObject) : base(gameObject)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
        go = gameObject;
        lightComp = go.GetComponent<LightComponent>();
        if (lightComp == null) lightComp = go.AddComponent<LightComponent>();
    }

    public Vector3 Rotation
    {
        get => lightComp != null ? lightComp.Rotation : Vector3.zero;
        set { if (lightComp != null) lightComp.Rotation = value; }
    }

    public float Exposure
    {
        get => lightComp != null ? lightComp.Exposure : 1f;
        set { if (lightComp != null) lightComp.Exposure = value; }
    }

    public float Contrast
    {
        get => lightComp != null ? lightComp.Contrast : 1f;
        set { if (lightComp != null) lightComp.Contrast = value; }
    }

    public Color Tint
    {
        get => lightComp != null ? lightComp.Tint : Color.white;
        set { if (lightComp != null) lightComp.Tint = value; }
    }
}
