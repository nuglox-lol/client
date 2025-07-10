using Mirror;
using UnityEngine;

public class PlayerDefaults : NetworkBehaviour
{
    [SyncVar] public float maxHealth = 100;
    [SyncVar] public float walkSpeed = 8f;
    [SyncVar] public float jumpPower = 8f;
    [SyncVar] public float respawnTime = 5f;

    public void LoadDefaults(Player player)
    {
        if (isServer)
        {
            ApplyDefaults(player);
        }
        else
        {
            CmdRequestLoadDefaults(player.netIdentity);
        }
    }

    [Server]
    void ApplyDefaults(Player player)
    {
        if (player == null) return;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        player.health = (int)maxHealth;
        player.maximumHealth = (int)maxHealth;

        if (movement != null)
        {
            movement.speed = walkSpeed;
            movement.jumpForce = jumpPower;
        }

        Transform toolAttach = player.transform.Find("LeftArm/ToolAttachmentPoint");
        if (toolAttach == null) 
        {
            Debug.LogWarning("Player ToolAttachmentPoint not found");
            return;
        }

        Transform defaultsToolAttach = transform.Find("ToolAttachmentPoint");
        if (defaultsToolAttach == null) 
        {
            Debug.LogWarning("Defaults ToolAttachmentPoint not found");
            return;
        }

        foreach (Transform child in defaultsToolAttach)
        {
            GameObject copy = Instantiate(child.gameObject);

            copy.name = child.name;

            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = Vector3.one;

            copy.AddComponent<NetworkIdentity>();
            copy.AddComponent<NetworkTransformUnreliable>();

            NetworkServer.Spawn(copy, player.connectionToClient);

            ParentSync sync = copy.GetComponent<ParentSync>();
            if (sync == null) sync = copy.AddComponent<ParentSync>();
            sync.ForceUpdate();

            copy.transform.SetParent(toolAttach, false);
        }
    }


    [Command]
    void CmdRequestLoadDefaults(NetworkIdentity playerNetId)
    {
        Player player = playerNetId.GetComponent<Player>();
        ApplyDefaults(player);
    }

    public float GetMaxHealth() => maxHealth;
    public void SetMaxHealth(float value) => maxHealth = value;

    public float GetWalkSpeed() => walkSpeed;
    public void SetWalkSpeed(float value) => walkSpeed = value;

    public float GetJumpPower() => jumpPower;
    public void SetJumpPower(float value) => jumpPower = value;

    public float GetRespawnTime() => respawnTime;
    public void SetRespawnTime(float value) => respawnTime = value;
}