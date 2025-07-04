using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ObjectClass : MonoBehaviour
{
    public string className;

    void Start()
    {
        RenameIfDuplicate();

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "BCS")
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
        }

        if (currentScene.name == "Studio")
        {
            NetworkIdentity ni = GetComponent<NetworkIdentity>();
            if (ni != null)
                Destroy(ni);
        }

        if (!DataModel.TrustCheckDataModel(gameObject))
        {
            Destroy(gameObject);
        }
    }

    void RenameIfDuplicate()
    {
        string baseName = gameObject.name;
        int suffix = 1;

        bool Exists(string name)
        {
            var objs = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objs)
            {
                if (obj != gameObject && obj.name == name)
                    return true;
            }
            return false;
        }

        while (Exists(gameObject.name))
        {
            gameObject.name = baseName + "-" + suffix;
            suffix++;
        }
    }
}