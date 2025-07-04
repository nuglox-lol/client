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
        //if (SceneManager.GetActiveScene().name != "Studio")
            //yield break;

        if (isLocalScript)
        {
            if (!isLocalPlayer && SceneManager.GetActiveScene().name != "Studio")
                yield break;
        }
        else
        {
            if (!isServer && SceneManager.GetActiveScene().name != "Studio")
                yield break;
        }

        manager = gameObject.AddComponent<ScriptService>();
        manager.Init();

        yield return manager.RunScriptWhenReady(Script);
    }
}