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
        if(!isServer) return;

        GameObject defaultsObj = GameObject.Find("PlayerDefaults");
        if (defaultsObj == null)
        {
            return;
        }

        Transform toolAttachmentPoint = defaultsObj.transform.Find("ToolAttachmentPoint");
        if (toolAttachmentPoint == null)
        {
            return;
        }

        int childCount = toolAttachmentPoint.childCount;
        savedModels = new string[childCount];

        for (int i = 0; i < childCount; i++)
        {
            GameObject child = toolAttachmentPoint.GetChild(i).gameObject;
            savedModels[i] = DataService.SaveModel(child);
            GameObject.Destroy(child);
        }
    }

    void Update()
    {
        //tiny cleanup i added to fix the double glitch we have, todo: actually fix it for real

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

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        player.health = (int)maxHealth;
        player.maximumHealth = (int)maxHealth;

        if (movement != null)
        {
            movement.speed = walkSpeed;
            movement.jumpForce = jumpPower;
        }

        ApplyTools(player);
    }

    [Server]
    void ApplyTools(Player player)
    {
        if (savedModels == null || savedModels.Length == 0) return;

        Transform attachmentPoint = player.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null)
        {
            Debug.LogWarning("ToolAttachmentPoint not found on server-side player");
            return;
        }

        foreach (Transform child in attachmentPoint)
        {
            Debug.Log($"[connId {player.connectionToClient.connectionId}] Tool: {child.name} (InstanceID: {child.GetInstanceID()})");
        }

        NetworkConnectionToClient conn = player.connectionToClient;

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
                    model.transform.SetParent(attachmentPoint);

                //model.GetComponent<ParentSync>()?.ForceUpdate();

                int childCount = model.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = model.transform.GetChild(i);

                    if (child.name.EndsWith("-1"))
                    {
                        child.name = child.name.Substring(0, child.name.Length - 2);
                    }

                    child.GetComponent<ParentSync>()?.ForceUpdate();
                }
            }
        }

        //RpcApplyTools(player.netIdentity, savedModels);
    }

    [ClientRpc]
    void RpcApplyTools(NetworkIdentity playerNetId, string[] xml)
    {
        return;

        Player player = playerNetId.GetComponent<Player>();
        if (player == null) return;

        Transform attachmentPoint = player.transform.Find("LeftArm/ToolAttachmentPoint");
        if (attachmentPoint == null)
        {
            Debug.LogWarning("ToolAttachmentPoint not found on player");
            return;
        }

        foreach (string modelXml in xml)
        {
            GameObject[] models = DataService.LoadFromString(modelXml, false);
            foreach(GameObject model in models)
            {
                if(model.name == "PlayerDefaults")
                {
                    GameObject.Destroy(model);
                    return;
                }
                
                if(model.transform.parent == null)
                    model.transform.SetParent(attachmentPoint);

                model.GetComponent<ParentSync>().ForceUpdate();
            }
        }
    }

    [Command]
    void CmdRequestLoadDefaults(NetworkIdentity playerNetId)
    {
        return;

        Player player = playerNetId.GetComponent<Player>();
        ApplyDefaults(player);
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