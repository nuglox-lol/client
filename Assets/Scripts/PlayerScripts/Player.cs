using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCharacterAppearanceChanged))] public int characterAppearanceId;
    [SyncVar] public int userID;
    [SyncVar] public string username;
    [SyncVar] public bool isAdmin;
    [SyncVar(hook = nameof(OnHealthChanged))] public int health = 100;
    [SyncVar] public int maximumHealth = 100;
    [SyncVar] public bool isDead = false;
    [SyncVar] public string TeamName;

    private InstancePlayer instancePlayer;
    private bool _hasPlayedDeathSound = false;

    public float respawnTime = 5f;
    public GameObject playerBody;

    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (playerBody == null)
            playerBody = this.gameObject;

        instancePlayer = new InstancePlayer(gameObject);
#if UNITY_EDITOR
        instancePlayer.CharacterAppearance = characterAppearanceId;
#else
        if (characterAppearanceId > 0)
            instancePlayer.CharacterAppearance = characterAppearanceId;
#endif
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    void Update()
    {
        if (isLocalPlayer && health < 1 && !isDead)
            Kill();

        if (isLocalPlayer && transform.position.y < -100 && !isDead)
            Kill();
    }

    void OnCharacterAppearanceChanged(int oldValue, int newValue)
    {
        if (instancePlayer == null)
            instancePlayer = new InstancePlayer(gameObject);
        instancePlayer.CharacterAppearance = newValue;
    }

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (isLocalPlayer)
            Debug.Log($"Health changed: {newHealth}");
    }

    [Command]
    public void Kill()
    {
        if (isDead) return;
        isDead = true;
        health = 0;
        RpcOnDeath();
        StartCoroutine(Respawn());
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        if (playerBody != null)
            SetRenderersEnabled(playerBody, false);

        var movement = gameObject.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        if (isLocalPlayer && !_hasPlayedDeathSound)
        {
            AudioClip deathClip = Resources.Load<AudioClip>("Audio/Akh");
            if (deathClip != null)
            {
                AudioSource.PlayClipAtPoint(deathClip, transform.position);
                _hasPlayedDeathSound = true;
            }
        }
    }

    [Server]
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        if (spawnPoints.Count == 0)
            spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

        Transform respawnTransform = null;

        if (!string.IsNullOrEmpty(TeamName))
        {
            var teamSpawns = spawnPoints.FindAll(s => s.teamName == TeamName);
            if (teamSpawns.Count > 0)
                respawnTransform = teamSpawns[Random.Range(0, teamSpawns.Count)].transform;
        }

        if (respawnTransform == null && spawnPoints.Count > 0)
            respawnTransform = spawnPoints[Random.Range(0, spawnPoints.Count)].transform;

        Vector3 respawnPosition = respawnTransform != null ? respawnTransform.position : Vector3.zero;

        health = maximumHealth;
        isDead = false;
        gameObject.GetComponent<PlayerMovement>().enabled = true;
        RpcRespawn(respawnPosition);
    }

    [ClientRpc]
    void RpcRespawn(Vector3 position)
    {
        transform.position = position;
        if (playerBody != null)
            SetRenderersEnabled(playerBody, true);
        gameObject.GetComponent<PlayerMovement>().enabled = true;
        _hasPlayedDeathSound = false;
    }

    void SetRenderersEnabled(GameObject obj, bool enabled)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = enabled;
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
            ShowAlert(message, duration);
    }

    [TargetRpc]
    public void TargetShowAlert(NetworkConnection target, string message, float duration)
    {
        ShowAlert(message, duration);
    }

    private void ShowAlert(string message, float duration)
    {
        var coreGui = GameObject.Find("CoreGui");
        var alertPanel = coreGui.transform.Find("AlertPanel");
        var alertText = alertPanel?.Find("AlertMessage")?.GetComponent<TMPro.TextMeshProUGUI>();
        alertText.text = message;
        alertPanel.gameObject.SetActive(true);
        StartCoroutine(HideAlertAfterDelay(alertPanel.gameObject, duration));
    }

    private IEnumerator HideAlertAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
    }

    [Command]
    public void CmdSendChatMessage(string message)
    {
        RpcReceiveChatMessage(message);
    }

    [ClientRpc]
    void RpcReceiveChatMessage(string message)
    {
        ChatService.ReceiveMessage(message);
    }

    [TargetRpc]
    public void TargetSetCameraMode(NetworkConnection target, int mode)
    {
        var camController = Camera.main.GetComponent<CameraController>();
        if (camController != null)
            camController.cameraMode = (CameraController.CameraMode)mode;
    }

    [TargetRpc]
    public void TargetInterpolateCamera(NetworkConnection target, Vector3 pos, Quaternion rot, float time)
    {
        var camController = Camera.main.GetComponent<CameraController>();
        if (camController != null)
            camController.Interpolate(pos, rot, time);
    }

    [TargetRpc]
    public void TargetSetCameraYOffset(NetworkConnection target, float yOffset)
    {
        var camController = Camera.main.GetComponent<CameraController>();
        if (camController != null)
            camController.yOffset = yOffset;
    }
}
