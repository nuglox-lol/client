using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        DataService.Load(Application.persistentDataPath + "/SaveFile.npf", true);
    }
}
