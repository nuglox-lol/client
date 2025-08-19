using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IntValue : NetworkBehaviour
{
    [SyncVar] public int Value;
}
