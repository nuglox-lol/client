using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class AlertManager : NetworkBehaviour
{
    public static AlertManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Server]
    public void SendAlertToAll(string message, float duration = 3f)
    {
        Debug.Log($"[Server] Sending alert to all: {message}");
        RpcReceiveAlert(message, duration);
    }

    [TargetRpc]
    public void TargetSendAlert(NetworkConnection target, string message, float duration)
    {
        ShowAlert(message, duration);
    }

    [ClientRpc]
    private void RpcReceiveAlert(string message, float duration)
    {
        Debug.Log($"[Client] Received alert: {message}");
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

    private System.Collections.IEnumerator HideAlertAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }
}