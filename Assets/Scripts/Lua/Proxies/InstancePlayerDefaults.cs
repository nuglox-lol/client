using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class InstancePlayerDefaults : InstanceDatamodel
{

    public InstancePlayerDefaults(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        // really nothing more.
    }

    public float MaxHealth
    {
        get => Transform.GetComponent<PlayerDefaults>().maxHealth;
        set { Transform.GetComponent<PlayerDefaults>().maxHealth = value; }
    }

    public float WalkSpeed
    {
        get => Transform.GetComponent<PlayerDefaults>().walkSpeed;
        set { Transform.GetComponent<PlayerDefaults>().walkSpeed = value; }
    }

    public float JumpPower
    {
        get => Transform.GetComponent<PlayerDefaults>().jumpPower;
        set { Transform.GetComponent<PlayerDefaults>().jumpPower = value; }
    }

    public float RespawnTime
    {
        get => Transform.GetComponent<PlayerDefaults>().respawnTime;
        set { Transform.GetComponent<PlayerDefaults>().respawnTime = value; }
    }
}
