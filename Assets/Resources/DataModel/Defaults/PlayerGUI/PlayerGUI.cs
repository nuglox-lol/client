using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnVisibilityChanged))]
    private bool visible = true;

    private Canvas canvas;

    void Awake()
    {
        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Camera cam = Camera.main;
            if (cam != null) canvas.worldCamera = cam;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public bool Visible
    {
        get => visible;
        set
        {
            visible = value;
            if (canvas != null)
                canvas.gameObject.SetActive(value);
        }
    }

    void OnVisibilityChanged(bool oldValue, bool newValue)
    {
        if (canvas != null)
            canvas.gameObject.SetActive(newValue);
    }
}
