using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.EventSystems;
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

        GameObject playerPrefab = playerEntryPrefab != null ? playerEntryPrefab : Resources.Load<GameObject>("UI/PlayerEntry");
        GameObject teamPrefab = Resources.Load<GameObject>("UI/TeamEntry");

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Object");

        List<Team> teams = new();
        foreach (var obj in objects)
        {
            var tClass = obj.GetComponent<ObjectClass>();
            var t = obj.GetComponent<Team>();
            if (tClass != null && tClass.className == "Team" && t != null)
                teams.Add(t);
        }

        Color neutralColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        foreach (var team in teams)
        {
            GameObject teamEntryObj = Instantiate(teamPrefab, playerListContainer);
            RawImage teamEntry = teamEntryObj.GetComponent<RawImage>();
            if (teamEntry != null)
            {
                Color c = team.TeamColor;
                c.a = 0.8f;
                teamEntry.color = c;
            }

            TMP_Text teamName = teamEntryObj.transform.Find("Name")?.GetComponent<TMP_Text>();
            if (teamName != null)
                teamName.text = team.TeamName;

            foreach (var obj2 in objects)
            {
                var player = obj2.GetComponent<Player>();
                var classCheck = obj2.GetComponent<ObjectClass>();
                if (player != null && classCheck != null && classCheck.className == "Player" && player.TeamName == team.TeamName)
                {
                    obj2.transform.SetParent(team.transform, true);

                    GameObject entry = Instantiate(playerPrefab, playerListContainer);
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

                    TMP_Text teamNameText = entry.transform.Find("Name")?.GetComponent<TMP_Text>();
                    if (teamNameText != null)
                        teamNameText.text = team.TeamName;
                }
            }
        }

        GameObject neutralEntryObj = Instantiate(teamPrefab, playerListContainer);
        RawImage neutralEntry = neutralEntryObj.GetComponent<RawImage>();
        if (neutralEntry != null)
            neutralEntry.color = neutralColor;
        TMP_Text neutralName = neutralEntryObj.transform.Find("Name")?.GetComponent<TMP_Text>();
        if (neutralName != null)
            neutralName.text = "Neutral";

        foreach (var obj in objects)
        {
            var player = obj.GetComponent<Player>();
            var classCheck = obj.GetComponent<ObjectClass>();
            if (player != null && classCheck != null && classCheck.className == "Player" && (string.IsNullOrEmpty(player.TeamName) || teams.Find(t => t.TeamName == player.TeamName) == null))
            {
                obj.transform.SetParent(null, true);

                GameObject entry = Instantiate(playerPrefab, playerListContainer);
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

                TMP_Text teamNameText = entry.transform.Find("Name")?.GetComponent<TMP_Text>();
                if (teamNameText != null)
                    teamNameText.text = "Neutral";
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