using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEngine.SceneManagement;

public static class DataModel
{
    public static string[] AllowedClassNames = new string[]
    {
        "Part",
        "Script",
        "Player",
        "NPC",
        "BallPart",
        "CylinderPart",
        "Tool",
        "Light",
        "Fire",
        "Explosion",
        "Truss",
        "Seat",
        "PlayerDefaults",
        "Folder",
        "Text3D",
        "ToolAttachmentPoint"
    };

    public static GameObject LoadDataModel(string className)
    {
        GameObject a = Resources.Load<GameObject>("DataModel/Prefabs/" + className);
        try{
            if(a.GetComponent<ObjectClass>() == null){
                a.AddComponent<ObjectClass>();
                a.GetComponent<ObjectClass>().className = className;
            }
        }
        catch(Exception E)
        {
            Debug.LogError(E);
            GameObject.Destroy(a);
            return null;
        }

        return a;
    }

    public static bool TrustCheckDataModel(GameObject gm)
    {
        string ob = gm.GetComponent<ObjectClass>().className;
        if(!Array.Exists(AllowedClassNames, element => element == ob))
            return false;
        else
            return true;

        return false;
    }

    public static string GetOriginalClassName(string className)
    {
        GameObject a = Resources.Load<GameObject>("DataModel/Prefabs/" + className);

        return a.name;
    }

    public static GameObject SpawnClass(string className, bool isMultiplayer = false)
    {
        GameObject a = LoadDataModel(className);
        if(!TrustCheckDataModel(a)){
            GameObject.Destroy(a);
        }

        if(isMultiplayer && className == "Player") a.GetComponent<PlayerMovement>().isStudio = false;
        if(SceneManager.GetActiveScene().name == "Studio" && className == "Player") a.GetComponent<PlayerMovement>().isStudio = true;

        GameObject b = GameObject.Instantiate(a);
        b.name = GetOriginalClassName(className);

        if(isMultiplayer) NetworkServer.Spawn(b);

        return b;
    }
}
