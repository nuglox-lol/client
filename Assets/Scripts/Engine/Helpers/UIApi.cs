using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIApi : MonoBehaviour
{
    public static void Reset()
    {
        if (NetworkClient.localPlayer != null)
        {
            Player player = NetworkClient.localPlayer.GetComponent<Player>();
            if (player != null)
            {
                player.Kill();
            }
        }
    }

    public static void Leave()
    {
        Application.Quit();
    }
}