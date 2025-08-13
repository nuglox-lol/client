using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System;
using Mirror;

// THIS ISNT A CLASS DATAMODEL SO IT DOESNT DERIVE FROM INSTANCEDATAMODEL !!

public class InstanceCamera
{
    private Player player;

    public InstanceCamera(Player player)
    {
        this.player = player;
    }

    public float yOffset
    {
        set
        {
            if (player == null || !NetworkServer.active) return;
            player.TargetSetCameraYOffset(player.connectionToClient, value);
        }
    }

    public CameraController.CameraMode CameraMode
    {
        set
        {
            if (player == null || !NetworkServer.active) return;
            player.TargetSetCameraMode(player.connectionToClient, (int)value);
        }
    }

    public void Interpolate(Vector3 position, Quaternion rotation, float duration)
    {
        if (player == null || !NetworkServer.active) return;
        player.TargetInterpolateCamera(player.connectionToClient, position, rotation, duration);
    }
}