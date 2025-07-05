using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScriptInstanceMain : NetworkBehaviour
{
    public string Script;
    public bool isLocalScript;

    private ScriptService manager;

    private IEnumerator Start()
    {
        manager = GetComponent<ScriptService>();
        if (manager == null)
        {
            Debug.LogError("ScriptService component is missing! Please add it to this GameObject.");
            yield break;
        }

        if (SceneManager.GetActiveScene().name == "Studio")
            yield break;

        if (isLocalScript)
        {
            if (!isLocalPlayer || isServer)
            {
                Debug.Log("Blocked: Local script must run only on client");
                yield break;
            }
        }
        else
        {
            if (!isServer || isLocalPlayer)
            {
                Debug.Log("Blocked: Global script must run only on server");
                yield break;
            }
        }

        manager.Init();
        yield return manager.RunScriptWhenReady(Script);
    }
}