using System;
using Mirror;
using UnityEngine;

public class Seat : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.CanMove = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.CanMove = true;
        }
    }
}