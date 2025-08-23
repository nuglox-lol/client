using UnityEngine;

public static class ThumbnailGenerator
{
    private static Camera thumbnailCamera;
    private static RenderTexture renderTexture;

    public static string GenerateThumbnailBase64(Vector3 position, Vector3 eulerRotation, int width = 256, int height = 256, bool hideSky = true, bool isPlace = false)
    {
        if(isPlace){
            GameObject obj = GameObject.Find("MapCamera");
            if(obj){
                position = obj.transform.position;
                eulerRotation = obj.transform.rotation.eulerAngles;
            }
            width = 800;
            height = 600;
        }

        SetupCamera(width, height, hideSky);

        thumbnailCamera.transform.position = position;
        thumbnailCamera.transform.rotation = Quaternion.Euler(eulerRotation);

        thumbnailCamera.Render();

        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        return System.Convert.ToBase64String(bytes);
    }

    private static void SetupCamera(int width, int height, bool hideSky)
    {
        if (thumbnailCamera == null)
        {
            GameObject cameraGO = new GameObject("ThumbnailCamera");
            thumbnailCamera = cameraGO.AddComponent<Camera>();
            thumbnailCamera.enabled = false;
        }

        if (renderTexture == null || renderTexture.width != width || renderTexture.height != height)
        {
            if (renderTexture != null)
                renderTexture.Release();

            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
        }

        thumbnailCamera.targetTexture = renderTexture;

        if (hideSky)
        {
            thumbnailCamera.clearFlags = CameraClearFlags.SolidColor;
            thumbnailCamera.backgroundColor = Color.clear;
        }
        else
        {
            RenderSettings.skybox = Resources.Load<Material>("Textures/Sky");
            thumbnailCamera.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
