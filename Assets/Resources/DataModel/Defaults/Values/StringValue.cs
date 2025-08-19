using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StringValue : NetworkBehaviour
{
    [SyncVar] public string Value;
}
