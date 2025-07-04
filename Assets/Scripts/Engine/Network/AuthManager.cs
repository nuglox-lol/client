using System.Collections;
using UnityEngine;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine.Networking;

public struct AuthKeyMessage : NetworkMessage
{
    public string authKey;
}

public class AuthManager : NetworkManager
{
    private bool hadPlayers = false;
    private bool checkingEmpty = false;
    private float emptyCheckDelay = 40f;

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
        Debug.Log("OnClientConnect called");
        base.OnClientConnect();
        string authKey = GetArgs.Get("authkey");
        if (!string.IsNullOrEmpty(authKey))
        {
            Debug.Log("Sending auth key: " + authKey);
            NetworkClient.Send(new AuthKeyMessage { authKey = authKey });
        }
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("OnClientDisconnect called");
        base.OnClientDisconnect();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        string url = GetArgs.Get("baseUrl") + "placefiles/" + GetArgs.Get("gameid") + ".bpf";
        DataService.LoadURL(url, true);
        #if UNITY_EDITOR
        DataService.LoadURL("https://brikz.world/placefiles/1.bpf", true);
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
        else
        {
            Debug.LogWarning($"Connection {conn.connectionId} has no player object assigned yet!");
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (NetworkServer.connections.Count == 0)
        {
            if (hadPlayers)
            {
                StartCoroutine(SendWebRequest(GetArgs.Get("baseurl") + "v1/gameserver/close.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
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
            StartCoroutine(SendWebRequest(GetArgs.Get("baseurl") + "v1/gameserver/close.php?password=" + GetArgs.Get("password") + "&gameid=" + GetArgs.Get("gameid")));
        }
    }

    IEnumerator SendWebRequest(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
    }
}