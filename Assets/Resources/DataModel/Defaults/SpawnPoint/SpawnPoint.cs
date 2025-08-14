using UnityEngine;
using Mirror;

public class SpawnPoint : NetworkBehaviour
{
    [SyncVar] public string teamName;
}