using Mirror;
using UnityEngine;

public class Seat : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnOccupantChanged))]
    private NetworkIdentity occupant;

    private bool IsOccupied => occupant != null;

    private void OnOccupantChanged(NetworkIdentity oldOcc, NetworkIdentity newOcc)
    {
        if (newOcc != null)
        {
            newOcc.transform.SetParent(transform);
            if (newOcc.isLocalPlayer)
            {
                newOcc.transform.position = transform.position;
                newOcc.GetComponent<PlayerMovement>().SitOn(this);
            }
        }
        else
        {
            if (oldOcc != null)
            {
                oldOcc.transform.SetParent(null);
                if (oldOcc.isLocalPlayer)
                {
                    oldOcc.GetComponent<PlayerMovement>().StandUp();
                    oldOcc.transform.position = transform.position + Vector3.forward;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (IsOccupied) return;

        if (other.CompareTag("Object"))
        {
            ObjectClass oc = other.GetComponent<ObjectClass>();
            if (oc != null && oc.className == "Player")
            {
                NetworkIdentity playerNetId = other.GetComponent<NetworkIdentity>();
                if (playerNetId != null)
                {
                    CmdRequestSit(playerNetId);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestSit(NetworkIdentity playerIdentity)
    {
        if (!IsOccupied)
        {
            occupant = playerIdentity;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestStand(NetworkIdentity playerIdentity)
    {
        if (occupant == playerIdentity)
        {
            occupant = null;
        }
    }
}