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
    public InstanceExplosion(GameObject gameObject, Script lua = null) : base(gameObject, lua) {
        if(gameObject.GetComponent<ObjectClass>().className != "Explosion")
            return;
    }

    public void Trigger(){
        Transform.GetComponent<Explosion>().Trigger();
    }
}
