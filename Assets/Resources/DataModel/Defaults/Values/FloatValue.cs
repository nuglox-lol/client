using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FloatValue : NetworkBehaviour
{
    [SyncVar] public float Value;
}
