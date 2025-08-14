using Mirror;
using UnityEngine;
using System.Collections;

public class ParentSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnParentChanged))]
    public uint parentNetId;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string syncedName;

    Transform lastParent;
    string lastName;

    public override void OnStartServer()
    {
        StartCoroutine(KeepUpdating());
    }

    private void UpdateParentNetId()
    {
        var parentIdentity = transform.parent ? transform.parent.GetComponent<NetworkIdentity>() : null;
        parentNetId = parentIdentity != null ? parentIdentity.netId : 0;
    }

    [Server]
    public void ForceUpdate()
    {
        UpdateParentNetId();
        lastParent = transform.parent;
        syncedName = gameObject.name;
        lastName = syncedName;
    }

    private void OnParentChanged(uint oldNetId, uint newNetId)
    {
        if (newNetId == 0)
        {
            transform.SetParent(null);
            return;
        }

        if (NetworkClient.spawned.TryGetValue(newNetId, out NetworkIdentity parentIdentity))
        {
            transform.SetParent(parentIdentity.transform, true);
        }
        else
        {
            StartCoroutine(WaitForParentSpawn(newNetId));
        }
    }

    private IEnumerator WaitForParentSpawn(uint netId)
    {
        while (!NetworkClient.spawned.ContainsKey(netId))
            yield return null;
        transform.SetParent(NetworkClient.spawned[netId].transform, true);
    }

    private void OnNameChanged(string oldName, string newName)
    {
        gameObject.name = newName;
    }

    private IEnumerator KeepUpdating()
    {
        while (true)
        {
            ForceUpdate();
            syncedName = gameObject.name;
            yield return new WaitForSeconds(1f);
        }
    }
}
