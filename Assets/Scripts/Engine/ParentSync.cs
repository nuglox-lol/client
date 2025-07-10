using Mirror;
using UnityEngine;
using System.Collections;

public class ParentSync : NetworkBehaviour
{
    // thanks mirror i have to use this shit system

    [SyncVar(hook = nameof(OnParentChanged))]
    public uint parentNetId;

    public override void OnStartServer()
    {
        UpdateParentNetId();
    }

    private void UpdateParentNetId()
    {
        //if (!isServer) return;
        var parentIdentity = transform.parent ? transform.parent.GetComponent<NetworkIdentity>() : null;
        parentNetId = parentIdentity != null ? parentIdentity.netId : 0;
    }

    [Server]
    public void ForceUpdate()
    {
        UpdateParentNetId();
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

    [ServerCallback]
    private void LateUpdate()
    {
        UpdateParentNetId();
    }
}