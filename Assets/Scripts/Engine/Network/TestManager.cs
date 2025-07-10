using System.Collections;
using UnityEngine;
using Mirror;

public class TestManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        DataService.Load(Application.persistentDataPath + "/SaveFile.npf", true);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        GameObject player = conn.identity.gameObject;
        StartCoroutine(DelayedDefaultsLoad(player));
    }

    private IEnumerator DelayedDefaultsLoad(GameObject player)
    {
        yield return new WaitForSeconds(0.25f);

        PlayerDefaults defaults = FindObjectOfType<PlayerDefaults>();
        if (defaults != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                defaults.LoadDefaults(playerComponent);
            }
        }
    }
}