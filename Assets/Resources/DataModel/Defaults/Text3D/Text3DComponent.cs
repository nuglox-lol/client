using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

public class Text3DComponent : NetworkBehaviour
{
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        var child = transform.Find("Text3D");
        if (child != null)
        {
            textComponent = child.GetComponent<TextMeshProUGUI>();
        }
    }
	
	public override void OnStartServer()
    {
        StartCoroutine(SyncTextToClients());
    }

    IEnumerator SyncTextToClients()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (textComponent != null)
            {
                string currentText = textComponent.text;
                RpcSetClientText(currentText);
            }
        }
    }

    [ClientRpc]
    void RpcSetClientText(string syncedText)
    {
        if (textComponent != null)
        {
            textComponent.text = syncedText;
        }
    }

    public void ChangeText(string newText)
    {
        if (textComponent != null)
            textComponent.text = newText;
    }

    public void ChangeTextColor(Color newColor)
    {
        if (textComponent != null)
            textComponent.color = newColor;
    }

    public string GetText()
    {
        return textComponent != null ? textComponent.text : "";
    }

    public Color GetColor()
    {
        return textComponent != null ? textComponent.color : Color.white;
    }
}
