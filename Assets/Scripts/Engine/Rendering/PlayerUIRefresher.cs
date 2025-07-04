using UnityEngine;
using ImGuiNET.Unity;
using System.Reflection;

public class PlayerUIRefresher : MonoBehaviour
{
    private Camera playerCamera;
    private DearImGui dearImGui;
    private FieldInfo cameraField;

    void Awake()
    {
        dearImGui = FindObjectOfType<DearImGui>();
        if (dearImGui != null)
            cameraField = typeof(DearImGui).GetField("_camera", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void Update()
    {
        if (playerCamera == null)
        {
            var camObj = GameObject.Find("PlayerCamera");
            if (camObj != null)
                playerCamera = camObj.GetComponent<Camera>();
        }

        if (dearImGui != null && playerCamera != null && cameraField != null)
        {
            Camera currentCamera = (Camera)cameraField.GetValue(dearImGui);
            if (currentCamera != playerCamera)
            {
                cameraField.SetValue(dearImGui, playerCamera);
                dearImGui.enabled = false;
                dearImGui.enabled = true;
            }
        }
    }
}
