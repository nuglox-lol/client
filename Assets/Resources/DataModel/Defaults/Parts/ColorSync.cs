using Mirror;
using UnityEngine;

public class ColorSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColorChanged))]
    private Color color;

    public void SetColor(Color newColor)
    {
        if (isServer)
        {
            color = newColor;
        }
    }

    void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor(newColor);
    }

    void ApplyColor(Color c)
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = c;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ApplyColor(color);
    }
}
