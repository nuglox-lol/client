using Mirror;
using UnityEngine;

public class NetworkTransformExcludeOwner : NetworkTransformReliable
{
    void Start()
    {
        if (isLocalPlayer)
            enabled = false;
    }
}
