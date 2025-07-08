using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Dummiesman;
using System;
using System.IO;

[MoonSharpUserData]
public class InstanceExplosion : InstanceDatamodel
{
    public InstanceExplosion(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        if (gameObject.GetComponent<ObjectClass>().className != "Explosion")
            return;
    }

    public void Trigger()
    {
        Transform.GetComponent<Explosion>().Trigger();
    }

    public float Radius
    {
        get => Transform.GetComponent<Explosion>().radius;
        set => Transform.GetComponent<Explosion>().radius = value;
    }

    public float ExplosionForce
    {
        get => Transform.GetComponent<Explosion>().explosionForce;
        set => Transform.GetComponent<Explosion>().explosionForce = value;
    }

    public float UpwardsModifier
    {
        get => Transform.GetComponent<Explosion>().upwardsModifier;
        set => Transform.GetComponent<Explosion>().upwardsModifier = value;
    }

    public float MassThreshold
    {
        get => Transform.GetComponent<Explosion>().massThreshold;
        set => Transform.GetComponent<Explosion>().massThreshold = value;
    }
}