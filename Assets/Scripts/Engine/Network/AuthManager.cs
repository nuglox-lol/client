using System.Collections;
using UnityEngine;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine.Networking;
using TMPro;

public struct AuthKeyMessage : NetworkMessage
{
    public string authKey;
}

public class AuthManager : NetworkManager
{
    private bool hadPlayers = false;
    private bool checkingEmpty = false;
    private float emptyCheckDelay = 40f;
    private GameObject loadingPanel;
    private TMP_Text titleText;
    private TMP_Text creatorText;

    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        bool isServer = false;

        foreach (string arg in args)
        {
            if (arg == "--server")
            {
                isServer = true;
                break;
            }
        }

        if (isServer)
        {
            string portArg = GetArgs.Get("port");
            if (ushort.TryParse(portArg, out ushort port))
            {
                Transport activeTransport = Transport.active;
                if (activeTransport is TelepathyTransport telepathy)
                {
                    telepathy.port = port;
                }
                else if (activeTransport is SimpleWebTransport simpleWeb)
                {
                    simpleWeb.port = port;
                }

                StartServer();
                StartCoroutine(CheckInitialEmpty());
            }
            else
            {
                Application.Quit();
            }
        }
        else
        {
            string ip = GetArgs.Get("ip");
            string portArg = GetArgs.Get("port");
            string authKey = GetArgs.Get("authkey");

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portArg) || string.IsNullOrEmpty(authKey))
            {
                Application.Quit();
                return;
            }

            if (ushort.TryParse(portArg, out ushort port))
            {
                networkAddress = ip;

                Transport activeTransport = Transport.active;
                if (activeTransport is TelepathyTransport telepathy)
                {
                    telepathy.port = port;
                }
                else if (activeTransport is SimpleWebTransport simpleWeb)
                {
                    simpleWeb.port = port;
                }

                StartClient();
            }
            else
            {
                Application.Quit();
            }
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        loadingPanel = GameObject.Find("CoreGui/LoadingPanel");
        if (loadingPanel != null)
        {
            titleText = loadingPanel.transform.Find("Title").GetComponent<TMP_Text>();
            creatorText = loadingPanel.transform.Find("Creator").GetComponent<TMP_Text>();
            loadingPanel.SetActive(true);
            StartCoroutine(LoadGameInfo());
        }

        string authKey = GetArgs.Get("authkey");
        if (!string.IsNullOrEmpty(authKey))
        {
            NetworkClient.Send(new AuthKeyMessage { authKey = authKey });
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

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        string url = GetArgs.Get("baseUrl") + "placefiles/" + GetArgs.Get("gameid") + ".npf";
        DataService.LoadURL(url, true);
#if UNITY_EDITOR
        DataService.LoadURL("https://nuglox.com/placefiles/1.bpf", true);
#endif
        NetworkServer.RegisterHandler<AuthKeyMessage>(OnAuthKeyMessageReceived, false);
    }

    [Server]
    void OnAuthKeyMessageReceived(NetworkConnectionToClient conn, AuthKeyMessage msg)
    {
        CheckAuth(conn, msg.authKey);
    }

    [Server]
    public void CheckAuth(NetworkConnectionToClient conn, string authKey)
    {
        StartCoroutine(AuthTrustCheck.CheckAuth(conn, authKey));
    }

    [Server]
    public void FinalizeAuth(NetworkConnectionToClient conn, AuthTrustCheck.AuthResponse authResponse)
    {
        if (conn.identity != null)
        {
            GameObject player = conn.identity.gameObject;
            Player playerComponent = player.GetComponent<Player>();
            playerComponent.username = authResponse.username;
            playerComponent.userID = authResponse.userid;
            playerComponent.isAdmin = authResponse.isadmin;
            playerComponent.characterAppearanceId = authResponse.userid;
            player.name = authResponse.username;
        }
        NetworkServer.SetClientReady(conn);
        hadPlayers = true;
        StartCoroutine(SendWebRequest(GetArgs.Get("baseUrl") + "v1/gameserver/add.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
    }

    [Server]
    public void RejectAuth(NetworkConnectionToClient conn)
    {
        conn.Disconnect();
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (NetworkServer.connections.Count == 0)
        {
            if (hadPlayers)
            {
                StartCoroutine(SendWebRequest(GetArgs.Get("baseUrl") + "v1/gameserver/close.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
            }
        }
        StartCoroutine(SendWebRequest(GetArgs.Get("baseUrl") + "v1/gameserver/remove.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
    }

    IEnumerator CheckInitialEmpty()
    {
        if (checkingEmpty) yield break;
        checkingEmpty = true;
        yield return new WaitForSeconds(emptyCheckDelay);
        if (!hadPlayers && NetworkServer.connections.Count == 0)
        {
            StartCoroutine(SendWebRequest(GetArgs.Get("baseUrl") + "v1/gameserver/close.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
        }
    }

    IEnumerator SendWebRequest(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
    }
}
