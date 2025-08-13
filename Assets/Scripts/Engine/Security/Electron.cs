using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Electron : MonoBehaviour
{
    void Awake()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GetArgs.SetDeepLinkArgs(Application.absoluteURL);
        }
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            StartCoroutine(CheckVersion());
            string sceneArg = GetArgs.Get("authkey");
            if (string.IsNullOrEmpty(sceneArg))
            {
                Application.OpenURL("https://nuglox.com");
                Application.Quit();
            }
        }
    }

    IEnumerator CheckVersion()
    {
        string currentVersion = Application.version;
        UnityWebRequest request = UnityWebRequest.Get("https://nuglox.com/v1/mobile/getversion.php");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string latestVersion = request.downloadHandler.text.Trim();
            if (currentVersion != latestVersion)
            {
                Application.OpenURL("https://nuglox.com/install.php?androidoutdated=" + currentVersion);
                Application.Quit();
            }
        }
        else
        {
            Debug.LogWarning("Failed to check version: " + request.error);
        }
    }
}