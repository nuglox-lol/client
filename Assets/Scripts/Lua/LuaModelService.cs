using UnityEngine;
using Dummiesman;
using System.IO;
using System.Net;

public class LuaModelService
{
    public static void LoadFromUrl(string objUrl, string textureUrl = null)
    {
        string objPath = Path.Combine(Application.temporaryCachePath, "temp.obj");
        string texturePath = string.IsNullOrEmpty(textureUrl) ? null : Path.Combine(Application.temporaryCachePath, "temp.png");

        using (WebClient client = new WebClient())
        {
            try
            {
                client.DownloadFile(objUrl, objPath);

                if (!string.IsNullOrEmpty(textureUrl))
                    client.DownloadFile(textureUrl, texturePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Download failed: " + e.Message);
                return;
            }
        }

        Load(objPath, texturePath);
    }

    public static void Load(string objPath, string texturePath = null)
    {
        if (!File.Exists(objPath))
        {
            Debug.LogError("OBJ file not found: " + objPath);
            return;
        }

        GameObject obj = null;
        try
        {
            obj = new OBJLoader().Load(objPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("OBJ load failed: " + e.Message);
            return;
        }

        if (obj == null)
        {
            Debug.LogError("OBJLoader returned null object");
            return;
        }

        obj.name = "OBJ";
        obj.tag = "Object";
        var comp = obj.AddComponent<ObjectClass>();
        comp.className = "Part";
        obj.transform.localScale = new Vector3(-1, 1, 1);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.SetParent(null);

        if (!string.IsNullOrEmpty(texturePath) && File.Exists(texturePath))
        {
            byte[] data = File.ReadAllBytes(texturePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.material != null)
                {
                    r.material.shader = Shader.Find("Standard");
                    r.material.mainTexture = tex;
                }
            }
        }
    }
}