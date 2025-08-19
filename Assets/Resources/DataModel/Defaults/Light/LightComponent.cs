using UnityEngine;
using Mirror;

public class LightComponent : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnRotationChanged))] public Vector3 Rotation;
    [SyncVar(hook = nameof(OnExposureChanged))] public float Exposure;
    [SyncVar(hook = nameof(OnContrastChanged))] public float Contrast;
    [SyncVar(hook = nameof(OnTintChanged))] public Color Tint;

    private Material skyMat;

    private void Start()
    {
        var originalMat = Resources.Load<Material>("Textures/Sky");
        if (originalMat != null)
            skyMat = Instantiate(originalMat);
        
        ApplyAll();
    }

    void ApplyAll()
    {
        if (skyMat != null)
        {
            skyMat.SetVector("_Rotation", Rotation);
            skyMat.SetFloat("_Exposure", Exposure);
            skyMat.SetFloat("_Contrast", Contrast);
            skyMat.SetColor("_Tint", Tint);
        }
    }

    void OnRotationChanged(Vector3 oldVal, Vector3 newVal)
    {
        if (skyMat != null) skyMat.SetVector("_Rotation", newVal);
    }

    void OnExposureChanged(float oldVal, float newVal)
    {
        if (skyMat != null) skyMat.SetFloat("_Exposure", newVal);
    }

    void OnContrastChanged(float oldVal, float newVal)
    {
        if (skyMat != null) skyMat.SetFloat("_Contrast", newVal);
    }

    void OnTintChanged(Color oldVal, Color newVal)
    {
        if (skyMat != null) skyMat.SetColor("_Tint", newVal);
    }
}
