using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class AuthTrustCheck
{
    public class AuthResponse
    {
        public bool success;
        public string username;
        public int userid;
        public bool isadmin;
    }

    public static IEnumerator CheckAuth(NetworkConnectionToClient conn, string authKey)
    {
        string url = GetArgs.Get("baseUrl") + "v1/auth/checkauth?auth=" + UnityWebRequest.EscapeURL(authKey);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Web request failed: " + www.error);
            conn.Disconnect();
            yield break;
        }

        AuthResponse response = JsonConvert.DeserializeObject<AuthResponse>(www.downloadHandler.text);

        if (!response.success)
        {
            Debug.Log("Auth failed. Forcing client quit.");
            TargetForceQuit(conn);
        }
        else
        {
            AuthManager authManager = NetworkManager.singleton as AuthManager;
            authManager.FinalizeAuth(conn, response);
        }
    }

    [TargetRpc]
    static void TargetForceQuit(NetworkConnectionToClient conn)
    {
        Application.Quit();
    }
}