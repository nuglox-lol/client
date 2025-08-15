using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Mirror;

public class TestManager : NetworkManager
{
    private GameObject loadingPanel;
    private TMP_Text titleText;
    private TMP_Text creatorText;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        loadingPanel = GameObject.Find("CoreGui/LoadingPanel");
        if (loadingPanel != null)
        {
            titleText = loadingPanel.transform.Find("Title").GetComponent<TMP_Text>();
            creatorText = loadingPanel.transform.Find("Creator").GetComponent<TMP_Text>();
            loadingPanel.SetActive(true);
            StartCoroutine(LoadGameInfo());
        }
    }

    private IEnumerator LoadGameInfo()
    {
        string url = GetArgs.Get("baseUrl") + "v1/game/getinfo.php";
        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            GameInfo info = JsonUtility.FromJson<GameInfo>(json);
            if (titleText != null) titleText.text = info.Title;
            if (creatorText != null) creatorText.text = info.Creator;
        }
        yield return new WaitForSeconds(3f);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    [System.Serializable]
    private class GameInfo
    {
        public string Title;
        public string Creator;
    }
}
