using Mirror;
using UnityEngine;
using System.Collections;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCharacterAppearanceChanged))] public int characterAppearanceId;
    [SyncVar] public int userID;
    [SyncVar] public string username;
    [SyncVar] public bool isAdmin;
    [SyncVar(hook = nameof(OnHealthChanged))] public int health = 100;
    [SyncVar] public int maximumHealth = 100;

    private InstancePlayer instancePlayer;

    public float respawnTime = 5f;
    public GameObject playerBody;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (playerBody == null)
        {
            playerBody = this.gameObject;
        }

        instancePlayer = new InstancePlayer(gameObject);
        if (characterAppearanceId > 0)
        {
            instancePlayer.CharacterAppearance = characterAppearanceId;
        }
    }

    public void Update()
    {
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.K))
        {
            CmdTakeDamage(9999);
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

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (isLocalPlayer)
        {
            Debug.Log($"Health changed: {newHealth}");
        }
    }

    [Command]
    public void CmdTakeDamage(int damage)
    {
        if (health <= 0) return;

        health -= damage;
        if (health <= 0)
        {
            health = 0;
            RpcOnDeath();
            StartCoroutine(Respawn());
        }
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        if (playerBody != null)
        {
            SetRenderersEnabled(playerBody, false);
        }
        gameObject.GetComponent<PlayerMovement>().enabled = false;

        AudioClip deathClip = Resources.Load<AudioClip>("Audio/Akh");
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position);
        }
    }

    [Server]
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        Vector3 respawnPosition = Vector3.zero;
        health = maximumHealth;
        gameObject.GetComponent<PlayerMovement>().enabled = true;
        RpcRespawn(respawnPosition);
    }

    [ClientRpc]
    void RpcRespawn(Vector3 position)
    {
        transform.position = position;
        if (playerBody != null)
        {
            SetRenderersEnabled(playerBody, true);
        }
        gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    void SetRenderersEnabled(GameObject obj, bool enabled)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = enabled;
        }
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
}