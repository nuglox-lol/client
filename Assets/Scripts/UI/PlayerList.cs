using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerList : NetworkBehaviour
{
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;

    public float updateInterval = 2f;

    private Dictionary<int, Texture> cachedTextures = new();

    public override void OnStartServer()
    {
        if (!isServer) return;
        StartCoroutine(UpdatePlayerList());
    }

    IEnumerator UpdatePlayerList()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn != null && conn.isReady)
                {
                    RpcUpdatePlayerList();
                    break;
                }
            }
        }
    }

    [ClientRpc]
    void RpcUpdatePlayerList()
    {
        if (playerListContainer == null) return;

        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        GameObject prefab = playerEntryPrefab != null ? playerEntryPrefab : Resources.Load<GameObject>("UI/PlayerEntry");

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in objects)
        {
            var player = obj.GetComponent<Player>();
            var classCheck = obj.GetComponent<ObjectClass>();

            if (player != null && classCheck != null && classCheck.className == "Player")
            {
                GameObject entry = Instantiate(prefab, playerListContainer);
                TMP_Text usernameText = entry.transform.Find("Username")?.GetComponent<TMP_Text>();
                if (usernameText != null)
                    usernameText.text = player.username;

                RawImage thumbnail = entry.transform.Find("Thumbnail")?.GetComponent<RawImage>();
                if (thumbnail != null)
                {
                    if (cachedTextures.TryGetValue(player.userID, out var cachedTex))
                    {
                        thumbnail.texture = cachedTex;
                    }
                    else
                    {
                        StartCoroutine(LoadImage(GetArgs.Get("baseUrl") + $"rendering/users/{player.userID}-headshot.png", thumbnail, player.userID));
                    }
                }
            }
        }
    }


    IEnumerator LoadImage(string url, RawImage target, int userId)
    {
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture tex = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            cachedTextures[userId] = tex;
            target.texture = tex;
        }
    }
}