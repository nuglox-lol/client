using Mirror;
using UnityEngine;
using System.Collections;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCharacterAppearanceChanged))] public int characterAppearanceId;
    [SyncVar] public int userID;
    [SyncVar] public string username;
    [SyncVar] public bool isAdmin;

    private InstancePlayer instancePlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();
        instancePlayer = new InstancePlayer(gameObject);
        if (characterAppearanceId > 0)
        {
            instancePlayer.CharacterAppearance = characterAppearanceId;
        }
    }

    void OnCharacterAppearanceChanged(int oldValue, int newValue)
    {
        if (instancePlayer == null)
        {
            instancePlayer = new InstancePlayer(gameObject);
        }

        instancePlayer.CharacterAppearance = newValue;
    }

    [Command]
    public void CmdSetCharacterAppearance(int newId)
    {
        characterAppearanceId = newId;
    }

    [ClientRpc]
    public void RpcShowMessageOnAllClients(string message, float duration)
    {
        if (!isClient) return;
        if (isOwned || isLocalPlayer || NetworkClient.localPlayer == this)
        {
            ShowAlert(message, duration);
        }
    }

    [TargetRpc]
    public void TargetShowAlert(NetworkConnection target, string message, float duration)
    {
        ShowAlert(message, duration);
    }

    private void ShowAlert(string message, float duration)
    {
        var coreGui = GameObject.Find("CoreGui");
        if (coreGui == null) return;

        var alertPanel = coreGui.transform.Find("AlertPanel");
        var alertText = alertPanel?.Find("AlertMessage")?.GetComponent<TMPro.TextMeshProUGUI>();

        if (alertPanel == null || alertText == null) return;

        alertText.text = message;
        alertPanel.gameObject.SetActive(true);

        StartCoroutine(HideAlertAfterDelay(alertPanel.gameObject, duration));
    }

    private IEnumerator HideAlertAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
    }
}