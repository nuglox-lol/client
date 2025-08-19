using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Decal : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnIdChanged))] 
    public int DecalId;

    [SyncVar(hook = nameof(OnFaceChanged))]
    public string Face = "FrontFace";

    private Material decalMat;
    private int lastDecalId;
    private string lastFace;

    void Update()
    {
        if (SceneHelper.GetCurrentSceneName() != "Studio") return;

        if (DecalId != lastDecalId)
        {
            lastDecalId = DecalId;
            StartCoroutine(DownloadDecal(DecalId));
        }

        if (Face != lastFace)
        {
            RemovePreviousDecal();
            lastFace = Face;
            if (Face == "TopFace")
            {
                var parent = transform.parent?.Find(Face)?.parent;
                if (parent != null)
                {
                    var sr = parent.GetComponent<StudRenderer>();
                    if (sr != null) sr.enabled = false;
                }
            }
            if (decalMat != null)
            {
                ApplyDecal(decalMat);
            }
        }
    }

    void OnIdChanged(int oldId, int newId)
    {
        if (SceneHelper.GetCurrentSceneName() != "Studio")
        {
            StartCoroutine(DownloadDecal(newId));
        }
    }

    void OnFaceChanged(string oldFace, string newFace)
    {
        if (SceneHelper.GetCurrentSceneName() != "Studio")
        {
            RemovePreviousDecal();
            if (newFace == "TopFace")
            {
                var parent = transform.parent?.Find(newFace)?.parent;
                if (parent != null)
                {
                    var sr = parent.GetComponent<StudRenderer>();
                    if (sr != null) sr.enabled = false;
                }
            }
            if (decalMat != null)
            {
                ApplyDecal(decalMat);
            }
        }
    }

    IEnumerator DownloadDecal(int id)
    {
        string url = GetArgs.Get("baseUrl") + "catalog_storage/decals/" + id + ".png";
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) yield break;
            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            decalMat = new Material(Shader.Find("Standard"));
            decalMat.mainTexture = tex;
            ApplyDecal(decalMat);
        }
    }

    void ApplyDecal(Material mat)
    {
        var parent = transform.parent;
        if (parent == null) return;
        var f = parent.Find(Face);
        if (f != null)
        {
            var rend = f.GetComponent<Renderer>();
            if (rend != null) rend.material = mat;
        }
    }

    void RemovePreviousDecal()
    {
        if (string.IsNullOrEmpty(lastFace)) return;
        var parent = transform.parent;
        if (parent == null) return;
        var prev = parent.Find(lastFace);
        if (prev != null)
        {
            var rend = prev.GetComponent<Renderer>();
            if (rend != null) rend.material = null;
        }
    }
}
