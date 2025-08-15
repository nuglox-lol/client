using System.Collections;
using UnityEngine;
using Mirror;
using Dummiesman;

public class MeshComponent : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnMeshIDChanged))]
    public int meshID;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (meshID > 0)
            StartCoroutine(LoadOBJFromURL(GetArgs.Get("baseUrl") + "catalog_storage/meshes/" + meshID + ".obj"));
    }

    private void OnMeshIDChanged(int oldID, int newID)
    {
        if (newID > 0)
            StartCoroutine(LoadOBJFromURL(GetArgs.Get("baseUrl") + "catalog_storage/meshes/" + newID + ".obj"));
    }

    private IEnumerator LoadOBJFromURL(string url)
    {
        Transform parentObject = transform.parent;
        if (parentObject == null) yield break;

        using (var www = new WWW(url))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) yield break;

            byte[] objData = www.bytes;
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "temp.obj");
            System.IO.File.WriteAllBytes(tempPath, objData);

            OBJLoader loader = new OBJLoader();
            GameObject loaded = loader.Load(tempPath);
            MeshFilter loadedFilter = loaded.GetComponentInChildren<MeshFilter>();
            MeshRenderer loadedRenderer = loaded.GetComponentInChildren<MeshRenderer>();

            MeshFilter parentFilter = parentObject.GetComponent<MeshFilter>();
            MeshRenderer parentRenderer = parentObject.GetComponent<MeshRenderer>();

            if (loadedFilter && parentFilter)
                parentFilter.mesh = loadedFilter.sharedMesh;
            if (loadedRenderer && parentRenderer)
                parentRenderer.materials = loadedRenderer.sharedMaterials;

            Destroy(loaded);
        }
    }
}
