using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.Networking;

public class Sky : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnIdChanged))] public int id;

    private Material skyMat;

    private void Awake()
    {
        var originalMat = Resources.Load<Material>("Textures/Sky");
        if (originalMat != null)
            skyMat = Instantiate(originalMat);

        if (skyMat != null)
            RenderSettings.skybox = skyMat;
    }

    private void Start()
    {
        if (id != 0)
            StartCoroutine(DownloadSky(id));
    }

    void OnIdChanged(int oldVal, int newVal)
    {
        if (newVal != 0)
            StartCoroutine(EnsureMaterialAndDownload(newVal));
    }

    IEnumerator EnsureMaterialAndDownload(int skyId)
    {
        while (skyMat == null)
            yield return null;

        yield return DownloadSky(skyId);
    }

    IEnumerator DownloadSky(int skyId)
    {
        string url = GetArgs.Get("baseUrl") + "catalog_storage/decals/" + skyId + ".png";

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);

                if (skyMat != null)
                {
                    skyMat.shader = Shader.Find("Skybox/Panoramic");
                    skyMat.SetTexture("_MainTex", tex);
                    RenderSettings.skybox = skyMat;
                }
            }
        }
    }
}
