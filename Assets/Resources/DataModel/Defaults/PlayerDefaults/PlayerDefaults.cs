using Mirror;
using UnityEngine;

public class PlayerDefaults : NetworkBehaviour
{
    [SyncVar] public float maxHealth = 100;
    [SyncVar] public float walkSpeed = 8f;
    [SyncVar] public float jumpPower = 8f;
    [SyncVar] public float respawnTime = 5f;

    string[] savedModels = null;

    void Start()
    {
        if (!isServer) return;

        GameObject defaultsObj = GameObject.Find("PlayerDefaults");
        if (defaultsObj == null)
            return;

        Transform toolAttachmentPoint = defaultsObj.transform.Find("ToolAttachmentPoint");
        if (toolAttachmentPoint == null)
            return;

        int childCount = toolAttachmentPoint.childCount;
        savedModels = new string[childCount];

        for (int i = 0; i < childCount; i++)
        {
            GameObject child = toolAttachmentPoint.GetChild(i).gameObject;
            savedModels[i] = DataService.SaveModel(child);
            GameObject.Destroy(child);
        }

        RpcApplyTools(netIdentity, savedModels);
    }

    void Update()
    {
        if (!NetworkServer.active && isLocalPlayer)
        {
            var existingTools = GameObject.FindGameObjectsWithTag("Object");
            foreach (var t in existingTools)
            {
                if (!t.GetComponent<NetworkIdentity>() && t.GetComponent<ObjectClass>().className == "Tool")
                {
                    GameObject.Destroy(t);
                }
            }
        }
    }

    public void LoadDefaults(Player player)
    {
        if (isServer)
            ApplyDefaults(player);
        else
            CmdRequestLoadDefaults(player.netIdentity);
    }

    [Server]
    void ApplyDefaults(Player player)
    {
        if (player == null) return;

        player.health = (int)maxHealth;
        player.maximumHealth = (int)maxHealth;

        TargetApplyMovement(player.connectionToClient, walkSpeed, jumpPower);

        ApplyTools(player);
    }

    [TargetRpc]
    void TargetApplyMovement(NetworkConnection target, float walkSpeed, float jumpForce)
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.speed = walkSpeed;
            movement.jumpForce = jumpForce;
        }
    }

    [Server]
    void ApplyTools(Player player)
    {
        if (savedModels == null || savedModels.Length == 0) return;

        Transform attachmentPoint = player.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null)
            return;

        foreach (string modelXml in savedModels)
        {
            GameObject[] models = DataService.LoadFromString(modelXml, true);

            foreach (GameObject model in models)
            {
                if (model.name.StartsWith("PlayerDefaults"))
                {
                    GameObject.Destroy(model);
                    continue;
                }

                if (model.name.EndsWith("-1"))
                {
                    model.name = model.name.Substring(0, model.name.Length - 2);
                }

                if (model.transform.parent == null)
                {
                    model.transform.SetParent(attachmentPoint);
                }

                int childCount = model.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = model.transform.GetChild(i);

                    if (child.name.EndsWith("-1"))
                    {
                        child.name = child.name.Substring(0, child.name.Length - 2);
                    }

                    var childParentSync = child.GetComponent<ParentSync>();
                    if (childParentSync != null)
                    {
                        childParentSync.ForceUpdate();
                    }
                }

                NetworkServer.Spawn(model);
            }
        }
    }

    [ClientRpc]
    void RpcApplyTools(NetworkIdentity playerNetId, string[] xml)
    {
        GameObject playerObj = playerNetId != null ? playerNetId.gameObject : null;
        if (playerObj == null) return;

        Transform attachmentPoint = playerObj.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null) return;

        foreach (string modelXml in xml)
        {
            GameObject[] models = DataService.LoadFromString(modelXml, false);

            foreach (GameObject model in models)
            {
                if (model.name.StartsWith("PlayerDefaults"))
                {
                    GameObject.Destroy(model);
                    continue;
                }

                if (model.name.EndsWith("-1"))
                {
                    model.name = model.name.Substring(0, model.name.Length - 2);
                }

                if (model.transform.parent == null)
                {
                    model.transform.SetParent(attachmentPoint);
                }

                var parentSync = model.GetComponent<ParentSync>();
                if (parentSync != null)
                {
                    parentSync.ForceUpdate();
                }

                int childCount = model.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = model.transform.GetChild(i);

                    if (child.name.EndsWith("-1"))
                    {
                        child.name = child.name.Substring(0, child.name.Length - 2);
                    }

                    var childParentSync = child.GetComponent<ParentSync>();
                    if (childParentSync != null)
                    {
                        childParentSync.ForceUpdate();
                    }
                }
            }
        }
    }

    [Command]
    void CmdRequestLoadDefaults(NetworkIdentity playerNetId)
    {
        if (playerNetId != null)
        {
            RpcApplyTools(playerNetId, savedModels);
        }
    }

    [Command]
    public void CmdSetWalkSpeed(float value)
    {
        walkSpeed = value;
    }

    [Command]
    public void CmdSetJumpPower(float value)
    {
        jumpPower = value;
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